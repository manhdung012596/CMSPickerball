using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// Thực thể ghi nhận các giao dịch thu/chi tiền thủ công trong sổ quỹ (Chi phí vận hành, thu chi khác)
/// </summary>
public class CashTransaction
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string TransactionCode { get; set; } = string.Empty;

    /// <summary>1 = Thu (Receipt), 0 = Chi (Payment)</summary>
    [Required]
    public int TransactionType { get; set; }

    /// <summary>
    /// Phân loại giao dịch:
    /// Thu: 12 = Thu khác
    /// Chi: 21 = Chi điện nước, 22 = Chi sửa chữa/vận hành, 23 = Chi lương nhân viên, 24 = Chi khác
    /// </summary>
    [Required]
    public int Category { get; set; }

    [Required(ErrorMessage = "Số tiền không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày giao dịch")]
    public DateTime TransactionDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Vui lòng nhập tên người nộp/nhận")]
    [MaxLength(150, ErrorMessage = "Tối đa 150 ký tự")]
    public string PayerPayee { get; set; } = string.Empty;

    /// <summary>0 = Tiền mặt, 1 = Chuyển khoản, 2 = Ví điện tử</summary>
    [Required]
    public int PaymentMethod { get; set; } = 0;

    [MaxLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
