using Microsoft.EntityFrameworkCore;
using SoccerPitchMvc.Data;
using SoccerPitchMvc.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SoccerPitchMvc.Services;

public class LanguageService : ILanguageService
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbFactory;

    public LanguageService(IDbContextFactory<ApplicationDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<Language>> GetLanguagesAsync()
    {
        using var context = _dbFactory.CreateDbContext();
        return await context.Languages
            .OrderBy(l => l.SortOrder)
            .ToListAsync();
    }

    public async Task<bool> CreateLanguageAsync(Language lang)
    {
        using var context = _dbFactory.CreateDbContext();
        bool exists = await context.Languages.AnyAsync(l => l.Code.ToLower() == lang.Code.ToLower());
        if (exists) return false;

        context.Languages.Add(lang);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLanguageAsync(Language lang)
    {
        using var context = _dbFactory.CreateDbContext();
        bool exists = await context.Languages.AnyAsync(l => l.Code.ToLower() == lang.Code.ToLower() && l.Id != lang.Id);
        if (exists) return false;

        var dbLang = await context.Languages.FindAsync(lang.Id);
        if (dbLang == null) return false;

        dbLang.Code = lang.Code;
        dbLang.Name = lang.Name;
        dbLang.AlternateName = lang.AlternateName;
        dbLang.Description = lang.Description;
        dbLang.SortOrder = lang.SortOrder;
        dbLang.Status = lang.Status;

        context.Languages.Update(dbLang);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteLanguageAsync(int id)
    {
        using var context = _dbFactory.CreateDbContext();
        var dbLang = await context.Languages.FindAsync(id);
        if (dbLang == null) return false;

        context.Languages.Remove(dbLang);
        await context.SaveChangesAsync();
        return true;
    }
}
