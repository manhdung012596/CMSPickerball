using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý danh sách sân bóng (Pitches).
/// Hỗ trợ các thao tác CRUD: Xem danh sách, Chi tiết, Thêm mới, Chỉnh sửa, và Xóa mềm (Soft Delete).
/// </summary>
public class PitchesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PitchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /Pitches
    /// Hiển thị danh sách sân bóng chưa bị xóa (Status != 2).
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Chỉ lấy các sân có trạng thái khác 2 (2 là trạng thái đã xóa mềm)
        var pitches = await _context.Pitches
            .Where(p => p.Status != 2)
            .ToListAsync();
        return View(pitches);
    }

    /// <summary>
    /// GET: /Pitches/Create
    /// Trả về View để thêm mới sân bóng.
    /// </summary>
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// POST: /Pitches/Create
    /// Xử lý thêm mới sân bóng, kiểm tra trùng tên sân.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pitch pitch)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra trùng tên sân bóng (trừ các sân đã bị xóa mềm)
            bool isNameExists = await _context.Pitches
                .AnyAsync(p => p.Name.ToLower() == pitch.Name.ToLower() && p.Status != 2);

            if (isNameExists)
            {
                ModelState.AddModelError("Name", "Tên sân bóng này đã tồn tại.");
                return View(pitch);
            }

            // Mặc định ngày tạo và trạng thái hoạt động ban đầu
            pitch.CreatedDate = DateTime.Now;
            if (pitch.PricePerHour <= 0)
            {
                ModelState.AddModelError("PricePerHour", "Giá thuê theo giờ phải lớn hơn 0.");
                return View(pitch);
            }

            _context.Add(pitch);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(pitch);
    }

    /// <summary>
    /// GET: /Pitches/Edit/5
    /// Trả về View chỉnh sửa thông tin sân bóng.
    /// </summary>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pitch = await _context.Pitches.FindAsync(id);
        if (pitch == null || pitch.Status == 2)
        {
            return NotFound();
        }
        return View(pitch);
    }

    /// <summary>
    /// POST: /Pitches/Edit/5
    /// Xử lý cập nhật thông tin sân bóng, kiểm tra trùng tên sân bóng với sân khác.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pitch pitch)
    {
        if (id != pitch.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Kiểm tra trùng tên sân với sân khác
            bool isNameExists = await _context.Pitches
                .AnyAsync(p => p.Id != id && p.Name.ToLower() == pitch.Name.ToLower() && p.Status != 2);

            if (isNameExists)
            {
                ModelState.AddModelError("Name", "Tên sân bóng này đã tồn tại.");
                return View(pitch);
            }

            if (pitch.PricePerHour <= 0)
            {
                ModelState.AddModelError("PricePerHour", "Giá thuê theo giờ phải lớn hơn 0.");
                return View(pitch);
            }

            try
            {
                _context.Update(pitch);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PitchExists(pitch.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(pitch);
    }

    /// <summary>
    /// POST: /Pitches/Delete/5
    /// Thực hiện xóa mềm sân bóng bằng cách gán trạng thái Status = 2.
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var pitch = await _context.Pitches.FindAsync(id);
        if (pitch != null)
        {
            // Thực hiện Soft Delete bằng cách chuyển Status thành 2
            pitch.Status = 2;
            _context.Update(pitch);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> PitchExists(int id)
    {
        return await _context.Pitches.AnyAsync(e => e.Id == id);
    }
}
