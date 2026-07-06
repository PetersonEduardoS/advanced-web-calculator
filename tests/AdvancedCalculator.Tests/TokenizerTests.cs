using AdvancedCalculator.Web.Services.ExpressionEvaluator;
using Xunit;

namespace AdvancedCalculator.Tests;

public class TokenizerTests
{
    [Fact]
    public void Tokenize_SimpleAddition_ProducesExpectedTokenSequence()
    {
        var tokens = new Tokenizer("3 + 4").Tokenize();

        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(TokenType.Plus, tokens[1].Type);
        Assert.Equal(TokenType.Number, tokens[2].Type);
        Assert.Equal(TokenType.EndOfInput, tokens[3].Type);
    }

    [Fact]
    public void Tokenize_MinusSign_AlwaysProducesMinusToken_RegardlessOfContext()
    {
        // The tokenizer must NOT try to distinguish unary from binary minus.
        // Both of these should simply produce a Minus token in the same position pattern.
        var afterOperand = new Tokenizer("3 - 4").Tokenize();
        var atStart = new Tokenizer("-4").Tokenize();

        Assert.Equal(TokenType.Minus, afterOperand[1].Type);
        Assert.Equal(TokenType.Minus, atStart[0].Type);
    }

    [Fact]
    public void Tokenize_DecimalNumber_ParsesCorrectValue()
    {
        var tokens = new Tokenizer("3.14").Tokenize();

        Assert.Equal(TokenType.Number, tokens[0].Type);
        Assert.Equal(3.14, tokens[0].NumericValue, precision: 10);
    }

    [Fact]
    public void Tokenize_FunctionName_ProducesFunctionToken()
    {
        var tokens = new Tokenizer("sqrt(9)").Tokenize();

        Assert.Equal(TokenType.Function, tokens[0].Type);
        Assert.Equal("sqrt", tokens[0].Text);
    }

    [Fact]
    public void Tokenize_UnknownCharacter_ThrowsWithPosition()
    {
        var exception = Assert.Throws<ExpressionEvaluationException>(
            () => new Tokenizer("3 @ 4").Tokenize());

        Assert.Contains("position 2", exception.Message);
    }

    [Fact]
    public void Tokenize_EmptyInput_ProducesOnlyEndOfInputToken()
    {
        var tokens = new Tokenizer("").Tokenize();

        Assert.Single(tokens);
        Assert.Equal(TokenType.EndOfInput, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_WhitespaceIsIgnored()
    {
        var withSpaces = new Tokenizer("3    +     4").Tokenize();
        var withoutSpaces = new Tokenizer("3+4").Tokenize();

        Assert.Equal(withSpaces.Select(t => t.Type), withoutSpaces.Select(t => t.Type));
    }
}