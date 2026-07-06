namespace AdvancedCalculator.Web.Services.ExpressionEvaluator;

/// <summary>
/// Thrown when an expression cannot be tokenized, parsed, or evaluated —
/// for example due to unexpected characters, unbalanced parentheses,
/// division by zero, or a malformed function call.
/// This is the single exception type surfaced to the UI layer, so the
/// Razor Page handler can catch it and return a clean error message
/// instead of a raw stack trace.
/// </summary>
public sealed class ExpressionEvaluationException : Exception
{
    public ExpressionEvaluationException(string message) : base(message)
    {
    }

    public ExpressionEvaluationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}