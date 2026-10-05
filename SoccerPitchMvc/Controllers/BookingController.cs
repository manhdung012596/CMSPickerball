using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

[Authorize]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public BookingController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Booking/MyBookings
    public async Task<IActionResult> MyBookings()
    {
        var userId = _userManager.GetUserId(User);
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null)
        {
            // Nếu không tìm thấy customer (ví dụ là admin), trả về view với danh sách rỗng
            return View(new List<Booking>());
        }

        var bookings = await _context.Bookings
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .Where(b => b.CustomerId == customer.Id)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }

    // GET: /Booking/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null) return NotFound();

        var booking = await _context.Bookings
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == customer.Id);

        if (booking == null) return NotFound();

        return Json(new
        {
            id = booking.Id,
            courtName = booking.Pitch.Name,
            courtType = booking.Pitch.Type switch { 1 => "Sân đơn", 2 => "Sân đôi", 3 => "Sân tiêu chuẩn", _ => "Sân Pickleball" },
            bookingDate = booking.BookingDate.ToString("dd/MM/yyyy"),
            timeSlot = $"{booking.TimeSlot.StartTime:hh\\:mm} - {booking.TimeSlot.EndTime:hh\\:mm}",
            totalPrice = booking.TotalPrice.ToString("N0") + " VNĐ",
            createdAt = booking.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
            note = booking.Note ?? "Không có ghi chú",
            status = booking.Status,
            statusLabel = booking.Status switch
            {
                0 => "Chờ xác nhận",
                1 => "Đã xác nhận",
                2 => "Đã thanh toán",
                3 => "Đã hủy",
                _ => "Không rõ"
            }
        });
    }

    // POST: /Booking/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (customer == null) return NotFound();

        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.Id == id && b.CustomerId == customer.Id);

        if (booking == null) return NotFound();

        // Chỉ cho phép hủy khi trạng thái là Chờ xác nhận (0)
        if (booking.Status != 0)
        {
            TempData["Error"] = "Chỉ có thể hủy lượt đặt sân ở trạng thái Chờ xác nhận.";
            return RedirectToAction(nameof(MyBookings));
        }

        booking.Status = 3; // Đã hủy
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã hủy lượt đặt sân thành công.";
        return RedirectToAction(nameof(MyBookings));
    }
}
