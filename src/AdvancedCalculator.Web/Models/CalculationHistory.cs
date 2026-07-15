using System.ComponentModel.DataAnnotations;

namespace AdvancedCalculator.Web.Models;

/// <summary>
/// A single successfully evaluated expression, persisted for the History page.
/// Saved automatically whenever the user presses "=" and the expression
/// evaluates without error.
/// </summary>
public class CalculationHistory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Expression { get; set; } = string.Empty;

    public double Result { get; set; }

    public bool IsFavorite { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}