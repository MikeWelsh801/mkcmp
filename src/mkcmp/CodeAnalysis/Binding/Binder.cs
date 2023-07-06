using System.Collections.Immutable;
using Mkcmp.CodeAnalysis.Symbols;
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
        var expression = binder.BindStatement(syntax.Statement);
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        if (previous != null)
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

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

        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);
            foreach (var v in previous.Variables)
                scope.TryDeclare(v);

            parent = scope;
        }

        return parent;
    }

    public DiagnosticBag Diagnostics => _diagostics;

    private BoundStatement BindStatement(StatementSyntax syntax)
    {
        return syntax.Kind switch
        {
            SyntaxKind.BlockStatement =>
                BindBlockStatement((BlockStatementSyntax)syntax),
            SyntaxKind.VariableDeclaration =>
                BindVariableDeclaration((VariableDeclarationSyntax)syntax),
            SyntaxKind.IfStatement =>
                BindIfStatement((IfStatementSyntax)syntax),
            SyntaxKind.WhileStatement =>
                BindWhileStatement((WhileStatementSyntax)syntax),
            SyntaxKind.ForStatement =>
                BindForStatement((ForStatementSyntax)syntax),
            SyntaxKind.ExpressionStatement =>
                BindExpressionStatement((ExpressionStatementSyntax)syntax),
            _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
        };
    }

    private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
    {
        var statements = ImmutableArray.CreateBuilder<BoundStatement>();
        _scope = new BoundScope(_scope);

        foreach (var statementSyntax in syntax.Statements)
        {
            var statement = BindStatement(statementSyntax);
            statements.Add(statement);
        }

        _scope = _scope.Parent;

        return new BoundBlockStatement(statements.ToImmutable());
    }

    private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
    {
        var name = syntax.Identifier.Text;
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var initializer = BindExpression(syntax.Initializer);
        var variable = new VariableSymbol(name, isReadOnly, initializer.Type);

        if (!_scope.TryDeclare(variable))
            _diagostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

        return new BoundVariableDeclaration(variable, initializer);
    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, typeof(bool));
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, typeof(bool));
        var body = BindStatement(syntax.Body);
        return new BoundWhileStatement(condition, body);
    }

    private BoundStatement BindForStatement(ForStatementSyntax syntax)
    {
        var lowerBound = BindExpression(syntax.LowerBound, typeof(int));
        var rangeKeyword = syntax.RangeKeyword;
        var upperBound = BindExpression(syntax.UpperBound, typeof(int));

        _scope = new BoundScope(_scope);

        var name = syntax.Identifier.Text;
        var variable = new VariableSymbol(name, true, typeof(int));
        if (!_scope.TryDeclare(variable))
            _diagostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);

        var body = BindStatement(syntax.Body);
        _scope = _scope.Parent;

        return new BoundForStatement(variable, lowerBound, rangeKeyword, upperBound, body);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
    {
        var expression = BindExpression(syntax.Expression);
        return new BoundExpressionStatement(expression);
    }

    public BoundExpression BindExpression(ExpressionSyntax syntax, Type targetType)
    {
        var result = BindExpression(syntax);
        if (result.Type != targetType)
            _diagostics.ReportCannotConvert(syntax.Span, result.Type, targetType);
        return result;
    }

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
        if (string.IsNullOrEmpty(name))
        {
            // A token was inserted by the parser. We already reported an error,
            // so we can just return an errror expression.
            return new BoundLiteralExpression(0);
        }

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

        if (!_scope.TryLookup(name, out var variable))
        {
            _diagostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return boundExpression;
        }

        if (variable.IsReadOnly)
            _diagostics.ReportCannotAssign(syntax.EqualsToken.Span, name);


        if (boundExpression.Type != variable.Type)
        {
            _diagostics.ReportCannotConvert(syntax.Expression.Span, boundExpression.Type, variable.Type);
            return boundExpression;
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
