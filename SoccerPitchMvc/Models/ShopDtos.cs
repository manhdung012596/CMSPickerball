using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoccerPitchMvc.Models;

#region Product DTOs
public class ProductListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitSymbol { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public string StatusText => Status == 1 ? "Đang bán" : "Ngừng bán";
    public string StatusBadge => Status == 1 ? "bg-success" : "bg-secondary";
    
    public string StockStatus => StockQuantity <= 0 ? "Hết hàng" : (StockQuantity < 10 ? "Sắp hết hàng" : "Còn hàng");
    public string StockStatusBadge => StockQuantity <= 0 ? "bg-danger" : (StockQuantity < 10 ? "bg-warning text-dark" : "bg-info text-dark");
}

public class CreateProductDto
{
    [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên sản phẩm tối đa 200 ký tự")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50, ErrorMessage = "SKU tối đa 50 ký tự")]
    public string? SKU { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục hợp lệ")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn đơn vị tính")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn đơn vị tính hợp lệ")]
    public int UnitId { get; set; }

    [Required(ErrorMessage = "Giá nhập không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn hoặc bằng 0")]
    public decimal PurchasePrice { get; set; }

    [Required(ErrorMessage = "Giá bán không được để trống")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn hoặc bằng 0")]
    public decimal SellingPrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho không được âm")]
    public int StockQuantity { get; set; } = 0;

    [MaxLength(500, ErrorMessage = "Đường dẫn ảnh tối đa 500 ký tự")]
    public string? ImageUrl { get; set; }

    [MaxLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
    public string? Description { get; set; }

    public int Status { get; set; } = 1;
}

public class UpdateProductDto : CreateProductDto
{
    [Required]
    public int Id { get; set; }
}
#endregion

#region Order DTOs
public class OrderListDto
{
    public int Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public int PaymentMethod { get; set; }
    public int PaymentStatus { get; set; }
    public int OrderStatus { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }

    public string PaymentMethodText => PaymentMethod switch
    {
        0 => "Tiền mặt",
        1 => "Chuyển khoản",
        2 => "Ví điện tử",
        _ => "Khác"
    };

    public string PaymentStatusText => PaymentStatus == 1 ? "Đã thanh toán" : "Chưa thanh toán";
    public string PaymentStatusBadge => PaymentStatus == 1 ? "bg-success" : "bg-warning text-dark";

    public string OrderStatusText => OrderStatus switch
    {
        0 => "Chờ xử lý",
        1 => "Hoàn thành",
        2 => "Đã hủy",
        _ => "Không xác định"
    };

    public string OrderStatusBadge => OrderStatus switch
    {
        0 => "bg-warning text-dark",
        1 => "bg-success",
        2 => "bg-danger",
        _ => "bg-secondary"
    };
}

public class CreateOrderDto
{
    public int? CustomerId { get; set; }

    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [Required]
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();

    public decimal DiscountAmount { get; set; } = 0;
    public string? CouponCode { get; set; }

    public int PaymentMethod { get; set; } = 0;
    public int PaymentStatus { get; set; } = 1; // Default paid for POS
    public int OrderStatus { get; set; } = 1; // Default completed for POS

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải tối thiểu là 1")]
    public int Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; }
}
#endregion

#region Stock Import DTOs
public class StockImportListDto
{
    public int Id { get; set; }
    public string ImportCode { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public DateTime ImportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public int ItemCount { get; set; }
}

public class CreateStockImportDto
{
    [MaxLength(150, ErrorMessage = "Tên nhà cung cấp tối đa 150 ký tự")]
    public string? SupplierName { get; set; }

    [Required(ErrorMessage = "Danh sách hàng nhập không được để trống")]
    public List<CreateStockImportItemDto> ImportItems { get; set; } = new();

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class CreateStockImportItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải tối thiểu là 1")]
    public int Quantity { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn hoặc bằng 0")]
    public decimal PurchasePrice { get; set; }
}
#endregion

#region Statistics DTOs
public class ShopStatsDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrdersCount { get; set; }
    public decimal TotalProfit { get; set; } // Revenue - PurchasePrice cost
    public int LowStockProductsCount { get; set; }
    public int OutOfStockProductsCount { get; set; }
}

public class InventoryWarningDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public int CurrentStock { get; set; }
    public string UnitName { get; set; } = string.Empty;
}
#endregion
