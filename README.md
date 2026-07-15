# Advanced Calculator

A scientific web calculator built from scratch with ASP.NET Core, C#, and vanilla JavaScript — no calculator libraries, no Bootstrap, no jQuery.

The core of the project is a hand-written **recursive-descent expression parser** that correctly resolves operator precedence, nested parentheses, and the classic unary-vs-binary minus ambiguity — a common source of bugs in naive calculator implementations that split expressions with regex or evaluate left-to-right without a real grammar.

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
