namespace AdvancedCalculator.Web.Services.ExpressionEvaluator;

/// <summary>
/// Recursive descent parser and evaluator for arithmetic and scientific expressions.
/// Grammar (highest level first, lowest precedence first):
///
///   expression   → term (('+' | '-') term)*
///   term         → unary (('*' | '/') unary)*
///   unary        → ('-' | '+') unary | power
///   power        → factorial ('^' unary)*      // right-associative; exponent may itself be unary
///   factorial    → primary ('!')*
///   primary      → NUMBER | CONSTANT | functionCall | '(' expression ')'
///   functionCall → FUNCTION '(' expression ')'
///
/// Design note: a MINUS token is only ever interpreted as unary inside <see cref="ParseUnary"/>,
/// which is exclusively called at points where the grammar expects the START of an operand
/// (right after '(', right after another operator, or at the very beginning of the expression).
/// Everywhere else (ParseExpression, ParseTerm), a MINUS/PLUS is interpreted as binary.
/// This resolves the unary-vs-binary ambiguity structurally, via the grammar itself,
/// rather than via any lexical guesswork in the tokenizer.
/// </summary>
public sealed class ExpressionParser
{
    private List<Token> _tokens = new();
    private int _position;

    /// <summary>
    /// Tokenizes and evaluates a full expression string, returning the numeric result.
    /// </summary>
    public double Parse(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ExpressionEvaluationException("Expression cannot be empty.");
        }

        _tokens = new Tokenizer(expression).Tokenize();
        _position = 0;

        double result = ParseExpression();

        if (Current.Type != TokenType.EndOfInput)
        {
            throw new ExpressionEvaluationException(
                $"Unexpected token '{Current.Text}' at position {Current.Position}. " +
                "Check for mismatched parentheses.");
        }

        return result;
    }

    private Token Current => _tokens[_position];

    private Token Advance()
    {
        Token token = _tokens[_position];
        if (_position < _tokens.Count - 1)
        {
            _position++;
        }
        return token;
    }

    private Token Expect(TokenType type, string errorMessage)
    {
        if (Current.Type != type)
        {
            throw new ExpressionEvaluationException($"{errorMessage} (at position {Current.Position}).");
        }
        return Advance();
    }

    // expression → term (('+' | '-') term)*
    private double ParseExpression()
    {
        double left = ParseTerm();

        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            TokenType operatorType = Advance().Type;
            double right = ParseTerm();
            left = operatorType == TokenType.Plus ? left + right : left - right;
        }

        return left;
    }

    // term → unary (('*' | '/') unary)*
    private double ParseTerm()
    {
        double left = ParseUnary();

        while (Current.Type is TokenType.Multiply or TokenType.Divide)
        {
            TokenType operatorType = Advance().Type;
            double right = ParseUnary();

            if (operatorType == TokenType.Divide)
            {
                if (right == 0)
                {
                    throw new ExpressionEvaluationException("Division by zero.");
                }
                left /= right;
            }
            else
            {
                left *= right;
            }
        }

        return left;
    }

    // unary → ('-' | '+') unary | power
    //
    // This is the ONLY place where a leading '-' or '+' is treated as a sign rather than
    // a binary operator. It is reached whenever the grammar needs a fresh operand:
    // at the start of the whole expression, right after '(', or right after another
    // binary operator (+, -, *, /) — exactly the positions where a "negative number"
    // is mathematically meaningful.
    private double ParseUnary()
    {
        if (Current.Type == TokenType.Minus)
        {
            Advance();
            return -ParseUnary();
        }

        if (Current.Type == TokenType.Plus)
        {
            Advance();
            return ParseUnary();
        }

        return ParsePower();
    }

    // power → factorial ('^' unary)*
    // Right-associative: 2^3^2 = 2^(3^2) = 512, achieved by recursing back into
    // ParseUnary (not ParsePower) on the right-hand side, which also lets exponents
    // carry their own sign, e.g. 2^-1 = 0.5.
    private double ParsePower()
    {
        double left = ParseFactorial();

        if (Current.Type == TokenType.Power)
        {
            Advance();
            double right = ParseUnary();
            return Math.Pow(left, right);
        }

        return left;
    }

    // factorial → primary ('!')*
    private double ParseFactorial()
    {
        double value = ParsePrimary();

        while (Current.Type == TokenType.Factorial)
        {
            Advance();
            value = ComputeFactorial(value);
        }

        return value;
    }

    // primary → NUMBER | CONSTANT | functionCall | '(' expression ')'
    private double ParsePrimary()
    {
        switch (Current.Type)
        {
            case TokenType.Number:
                return Advance().NumericValue;

            case TokenType.Constant:
                return Advance().Text switch
                {
                    "π" => Math.PI,
                    "e" => Math.E,
                    var text => throw new ExpressionEvaluationException($"Unknown constant '{text}'.")
                };

            case TokenType.Function:
                return ParseFunctionCall();

            case TokenType.LeftParenthesis:
                Advance();
                double innerValue = ParseExpression();
                Expect(TokenType.RightParenthesis, "Expected closing parenthesis ')'");
                return innerValue;

            default:
                throw new ExpressionEvaluationException(
                    $"Unexpected token '{Current.Text}' at position {Current.Position}. Expected a number, constant, function, or '('.");
        }
    }

    // functionCall → FUNCTION '(' expression ')'
    private double ParseFunctionCall()
    {
        Token functionToken = Advance();
        Expect(TokenType.LeftParenthesis, $"Expected '(' after function '{functionToken.Text}'");
        double argument = ParseExpression();
        Expect(TokenType.RightParenthesis, $"Expected closing parenthesis ')' for function '{functionToken.Text}'");

        return functionToken.Text switch
        {
            "sin" => Math.Sin(argument),
            "cos" => Math.Cos(argument),
            "tan" => Math.Tan(argument),
            "log" => argument > 0
                ? Math.Log10(argument)
                : throw new ExpressionEvaluationException("log() argument must be positive."),
            "ln" => argument > 0
                ? Math.Log(argument)
                : throw new ExpressionEvaluationException("ln() argument must be positive."),
            "sqrt" => argument >= 0
                ? Math.Sqrt(argument)
                : throw new ExpressionEvaluationException("sqrt() argument must be non-negative."),
            _ => throw new ExpressionEvaluationException($"Unknown function '{functionToken.Text}'.")
        };
    }

    private static double ComputeFactorial(double value)
    {
        if (value < 0 || value != Math.Floor(value))
        {
            throw new ExpressionEvaluationException("Factorial is only defined for non-negative integers.");
        }

        if (value > 170)
        {
            throw new ExpressionEvaluationException("Factorial argument too large (overflow beyond 170!).");
        }

        double result = 1;
        for (int i = 2; i <= value; i++)
        {
            result *= i;
        }

        return result;
    }
}