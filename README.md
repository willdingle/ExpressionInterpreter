# ExpressionInterpreter
A custom language for intrepreting maths expressions and plotting functions.

## Features
- Addition, subtraction, multiplication, division
  - `+`, `-`, `*`, `/`
- Brackets, Power, modulo, floating point, exponent E
  - `()`, `^`, `%`, `.`, `E`
- Trigonometric and logarithmic functions
  - `sin`, `cos`, `tan`, `log`
- Variable assignment
  - `foo = 5`
- Function assignment
  - `def f(x) = 2*x^2 + x - 3`
- Derivatives calculation and storage
  1. Check 'Calculate Derivatives' above the input box
  2. Use `fDeriv` for the derivative of `f`
- Function plotting
  - `plot f()`
  - Infinitely extending plots
  - Panning and zooming
- Helpful errors
- Intuitive UI
- Multi-line input and output

## Tools and Technologies Used
- Interpreter: F#
- GUI: C# and WPF
- Plotting: Oxyplot
