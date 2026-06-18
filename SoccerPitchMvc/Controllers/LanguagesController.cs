using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý danh sách Ngôn ngữ hệ thống (Languages).
/// Hỗ trợ tìm kiếm, phân trang và xem danh sách.
/// </summary>
public class LanguagesController : Controller
{
    private readonly ApplicationDbContext _context;

    public LanguagesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Languages
    /// Hiển thị danh sách ngôn ngữ với bộ lọc tìm kiếm và phân trang.
    /// </summary>
    public async Task<IActionResult> Index(string searchTerm, int page = 1)
    {
        int pageSize = 10;
        var query = _context.Languages.AsNoTracking();

        // Tìm kiếm theo tên hoặc mã hoặc tên thay thế
        if (!string.IsNullOrEmpty(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(searchTerm) || 
                                     l.Code.ToLower().Contains(searchTerm) || 
                                     (l.AlternateName != null && l.AlternateName.ToLower().Contains(searchTerm)));
        }

        // Tính tổng số mục
        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
        if (page < 1) page = 1;
        if (totalPages > 0 && page > totalPages) page = totalPages;

        // Lấy danh sách phân trang
        var languages = await query
            .OrderBy(l => l.SortOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;
        ViewBag.PageSize = pageSize;

        return View(languages);
    }
}
