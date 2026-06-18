using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IPromotionService
{
    Task<List<PromotionListDto>> GetPromotionsAsync(bool? isCoupon = null);
    Task<PromotionListDto?> GetPromotionByIdAsync(int id);
    Task<PromotionListDto?> GetPromotionByCodeAsync(string code);
    Task<PromotionListDto> CreatePromotionAsync(CreatePromotionDto dto);
    Task<PromotionListDto> UpdatePromotionAsync(UpdatePromotionDto dto);
    Task<bool> DeletePromotionAsync(int id);
    Task<(bool IsValid, string Message, decimal DiscountAmount)> ValidateCouponAsync(string code, decimal currentOrderAmount);
}
