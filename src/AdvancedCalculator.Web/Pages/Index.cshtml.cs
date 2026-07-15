using AdvancedCalculator.Web.Data;
using AdvancedCalculator.Web.Models;
using AdvancedCalculator.Web.Services.ExpressionEvaluator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdvancedCalculator.Web.Pages;

public class IndexModel : PageModel
{
    private const int RecentHistoryLimit = 10;

    private readonly AppDbContext _dbContext;

    public IndexModel(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<CalculationHistory> RecentEntries { get; private set; } = new();

    public async Task OnGetAsync()
    {
        RecentEntries = await LoadRecentEntriesAsync();
    }

    /// <summary>
    /// Evaluates an expression submitted from the calculator UI via fetch(),
    /// and persists it to the History table on success.
    /// Antiforgery validation happens automatically (Razor Pages applies it
    /// to all POST handlers by default); the client must send the token
    /// in the "X-CSRF-TOKEN" header, configured in Program.cs.
    /// </summary>
    public async Task<IActionResult> OnPostCalculateAsync([FromBody] CalculateRequest request)
    {
        try
        {
            var angleMode = request.AngleMode == "degrees" ? AngleMode.Degrees : AngleMode.Radians;
            var parser = new ExpressionParser(angleMode);

            double result = parser.Parse(request.Expression);

            _dbContext.CalculationHistories.Add(new CalculationHistory
            {
                Expression = request.Expression,
                Result = result,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            return new JsonResult(new CalculateResponse(Success: true, Result: result, Error: null));
        }
        catch (ExpressionEvaluationException ex)
        {
            return new JsonResult(new CalculateResponse(Success: false, Result: null, Error: ex.Message));
        }
    }

    /// <summary>
    /// Returns the most recent calculations as JSON, used by the sidebar panel
    /// to refresh itself after each successful calculation without a full page reload.
    /// </summary>
    public async Task<IActionResult> OnGetRecentHistoryAsync()
    {
        var entries = await LoadRecentEntriesAsync();

        var payload = entries.Select(e => new
        {
            e.Id,
            e.Expression,
            e.Result
        });

        return new JsonResult(payload);
    }

    private async Task<List<CalculationHistory>> LoadRecentEntriesAsync()
    {
        return await _dbContext.CalculationHistories
            .OrderByDescending(e => e.CreatedAtUtc)
            .Take(RecentHistoryLimit)
            .ToListAsync();
    }
}

public record CalculateRequest(string Expression, string AngleMode = "radians");

public record CalculateResponse(bool Success, double? Result, string? Error);