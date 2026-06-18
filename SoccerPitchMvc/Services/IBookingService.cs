using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IBookingService
{
    Task<List<Booking>> GetActiveBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(int id);
    Task<bool> CreateBookingAsync(Booking booking);
    Task<bool> UpdateBookingStatusAsync(int id, int status);
    Task<bool> DeleteBookingAsync(int id);
    Task<List<Booking>> GetBookingsByDateRangeAsync(DateOnly start, DateOnly end);
    Task<List<Booking>> GetBookingsByDateAsync(DateOnly date);
    Task<List<TimeSlot>> GetTimeSlotsForPitchAsync(int pitchId);
    Task<List<Customer>> GetCustomersAsync();
    Task<List<Pitch>> GetActivePitchesAsync();
}
