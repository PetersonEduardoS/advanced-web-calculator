# Advanced Calculator

A scientific web calculator built from scratch with ASP.NET Core, C#, and vanilla JavaScript: no calculator libraries, no Bootstrap, no jQuery.

The core of the project is a hand-written **recursive-descent expression parser** that correctly resolves operator precedence, nested parentheses, and the classic unary-vs-binary minus ambiguity, a common source of bugs in naive calculator implementations that split expressions with regex or evaluate left-to-right without a real grammar.

Built as a portfolio project for Junior .NET Developer / Back-end Developer roles.

## Features

- **Standard mode**: the four basic operations, percentage, sign toggle, decimal input
- **Scientific mode**: sin, cos, tan (with DEG/RAD toggle), log, ln, square root, powers (`x^y`), factorial, π, e, and parentheses for compound expressions
- **Real expression parsing**: supports operator precedence and nested parentheses (`(2 + 3) * (4 - 1)`), not just left-to-right evaluation
- **Persistent history**: every successful calculation is saved to a local SQLite database via Entity Framework Core
  - Live sidebar panel showing the 10 most recent calculations, updated without a page reload
  - Dedicated `/History` page with favoriting, copy, and delete
- **Light / dark theme**, persisted across sessions via `localStorage`
- **Responsive design**, tested down to ~360px viewport width
- **74 automated unit tests** (xUnit) covering the parser: arithmetic, negative numbers, chained subtraction, operator precedence, nested parentheses, scientific functions, degrees/radians conversion, and invalid input handling

## Tech Stack

- **Backend**: ASP.NET Core 8 (Razor Pages), C#
- **Data**: Entity Framework Core + SQLite
- **Frontend**: Vanilla JavaScript (no frameworks), custom CSS (no Bootstrap/jQuery)
- **Testing**: xUnit

## Why a hand-written parser?

An earlier version of this calculator tokenized expressions with a naive approach that couldn't reliably distinguish a subtraction operator (`3 - 4`) from a negative number (`-4`), causing incorrect results on expressions like `3 - -5` or `10 - 3 - 2`.

This version fixes that at the design level with a proper **recursive-descent parser** following a formal grammar:
expression   → term (('+' | '-') term)*
term         → unary (('' | '/') unary)
unary        → ('-' | '+') unary | power
power        → factorial ('^' unary)*
factorial    → primary ('!')*
primary      → NUMBER | CONSTANT | functionCall | '(' expression ')'
functionCall → FUNCTION '(' expression ')'

The key insight: a `-` is only ever treated as **unary** inside `ParseUnary()`, which is exclusively reached when the grammar expects the *start* of a new operand: right after `(`, right after another operator, or at the very beginning of the expression. Everywhere else, it's binary. This resolves the ambiguity structurally, through the grammar itself, rather than through lexical guesswork in the tokenizer.

See [`ExpressionParser.cs`](src/AdvancedCalculator.Web/Services/ExpressionEvaluator/ExpressionParser.cs) for the full implementation and [`ExpressionParserTests.cs`](tests/AdvancedCalculator.Tests/ExpressionParserTests.cs) for the test suite.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) (`dotnet tool install --global dotnet-ef`)

### Setup

```bash
git clone https://github.com/PetersonEduardoS/advanced-web-calculator.git
cd advanced-web-calculator

# Apply database migrations (creates calculator.db)
cd src/AdvancedCalculator.Web
dotnet ef database update

# Run the app
dotnet run
```

Open `https://localhost:7243` in your browser.

### Running tests

```bash
# From the repository root
dotnet test
```

Expected output: `74 passed, 0 failed`.

## Project Structure

advanced-web-calculator/
├── src/
│   └── AdvancedCalculator.Web/
│       ├── Data/                    # EF Core DbContext
│       ├── Migrations/              # EF Core migrations
│       ├── Models/                  # CalculationHistory entity
│       ├── Pages/                   # Razor Pages (Index, History, Error)
│       ├── Services/
│       │   └── ExpressionEvaluator/ # Tokenizer, Parser, custom exception
│       └── wwwroot/                 # CSS, JS (vanilla, no frameworks)
└── tests/
└── AdvancedCalculator.Tests/    # xUnit test suite (74 tests)

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Author

**Peterson Eduardo Sampaio Silva**
[GitHub](https://github.com/PetersonEduardoS) · [LinkedIn](https://linkedin.com/in/peterson-eduardo-silva)
