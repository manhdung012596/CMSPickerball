using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoccerPitchMvc.Services;

public class PitchService : IPitchService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public PitchService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Pitch>> GetActivePitchesAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Pitches
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<Pitch?> GetPitchByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Pitches
            .Include(p => p.TimeSlots)
            .FirstOrDefaultAsync(p => p.Id == id && p.Status != 2);
    }

    public async Task<bool> IsPitchNameExistsAsync(string name, int? excludeId = null)
    {
        using var context = _dbFactory.CreateDbContext();
        if (excludeId.HasValue)
        {
            return await context.Pitches
                .AnyAsync(p => p.Name.ToLower() == name.ToLower() && p.Id != excludeId.Value && p.Status != 2);
        }
        return await context.Pitches
            .AnyAsync(p => p.Name.ToLower() == name.ToLower() && p.Status != 2);
    }

    public async Task<bool> CreatePitchAsync(Pitch pitch)
    {
        using var context = _dbFactory.CreateDbContext();
        if (await IsPitchNameExistsAsync(pitch.Name)) return false;
        pitch.CreatedDate = DateTime.Now;
        pitch.PricePerHour = 130000; // Flat rate
        context.Pitches.Add(pitch);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePitchAsync(Pitch pitch)
    {
        using var context = _dbFactory.CreateDbContext();
        if (await IsPitchNameExistsAsync(pitch.Name, pitch.Id)) return false;
        var dbPitch = await context.Pitches.FindAsync(pitch.Id);
        if (dbPitch == null || dbPitch.Status == 2) return false;
        
        dbPitch.Name = pitch.Name;
        dbPitch.Status = pitch.Status;
        dbPitch.PricePerHour = 130000; // Flat rate
        
        context.Pitches.Update(dbPitch);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePitchAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbPitch = await context.Pitches.FindAsync(id);
        if (dbPitch == null) return false;
        dbPitch.Status = 2; // Soft delete
        context.Pitches.Update(dbPitch);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<TimeSlot>> GetTimeSlotsByPitchIdAsync(int pitchId)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.TimeSlots
            .Where(t => t.PitchId == pitchId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<bool> CreateTimeSlotAsync(TimeSlot slot)
    {
        using var context = _dbFactory.CreateDbContext();
        bool exists = await context.TimeSlots
            .AnyAsync(t => t.PitchId == slot.PitchId && t.StartTime == slot.StartTime && t.EndTime == slot.EndTime);
        if (exists) return false;

        context.TimeSlots.Add(slot);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateTimeSlotAsync(TimeSlot slot)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbSlot = await context.TimeSlots.FindAsync(slot.Id);
        if (dbSlot == null) return false;

        dbSlot.StartTime = slot.StartTime;
        dbSlot.EndTime = slot.EndTime;
        dbSlot.PriceOverride = slot.PriceOverride;

        context.TimeSlots.Update(dbSlot);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTimeSlotAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbSlot = await context.TimeSlots.FindAsync(id);
        if (dbSlot == null) return false;

        context.TimeSlots.Remove(dbSlot);
        await context.SaveChangesAsync();
        return true;
    }
}
