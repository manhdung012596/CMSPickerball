using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    // GET: /Home/Index
    public async Task<IActionResult> Index()
    {
        var pitches = await _context.Pitches
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewBag.Pitches = pitches;
        ViewBag.TotalPitches = pitches.Count;
        ViewBag.TotalCustomers = await _context.Customers.CountAsync();
        ViewBag.TotalBookings = await _context.Bookings.CountAsync();

        return View();
    }

    // GET: /Home/Pitches
    public async Task<IActionResult> Pitches()
    {
        var pitches = await _context.Pitches
            .Include(p => p.TimeSlots)
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Type)
            .ThenBy(p => p.Name)
            .ToListAsync();

        ViewBag.Pitches = pitches;
        return View();
    }

    // GET: /Home/Booking?pitchId=1
    public async Task<IActionResult> Booking(int? pitchId)
    {
        var pitches = await _context.Pitches
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewBag.Pitches = pitches;
        ViewBag.SelectedPitchId = pitchId ?? (pitches.FirstOrDefault()?.Id ?? 0);

        // Load timeslots of selected pitch
        int selectedId = pitchId ?? (pitches.FirstOrDefault()?.Id ?? 0);
        if (selectedId > 0)
        {
            var slots = await _context.TimeSlots
                .Where(t => t.PitchId == selectedId)
                .OrderBy(t => t.StartTime)
                .ToListAsync();
            ViewBag.TimeSlots = slots;

            var selectedPitch = pitches.FirstOrDefault(p => p.Id == selectedId);
            ViewBag.SelectedPitch = selectedPitch;
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer != null)
            {
                ViewBag.CustomerName = customer.FullName;
                ViewBag.CustomerPhone = customer.PhoneNumber;
            }
        }

        return View();
    }

    // GET: /Home/GetTimeSlots?pitchId=1&date=2026-07-30
    // API JSON — dùng bởi AJAX khi đổi sân hoặc ngày
    [HttpGet]
    public async Task<IActionResult> GetTimeSlots(int pitchId, string date)
    {
        if (!DateOnly.TryParse(date, out var bookingDate))
            bookingDate = DateOnly.FromDateTime(DateTime.Today);

        var slots = await _context.TimeSlots
            .Where(t => t.PitchId == pitchId)
            .OrderBy(t => t.StartTime)
            .Select(t => new
            {
                t.Id,
                StartTime = t.StartTime.ToString("HH:mm"),
                EndTime = t.EndTime.ToString("HH:mm"),
                Price = t.PriceOverride
            })
            .ToListAsync();

        // Lấy các slot đã được đặt trong ngày đó (status != Cancelled=3)
        var booked = await _context.Bookings
            .Where(b => b.PitchId == pitchId
                     && b.BookingDate == bookingDate
                     && b.Status != 3)
            .Select(b => b.TimeSlotId)
            .ToListAsync();

        // Lấy giá mặc định của sân
        var pitch = await _context.Pitches.FindAsync(pitchId);
        decimal defaultPrice = pitch?.PricePerHour ?? 0;

        var result = slots.Select(s => new
        {
            s.Id,
            s.StartTime,
            s.EndTime,
            Price = s.Price ?? defaultPrice,
            IsBooked = booked.Contains(s.Id)
        });

        return Json(result);
    }

    // GET: /Home/CheckCoupon?code=SAVE50&pitchId=1&originalPrice=200000
    [HttpGet]
    public async Task<IActionResult> CheckCoupon(string code, int pitchId, decimal originalPrice)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });
        }

        var promo = await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code == code.Trim() && p.IsCoupon && p.Status == 1);

        if (promo == null)
        {
            return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã bị tạm ngưng." });
        }

        var now = DateTime.Now;
        if (now < promo.StartDate || now > promo.EndDate)
        {
            return Json(new { success = false, message = "Mã giảm giá đã hết hạn hoặc chưa đến ngày sử dụng." });
        }

        if (promo.UsageLimit.HasValue && promo.UsageCount >= promo.UsageLimit.Value)
        {
            return Json(new { success = false, message = "Mã giảm giá đã đạt giới hạn lượt sử dụng." });
        }

        if (promo.MinOrderAmount.HasValue && originalPrice < promo.MinOrderAmount.Value)
        {
            return Json(new { success = false, message = $"Giá trị đặt sân tối thiểu phải đạt {promo.MinOrderAmount.Value:N0} VNĐ để áp dụng mã này." });
        }

        decimal discount = 0;
        if (promo.DiscountType == 0) // VND
        {
            discount = promo.DiscountValue;
        }
        else if (promo.DiscountType == 1) // %
        {
            discount = originalPrice * (promo.DiscountValue / 100);
            if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
            {
                discount = promo.MaxDiscountAmount.Value;
            }
        }

        if (discount > originalPrice)
        {
            discount = originalPrice;
        }

        return Json(new
        {
            success = true,
            discountAmount = discount,
            newTotal = originalPrice - discount,
            message = $"Áp dụng thành công mã {promo.Code}!"
        });
    }

    // POST: /Home/SubmitBooking
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitBooking(
        int pitchId,
        int timeSlotId,
        string bookingDate,
        string customerName,
        string customerPhone,
        string? notes,
        string? couponCode)
    {
        // Validate
        if (!DateOnly.TryParse(bookingDate, out var bDate))
        {
            TempData["BookingError"] = "Ngày không hợp lệ.";
            return RedirectToAction(nameof(Booking), new { pitchId });
        }

        // Kiểm tra slot đã bị đặt chưa
        bool alreadyBooked = await _context.Bookings
            .AnyAsync(b => b.PitchId == pitchId
                        && b.TimeSlotId == timeSlotId
                        && b.BookingDate == bDate
                        && b.Status != 3);

        if (alreadyBooked)
        {
            TempData["BookingError"] = "Khung giờ này đã được đặt. Vui lòng chọn giờ khác!";
            return RedirectToAction(nameof(Booking), new { pitchId });
        }

        Customer? customer = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);
            customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null)
            {
                customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == customerPhone.Trim());
                if (customer == null)
                {
                    customer = new Customer
                    {
                        FullName = customerName.Trim(),
                        PhoneNumber = customerPhone.Trim(),
                        UserId = userId
                    };
                    _context.Customers.Add(customer);
                }
                else
                {
                    customer.UserId = userId;
                    customer.FullName = customerName.Trim();
                    _context.Update(customer);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                bool changed = false;
                if (customer.FullName != customerName.Trim())
                {
                    customer.FullName = customerName.Trim();
                    changed = true;
                }
                if (customer.PhoneNumber != customerPhone.Trim())
                {
                    var another = await _context.Customers.AnyAsync(c => c.PhoneNumber == customerPhone.Trim() && c.Id != customer.Id);
                    if (!another)
                    {
                        customer.PhoneNumber = customerPhone.Trim();
                        changed = true;
                    }
                }
                if (changed)
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
            }
        }
        else
        {
            customer = await _context.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == customerPhone.Trim());
            if (customer == null)
            {
                customer = new Customer
                {
                    FullName = customerName.Trim(),
                    PhoneNumber = customerPhone.Trim()
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            else if (customer.FullName != customerName.Trim())
            {
                customer.FullName = customerName.Trim();
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
        }

        // Tính giá
        var timeSlot = await _context.TimeSlots
            .Include(t => t.Pitch)
            .FirstOrDefaultAsync(t => t.Id == timeSlotId);

        if (timeSlot == null)
        {
            TempData["BookingError"] = "Khung giờ không tồn tại.";
            return RedirectToAction(nameof(Booking), new { pitchId });
        }

        decimal totalPrice = timeSlot.PriceOverride ?? timeSlot.Pitch.PricePerHour;

        decimal discountAmount = 0;
        string couponNote = "";

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            var promo = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == couponCode.Trim() && p.IsCoupon && p.Status == 1);

            if (promo != null)
            {
                var now = DateTime.Now;
                bool dateValid = now >= promo.StartDate && now <= promo.EndDate;
                bool limitValid = !promo.UsageLimit.HasValue || promo.UsageCount < promo.UsageLimit.Value;
                bool minOrderValid = !promo.MinOrderAmount.HasValue || totalPrice >= promo.MinOrderAmount.Value;

                if (dateValid && limitValid && minOrderValid)
                {
                    if (promo.DiscountType == 0) // VND
                    {
                        discountAmount = promo.DiscountValue;
                    }
                    else if (promo.DiscountType == 1) // %
                    {
                        discountAmount = totalPrice * (promo.DiscountValue / 100);
                        if (promo.MaxDiscountAmount.HasValue && discountAmount > promo.MaxDiscountAmount.Value)
                        {
                            discountAmount = promo.MaxDiscountAmount.Value;
                        }
                    }

                    if (discountAmount > totalPrice)
                    {
                        discountAmount = totalPrice;
                    }

                    totalPrice -= discountAmount;

                    promo.UsageCount++;
                    _context.Promotions.Update(promo);

                    couponNote = $"[Sử dụng Voucher {promo.Code}: -{discountAmount:N0} VNĐ] ";
                }
            }
        }

        // Tạo Booking
        var booking = new Booking
        {
            PitchId = pitchId,
            TimeSlotId = timeSlotId,
            CustomerId = customer.Id,
            BookingDate = bDate,
            TotalPrice = totalPrice,
            Note = string.IsNullOrEmpty(notes) ? couponNote.Trim() : (couponNote + notes.Trim()),
            Status = 0, // Pending — chờ xác nhận
            CreatedAt = DateTime.Now
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(BookingConfirmed), new { id = booking.Id });
    }

    // GET: /Home/BookingConfirmed/5
    public async Task<IActionResult> BookingConfirmed(int id)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        return View(booking);
    }

    // GET: /Home/Contact
    public IActionResult Contact()
    {
        return View();
    }

    // POST: /Home/SubmitFeedback
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitFeedback(string name, string email, string? phone, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
        {
            return Json(new { success = false, message = "Vui lòng điền đầy đủ các thông tin bắt buộc (*)." });
        }

        try
        {
            var feedback = new Article
            {
                Title = name.Trim(),
                Slug = Guid.NewGuid().ToString(),
                Summary = email.Trim(),
                ThumbnailUrl = phone?.Trim(),
                Content = message.Trim(),
                AuthorName = "Customer Feedback",
                CreatedAt = DateTime.Now,
                Status = 0 // 0 = Chưa đọc
            };

            _context.Articles.Add(feedback);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Gửi phản hồi thành công! Cảm ơn {name} đã đóng góp ý kiến." });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Đã xảy ra lỗi khi gửi phản hồi: {ex.Message}" });
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
