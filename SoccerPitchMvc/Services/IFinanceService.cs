using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IFinanceService
{
    // Cash Transaction CRUD
    Task<List<CashTransactionListDto>> GetTransactionsAsync(string? search = null, int? type = null, int? category = null, int? paymentMethod = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<CashTransaction?> GetTransactionByIdAsync(int id);
    Task<CashTransaction> CreateTransactionAsync(CreateCashTransactionDto dto);
    Task<CashTransaction> UpdateTransactionAsync(UpdateCashTransactionDto dto);
    Task<bool> DeleteTransactionAsync(int id);

    // Court Income Report
    Task<List<CourtIncomeDto>> GetCourtIncomeReportAsync(string? search = null, DateTime? startDate = null, DateTime? endDate = null);

    // Consolidated Reports
    Task<CashFlowSummaryDto> GetCashFlowSummaryAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CategoryBreakdownDto>> GetCategoryReportAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Reconciliation history (simulation)
    Task<List<ReconciliationItemDto>> GetReconciliationHistoryAsync();
    Task<bool> ReconcileCashAsync(decimal actualCash);
}
