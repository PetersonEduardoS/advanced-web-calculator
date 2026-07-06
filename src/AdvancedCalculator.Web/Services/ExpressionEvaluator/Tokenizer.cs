using System.Globalization;
using System.Text;

namespace AdvancedCalculator.Web.Services.ExpressionEvaluator;

/// <summary>
/// Converts a raw expression string into a flat list of <see cref="Token"/>s.
/// This class is purely lexical: it has no knowledge of operator precedence,
/// associativity, or whether a '-' character is a unary or binary minus.
/// That interpretation is the responsibility of <see cref="ExpressionParser"/>.
/// </summary>
public sealed class Tokenizer
{
    private static readonly HashSet<string> KnownFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sin", "cos", "tan", "log", "ln", "sqrt"
    };

    private readonly string _input;
    private int _position;

    public Tokenizer(string input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
    }

    /// <summary>
    /// Tokenizes the entire input and returns the resulting list, always terminated
    /// by a single <see cref="TokenType.EndOfInput"/> token.
    /// </summary>
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (true)
        {
            SkipWhitespace();

            if (_position >= _input.Length)
            {
                tokens.Add(new Token(TokenType.EndOfInput, string.Empty, 0, _position));
                break;
            }

            char current = _input[_position];
            int startPosition = _position;

            if (char.IsDigit(current) || current == '.')
            {
                tokens.Add(ReadNumber());
                continue;
            }

            if (current == 'π')
            {
                tokens.Add(new Token(TokenType.Constant, "π", 0, startPosition));
                _position++;
                continue;
            }

            if (char.IsLetter(current))
            {
                tokens.Add(ReadIdentifier());
                continue;
            }

            switch (current)
            {
                case '+':
                    tokens.Add(new Token(TokenType.Plus, "+", 0, startPosition));
                    _position++;
                    break;
                case '-':
                    tokens.Add(new Token(TokenType.Minus, "-", 0, startPosition));
                    _position++;
                    break;
                case '*':
                    tokens.Add(new Token(TokenType.Multiply, "*", 0, startPosition));
                    _position++;
                    break;
                case '/':
                    tokens.Add(new Token(TokenType.Divide, "/", 0, startPosition));
                    _position++;
                    break;
                case '^':
                    tokens.Add(new Token(TokenType.Power, "^", 0, startPosition));
                    _position++;
                    break;
                case '!':
                    tokens.Add(new Token(TokenType.Factorial, "!", 0, startPosition));
                    _position++;
                    break;
                case '(':
                    tokens.Add(new Token(TokenType.LeftParenthesis, "(", 0, startPosition));
                    _position++;
                    break;
                case ')':
                    tokens.Add(new Token(TokenType.RightParenthesis, ")", 0, startPosition));
                    _position++;
                    break;
                
                default:
                    throw new ExpressionEvaluationException(
                        $"Unexpected character '{current}' at position {startPosition}.");
            }
        }

        return tokens;
    }

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }
    }

    private Token ReadNumber()
    {
        int startPosition = _position;
        var buffer = new StringBuilder();
        bool hasDecimalPoint = false;

        while (_position < _input.Length &&
               (char.IsDigit(_input[_position]) || _input[_position] == '.'))
        {
            if (_input[_position] == '.')
            {
                if (hasDecimalPoint)
                {
                    throw new ExpressionEvaluationException(
                        $"Malformed number with multiple decimal points near position {startPosition}.");
                }
                hasDecimalPoint = true;
            }

            buffer.Append(_input[_position]);
            _position++;
        }

        string text = buffer.ToString();

        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new ExpressionEvaluationException(
                $"Malformed number '{text}' at position {startPosition}.");
        }

        return new Token(TokenType.Number, text, value, startPosition);
    }

    /// <summary>
    /// Reads a run of letters and classifies it as either a known function name
    /// (e.g. "sin") or the constant "e". Any other identifier is invalid.
    /// </summary>
    private Token ReadIdentifier()
    {
        int startPosition = _position;
        var buffer = new StringBuilder();

        while (_position < _input.Length && char.IsLetter(_input[_position]))
        {
            buffer.Append(_input[_position]);
            _position++;
        }

        string text = buffer.ToString();

        if (KnownFunctions.Contains(text))
        {
            return new Token(TokenType.Function, text.ToLowerInvariant(), 0, startPosition);
        }

        if (string.Equals(text, "e", StringComparison.OrdinalIgnoreCase))
        {
            return new Token(TokenType.Constant, "e", 0, startPosition);
        }

        throw new ExpressionEvaluationException(
            $"Unknown identifier '{text}' at position {startPosition}.");
    }
}