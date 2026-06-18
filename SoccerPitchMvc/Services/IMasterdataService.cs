using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IMasterdataService
{
    // === DEFAULT TIME SLOTS ===
    Task<List<DefaultTimeSlotListDto>> GetAllTimeSlotsAsync();
    Task<UpdateDefaultTimeSlotDto?> GetTimeSlotForEditAsync(int id);
    Task<(bool Success, string Message)> CreateTimeSlotAsync(CreateDefaultTimeSlotDto dto);
    Task<(bool Success, string Message)> UpdateTimeSlotAsync(UpdateDefaultTimeSlotDto dto);
    Task<(bool Success, string Message)> DeleteTimeSlotAsync(int id);

    // === SERVICE PRICES ===
    Task<List<ServicePriceListDto>> GetAllServicePricesAsync(int? serviceType = null);
    Task<UpdateServicePriceDto?> GetServicePriceForEditAsync(int id);
    Task<(bool Success, string Message)> CreateServicePriceAsync(CreateServicePriceDto dto);
    Task<(bool Success, string Message)> UpdateServicePriceAsync(UpdateServicePriceDto dto);
    Task<(bool Success, string Message)> DeleteServicePriceAsync(int id);

    // === PRODUCT CATEGORIES ===
    Task<List<ProductCategoryListDto>> GetAllProductCategoriesAsync();
    Task<List<ProductCategory>> GetProductCategorySelectListAsync();
    Task<UpdateProductCategoryDto?> GetProductCategoryForEditAsync(int id);
    Task<(bool Success, string Message)> CreateProductCategoryAsync(CreateProductCategoryDto dto);
    Task<(bool Success, string Message)> UpdateProductCategoryAsync(UpdateProductCategoryDto dto);
    Task<(bool Success, string Message)> DeleteProductCategoryAsync(int id);

    // === UNITS ===
    Task<List<UnitListDto>> GetAllUnitsAsync();
    Task<UpdateUnitDto?> GetUnitForEditAsync(int id);
    Task<(bool Success, string Message)> CreateUnitAsync(CreateUnitDto dto);
    Task<(bool Success, string Message)> UpdateUnitAsync(UpdateUnitDto dto);
    Task<(bool Success, string Message)> DeleteUnitAsync(int id);
}
