using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý Giá Dịch Vụ (ServicePrices).
/// Dùng chung cho tất cả loại dịch vụ: Thuê vợt, Thuê bóng, Huấn luyện, Tổ chức giải, v.v.
/// Filter theo ServiceType thông qua query param.
/// </summary>
[Authorize(Roles = "Admin,Staff")]
public class ServicePricesController : Controller
{
    private readonly ApplicationDbContext _context;

    // Mapping ServiceType -> tên hiển thị
    private static readonly Dictionary<int, string> ServiceTypeNames = new()
    {
        { 0, "Dịch vụ thuê sân" },
        { 1, "Cho thuê vợt" },
        { 2, "Cho thuê bóng" },
        { 3, "Dịch vụ huấn luyện" },
        { 4, "Tổ chức giải đấu" },
        { 5, "Dịch vụ khác" }
    };

    public ServicePricesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /ServicePrices?serviceType=1
    /// Hiển thị danh sách dịch vụ, lọc theo loại (serviceType).
    /// Nếu không có serviceType thì hiện tất cả.
    /// </summary>
    public async Task<IActionResult> Index(int? serviceType)
    {
        ViewBag.ServiceType = serviceType;
        ViewBag.ServiceTypeNames = ServiceTypeNames;
        ViewBag.CurrentTypeName = serviceType.HasValue && ServiceTypeNames.ContainsKey(serviceType.Value)
            ? ServiceTypeNames[serviceType.Value]
            : "Tất cả dịch vụ";

        var query = _context.ServicePrices.AsQueryable();

        if (serviceType.HasValue)
            query = query.Where(s => s.ServiceType == serviceType.Value);

        var services = await query
            .OrderBy(s => s.ServiceType)
            .ThenBy(s => s.SortOrder)
            .ThenBy(s => s.ServiceName)
            .ToListAsync();

        return View(services);
    }

    /// <summary>
    /// GET: /ServicePrices/Create?serviceType=1
    /// Form thêm dịch vụ mới, pre-select loại dịch vụ nếu có.
    /// </summary>
    public IActionResult Create(int? serviceType)
    {
        ViewBag.ServiceTypeNames = ServiceTypeNames;
        var model = new CreateServicePriceDto
        {
            ServiceType = serviceType ?? 5,
            Status = 1
        };
        return View(model);
    }

    /// <summary>
    /// POST: /ServicePrices/Create
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateServicePriceDto dto)
    {
        if (ModelState.IsValid)
        {
            var service = new ServicePrice
            {
                ServiceName = dto.ServiceName.Trim(),
                Description = dto.Description?.Trim(),
                ServiceType = dto.ServiceType,
                Price = dto.Price,
                PriceUnit = dto.PriceUnit?.Trim(),
                Note = dto.Note?.Trim(),
                Status = dto.Status,
                SortOrder = dto.SortOrder
            };

            _context.Add(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm dịch vụ \"{service.ServiceName}\" thành công!";
            return RedirectToAction(nameof(Index), new { serviceType = dto.ServiceType });
        }

        ViewBag.ServiceTypeNames = ServiceTypeNames;
        return View(dto);
    }

    /// <summary>
    /// GET: /ServicePrices/Edit/5
    /// </summary>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var service = await _context.ServicePrices.FindAsync(id);
        if (service == null) return NotFound();

        ViewBag.ServiceTypeNames = ServiceTypeNames;
        var dto = new UpdateServicePriceDto
        {
            Id = service.Id,
            ServiceName = service.ServiceName,
            Description = service.Description,
            ServiceType = service.ServiceType,
            Price = service.Price,
            PriceUnit = service.PriceUnit,
            Note = service.Note,
            Status = service.Status,
            SortOrder = service.SortOrder
        };
        return View(dto);
    }

    /// <summary>
    /// POST: /ServicePrices/Edit/5
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateServicePriceDto dto)
    {
        if (id != dto.Id) return NotFound();

        if (ModelState.IsValid)
        {
            var service = await _context.ServicePrices.FindAsync(id);
            if (service == null) return NotFound();

            service.ServiceName = dto.ServiceName.Trim();
            service.Description = dto.Description?.Trim();
            service.ServiceType = dto.ServiceType;
            service.Price = dto.Price;
            service.PriceUnit = dto.PriceUnit?.Trim();
            service.Note = dto.Note?.Trim();
            service.Status = dto.Status;
            service.SortOrder = dto.SortOrder;

            _context.Update(service);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật dịch vụ \"{service.ServiceName}\" thành công!";
            return RedirectToAction(nameof(Index), new { serviceType = dto.ServiceType });
        }

        ViewBag.ServiceTypeNames = ServiceTypeNames;
        return View(dto);
    }

    /// <summary>
    /// POST: /ServicePrices/Delete/5
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var service = await _context.ServicePrices.FindAsync(id);
        if (service == null) return NotFound();

        int serviceType = service.ServiceType;
        string name = service.ServiceName;

        _context.ServicePrices.Remove(service);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đã xóa dịch vụ \"{name}\" thành công!";
        return RedirectToAction(nameof(Index), new { serviceType });
    }

    /// <summary>
    /// POST: /ServicePrices/ToggleStatus/5
    /// Bật/tắt trạng thái hiển thị nhanh.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, int serviceType)
    {
        var service = await _context.ServicePrices.FindAsync(id);
        if (service == null) return NotFound();

        service.Status = service.Status == 1 ? 0 : 1;
        _context.Update(service);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Đã {(service.Status == 1 ? "bật" : "tắt")} hiển thị dịch vụ \"{service.ServiceName}\"!";
        return RedirectToAction(nameof(Index), new { serviceType });
    }
}
