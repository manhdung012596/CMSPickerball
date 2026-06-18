using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoccerPitchMvc.Services;

public class BookingService : IBookingService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public BookingService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Booking>> GetActiveBookingsAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .OrderByDescending(b => b.BookingDate)
            .ThenByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> CreateBookingAsync(Booking booking)
    {
        using var context = _dbFactory.CreateDbContext();
        // Kiểm tra xem khung giờ này trên sân này vào ngày này đã được đặt chưa (không tính các booking đã hủy - Status = 3)
        bool isSlotTaken = await context.Bookings
            .AnyAsync(b => b.PitchId == booking.PitchId 
                        && b.TimeSlotId == booking.TimeSlotId 
                        && b.BookingDate == booking.BookingDate 
                        && b.Status != 3);
        
        if (isSlotTaken) return false;

        booking.CreatedAt = DateTime.Now;
        
        // Tính tiền: giá thuê sân mặc định là 130,000 VNĐ. Có thể override nếu TimeSlot có PriceOverride.
        var timeSlot = await context.TimeSlots.FindAsync(booking.TimeSlotId);
        decimal price = (timeSlot?.PriceOverride ?? 130000.00m);
        booking.TotalPrice = price;

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBookingStatusAsync(int id, int status)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbBooking = await context.Bookings.FindAsync(id);
        if (dbBooking == null) return false;
        dbBooking.Status = status;
        context.Bookings.Update(dbBooking);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBookingAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbBooking = await context.Bookings.FindAsync(id);
        if (dbBooking == null) return false;
        context.Bookings.Remove(dbBooking);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Booking>> GetBookingsByDateRangeAsync(DateOnly start, DateOnly end)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .Where(b => b.BookingDate >= start && b.BookingDate <= end && b.Status != 3)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetBookingsByDateAsync(DateOnly date)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Pitch)
            .Include(b => b.TimeSlot)
            .Where(b => b.BookingDate == date)
            .ToListAsync();
    }

    public async Task<List<TimeSlot>> GetTimeSlotsForPitchAsync(int pitchId)
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.TimeSlots
            .Where(t => t.PitchId == pitchId)
            .OrderBy(t => t.StartTime)
            .ToListAsync();
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Customers
            .OrderBy(c => c.FullName)
            .ToListAsync();
    }

    public async Task<List<Pitch>> GetActivePitchesAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Pitches
            .Where(p => p.Status != 2)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
