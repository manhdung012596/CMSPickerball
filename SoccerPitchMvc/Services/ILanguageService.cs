using System.Collections.Generic;
using System.Threading.Tasks;
using SoccerPitchMvc.Models;

namespace SoccerPitchMvc.Services;

public interface ILanguageService
{
    Task<List<Language>> GetLanguagesAsync();
    Task<bool> CreateLanguageAsync(Language lang);
    Task<bool> UpdateLanguageAsync(Language lang);
    Task<bool> DeleteLanguageAsync(int id);
}
