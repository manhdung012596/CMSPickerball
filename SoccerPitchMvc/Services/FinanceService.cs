using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public class FinanceService : IFinanceService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IMapper _mapper;

    // Static list for reconciliation history simulation in memory
    private static readonly List<ReconciliationItemDto> _reconHistory = new()
    {
        new ReconciliationItemDto
        {
            ReconciledAt = DateTime.Now.AddDays(-3),
            ExpectedCash = 12500000,
            ActualCash = 12500000,
            Auditor = "Kế toán Nguyễn Thị D"
        },
        new ReconciliationItemDto
        {
            ReconciledAt = DateTime.Now.AddDays(-2),
            ExpectedCash = 8400000,
            ActualCash = 8350000,
            Auditor = "Kế toán Nguyễn Thị D"
        },
        new ReconciliationItemDto
        {
            ReconciledAt = DateTime.Now.AddDays(-1),
            ExpectedCash = 15600000,
            ActualCash = 15600000,
            Auditor = "Kế toán Nguyễn Thị D"
        }
    };

    public FinanceService(IDbContextFactory<ApplicationDbContext> dbFactory, IMapper mapper)
    {
        _dbFactory = dbFactory;
        _mapper = mapper;
    }

    #region Cash Transaction CRUD
    public async Task<List<CashTransactionListDto>> GetTransactionsAsync(string? search = null, int? type = null, int? category = null, int? paymentMethod = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _dbFactory.CreateDbContext();
        var query = context.CashTransactions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(t => t.TransactionCode.ToLower().Contains(s) || 
                                     t.PayerPayee.ToLower().Contains(s) || 
                                     (t.Notes != null && t.Notes.ToLower().Contains(s)));
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        if (category.HasValue && category.Value > 0)
        {
            query = query.Where(t => t.Category == category.Value);
        }

        if (paymentMethod.HasValue)
        {
            query = query.Where(t => t.PaymentMethod == paymentMethod.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        var list = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
        return _mapper.Map<List<CashTransactionListDto>>(list);
    }

    public async Task<CashTransaction?> GetTransactionByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.CashTransactions.FindAsync(id);
    }

    public async Task<CashTransaction> CreateTransactionAsync(CreateCashTransactionDto dto)
    {
        using var context = _dbFactory.CreateDbContext();

        // Generate Transaction Code: PT-yyyyMMdd-XXXX or PC-yyyyMMdd-XXXX
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var prefix = dto.TransactionType == 1 ? "PT" : "PC";
        var countToday = await context.CashTransactions
            .CountAsync(t => t.TransactionCode.StartsWith($"{prefix}-{dateStr}"));
        var code = $"{prefix}-{dateStr}-{(countToday + 1):D4}";

        var transaction = _mapper.Map<CashTransaction>(dto);
        transaction.TransactionCode = code;
        transaction.CreatedAt = DateTime.Now;

        context.CashTransactions.Add(transaction);
        await context.SaveChangesAsync();
        return transaction;
    }

    public async Task<CashTransaction> UpdateTransactionAsync(UpdateCashTransactionDto dto)
    {
        using var context = _dbFactory.CreateDbContext();
        var transaction = await context.CashTransactions.FindAsync(dto.Id);
        if (transaction == null)
        {
            throw new KeyNotFoundException("Không tìm thấy phiếu giao dịch cần cập nhật.");
        }

        // Keep original code and created date
        var origCode = transaction.TransactionCode;
        var origCreated = transaction.CreatedAt;

        _mapper.Map(dto, transaction);
        
        transaction.TransactionCode = origCode;
        transaction.CreatedAt = origCreated;

        await context.SaveChangesAsync();
        return transaction;
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var transaction = await context.CashTransactions.FindAsync(id);
        if (transaction == null) return false;

        context.CashTransactions.Remove(transaction);
        await context.SaveChangesAsync();
        return true;
    }
    #endregion

    #region Court Income Report
    public async Task<List<CourtIncomeDto>> GetCourtIncomeReportAsync(string? search = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _dbFactory.CreateDbContext();
        
        // Paid bookings (Status = 2)
        var query = context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .Where(b => b.Status == 2)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b => b.Customer.FullName.ToLower().Contains(s) || 
                                     b.Customer.PhoneNumber.Contains(s) || 
                                     b.Pitch.Name.ToLower().Contains(s));
        }

        if (startDate.HasValue)
        {
            var startOnly = DateOnly.FromDateTime(startDate.Value);
            query = query.Where(b => b.BookingDate >= startOnly);
        }

        if (endDate.HasValue)
        {
            var endOnly = DateOnly.FromDateTime(endDate.Value);
            query = query.Where(b => b.BookingDate <= endOnly);
        }

        var list = await query.OrderByDescending(b => b.BookingDate).ThenBy(b => b.TimeSlot.StartTime).ToListAsync();
        
        return list.Select(b => new CourtIncomeDto
        {
            BookingId = b.Id,
            PitchName = b.Pitch.Name,
            CustomerName = b.Customer.FullName,
            CustomerPhone = b.Customer.PhoneNumber,
            BookingDate = b.BookingDate,
            TimeSlot = $"{b.TimeSlot.StartTime:HH:mm} - {b.TimeSlot.EndTime:HH:mm}",
            Amount = b.TotalPrice,
            CreatedAt = b.CreatedAt
        }).ToList();
    }
    #endregion

    #region Consolidated Reports
    public async Task<CashFlowSummaryDto> GetCashFlowSummaryAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _dbFactory.CreateDbContext();

        // 1. Get Court Booking Revenue (assume Transfer)
        var bookingsQuery = context.Bookings.Where(b => b.Status == 2).AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            var start = DateOnly.FromDateTime(startDate.Value);
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= start);
        }
        if (endDate.HasValue)
        {
            var end = DateOnly.FromDateTime(endDate.Value);
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= end);
        }
        var courtIncome = await bookingsQuery.SumAsync(b => b.TotalPrice);

        // 2. Get Shop Order Revenue (Completed)
        var ordersQuery = context.Orders.Where(o => o.OrderStatus == 1).AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }
        var orders = await ordersQuery.ToListAsync();
        var shopIncome = orders.Sum(o => o.FinalAmount);

        // 3. Get Shop Stock Imports Cost (Expense, assume Transfer)
        var importsQuery = context.StockImports.AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            importsQuery = importsQuery.Where(si => si.ImportDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            importsQuery = importsQuery.Where(si => si.ImportDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }
        var shopExpense = await importsQuery.SumAsync(si => si.TotalAmount);

        // 4. Get Manual CashTransactions
        var transQuery = context.CashTransactions.AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            transQuery = transQuery.Where(t => t.TransactionDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            transQuery = transQuery.Where(t => t.TransactionDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }
        var trans = await transQuery.ToListAsync();

        var manualIncome = trans.Where(t => t.TransactionType == 1).Sum(t => t.Amount);
        var manualExpense = trans.Where(t => t.TransactionType == 0).Sum(t => t.Amount);

        // Calculate balances by Payment Method
        // Cash (Method = 0)
        var cashIn = orders.Where(o => o.PaymentMethod == 0).Sum(o => o.FinalAmount) +
                      trans.Where(t => t.TransactionType == 1 && t.PaymentMethod == 0).Sum(t => t.Amount);
        var cashOut = trans.Where(t => t.TransactionType == 0 && t.PaymentMethod == 0).Sum(t => t.Amount);

        // Bank Transfer (Method = 1)
        var bankIn = courtIncome + // assume all bookings are bank transfers
                     orders.Where(o => o.PaymentMethod == 1).Sum(o => o.FinalAmount) +
                     trans.Where(t => t.TransactionType == 1 && t.PaymentMethod == 1).Sum(t => t.Amount);
        var bankOut = shopExpense + // assume stock imports are paid by bank
                      trans.Where(t => t.TransactionType == 0 && t.PaymentMethod == 1).Sum(t => t.Amount);

        // E-Wallet (Method = 2)
        var walletIn = orders.Where(o => o.PaymentMethod == 2).Sum(o => o.FinalAmount) +
                       trans.Where(t => t.TransactionType == 1 && t.PaymentMethod == 2).Sum(t => t.Amount);
        var walletOut = trans.Where(t => t.TransactionType == 0 && t.PaymentMethod == 2).Sum(t => t.Amount);

        return new CashFlowSummaryDto
        {
            TotalRevenue = courtIncome + shopIncome + manualIncome,
            TotalExpenses = shopExpense + manualExpense,
            CashBalance = cashIn - cashOut,
            BankBalance = bankIn - bankOut,
            EWalletBalance = walletIn - walletOut
        };
    }

    public async Task<List<CategoryBreakdownDto>> GetCategoryReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _dbFactory.CreateDbContext();

        // 1. Get Court Booking Revenue (Cat 10)
        var bookingsQuery = context.Bookings.Where(b => b.Status == 2).AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            var start = DateOnly.FromDateTime(startDate.Value);
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate >= start);
        }
        if (endDate.HasValue)
        {
            var end = DateOnly.FromDateTime(endDate.Value);
            bookingsQuery = bookingsQuery.Where(b => b.BookingDate <= end);
        }
        var courtIncomeCount = await bookingsQuery.CountAsync();
        var courtIncomeAmount = await bookingsQuery.SumAsync(b => b.TotalPrice);

        // 2. Get Shop Order Revenue (Cat 11)
        var ordersQuery = context.Orders.Where(o => o.OrderStatus == 1).AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }
        var shopIncomeCount = await ordersQuery.CountAsync();
        var shopIncomeAmount = await ordersQuery.SumAsync(o => o.FinalAmount);

        // 3. Get Shop Stock Imports Cost (Cat 20)
        var importsQuery = context.StockImports.AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            importsQuery = importsQuery.Where(si => si.ImportDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            importsQuery = importsQuery.Where(si => si.ImportDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }
        var shopExpenseCount = await importsQuery.CountAsync();
        var shopExpenseAmount = await importsQuery.SumAsync(si => si.TotalAmount);

        // 4. Get Manual CashTransactions
        var transQuery = context.CashTransactions.AsNoTracking().AsQueryable();
        if (startDate.HasValue)
        {
            transQuery = transQuery.Where(t => t.TransactionDate >= startDate.Value.Date);
        }
        if (endDate.HasValue)
        {
            transQuery = transQuery.Where(t => t.TransactionDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }
        var trans = await transQuery.ToListAsync();

        // Prepare Category List
        var list = new List<CategoryBreakdownDto>
        {
            new CategoryBreakdownDto { CategoryId = 10, CategoryName = "Thu tiền đặt sân", TransactionCount = courtIncomeCount, TotalAmount = courtIncomeAmount },
            new CategoryBreakdownDto { CategoryId = 11, CategoryName = "Thu bán hàng shop", TransactionCount = shopIncomeCount, TotalAmount = shopIncomeAmount },
            new CategoryBreakdownDto { CategoryId = 12, CategoryName = "Thu khác", TransactionCount = trans.Count(t => t.TransactionType == 1 && t.Category == 12), TotalAmount = trans.Where(t => t.TransactionType == 1 && t.Category == 12).Sum(t => t.Amount) },
            new CategoryBreakdownDto { CategoryId = 20, CategoryName = "Chi nhập hàng", TransactionCount = shopExpenseCount, TotalAmount = shopExpenseAmount },
            new CategoryBreakdownDto { CategoryId = 21, CategoryName = "Chi điện nước", TransactionCount = trans.Count(t => t.TransactionType == 0 && t.Category == 21), TotalAmount = trans.Where(t => t.TransactionType == 0 && t.Category == 21).Sum(t => t.Amount) },
            new CategoryBreakdownDto { CategoryId = 22, CategoryName = "Chi sửa chữa/vận hành", TransactionCount = trans.Count(t => t.TransactionType == 0 && t.Category == 22), TotalAmount = trans.Where(t => t.TransactionType == 0 && t.Category == 22).Sum(t => t.Amount) },
            new CategoryBreakdownDto { CategoryId = 23, CategoryName = "Chi lương nhân viên", TransactionCount = trans.Count(t => t.TransactionType == 0 && t.Category == 23), TotalAmount = trans.Where(t => t.TransactionType == 0 && t.Category == 23).Sum(t => t.Amount) },
            new CategoryBreakdownDto { CategoryId = 24, CategoryName = "Chi khác", TransactionCount = trans.Count(t => t.TransactionType == 0 && t.Category == 24), TotalAmount = trans.Where(t => t.TransactionType == 0 && t.Category == 24).Sum(t => t.Amount) }
        };

        // Calculate percentages
        var totalRevenue = list.Where(c => c.CategoryId < 20).Sum(c => c.TotalAmount);
        var totalExpenses = list.Where(c => c.CategoryId >= 20).Sum(c => c.TotalAmount);

        foreach (var item in list)
        {
            if (item.CategoryId < 20)
            {
                item.Percentage = totalRevenue > 0 ? (double)(item.TotalAmount * 100 / totalRevenue) : 0;
            }
            else
            {
                item.Percentage = totalExpenses > 0 ? (double)(item.TotalAmount * 100 / totalExpenses) : 0;
            }
        }

        return list;
    }
    #endregion

    #region Reconciliation history (simulation)
    public Task<List<ReconciliationItemDto>> GetReconciliationHistoryAsync()
    {
        return Task.FromResult(_reconHistory.OrderByDescending(r => r.ReconciledAt).ToList());
    }

    public async Task<bool> ReconcileCashAsync(decimal actualCash)
    {
        var summary = await GetCashFlowSummaryAsync();
        var expectedCash = summary.CashBalance;

        var newItem = new ReconciliationItemDto
        {
            ReconciledAt = DateTime.Now,
            ExpectedCash = expectedCash,
            ActualCash = actualCash,
            Auditor = "Kế toán Admin"
        };

        _reconHistory.Add(newItem);
        return true;
    }
    #endregion
}
