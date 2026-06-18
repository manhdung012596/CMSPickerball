using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

/// <summary>
/// DTO hiển thị danh sách Khuyến mại/Coupon.
/// </summary>
public class PromotionListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public int DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; }
    public int Status { get; set; }
    public bool IsCoupon { get; set; }
    public DateTime CreatedAt { get; set; }

    // Helper computed properties
    public string DiscountDisplay => DiscountType == 0
        ? $"-{DiscountValue:N0}đ"
        : $"-{DiscountValue}%";

    public string TypeLabel => IsCoupon ? "Coupon" : "Ưu đãi";
    public string TypeBadge => IsCoupon ? "info" : "warning";

    public bool IsExpired => DateTime.Now > EndDate;
    public bool IsScheduled => DateTime.Now < StartDate;
    public bool IsLimitReached => UsageLimit.HasValue && UsageCount >= UsageLimit.Value;
    public bool IsActive => Status == 1 && !IsExpired && !IsScheduled && !IsLimitReached;

    public string ComputedStatusText
    {
        get
        {
            if (Status == 0) return "Bản nháp";
            if (Status == 2) return "Tạm dừng";
            if (IsExpired) return "Hết hạn";
            if (IsLimitReached) return "Hết lượt";
            if (IsScheduled) return "Chờ hiệu lực";
            return "Hoạt động";
        }
    }

    public string ComputedStatusBadge
    {
        get
        {
            if (Status == 0) return "warning text-dark";
            if (Status == 2) return "danger";
            if (IsExpired || IsLimitReached) return "secondary";
            if (IsScheduled) return "info";
            return "success";
        }
    }

    public double UsagePercent => UsageLimit.HasValue && UsageLimit.Value > 0
        ? (double)UsageCount / UsageLimit.Value * 100
        : 0;
}

/// <summary>
/// DTO khi tạo mới khuyến mại.
/// </summary>
public class CreatePromotionDto : IValidatableObject
{
    [Required(ErrorMessage = "Tên khuyến mại là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên khuyến mại không được vượt quá 200 ký tự.")]
    public string Name { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự.")]
    public string? Code { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Loại giảm giá là bắt buộc.")]
    public int DiscountType { get; set; }

    [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc.")]
    [Range(0.01, 1000000000, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0.")]
    public decimal DiscountValue { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Đơn hàng tối thiểu không được âm.")]
    public decimal? MinOrderAmount { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Giảm tối đa không được âm.")]
    public decimal? MaxDiscountAmount { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

    [Range(1, 1000000, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0.")]
    public int? UsageLimit { get; set; }

    public int Status { get; set; } = 1;

    public bool IsCoupon { get; set; }

    public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate < StartDate)
        {
            yield return new ValidationResult("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.", new[] { nameof(EndDate) });
        }

        if (IsCoupon && string.IsNullOrWhiteSpace(Code))
        {
            yield return new ValidationResult("Mã giảm giá là bắt buộc khi chọn hình thức Coupon.", new[] { nameof(Code) });
        }

        if (DiscountType == 1 && DiscountValue > 100)
        {
            yield return new ValidationResult("Giảm giá theo phần trăm không được vượt quá 100%.", new[] { nameof(DiscountValue) });
        }
    }
}

/// <summary>
/// DTO khi cập nhật khuyến mại.
/// </summary>
public class UpdatePromotionDto : IValidatableObject
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên khuyến mại là bắt buộc.")]
    [StringLength(200, ErrorMessage = "Tên khuyến mại không được vượt quá 200 ký tự.")]
    public string Name { get; set; } = null!;

    [StringLength(50, ErrorMessage = "Mã giảm giá không được vượt quá 50 ký tự.")]
    public string? Code { get; set; }

    [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Loại giảm giá là bắt buộc.")]
    public int DiscountType { get; set; }

    [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc.")]
    [Range(0.01, 1000000000, ErrorMessage = "Giá trị giảm giá phải lớn hơn 0.")]
    public decimal DiscountValue { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Đơn hàng tối thiểu không được âm.")]
    public decimal? MinOrderAmount { get; set; }

    [Range(0, 1000000000, ErrorMessage = "Giảm tối đa không được âm.")]
    public decimal? MaxDiscountAmount { get; set; }

    [Required(ErrorMessage = "Ngày bắt đầu là bắt buộc.")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Ngày kết thúc là bắt buộc.")]
    public DateTime EndDate { get; set; }

    [Range(1, 1000000, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0.")]
    public int? UsageLimit { get; set; }

    public int Status { get; set; }

    public bool IsCoupon { get; set; }

    public System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate < StartDate)
        {
            yield return new ValidationResult("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.", new[] { nameof(EndDate) });
        }

        if (IsCoupon && string.IsNullOrWhiteSpace(Code))
        {
            yield return new ValidationResult("Mã giảm giá là bắt buộc khi chọn hình thức Coupon.", new[] { nameof(Code) });
        }

        if (DiscountType == 1 && DiscountValue > 100)
        {
            yield return new ValidationResult("Giảm giá theo phần trăm không được vượt quá 100%.", new[] { nameof(DiscountValue) });
        }
    }
}
