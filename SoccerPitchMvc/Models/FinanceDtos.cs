using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

#region Cash Transaction DTOs
public class CashTransactionListDto
{
    public int Id { get; set; }
    public string TransactionCode { get; set; } = string.Empty;
    public int TransactionType { get; set; } // 1 = Thu, 0 = Chi
    public int Category { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string PayerPayee { get; set; } = string.Empty;
    public int PaymentMethod { get; set; } // 0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public string TypeLabel => TransactionType == 1 ? "Thu" : "Chi";
    public string TypeBadge => TransactionType == 1 ? "bg-success" : "bg-danger";

    public string CategoryLabel => Category switch
    {
        10 => "Thu tiền đặt sân",
        11 => "Thu bán hàng shop",
        12 => "Thu khác",
        20 => "Chi nhập hàng",
        21 => "Chi điện nước",
        22 => "Chi sửa chữa/vận hành",
        23 => "Chi lương nhân viên",
        24 => "Chi khác",
        _ => "Khác"
    };

    public string CategoryIcon => Category switch
    {
        10 => "fa-solid fa-table-tennis-paddle-ball text-success",
        11 => "fa-solid fa-cart-shopping text-info",
        12 => "fa-solid fa-arrow-down-long text-primary",
        20 => "fa-solid fa-truck-loading text-warning",
        21 => "fa-solid fa-lightbulb text-warning",
        22 => "fa-solid fa-screwdriver-wrench text-secondary",
        23 => "fa-solid fa-user-tie text-primary",
        24 => "fa-solid fa-arrow-up-long text-danger",
        _ => "fa-solid fa-circle-question"
    };

    public string MethodLabel => PaymentMethod switch
    {
        0 => "Tiền mặt",
        1 => "Chuyển khoản",
        2 => "Ví điện tử",
        _ => "Khác"
    };

    public string MethodBadge => PaymentMethod switch
    {
        0 => "bg-light text-dark border",
        1 => "bg-primary text-white",
        2 => "bg-info text-dark",
        _ => "bg-secondary text-white"
    };
}

public class CreateCashTransactionDto
{
    [Required(ErrorMessage = "Loại giao dịch bắt buộc")]
    public int TransactionType { get; set; } = 0; // Default Chi

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Range(10, 24, ErrorMessage = "Vui lòng chọn danh mục hợp lệ")]
    public int Category { get; set; }

    [Required(ErrorMessage = "Số tiền không được để trống")]
    [Range(1000, double.MaxValue, ErrorMessage = "Số tiền tối thiểu 1.000đ")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày giao dịch")]
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Vui lòng nhập tên người nộp/nhận")]
    [MaxLength(150, ErrorMessage = "Tối đa 150 ký tự")]
    public string PayerPayee { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hình thức thanh toán bắt buộc")]
    public int PaymentMethod { get; set; } = 0; // Default Tiền mặt

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Notes { get; set; }
}

public class UpdateCashTransactionDto : CreateCashTransactionDto
{
    [Required]
    public int Id { get; set; }
}
#endregion

#region Income Ledger DTOs
public class CourtIncomeDto
{
    public int BookingId { get; set; }
    public string PitchName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}
#endregion

#region Financial Summary DTOs
public class CashFlowSummaryDto
{
    public decimal TotalRevenue { get; set; }    // Total Thu
    public decimal TotalExpenses { get; set; }   // Total Chi
    public decimal NetIncome => TotalRevenue - TotalExpenses; // Thu nhập ròng

    // Balance by payment method
    public decimal CashBalance { get; set; }
    public decimal BankBalance { get; set; }
    public decimal EWalletBalance { get; set; }
    public decimal TotalBalance => CashBalance + BankBalance + EWalletBalance;
}

public class CategoryBreakdownDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TransactionCount { get; set; }
    public decimal TotalAmount { get; set; }
    public double Percentage { get; set; }
}

public class ReconciliationItemDto
{
    public DateTime ReconciledAt { get; set; } = DateTime.Now;
    public decimal ExpectedCash { get; set; }
    public decimal ActualCash { get; set; }
    public decimal Difference => ActualCash - ExpectedCash;
    public string Auditor { get; set; } = "Kế toán trưởng";
    public string Status => Difference == 0 ? "Khớp quỹ" : (Difference > 0 ? "Thừa quỹ" : "Thiếu quỹ");
    public string StatusBadge => Difference == 0 ? "bg-success" : (Difference > 0 ? "bg-warning text-dark" : "bg-danger");
}
#endregion
