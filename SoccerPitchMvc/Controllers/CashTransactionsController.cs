using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý Sổ Quỹ (CashTransactions).
/// Thu tiền sân, Chi phí vận hành, Báo cáo thu chi, Đối soát.
/// TransactionType: 1 = Thu, 0 = Chi
/// </summary>
[Authorize(Roles = "Admin,Staff")]
public class CashTransactionsController : Controller
{
    private readonly ApplicationDbContext _context;

    // Mapping Category -> Tên
    private static readonly Dictionary<int, string> CategoryNames = new()
    {
        { 10, "Thu tiền sân (đặt sân)" },
        { 11, "Thu dịch vụ" },
        { 12, "Thu khác" },
        { 21, "Chi điện nước" },
        { 22, "Chi sửa chữa / vận hành" },
        { 23, "Chi lương nhân viên" },
        { 24, "Chi khác" }
    };

    private static readonly Dictionary<int, string> PaymentMethodNames = new()
    {
        { 0, "Tiền mặt" },
        { 1, "Chuyển khoản" },
        { 2, "Ví điện tử" }
    };

    public CashTransactionsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /CashTransactions/Income
    /// Danh sách phiếu thu (TransactionType = 1).
    /// </summary>
    public async Task<IActionResult> Income(int? month, int? year)
    {
        int y = year ?? DateTime.Today.Year;
        int m = month ?? DateTime.Today.Month;

        ViewBag.Month = m;
        ViewBag.Year = y;
        ViewBag.CategoryNames = CategoryNames;
        ViewBag.PaymentMethodNames = PaymentMethodNames;

        var list = await _context.CashTransactions
            .Where(t => t.TransactionType == 1
                     && t.TransactionDate.Month == m
                     && t.TransactionDate.Year == y)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        ViewBag.Total = list.Sum(t => t.Amount);
        return View(list);
    }

    /// <summary>
    /// GET: /CashTransactions/Expense
    /// Danh sách phiếu chi - Chi phí vận hành (TransactionType = 0).
    /// </summary>
    public async Task<IActionResult> Expense(int? month, int? year)
    {
        int y = year ?? DateTime.Today.Year;
        int m = month ?? DateTime.Today.Month;

        ViewBag.Month = m;
        ViewBag.Year = y;
        ViewBag.CategoryNames = CategoryNames;
        ViewBag.PaymentMethodNames = PaymentMethodNames;

        var list = await _context.CashTransactions
            .Where(t => t.TransactionType == 0
                     && t.TransactionDate.Month == m
                     && t.TransactionDate.Year == y)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        ViewBag.Total = list.Sum(t => t.Amount);
        return View(list);
    }

    /// <summary>
    /// GET: /CashTransactions/Create?type=1 (Thu) hoặc type=0 (Chi)
    /// </summary>
    public IActionResult Create(int type = 1)
    {
        ViewBag.TransactionType = type;
        ViewBag.CategoryNames = CategoryNames;
        ViewBag.PaymentMethodNames = PaymentMethodNames;

        var model = new CashTransaction
        {
            TransactionType = type,
            TransactionDate = DateTime.Now,
            Category = type == 1 ? 10 : 21,
            PaymentMethod = 0,
            TransactionCode = $"PT{DateTime.Now:yyyyMMddHHmm}"
        };
        return View(model);
    }

    /// <summary>
    /// POST: /CashTransactions/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CashTransaction model)
    {
        // Bỏ qua validation TransactionCode nếu rỗng — tự sinh
        ModelState.Remove("TransactionCode");

        if (string.IsNullOrWhiteSpace(model.TransactionCode))
        {
            string prefix = model.TransactionType == 1 ? "PT" : "PC";
            model.TransactionCode = $"{prefix}{DateTime.Now:yyyyMMddHHmmss}";
        }

        model.CreatedAt = DateTime.Now;

        if (ModelState.IsValid)
        {
            _context.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = model.TransactionType == 1
                ? $"Đã ghi phiếu thu {model.Amount:N0} VNĐ thành công!"
                : $"Đã ghi phiếu chi {model.Amount:N0} VNĐ thành công!";

            return model.TransactionType == 1
                ? RedirectToAction(nameof(Income))
                : RedirectToAction(nameof(Expense));
        }

        ViewBag.TransactionType = model.TransactionType;
        ViewBag.CategoryNames = CategoryNames;
        ViewBag.PaymentMethodNames = PaymentMethodNames;
        return View(model);
    }

