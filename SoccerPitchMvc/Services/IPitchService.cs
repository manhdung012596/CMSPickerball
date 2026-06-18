using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface IPitchService
{
    Task<List<Pitch>> GetActivePitchesAsync();
    Task<Pitch?> GetPitchByIdAsync(int id);
    Task<bool> CreatePitchAsync(Pitch pitch);
    Task<bool> UpdatePitchAsync(Pitch pitch);
    Task<bool> DeletePitchAsync(int id);
    Task<bool> IsPitchNameExistsAsync(string name, int? excludeId = null);
    Task<List<TimeSlot>> GetTimeSlotsByPitchIdAsync(int pitchId);
    Task<bool> CreateTimeSlotAsync(TimeSlot slot);
    Task<bool> UpdateTimeSlotAsync(TimeSlot slot);
    Task<bool> DeleteTimeSlotAsync(int id);
}
