using System.Collections;
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

    public void ReportInvalidNumber(TextSpan span, string text, Type type)
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

    public void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
    {
        var message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
        Report(span, message);
    }

    public void ReportUndefinedUnaryOperator(TextSpan span, string? operatorText, Type operandType)
    {
        var message =  $"Unary operator '{operatorText}' is not defined for type {operandType}.";
        Report(span, message);
    }

    public void ReportUndefinedBinaryOperator(TextSpan span, string? operatorText, Type leftType, Type rightType)
    {
        var message = $"Binary operator '{operatorText}' is not defined for types {leftType} and {rightType}.";
        Report(span, message);
    }

    public void ReportUndefinedName(TextSpan span, string name)
    {
        var message = $"Variable '{name}' doesn't exist.";
        Report(span, message);
    }

    public void ReportCannotConvert(TextSpan span, Type fromType, Type toType)
    {
        var message = $"Cannot convert variable of type '{fromType}' to type '{toType}'.";
        Report(span, message);
    }

    public void ReportVariableAlreadyDeclared(TextSpan span, string name)
    {
        var message = $"Variable '{name}' is already declared.";
        Report(span, message);
    }

    public void ReportCannotAssign(TextSpan span, string name)
    {
        var message = $"Variable '{name}' is read-only and cannot be assigned to.";
        Report(span, message);
    }
}



