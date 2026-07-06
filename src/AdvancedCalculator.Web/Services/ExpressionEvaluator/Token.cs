namespace AdvancedCalculator.Web.Services.ExpressionEvaluator;

/// <summary>
/// An immutable lexical token: a classified piece of the input expression.
/// </summary>
/// <param name="Type">The category of this token.</param>
/// <param name="Text">The raw text that produced this token (e.g. "3.14", "sin", "+").</param>
/// <param name="NumericValue">
/// The parsed numeric value, populated only when <see cref="Type"/> is
/// <see cref="TokenType.Number"/>. Otherwise 0 and unused.
/// </param>
/// <param name="Position">
/// The zero-based character index in the original expression where this token starts.
/// Used to produce precise error messages for invalid input.
/// </param>
public sealed record Token(TokenType Type, string Text, double NumericValue, int Position)
{
    public override string ToString() => $"{Type}('{Text}')";
}