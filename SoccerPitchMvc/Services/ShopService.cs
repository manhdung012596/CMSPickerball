using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public class ShopService : IShopService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IMapper _mapper;

    public ShopService(IDbContextFactory<ApplicationDbContext> dbFactory, IMapper mapper)
    {
        _dbFactory = dbFactory;
        _mapper = mapper;
    }

    #region Product CRUD
    public async Task<List<ProductListDto>> GetProductsAsync(string? search = null, int? categoryId = null, int? status = null)
    {
        using var context = _dbFactory.CreateDbContext();
        var query = context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchLower) || (p.SKU != null && p.SKU.ToLower().Contains(searchLower)));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        var products = await query.OrderByDescending(p => p.Id).ToListAsync();
        return _mapper.Map<List<ProductListDto>>(products);
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        using var context = _dbFactory.CreateDbContext();

        if (!string.IsNullOrWhiteSpace(dto.SKU))
        {
            dto.SKU = dto.SKU.Trim().ToUpper();
            var skuExists = await context.Products.AnyAsync(p => p.SKU == dto.SKU);
            if (skuExists)
            {
                throw new InvalidOperationException($"Mã SKU '{dto.SKU}' đã tồn tại trong hệ thống.");
            }
        }

        var product = _mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.Now;

        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(UpdateProductDto dto)
    {
        using var context = _dbFactory.CreateDbContext();
        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == dto.Id);
        if (product == null)
        {
            throw new KeyNotFoundException("Không tìm thấy sản phẩm cần cập nhật.");
        }

        if (!string.IsNullOrWhiteSpace(dto.SKU))
        {
            dto.SKU = dto.SKU.Trim().ToUpper();
            var skuExists = await context.Products.AnyAsync(p => p.SKU == dto.SKU && p.Id != dto.Id);
            if (skuExists)
            {
                throw new InvalidOperationException($"Mã SKU '{dto.SKU}' đã tồn tại trong hệ thống.");
            }
        }

        _mapper.Map(dto, product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var product = await context.Products.FindAsync(id);
        if (product == null) return false;

        // Check if product is in any order item or stock import item
        var isUsedInOrders = await context.OrderItems.AnyAsync(oi => oi.ProductId == id);
        var isUsedInImports = await context.StockImportItems.AnyAsync(si => si.ProductId == id);

        if (isUsedInOrders || isUsedInImports)
        {
            // Soft delete: Ngừng bán thay vì xóa hoàn toàn để giữ toàn vẹn dữ liệu báo cáo
            product.Status = 0;
            await context.SaveChangesAsync();
            return true;
        }

        context.Products.Remove(product);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CheckSKUExistsAsync(string sku, int? excludeId = null)
    {
        using var context = _dbFactory.CreateDbContext();
        var normalizedSku = sku.Trim().ToUpper();
        if (excludeId.HasValue)
        {
            return await context.Products.AnyAsync(p => p.SKU == normalizedSku && p.Id != excludeId.Value);
        }
        return await context.Products.AnyAsync(p => p.SKU == normalizedSku);
    }
    #endregion

    #region Order CRUD
    public async Task<List<OrderListDto>> GetOrdersAsync(string? search = null, int? paymentStatus = null, int? orderStatus = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _dbFactory.CreateDbContext();
        var query = context.Orders
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(o => o.OrderCode.ToLower().Contains(searchLower) || 
                                     (o.CustomerName != null && o.CustomerName.ToLower().Contains(searchLower)) ||
                                     (o.CustomerPhone != null && o.CustomerPhone.Contains(searchLower)));
        }

        if (paymentStatus.HasValue)
        {
            query = query.Where(o => o.PaymentStatus == paymentStatus.Value);
        }

        if (orderStatus.HasValue)
        {
            query = query.Where(o => o.OrderStatus == orderStatus.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(o => o.OrderDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        return _mapper.Map<List<OrderListDto>>(orders);
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        using var context = _dbFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Generate Code: DH-yyyyMMdd-XXXX
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            var countToday = await context.Orders.CountAsync(o => o.OrderCode.StartsWith($"DH-{dateStr}"));
            var orderCode = $"DH-{dateStr}-{(countToday + 1):D4}";

            // Map and calculate
            var order = new Order
            {
                OrderCode = orderCode,
                CustomerId = dto.CustomerId,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                OrderDate = DateTime.Now,
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = dto.PaymentStatus,
                OrderStatus = dto.OrderStatus,
                Notes = dto.Notes,
                DiscountAmount = dto.DiscountAmount,
                CreatedAt = DateTime.Now
            };

            decimal totalAmount = 0;

            foreach (var itemDto in dto.OrderItems)
            {
                var product = await context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID '{itemDto.ProductId}'.");
                }

                // Check stock if order is active (completed/pending)
                if (order.OrderStatus != 2) // Not cancelled
                {
                    if (product.StockQuantity < itemDto.Quantity)
                    {
                        throw new InvalidOperationException($"Sản phẩm '{product.Name}' không đủ tồn kho. Tồn thực tế: {product.StockQuantity}. Yêu cầu: {itemDto.Quantity}.");
                    }
                    // Deduct stock
                    product.StockQuantity -= itemDto.Quantity;
                }

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    Price = itemDto.Price,
                    TotalPrice = itemDto.Quantity * itemDto.Price
                };

                totalAmount += orderItem.TotalPrice;
                order.OrderItems.Add(orderItem);
            }

            order.TotalAmount = totalAmount;
            order.FinalAmount = totalAmount - order.DiscountAmount;
            if (order.FinalAmount < 0) order.FinalAmount = 0;

            context.Orders.Add(order);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateOrderStatusAsync(int orderId, int orderStatus, int paymentStatus)
    {
        using var context = _dbFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var order = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return false;

            int oldStatus = order.OrderStatus;
            int newStatus = orderStatus;

            // Handle stock transition
            if (oldStatus != 2 && newStatus == 2)
            {
                // Active -> Cancelled: Restore stock
                foreach (var item in order.OrderItems)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }
            else if (oldStatus == 2 && newStatus != 2)
            {
                // Cancelled -> Active: Deduct stock
                foreach (var item in order.OrderItems)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        if (product.StockQuantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"Không thể phục hồi đơn hàng. Sản phẩm '{product.Name}' không đủ tồn kho.");
                        }
                        product.StockQuantity -= item.Quantity;
                    }
                }
            }

            order.OrderStatus = orderStatus;
            order.PaymentStatus = paymentStatus;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var order = await context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return false;

            // If deleting an active order, restore stock first
            if (order.OrderStatus != 2)
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }

            context.Orders.Remove(order);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    #endregion

    #region Stock Import CRUD
    public async Task<List<StockImportListDto>> GetStockImportsAsync(string? search = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _dbFactory.CreateDbContext();
        var query = context.StockImports
            .Include(si => si.ImportItems)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(si => si.ImportCode.ToLower().Contains(searchLower) || 
                                      (si.SupplierName != null && si.SupplierName.ToLower().Contains(searchLower)));
        }

        if (startDate.HasValue)
        {
            query = query.Where(si => si.ImportDate >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(si => si.ImportDate <= endDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        var imports = await query.OrderByDescending(si => si.ImportDate).ToListAsync();
        return _mapper.Map<List<StockImportListDto>>(imports);
    }

    public async Task<StockImport?> GetStockImportByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.StockImports
            .Include(si => si.ImportItems)
            .FirstOrDefaultAsync(si => si.Id == id);
    }

    public async Task<StockImport> CreateStockImportAsync(CreateStockImportDto dto)
    {
        using var context = _dbFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Generate Code: NH-yyyyMMdd-XXXX
            var dateStr = DateTime.Now.ToString("yyyyMMdd");
            var countToday = await context.StockImports.CountAsync(si => si.ImportCode.StartsWith($"NH-{dateStr}"));
            var importCode = $"NH-{dateStr}-{(countToday + 1):D4}";

            var stockImport = new StockImport
            {
                ImportCode = importCode,
                SupplierName = dto.SupplierName,
                ImportDate = DateTime.Now,
                Notes = dto.Notes,
                CreatedAt = DateTime.Now
            };

            decimal totalAmount = 0;

            foreach (var itemDto in dto.ImportItems)
            {
                var product = await context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new KeyNotFoundException($"Không tìm thấy sản phẩm ID '{itemDto.ProductId}'.");
                }

                // Add to stock
                product.StockQuantity += itemDto.Quantity;

                var importItem = new StockImportItem
                {
                    ProductId = itemDto.ProductId,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    PurchasePrice = itemDto.PurchasePrice,
                    TotalPrice = itemDto.Quantity * itemDto.PurchasePrice
                };

                totalAmount += importItem.TotalPrice;
                stockImport.ImportItems.Add(importItem);
            }

            stockImport.TotalAmount = totalAmount;

            context.StockImports.Add(stockImport);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return stockImport;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteStockImportAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var stockImport = await context.StockImports.Include(si => si.ImportItems).FirstOrDefaultAsync(si => si.Id == id);
            if (stockImport == null) return false;

            // Reduce stock
            foreach (var item in stockImport.ImportItems)
            {
                var product = await context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Không thể hủy phiếu nhập. Số lượng tồn kho của sản phẩm '{product.Name}' hiện tại là {product.StockQuantity}, nhỏ hơn số lượng cần trừ đi ({item.Quantity}).");
                    }
                    product.StockQuantity -= item.Quantity;
                }
            }

            context.StockImports.Remove(stockImport);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    #endregion

    #region Stats and Inventory
    public async Task<ShopStatsDto> GetShopStatsAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        
        // Active orders only
        var activeOrders = await context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.OrderStatus == 1) // Hoàn thành
            .ToListAsync();

        var totalRevenue = activeOrders.Sum(o => o.FinalAmount);
        var totalOrdersCount = activeOrders.Count;

        // Calculate Cost Of Goods Sold (COGS) to find profit
        decimal totalCost = 0;
        foreach (var order in activeOrders)
        {
            foreach (var item in order.OrderItems)
            {
                var product = await context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    totalCost += item.Quantity * product.PurchasePrice;
                }
                else
                {
                    // Fallback using item price as cost (0 profit margin)
                    totalCost += item.Quantity * item.Price;
                }
            }
        }

        var totalProfit = totalRevenue - totalCost;

        // Inventory metrics
        var activeProducts = await context.Products.Where(p => p.Status == 1).ToListAsync();
        var lowStock = activeProducts.Count(p => p.StockQuantity > 0 && p.StockQuantity < 10);
        var outOfStock = activeProducts.Count(p => p.StockQuantity <= 0);

        return new ShopStatsDto
        {
            TotalRevenue = totalRevenue,
            TotalOrdersCount = totalOrdersCount,
            TotalProfit = totalProfit,
            LowStockProductsCount = lowStock,
            OutOfStockProductsCount = outOfStock
        };
    }

    public async Task<List<InventoryWarningDto>> GetInventoryWarningsAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Products
            .Include(p => p.Unit)
            .Where(p => p.Status == 1 && p.StockQuantity < 10)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new InventoryWarningDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                SKU = p.SKU,
                CurrentStock = p.StockQuantity,
                UnitName = p.Unit != null ? p.Unit.Name : "Cái"
            })
            .ToListAsync();
    }
    #endregion
}
