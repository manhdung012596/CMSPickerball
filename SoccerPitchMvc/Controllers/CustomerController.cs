using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý Khách hàng (Customers).
/// Hỗ trợ CRUD: Danh sách, Thêm mới, Chỉnh sửa, Xóa, và Lịch sử đặt sân theo khách hàng.
/// </summary>
[Authorize(Roles = "Admin,Staff")]
public class CustomerController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomerController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Customer
    /// Hiển thị danh sách tất cả khách hàng, có tìm kiếm theo tên hoặc số điện thoại.
    /// </summary>
    public async Task<IActionResult> Index(string? search)
    {
        ViewBag.Search = search;

        var query = _context.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.Trim().ToLower();
            query = query.Where(c =>
                c.FullName.ToLower().Contains(s) ||
                c.PhoneNumber.Contains(s) ||
                (c.Address != null && c.Address.ToLower().Contains(s)));
        }

        var customers = await query
            .OrderBy(c => c.FullName)
            .ToListAsync();

        // Đếm số lần đặt sân cho mỗi khách hàng
        var bookingCounts = await _context.Bookings
            .Where(b => b.Status != 3)
            .GroupBy(b => b.CustomerId)
            .Select(g => new { CustomerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Count);

        ViewBag.BookingCounts = bookingCounts;
        return View(customers);
    }

    /// <summary>
    /// GET: /Customer/Create
    /// Form thêm mới khách hàng.
    /// </summary>
    public IActionResult Create()
    {
        return View(new CreateCustomerDto());
    }

    /// <summary>
    /// POST: /Customer/Create
    /// Xử lý thêm mới khách hàng, kiểm tra trùng số điện thoại.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra trùng số điện thoại
            bool phoneExists = await _context.Customers
                .AnyAsync(c => c.PhoneNumber == dto.PhoneNumber);

            if (phoneExists)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được đăng ký cho khách hàng khác.");
                return View(dto);
            }

            var customer = new Customer
            {
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Address = dto.Address?.Trim()
            };

            _context.Add(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm khách hàng \"{customer.FullName}\" thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    /// <summary>
    /// GET: /Customer/Edit/5
    /// Form chỉnh sửa thông tin khách hàng.
    /// </summary>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        var dto = new UpdateCustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address
        };

        return View(dto);
    }

    /// <summary>
    /// POST: /Customer/Edit/5
    /// Xử lý cập nhật thông tin khách hàng, kiểm tra trùng SĐT với KH khác.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCustomerDto dto)
    {
        if (id != dto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // Kiểm tra trùng SĐT với khách hàng khác
            bool phoneExists = await _context.Customers
                .AnyAsync(c => c.PhoneNumber == dto.PhoneNumber && c.Id != id);

            if (phoneExists)
            {
                ModelState.AddModelError("PhoneNumber", "Số điện thoại này đã được đăng ký cho khách hàng khác.");
                return View(dto);
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.FullName = dto.FullName.Trim();
            customer.PhoneNumber = dto.PhoneNumber.Trim();
            customer.Address = dto.Address?.Trim();

            _context.Update(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật thông tin khách hàng \"{customer.FullName}\" thành công!";
            return RedirectToAction(nameof(Index));
        }
        return View(dto);
    }

    /// <summary>
    /// POST: /Customer/Delete/5
    /// Xóa khách hàng (kiểm tra không có booking đang active).
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await _context.Customers
            .Include(c => c.Bookings)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return NotFound();

        // Kiểm tra có booking đang active không (Pending=0, Confirmed=1)
        bool hasActiveBookings = customer.Bookings.Any(b => b.Status == 0 || b.Status == 1);
        if (hasActiveBookings)
        {
            TempData["Error"] = $"Không thể xóa khách hàng \"{customer.FullName}\" vì đang có lượt đặt sân chưa hoàn thành.";
            return RedirectToAction(nameof(Index));
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đã xóa khách hàng \"{customer.FullName}\" thành công!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET: /Customer/History/5
    /// Xem lịch sử đặt sân của một khách hàng cụ thể.
    /// </summary>
    public async Task<IActionResult> History(int? id)
    {
        if (id == null) return NotFound();

        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        var bookings = await _context.Bookings
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .Where(b => b.CustomerId == id)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        ViewBag.Customer = customer;
        ViewBag.TotalBookings = bookings.Count;
        ViewBag.TotalPaid = bookings.Count(b => b.Status == 2);
        ViewBag.TotalRevenue = bookings.Where(b => b.Status == 2).Sum(b => b.TotalPrice);

        return View(bookings);
    }

    public IActionResult Profile()
    {
        return View();
    }
}
