using AdvancedCalculator.Web.Services.ExpressionEvaluator;
using Xunit;

namespace AdvancedCalculator.Tests;

public class ExpressionParserTests
{
    private readonly ExpressionParser _parser = new();

    // ---- Basic arithmetic ----

    [Theory]
    [InlineData("2 + 3", 5)]
    [InlineData("10 - 4", 6)]
    [InlineData("6 * 7", 42)]
    [InlineData("20 / 4", 5)]
    [InlineData("7 / 2", 3.5)]
    public void Parse_BasicArithmetic_ReturnsExpectedResult(string expression, double expected)
    {
        double result = _parser.Parse(expression);
        Assert.Equal(expected, result, precision: 10);
    }

    // ---- Negative numbers (the historical bug class) ----

    [Theory]
    [InlineData("-5", -5)]
    [InlineData("-5 + 3", -2)]
    [InlineData("3 + -5", -2)]
    [InlineData("3 - -5", 8)]
    [InlineData("-3 - -5", 2)]
    [InlineData("-3 * -4", 12)]
    [InlineData("-3 * 4", -12)]
    [InlineData("6 / -2", -3)]
    public void Parse_NegativeNumbers_ReturnsExpectedResult(string expression, double expected)
    {
        double result = _parser.Parse(expression);
        Assert.Equal(expected, result, precision: 10);
    }

    // ---- Chained subtraction ----

    [Theory]
    [InlineData("10 - 3 - 2", 5)]
    [InlineData("10 - -3 - -2", 15)]
    [InlineData("10 - 3 - 2 - 1", 4)]
    [InlineData("1 - 2 - 3 - 4", -8)]
    public void Parse_ChainedSubtraction_IsLeftAssociative(string expression, double expected)
    {
        double result = _parser.Parse(expression);
        Assert.Equal(expected, result, precision: 10);
    }

    // ---- Operator precedence ----

    [Theory]
    [InlineData("2 + 3 * 4", 14)]
    [InlineData("2 * 3 + 4", 10)]
    [InlineData("2 + 3 * 4 - 1", 13)]
    [InlineData("10 / 2 + 3", 8)]
    [InlineData("2 + 10 / 2", 7)]
    [InlineData("2 ^ 3 + 1", 9)]
    [InlineData("1 + 2 ^ 3", 9)]
    [InlineData("2 ^ 3 ^ 2", 512)] // right-associative: 2^(3^2), not (2^3)^2
    [InlineData("-2 ^ 2", -4)]     // unary minus binds looser than power
    [InlineData("2 ^ -1", 0.5)]    // exponent can itself be negative
    public void Parse_OperatorPrecedence_IsRespected(string expression, double expected)
    {
        double result = _parser.Parse(expression);
        Assert.Equal(expected, result, precision: 10);
    }

    // ---- Parentheses, including nested ----

    [Theory]
    [InlineData("(2 + 3) * 4", 20)]
    [InlineData("2 * (3 + 4)", 14)]
    [InlineData("((2 + 3) * (4 - 1))", 15)]
    [InlineData("-(3 + 4)", -7)]
    [InlineData("-(3 + 4) * -1", 7)]
    [InlineData("((((1 + 1))))", 2)]
    [InlineData("(1 + (2 + (3 + (4 + 5))))", 15)]
    public void Parse_Parentheses_ResolvesCorrectly(string expression, double expected)
    {
        double result = _parser.Parse(expression);
        Assert.Equal(expected, result, precision: 10);
    }

    // ---- Scientific functions ----

    [Theory]
    [InlineData("sin(0)", 0)]
    [InlineData("cos(0)", 1)]
    [InlineData("sqrt(16)", 4)]
    [InlineData("sqrt(2)", 1.4142135623730951)]
    [InlineData("log(100)", 2)]
    [InlineData("ln(1)", 0)]
    [InlineData("5!", 120)]
    [InlineData("0!", 1)]
    [InlineData("1!", 1)]
    public void Parse_ScientificFunctions_ReturnsExpectedResult(string expression, double expected)
    {
        double result = _parser.Parse(expression);
        Assert.Equal(expected, result, precision: 10);
    }

    [Fact]
    public void Parse_Pi_ReturnsMathPiValue()
    {
        double result = _parser.Parse("π");
        Assert.Equal(Math.PI, result, precision: 10);
    }

    [Fact]
    public void Parse_EulerNumber_ReturnsMathEValue()
    {
        double result = _parser.Parse("e");
        Assert.Equal(Math.E, result, precision: 10);
    }

    [Fact]
    public void Parse_FunctionWithCompoundExpression_EvaluatesInnerExpressionFirst()
    {
        // sqrt(3 + 1) = sqrt(4) = 2
        double result = _parser.Parse("sqrt(3 + 1)");
        Assert.Equal(2, result, precision: 10);
    }

    [Fact]
    public void Parse_NestedFunctionCalls_EvaluatesCorrectly()
    {
        // sqrt(sqrt(16)) = sqrt(4) = 2
        double result = _parser.Parse("sqrt(sqrt(16))");
        Assert.Equal(2, result, precision: 10);
    }

    // ---- Invalid input ----

    [Fact]
    public void Parse_EmptyExpression_ThrowsExpressionEvaluationException()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse(""));
    }

    [Fact]
    public void Parse_UnbalancedParentheses_MissingClosing_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("(2 + 3"));
    }

    [Fact]
    public void Parse_UnbalancedParentheses_ExtraClosing_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("2 + 3)"));
    }

    [Fact]
    public void Parse_DivisionByZero_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("5 / 0"));
    }

    [Fact]
    public void Parse_UnknownCharacter_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("3 @ 4"));
    }

    [Fact]
    public void Parse_FunctionWithoutArgument_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("sin()"));
    }

    [Fact]
    public void Parse_FunctionWithoutParentheses_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("sin 5"));
    }

    [Fact]
    public void Parse_SqrtOfNegativeNumber_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("sqrt(-4)"));
    }

    [Fact]
    public void Parse_LogOfZeroOrNegative_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("log(0)"));
    }

    [Fact]
    public void Parse_FactorialOfNegativeNumber_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("(-5)!"));
    }

    [Fact]
    public void Parse_FactorialOfNonInteger_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("2.5!"));
    }

    [Fact]
    public void Parse_MalformedNumber_MultipleDecimalPoints_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("3.4.5"));
    }

    [Fact]
    public void Parse_UnknownIdentifier_Throws()
    {
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("foo(1)"));
    }

    [Fact]
    public void Parse_TrailingGarbageAfterValidExpression_Throws()
    {
        // "3 + 4 5" - after parsing "3 + 4" successfully, a stray "5" remains unconsumed
        Assert.Throws<ExpressionEvaluationException>(() => _parser.Parse("3 + 4 5"));
    }
}