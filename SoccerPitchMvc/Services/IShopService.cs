using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IShopService
{
    // Product CRUD
    Task<List<ProductListDto>> GetProductsAsync(string? search = null, int? categoryId = null, int? status = null);
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(CreateProductDto dto);
    Task<Product> UpdateProductAsync(UpdateProductDto dto);
    Task<bool> DeleteProductAsync(int id);
    Task<bool> CheckSKUExistsAsync(string sku, int? excludeId = null);

    // Order CRUD
    Task<List<OrderListDto>> GetOrdersAsync(string? search = null, int? paymentStatus = null, int? orderStatus = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<Order?> GetOrderByIdAsync(int id);
    Task<Order> CreateOrderAsync(CreateOrderDto dto);
    Task<bool> UpdateOrderStatusAsync(int orderId, int orderStatus, int paymentStatus);
    Task<bool> DeleteOrderAsync(int id);

    // Stock Import CRUD
    Task<List<StockImportListDto>> GetStockImportsAsync(string? search = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<StockImport?> GetStockImportByIdAsync(int id);
    Task<StockImport> CreateStockImportAsync(CreateStockImportDto dto);
    Task<bool> DeleteStockImportAsync(int id);

    // Statistics and Inventory
    Task<ShopStatsDto> GetShopStatsAsync();
    Task<List<InventoryWarningDto>> GetInventoryWarningsAsync();
}
