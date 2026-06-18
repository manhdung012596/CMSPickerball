using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public class MasterdataService : IMasterdataService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public MasterdataService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ===================== DEFAULT TIME SLOTS =====================

    public async Task<List<DefaultTimeSlotListDto>> GetAllTimeSlotsAsync()
    {
        var slots = await _context.DefaultTimeSlots
            .OrderBy(s => s.SortOrder).ThenBy(s => s.StartHour).ThenBy(s => s.StartMinute)
            .ToListAsync();
        return _mapper.Map<List<DefaultTimeSlotListDto>>(slots);
    }

    public async Task<UpdateDefaultTimeSlotDto?> GetTimeSlotForEditAsync(int id)
    {
        var slot = await _context.DefaultTimeSlots.FindAsync(id);
        return slot == null ? null : _mapper.Map<UpdateDefaultTimeSlotDto>(slot);
    }

    public async Task<(bool Success, string Message)> CreateTimeSlotAsync(CreateDefaultTimeSlotDto dto)
    {
        try
        {
            if (dto.StartHour > dto.EndHour || (dto.StartHour == dto.EndHour && dto.StartMinute >= dto.EndMinute))
                return (false, "Giờ kết thúc phải sau giờ bắt đầu!");

            var slot = _mapper.Map<DefaultTimeSlot>(dto);
            slot.CreatedAt = DateTime.Now;
            _context.DefaultTimeSlots.Add(slot);
            await _context.SaveChangesAsync();
            return (true, "Thêm khung giờ thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> UpdateTimeSlotAsync(UpdateDefaultTimeSlotDto dto)
    {
        try
        {
            var slot = await _context.DefaultTimeSlots.FindAsync(dto.Id);
            if (slot == null) return (false, "Khung giờ không tồn tại.");

            if (dto.StartHour > dto.EndHour || (dto.StartHour == dto.EndHour && dto.StartMinute >= dto.EndMinute))
                return (false, "Giờ kết thúc phải sau giờ bắt đầu!");

            _mapper.Map(dto, slot);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật khung giờ thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> DeleteTimeSlotAsync(int id)
    {
        try
        {
            var slot = await _context.DefaultTimeSlots.FindAsync(id);
            if (slot == null) return (false, "Khung giờ không tồn tại.");
            _context.DefaultTimeSlots.Remove(slot);
            await _context.SaveChangesAsync();
            return (true, "Xóa khung giờ thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    // ===================== SERVICE PRICES =====================

    public async Task<List<ServicePriceListDto>> GetAllServicePricesAsync(int? serviceType = null)
    {
        var query = _context.ServicePrices.AsQueryable();
        if (serviceType.HasValue) query = query.Where(s => s.ServiceType == serviceType);
        var prices = await query.OrderBy(s => s.SortOrder).ThenBy(s => s.ServiceType).ToListAsync();
        return _mapper.Map<List<ServicePriceListDto>>(prices);
    }

    public async Task<UpdateServicePriceDto?> GetServicePriceForEditAsync(int id)
    {
        var price = await _context.ServicePrices.FindAsync(id);
        return price == null ? null : _mapper.Map<UpdateServicePriceDto>(price);
    }

    public async Task<(bool Success, string Message)> CreateServicePriceAsync(CreateServicePriceDto dto)
    {
        try
        {
            var item = _mapper.Map<ServicePrice>(dto);
            _context.ServicePrices.Add(item);
            await _context.SaveChangesAsync();
            return (true, "Thêm giá dịch vụ thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> UpdateServicePriceAsync(UpdateServicePriceDto dto)
    {
        try
        {
            var item = await _context.ServicePrices.FindAsync(dto.Id);
            if (item == null) return (false, "Giá dịch vụ không tồn tại.");
            _mapper.Map(dto, item);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật giá dịch vụ thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> DeleteServicePriceAsync(int id)
    {
        try
        {
            var item = await _context.ServicePrices.FindAsync(id);
            if (item == null) return (false, "Giá dịch vụ không tồn tại.");
            _context.ServicePrices.Remove(item);
            await _context.SaveChangesAsync();
            return (true, "Xóa giá dịch vụ thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    // ===================== PRODUCT CATEGORIES =====================

    public async Task<List<ProductCategoryListDto>> GetAllProductCategoriesAsync()
    {
        var cats = await _context.ProductCategories
            .Include(c => c.Parent)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .ToListAsync();
        return _mapper.Map<List<ProductCategoryListDto>>(cats);
    }

    public async Task<List<ProductCategory>> GetProductCategorySelectListAsync()
    {
        return await _context.ProductCategories
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<UpdateProductCategoryDto?> GetProductCategoryForEditAsync(int id)
    {
        var cat = await _context.ProductCategories.FindAsync(id);
        return cat == null ? null : _mapper.Map<UpdateProductCategoryDto>(cat);
    }

    public async Task<(bool Success, string Message)> CreateProductCategoryAsync(CreateProductCategoryDto dto)
    {
        try
        {
            var exists = await _context.ProductCategories.AnyAsync(c => c.Name == dto.Name);
            if (exists) return (false, "Tên danh mục này đã tồn tại!");

            var cat = _mapper.Map<ProductCategory>(dto);
            _context.ProductCategories.Add(cat);
            await _context.SaveChangesAsync();
            return (true, "Thêm danh mục sản phẩm thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> UpdateProductCategoryAsync(UpdateProductCategoryDto dto)
    {
        try
        {
            var cat = await _context.ProductCategories.FindAsync(dto.Id);
            if (cat == null) return (false, "Danh mục không tồn tại.");

            var exists = await _context.ProductCategories.AnyAsync(c => c.Name == dto.Name && c.Id != dto.Id);
            if (exists) return (false, "Tên danh mục này đã được sử dụng!");

            _mapper.Map(dto, cat);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật danh mục thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> DeleteProductCategoryAsync(int id)
    {
        try
        {
            var cat = await _context.ProductCategories.FindAsync(id);
            if (cat == null) return (false, "Danh mục không tồn tại.");

            var hasChildren = await _context.ProductCategories.AnyAsync(c => c.ParentId == id);
            if (hasChildren) return (false, "Không thể xóa! Danh mục này có danh mục con.");

            _context.ProductCategories.Remove(cat);
            await _context.SaveChangesAsync();
            return (true, "Xóa danh mục thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    // ===================== UNITS =====================

    public async Task<List<UnitListDto>> GetAllUnitsAsync()
    {
        var units = await _context.Units
            .OrderBy(u => u.SortOrder).ThenBy(u => u.Name)
            .ToListAsync();
        return _mapper.Map<List<UnitListDto>>(units);
    }

    public async Task<UpdateUnitDto?> GetUnitForEditAsync(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        return unit == null ? null : _mapper.Map<UpdateUnitDto>(unit);
    }

    public async Task<(bool Success, string Message)> CreateUnitAsync(CreateUnitDto dto)
    {
        try
        {
            var exists = await _context.Units.AnyAsync(u => u.Name == dto.Name);
            if (exists) return (false, "Đơn vị tính này đã tồn tại!");

            var unit = _mapper.Map<Unit>(dto);
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return (true, "Thêm đơn vị tính thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> UpdateUnitAsync(UpdateUnitDto dto)
    {
        try
        {
            var unit = await _context.Units.FindAsync(dto.Id);
            if (unit == null) return (false, "Đơn vị tính không tồn tại.");

            var exists = await _context.Units.AnyAsync(u => u.Name == dto.Name && u.Id != dto.Id);
            if (exists) return (false, "Tên đơn vị tính này đã được sử dụng!");

            _mapper.Map(dto, unit);
            await _context.SaveChangesAsync();
            return (true, "Cập nhật đơn vị tính thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }

    public async Task<(bool Success, string Message)> DeleteUnitAsync(int id)
    {
        try
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null) return (false, "Đơn vị tính không tồn tại.");
            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return (true, "Xóa đơn vị tính thành công!");
        }
        catch (Exception ex) { return (false, $"Lỗi: {ex.Message}"); }
    }
}
