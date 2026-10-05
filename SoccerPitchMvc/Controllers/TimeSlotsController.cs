using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Controllers;

/// <summary>
/// Controller quản lý khung giờ (TimeSlots) cho từng sân bóng.
/// Hỗ trợ CRUD: Danh sách, Thêm mới, Chỉnh sửa, Xóa.
/// </summary>
[Authorize(Roles = "Admin,Staff")]
public class TimeSlotsController : Controller
{
    private readonly ApplicationDbContext _context;

    public TimeSlotsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// GET: /TimeSlots
    /// Hiển thị danh sách tất cả khung giờ, có thể lọc theo sân.
    /// </summary>
    public async Task<IActionResult> Index(int? pitchId)
    {
        var pitches = await _context.Pitches
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewBag.Pitches = new SelectList(pitches, "Id", "Name", pitchId);
        ViewBag.SelectedPitchId = pitchId;

        var query = _context.TimeSlots
            .Include(t => t.Pitch)
            .Where(t => t.Pitch.Status != 2)
            .AsQueryable();

        if (pitchId.HasValue)
        {
            query = query.Where(t => t.PitchId == pitchId.Value);
        }

        var timeSlots = await query
            .OrderBy(t => t.PitchId)
            .ThenBy(t => t.StartTime)
            .ToListAsync();

        return View(timeSlots);
    }

    /// <summary>
    /// GET: /TimeSlots/Create?pitchId=1
    /// Trả về form thêm mới khung giờ, pre-select sân nếu có pitchId.
    /// </summary>
    public async Task<IActionResult> Create(int? pitchId)
    {
        await LoadPitchesDropdown(pitchId);
        var model = new TimeSlot
        {
            PitchId = pitchId ?? 0,
            StartTime = new TimeOnly(6, 0),
            EndTime = new TimeOnly(7, 0)
        };
        return View(model);
    }

    /// <summary>
    /// POST: /TimeSlots/Create
    /// Xử lý thêm mới khung giờ, kiểm tra trùng khung giờ trong cùng sân.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TimeSlot timeSlot)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra sân tồn tại
            var pitch = await _context.Pitches.FindAsync(timeSlot.PitchId);
            if (pitch == null || pitch.Status == 2)
            {
                ModelState.AddModelError("PitchId", "Sân được chọn không tồn tại.");
                await LoadPitchesDropdown(timeSlot.PitchId);
                return View(timeSlot);
            }

            // Kiểm tra thời gian hợp lệ
            if (timeSlot.EndTime <= timeSlot.StartTime)
            {
                ModelState.AddModelError("EndTime", "Giờ kết thúc phải sau giờ bắt đầu.");
                await LoadPitchesDropdown(timeSlot.PitchId);
                return View(timeSlot);
            }

            // Kiểm tra trùng lặp khung giờ trong cùng sân
            bool hasOverlap = await _context.TimeSlots.AnyAsync(t =>
                t.PitchId == timeSlot.PitchId &&
                t.StartTime < timeSlot.EndTime &&
                t.EndTime > timeSlot.StartTime);

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Khung giờ này bị trùng với một khung giờ đã tồn tại của sân.");
                await LoadPitchesDropdown(timeSlot.PitchId);
                return View(timeSlot);
            }

            _context.Add(timeSlot);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm khung giờ {timeSlot.StartTime:HH\\:mm} - {timeSlot.EndTime:HH\\:mm} thành công!";
            return RedirectToAction(nameof(Index), new { pitchId = timeSlot.PitchId });
        }

        await LoadPitchesDropdown(timeSlot.PitchId);
        return View(timeSlot);
    }

    /// <summary>
    /// GET: /TimeSlots/Edit/5
    /// Trả về form chỉnh sửa khung giờ.
    /// </summary>
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var timeSlot = await _context.TimeSlots
            .Include(t => t.Pitch)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (timeSlot == null) return NotFound();

        await LoadPitchesDropdown(timeSlot.PitchId);
        return View(timeSlot);
    }

    /// <summary>
    /// POST: /TimeSlots/Edit/5
    /// Xử lý cập nhật khung giờ, kiểm tra trùng với khung giờ khác trong cùng sân.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TimeSlot timeSlot)
    {
        if (id != timeSlot.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // Kiểm tra thời gian hợp lệ
            if (timeSlot.EndTime <= timeSlot.StartTime)
            {
                ModelState.AddModelError("EndTime", "Giờ kết thúc phải sau giờ bắt đầu.");
                await LoadPitchesDropdown(timeSlot.PitchId);
                return View(timeSlot);
            }

            // Kiểm tra trùng lặp với các khung giờ khác trong cùng sân
            bool hasOverlap = await _context.TimeSlots.AnyAsync(t =>
                t.Id != id &&
                t.PitchId == timeSlot.PitchId &&
                t.StartTime < timeSlot.EndTime &&
                t.EndTime > timeSlot.StartTime);

            if (hasOverlap)
            {
                ModelState.AddModelError("", "Khung giờ này bị trùng với một khung giờ đã tồn tại của sân.");
                await LoadPitchesDropdown(timeSlot.PitchId);
                return View(timeSlot);
            }

            try
            {
                _context.Update(timeSlot);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật khung giờ thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TimeSlots.AnyAsync(t => t.Id == id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index), new { pitchId = timeSlot.PitchId });
        }

        await LoadPitchesDropdown(timeSlot.PitchId);
        return View(timeSlot);
    }

    /// <summary>
    /// POST: /TimeSlots/Delete/5
    /// Xóa khung giờ (kiểm tra không có booking đang dùng).
    /// </summary>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var timeSlot = await _context.TimeSlots
            .Include(t => t.Bookings)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (timeSlot == null) return NotFound();

        // Kiểm tra có booking đang dùng khung giờ này không
        bool hasActiveBookings = timeSlot.Bookings.Any(b => b.Status != 3); // 3 = Đã hủy
        if (hasActiveBookings)
        {
            TempData["Error"] = "Không thể xóa khung giờ này vì đang có lượt đặt sân liên kết.";
            return RedirectToAction(nameof(Index), new { pitchId = timeSlot.PitchId });
        }

        int pitchId = timeSlot.PitchId;
        _context.TimeSlots.Remove(timeSlot);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã xóa khung giờ thành công!";
        return RedirectToAction(nameof(Index), new { pitchId });
    }

    // Helper: Load danh sách sân vào ViewBag
    private async Task LoadPitchesDropdown(int? selectedPitchId = null)
    {
        var pitches = await _context.Pitches
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Name)
            .ToListAsync();

        ViewBag.PitchId = new SelectList(pitches, "Id", "Name", selectedPitchId);
    }
}
