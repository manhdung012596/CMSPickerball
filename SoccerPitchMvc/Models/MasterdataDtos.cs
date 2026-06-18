using System;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

// ===== DEFAULT TIME SLOT DTOs =====

public class DefaultTimeSlotListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int EndHour { get; set; }
    public int EndMinute { get; set; }
    public int DayType { get; set; }
    public decimal? PriceOverride { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }

    public string TimeDisplay => $"{StartHour:D2}:{StartMinute:D2} - {EndHour:D2}:{EndMinute:D2}";
    public string DayTypeText => DayType switch { 0 => "Tất cả ngày", 1 => "Ngày thường", 2 => "Cuối tuần", 3 => "Ngày lễ", _ => "—" };
    public string DayTypeBadge => DayType switch { 0 => "primary", 1 => "info", 2 => "warning", 3 => "danger", _ => "secondary" };
    public string StatusText => Status == 1 ? "Hoạt động" : "Ẩn";
    public string StatusBadge => Status == 1 ? "success" : "secondary";
    public string PriceDisplay => PriceOverride.HasValue ? PriceOverride.Value.ToString("N0") + " VNĐ" : "Giá mặc định";
}

public class CreateDefaultTimeSlotDto
{
    [Required(ErrorMessage = "Tên khung giờ không được để trống")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 23, ErrorMessage = "Giờ phải từ 0-23")]
    public int StartHour { get; set; } = 6;
    public int StartMinute { get; set; } = 0;

    [Range(0, 23, ErrorMessage = "Giờ phải từ 0-23")]
    public int EndHour { get; set; } = 7;
    public int EndMinute { get; set; } = 0;

    public int DayType { get; set; } = 0;
    public decimal? PriceOverride { get; set; }
    public int Status { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
}

public class UpdateDefaultTimeSlotDto : CreateDefaultTimeSlotDto
{
    public int Id { get; set; }
}

// ===== SERVICE PRICE DTOs =====

public class ServicePriceListDto
{
    public int Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ServiceType { get; set; }
    public decimal Price { get; set; }
    public string? PriceUnit { get; set; }
    public string? Note { get; set; }
    public int Status { get; set; }
    public int SortOrder { get; set; }

    public string ServiceTypeText => ServiceType switch { 1 => "Thuê vợt", 2 => "Thuê bóng", 3 => "Huấn luyện", 4 => "Tổ chức giải", 5 => "Khác", _ => "—" };
    public string ServiceTypeBadge => ServiceType switch { 1 => "primary", 2 => "info", 3 => "success", 4 => "warning", 5 => "secondary", _ => "light" };
    public string StatusText => Status == 1 ? "Hoạt động" : "Ẩn";
    public string StatusBadge => Status == 1 ? "success" : "secondary";
    public string PriceDisplay => Price.ToString("N0") + " VNĐ/" + (PriceUnit ?? "lần");
}

public class CreateServicePriceDto
{
    [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
    [MaxLength(150)]
    public string ServiceName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int ServiceType { get; set; } = 5;

    [Required(ErrorMessage = "Giá không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải >= 0")]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string? PriceUnit { get; set; } = "lần";

    [MaxLength(200)]
    public string? Note { get; set; }

    public int Status { get; set; } = 1;
    public int SortOrder { get; set; } = 0;
}

public class UpdateServicePriceDto : CreateServicePriceDto
{
    public int Id { get; set; }
}

// ===== PRODUCT CATEGORY DTOs =====

public class ProductCategoryListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconClass { get; set; }
    public string? ParentName { get; set; }
    public int SortOrder { get; set; }
    public int Status { get; set; }

    public string StatusText => Status == 1 ? "Hoạt động" : "Ẩn";
    public string StatusBadge => Status == 1 ? "success" : "secondary";
}

public class CreateProductCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? IconClass { get; set; }

    public int? ParentId { get; set; }
    public int SortOrder { get; set; } = 0;
    public int Status { get; set; } = 1;
}

public class UpdateProductCategoryDto : CreateProductCategoryDto
{
    public int Id { get; set; }
}

// ===== UNIT DTOs =====

public class UnitListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int Status { get; set; }

    public string StatusText => Status == 1 ? "Hoạt động" : "Ẩn";
    public string StatusBadge => Status == 1 ? "success" : "secondary";
    public string DisplayName => string.IsNullOrEmpty(Symbol) ? Name : $"{Name} ({Symbol})";
}

public class CreateUnitDto
{
    [Required(ErrorMessage = "Tên đơn vị không được để trống")]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Symbol { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;
    public int Status { get; set; } = 1;
}

public class UpdateUnitDto : CreateUnitDto
{
    public int Id { get; set; }
}