    /// <summary>
    /// POST: /CashTransactions/Delete/5
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tx = await _context.CashTransactions.FindAsync(id);
        if (tx == null) return NotFound();

        int type = tx.TransactionType;
        _context.CashTransactions.Remove(tx);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã xóa phiếu giao dịch thành công!";
        return type == 1 ? RedirectToAction(nameof(Income)) : RedirectToAction(nameof(Expense));
    }

    /// <summary>
    /// GET: /CashTransactions/Report
    /// Báo cáo thu chi theo tháng: tổng thu, tổng chi, lợi nhuận.
    /// </summary>
    public async Task<IActionResult> Report(int? year)
    {
        int y = year ?? DateTime.Today.Year;
        ViewBag.Year = y;

        // Tổng theo tháng
        var allTx = await _context.CashTransactions
            .Where(t => t.TransactionDate.Year == y)
            .ToListAsync();

        // Thu từ booking (Bookings đã thanh toán)
        var bookingRevenue = await _context.Bookings
            .Where(b => b.Status == 2 && b.BookingDate.Year == y)
            .GroupBy(b => b.BookingDate.Month)
            .Select(g => new { Month = g.Key, Amount = g.Sum(b => b.TotalPrice) })
            .ToDictionaryAsync(x => x.Month, x => x.Amount);

        var monthlyData = Enumerable.Range(1, 12).Select(m => new
        {
            Month = m,
            TotalIncome = allTx.Where(t => t.TransactionType == 1 && t.TransactionDate.Month == m).Sum(t => t.Amount),
            TotalExpense = allTx.Where(t => t.TransactionType == 0 && t.TransactionDate.Month == m).Sum(t => t.Amount),
            BookingRevenue = bookingRevenue.ContainsKey(m) ? bookingRevenue[m] : 0m
        }).ToList();

        ViewBag.MonthlyData = monthlyData;
        ViewBag.GrandIncome = allTx.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        ViewBag.GrandExpense = allTx.Where(t => t.TransactionType == 0).Sum(t => t.Amount);
        ViewBag.GrandBooking = bookingRevenue.Values.Sum();

        return View();
    }

    /// <summary>
    /// GET: /CashTransactions/Reconcile
    /// Đối soát: So sánh doanh thu đặt sân (Bookings đã TT) vs Thu ghi sổ quỹ theo tháng.
    /// </summary>
    public async Task<IActionResult> Reconcile(int? month, int? year)
    {
        int y = year ?? DateTime.Today.Year;
        int m = month ?? DateTime.Today.Month;

        ViewBag.Month = m;
        ViewBag.Year = y;

        // Booking đã thanh toán trong tháng
        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Where(b => b.Status == 2
                     && b.BookingDate.Month == m
                     && b.BookingDate.Year == y)
            .OrderBy(b => b.BookingDate)
            .ToListAsync();

        // Phiếu thu sổ quỹ trong tháng (Category 10 = thu tiền sân)
        var cashIncome = await _context.CashTransactions
            .Where(t => t.TransactionType == 1
                     && t.TransactionDate.Month == m
                     && t.TransactionDate.Year == y)
            .ToListAsync();

        ViewBag.Bookings = bookings;
        ViewBag.BookingTotal = bookings.Sum(b => b.TotalPrice);
        ViewBag.CashTotal = cashIncome.Sum(t => t.Amount);
        ViewBag.Difference = cashIncome.Sum(t => t.Amount) - bookings.Sum(b => b.TotalPrice);

        return View();
    }
}
