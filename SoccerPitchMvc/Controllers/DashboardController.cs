using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using System;
using System.Linq;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller xử lý hiển thị Báo cáo - Thống kê (Dashboard).
/// </summary>
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Dashboard/Index hoặc /
    /// Trang chủ thống kê doanh thu, tỷ lệ đặt sân và các dữ liệu đồ thị.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        // 1. Thống kê tổng số đơn đặt hôm nay (không tính các đơn bị hủy)
        int bookingsToday = await _context.Bookings
            .CountAsync(b => b.BookingDate == today && b.Status != 3);

        // 2. Thống kê tổng doanh thu thực tế (đã thanh toán - Status = 2) từ trước đến nay
        decimal totalRevenueAllTime = await _context.Bookings
            .Where(b => b.Status == 2)
            .SumAsync(b => b.TotalPrice);

        // 3. Tỷ lệ lấp đầy hôm nay (Tổng số slot đã được đặt / Tổng số slot được cấu hình của các sân)
        int totalActiveSlots = await _context.TimeSlots
            .CountAsync(t => _context.Pitches.Any(p => p.Id == t.PitchId && p.Status != 2));
        double occupancyRateToday = 0;
        if (totalActiveSlots > 0)
        {
            occupancyRateToday = (double)bookingsToday / totalActiveSlots * 100;
        }

        // 4. Top 3 sân bóng được đặt nhiều nhất
        var topPitchesData = await _context.Bookings
            .Where(b => b.Status != 3) // Trừ đơn đã hủy
            .GroupBy(b => b.Pitch.Name)
            .Select(g => new { PitchName = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(3)
            .ToListAsync();

        ViewBag.TopPitchesLabels = topPitchesData.Select(x => x.PitchName).ToList();
        ViewBag.TopPitchesCounts = topPitchesData.Select(x => x.Count).ToList();

        // 5. Thống kê doanh thu 7 ngày gần nhất (chỉ tính đơn đã thanh toán - Status = 2)
        var datesRange = Enumerable.Range(-6, 7)
            .Select(i => DateOnly.FromDateTime(DateTime.Today.AddDays(i)))
            .ToList();

        var bookingsInLast7Days = await _context.Bookings
            .Where(b => b.Status == 2 && b.BookingDate >= datesRange.First() && b.BookingDate <= datesRange.Last())
            .GroupBy(b => b.BookingDate)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
            .ToListAsync();

        // Ghép nối dữ liệu đủ 7 ngày (ngày không có doanh thu thì gán = 0)
        var revenueData = datesRange.Select(d => new
        {
            Label = d.ToString("dd/MM"),
            Revenue = bookingsInLast7Days.FirstOrDefault(b => b.Date == d)?.Revenue ?? 0
        }).ToList();

        ViewBag.RevenueLabels = revenueData.Select(x => x.Label).ToList();
        ViewBag.RevenueValues = revenueData.Select(x => x.Revenue).ToList();

        // Đưa các thông số thống kê ra View
        ViewBag.BookingsToday = bookingsToday;
        ViewBag.TotalRevenueAllTime = totalRevenueAllTime;
        ViewBag.OccupancyRateToday = Math.Round(occupancyRateToday, 1);

        return View();
    }
}
