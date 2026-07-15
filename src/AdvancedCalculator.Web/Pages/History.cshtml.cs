using AdvancedCalculator.Web.Data;
using AdvancedCalculator.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvancedCalculator.Web.Pages;

public class HistoryModel : PageModel
{
    private readonly AppDbContext _dbContext;

    public HistoryModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<CalculationHistory> Entries { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Entries = await _dbContext.CalculationHistories
            .OrderByDescending(e => e.IsFavorite)
            .ThenByDescending(e => e.CreatedAtUtc)
            .Take(100)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostToggleFavoriteAsync([FromBody] ToggleFavoriteRequest request)
    {
        var entry = await _dbContext.CalculationHistories.FindAsync(request.Id);
        if (entry is null)
        {
            return NotFound();
        }

        entry.IsFavorite = !entry.IsFavorite;
        await _dbContext.SaveChangesAsync();

        return new JsonResult(new { success = true, isFavorite = entry.IsFavorite });
    }

    public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteEntryRequest request)
    {
        var entry = await _dbContext.CalculationHistories.FindAsync(request.Id);
        if (entry is null)
        {
            return NotFound();
        }

        _dbContext.CalculationHistories.Remove(entry);
        await _dbContext.SaveChangesAsync();

        return new JsonResult(new { success = true });
    }
}

public record ToggleFavoriteRequest(int Id);

public record DeleteEntryRequest(int Id);