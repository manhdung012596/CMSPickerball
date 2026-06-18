using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoccerPitchMvc.Services;

public class PromotionService : IPromotionService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;
    private readonly IMapper _mapper;

    public PromotionService(IDbContextFactory<ApplicationDbContext> dbFactory, IMapper mapper)
    {
        _dbFactory = dbFactory;
        _mapper = mapper;
    }

    public async Task<List<PromotionListDto>> GetPromotionsAsync(bool? isCoupon = null)
    {
        using var context = _dbFactory.CreateDbContext();
        var query = context.Promotions.AsQueryable();

        if (isCoupon.HasValue)
        {
            query = query.Where(p => p.IsCoupon == isCoupon.Value);
        }

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<PromotionListDto>>(list);
    }

    public async Task<PromotionListDto?> GetPromotionByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var promo = await context.Promotions.FirstOrDefaultAsync(p => p.Id == id);
        return promo == null ? null : _mapper.Map<PromotionListDto>(promo);
    }

    public async Task<PromotionListDto?> GetPromotionByCodeAsync(string code)
    {
        using var context = _dbFactory.CreateDbContext();
        var promo = await context.Promotions
            .FirstOrDefaultAsync(p => p.Code != null && p.Code.ToLower() == code.ToLower());
        return promo == null ? null : _mapper.Map<PromotionListDto>(promo);
    }

    public async Task<PromotionListDto> CreatePromotionAsync(CreatePromotionDto dto)
    {
        using var context = _dbFactory.CreateDbContext();

        if (dto.IsCoupon && !string.IsNullOrWhiteSpace(dto.Code))
        {
            bool codeExists = await context.Promotions
                .AnyAsync(p => p.Code != null && p.Code.ToLower() == dto.Code.ToLower());
            if (codeExists)
            {
                throw new InvalidOperationException("Mã giảm giá đã tồn tại trong hệ thống.");
            }
        }

        var promotion = _mapper.Map<Promotion>(dto);
        promotion.CreatedAt = DateTime.Now;
        if (promotion.IsCoupon && !string.IsNullOrWhiteSpace(promotion.Code))
        {
            promotion.Code = promotion.Code.Trim().ToUpper();
        }
        else
        {
            promotion.Code = null;
        }

        context.Promotions.Add(promotion);
        await context.SaveChangesAsync();

        return _mapper.Map<PromotionListDto>(promotion);
    }

    public async Task<PromotionListDto> UpdatePromotionAsync(UpdatePromotionDto dto)
    {
        using var context = _dbFactory.CreateDbContext();

        if (dto.IsCoupon && !string.IsNullOrWhiteSpace(dto.Code))
        {
            bool codeExists = await context.Promotions
                .AnyAsync(p => p.Code != null && p.Code.ToLower() == dto.Code.ToLower() && p.Id != dto.Id);
            if (codeExists)
            {
                throw new InvalidOperationException("Mã giảm giá đã được sử dụng ở chương trình khác.");
            }
        }

        var dbPromo = await context.Promotions.FindAsync(dto.Id);
        if (dbPromo == null)
        {
            throw new KeyNotFoundException("Không tìm thấy chương trình khuyến mại cần cập nhật.");
        }

        _mapper.Map(dto, dbPromo);
        if (dbPromo.IsCoupon && !string.IsNullOrWhiteSpace(dbPromo.Code))
        {
            dbPromo.Code = dbPromo.Code.Trim().ToUpper();
        }
        else
        {
            dbPromo.Code = null;
        }

        context.Promotions.Update(dbPromo);
        await context.SaveChangesAsync();

        return _mapper.Map<PromotionListDto>(dbPromo);
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbPromo = await context.Promotions.FindAsync(id);
        if (dbPromo == null) return false;

        context.Promotions.Remove(dbPromo);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool IsValid, string Message, decimal DiscountAmount)> ValidateCouponAsync(string code, decimal currentOrderAmount)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return (false, "Mã giảm giá không được để trống.", 0);
        }

        using var context = _dbFactory.CreateDbContext();
        var promo = await context.Promotions
            .FirstOrDefaultAsync(p => p.IsCoupon && p.Code != null && p.Code.ToLower() == code.ToLower());

        if (promo == null)
        {
            return (false, "Mã giảm giá không tồn tại.", 0);
        }

        if (promo.Status != 1)
        {
            return (false, "Mã giảm giá hiện không hoạt động.", 0);
        }

        var now = DateTime.Now;
        if (now < promo.StartDate)
        {
            return (false, $"Mã giảm giá chưa bắt đầu hiệu lực (Từ ngày {promo.StartDate:dd/MM/yyyy HH:mm}).", 0);
        }

        if (now > promo.EndDate)
        {
            return (false, "Mã giảm giá đã hết hạn sử dụng.", 0);
        }

        if (promo.UsageLimit.HasValue && promo.UsageCount >= promo.UsageLimit.Value)
        {
            return (false, "Mã giảm giá đã hết lượt sử dụng.", 0);
        }

        if (promo.MinOrderAmount.HasValue && currentOrderAmount < promo.MinOrderAmount.Value)
        {
            return (false, $"Mã giảm giá chỉ áp dụng cho đơn hàng từ {promo.MinOrderAmount.Value:N0}đ trở lên.", 0);
        }

        decimal discount = 0;
        if (promo.DiscountType == 0) // Tiền mặt
        {
            discount = promo.DiscountValue;
            if (discount > currentOrderAmount)
            {
                discount = currentOrderAmount;
            }
        }
        else if (promo.DiscountType == 1) // Phần trăm
        {
            discount = currentOrderAmount * (promo.DiscountValue / 100);
            if (promo.MaxDiscountAmount.HasValue && discount > promo.MaxDiscountAmount.Value)
            {
                discount = promo.MaxDiscountAmount.Value;
            }
        }

        return (true, "Áp dụng mã giảm giá thành công.", Math.Round(discount, 2));
    }
}
