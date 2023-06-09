using System.Collections.Immutable;
using Mkcmp.CodeAnalysis.Syntax;

namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class Binder
{
    private readonly DiagnosticBag _diagostics = new();

    private BoundScope _scope;

    public Binder(BoundScope parent)
    {
        _scope = new(parent);
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
    {
        var parentScope = CreateParentScopes(previous);
        var binder = new Binder(parentScope);
        var expression = binder.BindExpression(syntax.Expression);
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();
        return new BoundGlobalScope(previous, diagnostics, variables, expression);
    }

    private static BoundScope CreateParentScopes(BoundGlobalScope previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous != null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }
        BoundScope parent = null;

        while(stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            foreach(var v in previous.Variables)
                scope.TryDeclare(v);

            parent = scope;
        }

        return parent;
    }

    public DiagnosticBag Diagnostics => _diagostics;

    public BoundExpression BindExpression(ExpressionSyntax syntax)
    {
        return syntax.Kind switch
        {
            SyntaxKind.ParenthesizedExpression =>
                BindParenthesizedExpression((ParenthesizedExpressionSyntax)syntax),
            SyntaxKind.LiteralExpression =>
                BindLiteralExpression((LiteralExpressionSyntax)syntax),
            SyntaxKind.NameExpression =>
                BindNameExpression((NameExpressionSyntax)syntax),
            SyntaxKind.AssignmentExpression =>
                BindAssignmentExpression((AssignmentExpressionSyntax)syntax),
            SyntaxKind.UnaryExpression =>
                BindUnaryExpression((UnaryExpressionSyntax)syntax),
            SyntaxKind.BinaryExpression =>
                BindBinaryExpression((BinaryExpressionSyntax)syntax),
            _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
        };
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        return BindExpression(syntax.Expression);
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
    {
        var value = syntax.Value ?? 0;
        return new BoundLiteralExpression(value);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;

        if (!_scope.TryLookup(name, out var variable))
        {
            _diagostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return new BoundLiteralExpression(0);
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        var boundExpression = BindExpression(syntax.Expression);
        var variable = new VariableSymbol(name, boundExpression.Type);

        if (!_scope.TryDeclare(variable))
        {
            _diagostics.ReportVariableAlreadyDeclared(syntax.IdentifierToken.Span, name);
        }

        return new BoundAssignmentExpression(variable, boundExpression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var boundOperand = BindExpression(syntax.Operand);
        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

        if (boundOperator == null)
        {
            _diagostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
            return boundOperand;
        }

        return new BoundUnaryExpression(boundOperator, boundOperand);

    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);
        var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

        if (boundOperator == null)
        {
            _diagostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
            return boundLeft;
        }

        return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
    }
}
