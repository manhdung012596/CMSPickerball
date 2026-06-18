using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý Lịch đặt sân bóng (Bookings).
/// Chứa logic đặt sân, kiểm tra trùng lịch, tính tiền tự động, quản lý giao dịch (Transaction) và hiển thị ma trận lịch biểu.
/// </summary>
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Bookings
    /// Hiển thị danh sách các lượt đặt sân bóng.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.TimeSlot.StartTime)
            .ToListAsync();
        return View(bookings);
    }

    /// <summary>
    /// GET: /Bookings/Create
    /// Trả về View để khách hàng hoặc nhân viên đặt sân bóng mới.
    /// </summary>
    public async Task<IActionResult> Create()
    {
        ViewBag.CustomerId = new SelectList(await _context.Customers.ToListAsync(), "Id", "FullName");
        ViewBag.PitchId = new SelectList(await _context.Pitches.Where(p => p.Status == 0).ToListAsync(), "Id", "Name");
        return View();
    }

    /// <summary>
    /// POST: /Bookings/Create
    /// Xử lý tạo mới một lượt đặt sân bóng.
    /// Có Transaction, tự động tính giá và kiểm tra trùng lịch đặt.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        // Kiểm tra hợp lệ dữ liệu cơ bản
        if (booking.CustomerId <= 0 || booking.PitchId <= 0 || booking.TimeSlotId <= 0)
        {
            ModelState.AddModelError("", "Vui lòng điền đầy đủ các thông tin bắt buộc.");
            await PopulateSelectLists(booking);
            return View(booking);
        }

        // Thực hiện lưu trong Transaction để đảm bảo toàn vẹn dữ liệu
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 1. Kiểm tra trùng lặp đặt sân: PitchId + BookingDate + TimeSlotId và trạng thái không phải Cancelled (3)
                bool isConflict = await _context.Bookings
                    .AnyAsync(b => b.PitchId == booking.PitchId 
                                && b.BookingDate == booking.BookingDate 
                                && b.TimeSlotId == booking.TimeSlotId 
                                && b.Status != 3); // 3 = Cancelled

                if (isConflict)
                {
                    ModelState.AddModelError("", "Khung giờ này đã có người đặt trên sân đã chọn.");
                    await PopulateSelectLists(booking);
                    return View(booking);
                }

                // Lấy thông tin sân bóng và khung giờ để tính giá
                var pitch = await _context.Pitches.FindAsync(booking.PitchId);
                var timeSlot = await _context.TimeSlots.FindAsync(booking.TimeSlotId);

                if (pitch == null || timeSlot == null)
                {
                    ModelState.AddModelError("", "Sân bóng hoặc Khung giờ không tồn tại.");
                    await PopulateSelectLists(booking);
                    return View(booking);
                }

                // 2. Tính toán TotalPrice
                if (timeSlot.PriceOverride.HasValue)
                {
                    booking.TotalPrice = timeSlot.PriceOverride.Value;
                }
                else
                {
                    // Tính theo PricePerHour nhân với số giờ thuê thực tế
                    var duration = timeSlot.EndTime - timeSlot.StartTime;
                    decimal durationHours = (decimal)duration.TotalHours;
                    if (durationHours <= 0) durationHours = 1; // Fallback phòng trường hợp khung giờ không hợp lệ
                    booking.TotalPrice = pitch.PricePerHour * durationHours;
                }

                // Mặc định Status là Pending (0) và ngày tạo là hiện tại
                booking.Status = 0; // 0 = Pending
                booking.CreatedAt = DateTime.Now;

                _context.Add(booking);
                await _context.SaveChangesAsync();

                // Commit Transaction thành công
                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Rollback nếu có lỗi xảy ra
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Đã xảy ra lỗi trong quá trình đặt sân: {ex.Message}");
            }
        }

        await PopulateSelectLists(booking);
        return View(booking);
    }

    /// <summary>
    /// POST: /Bookings/UpdateStatus/5
    /// Cập nhật trạng thái lượt đặt sân (Pending, Confirmed, Paid, Cancelled).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, int status)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null)
        {
            return NotFound();
        }

        booking.Status = status;
        _context.Update(booking);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: /Bookings/Calendar
    /// Hiển thị ma trận lịch biểu đặt sân dạng lưới (Hàng là Sân, Cột là Khung giờ).
    /// </summary>
    public async Task<IActionResult> Calendar(DateOnly? date)
    {
        // Mặc định là ngày hôm nay nếu không chọn ngày cụ thể
        DateOnly filterDate = date ?? DateOnly.FromDateTime(DateTime.Today);
        ViewBag.SelectedDate = filterDate.ToString("yyyy-MM-dd");

        // Lấy tất cả sân bóng hoạt động
        var pitches = await _context.Pitches
            .Where(p => p.Status != 2) // Không lấy sân đã xóa mềm
            .ToListAsync();

        // Lấy tất cả khung giờ hiện có
        var timeSlots = await _context.TimeSlots
            .OrderBy(t => t.StartTime)
            .ToListAsync();

        // Lấy danh sách lượt đặt sân trong ngày được chọn mà chưa bị hủy
        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Where(b => b.BookingDate == filterDate && b.Status != 3)
            .ToListAsync();

        ViewBag.Pitches = pitches;
        ViewBag.TimeSlots = timeSlots;
        ViewBag.Bookings = bookings;

        return View();
    }

    /// <summary>
    /// API Endpoint lấy danh sách khung giờ theo PitchId (Dùng Ajax khi người dùng chọn sân khác nhau)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTimeSlotsByPitch(int pitchId)
    {
        var slots = await _context.TimeSlots
            .Where(t => t.PitchId == pitchId)
            .OrderBy(t => t.StartTime)
            .Select(t => new {
                id = t.Id,
                display = $"{t.StartTime:hh\\:mm} - {t.EndTime:hh\\:mm} " + (t.PriceOverride.HasValue ? $"(Giá riêng: {t.PriceOverride.Value:N0}đ)" : "(Giá mặc định)")
            })
            .ToListAsync();
        return Json(slots);
    }

    private async Task PopulateSelectLists(Booking booking)
    {
        ViewBag.CustomerId = new SelectList(await _context.Customers.ToListAsync(), "Id", "FullName", booking.CustomerId);
        ViewBag.PitchId = new SelectList(await _context.Pitches.Where(p => p.Status == 0).ToListAsync(), "Id", "Name", booking.PitchId);
        
        // Nếu đã chọn sân, load danh sách khung giờ của sân đó
        if (booking.PitchId > 0)
        {
            var slots = await _context.TimeSlots.Where(t => t.PitchId == booking.PitchId).ToListAsync();
            ViewBag.TimeSlotId = new SelectList(slots, "Id", "StartTime", booking.TimeSlotId);
        }
        else
        {
            ViewBag.TimeSlotId = new SelectList(new List<TimeSlot>(), "Id", "StartTime");
        }
    }
}
