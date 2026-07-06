using AdvancedCalculator.Web.Services.ExpressionEvaluator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdvancedCalculator.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ExpressionParser _parser = new();

    public void OnGet()
    {
    }

    /// <summary>
    /// Evaluates an expression submitted from the calculator UI via fetch().
    /// Antiforgery validation happens automatically (Razor Pages applies it
    /// to all POST handlers by default); the client must send the token
    /// in the "X-CSRF-TOKEN" header, configured in Program.cs.
    /// </summary>
    public IActionResult OnPostCalculate([FromBody] CalculateRequest request)
    {
        try
        {
            double result = _parser.Parse(request.Expression);
            return new JsonResult(new CalculateResponse(Success: true, Result: result, Error: null));
        }
        catch (ExpressionEvaluationException ex)
        {
            return new JsonResult(new CalculateResponse(Success: false, Result: null, Error: ex.Message));
        }
    }
}

public record CalculateRequest(string Expression);

public record CalculateResponse(bool Success, double? Result, string? Error);