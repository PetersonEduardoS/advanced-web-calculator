namespace AdvancedCalculator.Web.Services.ExpressionEvaluator;

/// <summary>
/// Determines how the argument of trigonometric functions (sin, cos, tan)
/// is interpreted. Radians is the mathematical default used internally by
/// System.Math; Degrees is provided for a friendlier calculator UX and is
/// converted to radians before evaluation.
/// </summary>
public enum AngleMode
{
    Radians,
    Degrees
}