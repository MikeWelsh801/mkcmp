using System.Collections;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;
using Mkcmp.CodeAnalysis.Text;

namespace Mkcmp.CodeAnalysis;

internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void AddRange(DiagnosticBag diagnostics)
    {
        _diagnostics.AddRange(diagnostics._diagnostics);
    }

    private void Report(TextSpan span, string message)
    {
        _diagnostics.Add(new Diagnostic(span, message));
    }

    public void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
    {
        var message = $"The number {text} isn't a valid {type}.";
        Report(span, message);
    }

    public void ReportBadCharacter(int position, char character)
    {
        TextSpan span = new TextSpan(position, 1);
        var message = $"Bad character input: '{character}'.";
        Report(span, message);
    }

    public void ReportUnterminatedString(TextSpan span)
    {
        var message = "Unterminated string literal.";
        Report(span, message);
    }

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
    {
        var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, TypeSymbol operandType)
    {
        var message = $"Unary operator '{operatorText}' is not defined for type '{operandType}'.";
        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string? operatorText, TypeSymbol leftType, TypeSymbol rightType)
    {
        var message = $"Binary operator '{operatorText}' is not defined for types '{leftType}' and '{rightType}'.";
        Report(span, message);
    }

    public void ReportParameterAlreadyDeclared(TextSpan span, string parameterName)
    {
        var message = $"A parameter with the name '{parameterName}' already exists.";
        Report(span, message);
    }

    public void ReportUndefinedName(TextSpan span, string name)
    {
        var message = $"Variable '{name}' doesn't exist.";
        Report(span, message);
    }

    public void ReportUndefinedType(TextSpan span, string type)
    {
        var message = $"Type '{type}' doesn't exist.";
        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert variable of type '{fromType}' to type '{toType}'.";
        Report(span, message);
    }

    public void ReportCannotConvertImplicitly(TextSpan span, TypeSymbol fromType, TypeSymbol toType)
    {
        var message = $"Cannot convert variable of type '{fromType}' to type '{toType}'. An explicit conversion exists. (Are you missing a cast?)";
        Report(span, message);
    }

    public void ReportSymbolAlreadyDeclared(TextSpan span, string name)
    {
        var message = $"'{name}' is already declared.";
        Report(span, message);
    }

    public void ReportCannotAssign(TextSpan span, string name)
    {
        var message = $"Variable '{name}' is read-only and cannot be assigned to.";
        Report(span, message);
    }

    public void ReportUndefinedFunction(TextSpan span, string name)
    {
        var message = $"Function '{name}' doesn't exist.";
        Report(span, message);
    }

    public void ReportWrongArgumentCount(TextSpan span, string name, int expectedCount, int actualCount)
    {
        var message = $"Function '{name}' requires {expectedCount} arguments but was given {actualCount}.";
        Report(span, message);
    }

    public void ReportWrongArgumentType(TextSpan span, string functionName, string parameterName, TypeSymbol expectedType, TypeSymbol actualType)
    {
        var message = $"Function '{functionName}' requires parameter '{parameterName}' to be of type '{expectedType}' but was '{actualType}'.";
        Report(span, message);
    }

    public void ReportExpressionMustHaveValue(TextSpan span)
    {
        var message = $"Expression must have a value.";
        Report(span, message);
    }

    public void ReportInvalidBreakOrContinue(TextSpan span, string text)
    {
        var message = $"Cannot use '{text}' keyword outside of a loop.";
        Report(span, message);
    }

    public void ReportAllPathsMustReturn(TextSpan span)
    {
        var message = "Not all code paths return a value.";
        Report(span, message);
    }

    public void ReportInvalidReturn(TextSpan span)
    {
        var message = "The 'return' keyword can only be used inside of functions.";
        Report(span, message);
    }

    public void ReportInvalidReturnExpression(TextSpan span, string functionName)
    {
        var message = $"Since the function '{functionName}' does not return a value the 'return' keyword cannot be followed by an expression.";
        Report(span, message);
    }

    public void ReportMissingReturnExpression(TextSpan span, TypeSymbol returnType)
    {
        var message = $"An expression of type '{returnType}' expected.";
        Report(span, message);
    }
}
