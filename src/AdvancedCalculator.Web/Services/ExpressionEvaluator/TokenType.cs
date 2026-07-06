namespace AdvancedCalculator.Web.Services.ExpressionEvaluator;

/// <summary>
/// Represents the category of a lexical token produced by the <see cref="Tokenizer"/>.
/// The tokenizer only classifies characters into these categories; it does NOT decide
/// whether a MINUS token is unary or binary. That decision belongs to the parser,
/// which has the grammatical context to resolve it correctly.
/// </summary>
public enum TokenType
{
    Number,
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    Factorial,
    LeftParenthesis,
    RightParenthesis,
    Function,      // sin, cos, tan, log, ln, sqrt
    Constant,      // pi, e
    EndOfInput
}