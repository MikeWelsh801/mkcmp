using System.Collections.Immutable;
using Mkcmp.CodeAnalysis.Lowering;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;
using Mkcmp.CodeAnalysis.Text;

namespace Mkcmp.CodeAnalysis.Binding;

internal sealed class Binder
{
    private readonly DiagnosticBag _diagostics = new();
    private readonly FunctionSymbol _function;
    private BoundScope _scope;

    public Binder(BoundScope parent, FunctionSymbol function)
    {
        _scope = new(parent);
        _function = function;

        if (function != null)
        {
            foreach (var p in function.Parameters)
                _scope.TryDeclareVariable(p);
        }
    }

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
    {
        var parentScope = CreateParentScopes(previous);
        var binder = new Binder(parentScope, function: null);

        foreach (var function in syntax.Members.OfType<FunctionDeclarationSyntax>())
            binder.BindFunctionDeclaration(function);

        var statementBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach (var globasStatement in syntax.Members.OfType<GlobalStatementSyntax>())
        {
            var s = binder.BindStatement(globasStatement.Statement);
            statementBuilder.Add(s);
        }

        var statement = new BoundBlockStatement(statementBuilder.ToImmutable());
        var functions = binder._scope.GetDeclaredFunctions();
        var variables = binder._scope.GetDeclaredVariables();
        var diagnostics = binder.Diagnostics.ToImmutableArray();

        if (previous != null)
            diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

        return new BoundGlobalScope(previous, diagnostics, functions, variables, statement);
    }

    public static BoundProgram BindProgram(BoundGlobalScope globalScope)
    {
        var parentScope = CreateParentScopes(globalScope);

        var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        var diagnostics = new DiagnosticBag();

        var scope = globalScope;
        while (scope != null)
        {
            foreach (var function in scope.Functions)
            {
                var binder = new Binder(parentScope, function);
                var body = binder.BindStatement(function.Declaration.Body);
                var loweredBody = Lowerer.Lower(body);
                functionBodies.Add(function, loweredBody);

                diagnostics.AddRange(binder.Diagnostics);
            }
            scope = scope.Previous;
        }

        return new BoundProgram(globalScope, diagnostics, functionBodies.ToImmutable());
    }

    private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        var seenParameterNames = new HashSet<string>();

        foreach (var parameterSyntax in syntax.Parameters)
        {
            var parameterName = parameterSyntax.Identifier.Text;
            var parameterType = BindTypeClause(parameterSyntax.Type);
            if (!seenParameterNames.Add(parameterName))
            {
                _diagostics.ReportParameterAlreadyDeclared(parameterSyntax.Span, parameterName);
            }
            else
            {
                var parameter = new ParameterSymbol(parameterName, parameterType);
                parameters.Add(parameter);
            }
        }

        var type = BindTypeClause(syntax.Type) ?? TypeSymbol.Void;
        if (type != TypeSymbol.Void)
            _diagostics.XXX_ReportFunctionsAreUnsuported(syntax.Type.Span);

        var function = new FunctionSymbol(syntax.Identifier.Text, parameters.ToImmutable(), type, syntax);
        if (!_scope.TryDeclareFunction(function))
            _diagostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, function.Name);
    }

    private static BoundScope CreateParentScopes(BoundGlobalScope previous)
    {
        var stack = new Stack<BoundGlobalScope>();
        while (previous != null)
        {
            stack.Push(previous);
            previous = previous.Previous;
        }

        var parent = CreateRootScope();


        while (stack.Count > 0)
        {
            previous = stack.Pop();
            var scope = new BoundScope(parent);

            foreach (var f in previous.Functions)
                scope.TryDeclareFunction(f);

            foreach (var v in previous.Variables)
                scope.TryDeclareVariable(v);

            parent = scope;
        }

        return parent;
    }

    private static BoundScope CreateRootScope()
    {
        var result = new BoundScope(null);

        foreach (var f in BuiltinFunctions.GetAll())
            result.TryDeclareFunction(f);
        return result;
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
            SyntaxKind.DoWhileStatement =>
                BindDoWhileStatement((DoWhileStatementSyntax)syntax),
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
        var isReadOnly = syntax.Keyword.Kind == SyntaxKind.LetKeyword;
        var type = BindTypeClause(syntax.TypeClause);
        var initializer = BindExpression(syntax.Initializer);
        var variableType = type ?? initializer.Type;
        var variable = BindVariable(syntax.Identifier, isReadOnly, variableType);
        var convertedInitializer = BindConversion(syntax.Initializer.Span, initializer, variableType);

        return new BoundVariableDeclaration(variable, convertedInitializer);
    }

    private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
    {
        if (syntax == null)
            return null;

        var type = LookupType(syntax.Identifier.Text);
        if (type == null)
            _diagostics.ReportUndefinedType(syntax.Identifier.Span, syntax.Identifier.Text);

        return type;

    }

    private BoundStatement BindIfStatement(IfStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var thenStatement = BindStatement(syntax.ThenStatement);
        var elseStatement = syntax.ElseClause == null ? null : BindStatement(syntax.ElseClause.ElseStatement);
        return new BoundIfStatement(condition, thenStatement, elseStatement);
    }

    private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
    {
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        var body = BindStatement(syntax.Body);
        return new BoundWhileStatement(condition, body);
    }

    private BoundStatement BindDoWhileStatement(DoWhileStatementSyntax syntax)
    {
        var body = BindStatement(syntax.Body);
        var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
        return new BoundDoWhileStatement(body, condition);
    }

    private BoundStatement BindForStatement(ForStatementSyntax syntax)
    {
        var lowerBound = BindExpression(syntax.LowerBound, TypeSymbol.Int);
        var rangeKeyword = syntax.RangeKeyword;
        var upperBound = BindExpression(syntax.UpperBound, TypeSymbol.Int);

        _scope = new BoundScope(_scope);

        var variable = BindVariable(syntax.Identifier, isReadOnly: true, TypeSymbol.Int);
        var body = BindStatement(syntax.Body);

        _scope = _scope.Parent;

        return new BoundForStatement(variable, lowerBound, rangeKeyword, upperBound, body);
    }

    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
    {
        var expression = BindExpression(syntax.Expression, canBeVoid: true);
        return new BoundExpressionStatement(expression);
    }

    private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol targetType)
    {
        return BindConversion(syntax, targetType);
    }

    private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
    {
        var result = BindExpressionInternal(syntax);
        if (!canBeVoid && result.Type == TypeSymbol.Void)
        {
            _diagostics.ReportExpressionMustHaveValue(syntax.Span);
            return new BoundErrorExpression();
        }

        return result;
    }

    private BoundExpression BindExpressionInternal(ExpressionSyntax syntax)
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
            SyntaxKind.CallExpression =>
                BindCallExpression((CallExpressionSyntax)syntax),
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
        if (syntax.IdentifierToken.IsMissing)
        {
            // A token was inserted by the parser. We already reported an error,
            // so we can just return an errror expression.
            return new BoundErrorExpression();
        }

        if (!_scope.TryLookupVariable(name, out var variable))
        {
            _diagostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return new BoundErrorExpression();
        }

        return new BoundVariableExpression(variable);
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        var name = syntax.IdentifierToken.Text;
        var boundExpression = BindExpression(syntax.Expression);

        if (!_scope.TryLookupVariable(name, out var variable))
        {
            _diagostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
            return boundExpression;
        }

        if (variable.IsReadOnly)
            _diagostics.ReportCannotAssign(syntax.EqualsToken.Span, name);

        var convertedExpression = BindConversion(syntax.Expression.Span, boundExpression, variable.Type);

        return new BoundAssignmentExpression(variable, convertedExpression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
    {
        var boundOperand = BindExpression(syntax.Operand);

        if (boundOperand.Type == TypeSymbol.Error)
            return new BoundErrorExpression();

        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

        if (boundOperator == null)
        {
            _diagostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
            return new BoundErrorExpression();
        }

        return new BoundUnaryExpression(boundOperator, boundOperand);
    }

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
    {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);

        if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
            return new BoundErrorExpression();

        var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

        if (boundOperator == null)
        {
            _diagostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
            return new BoundErrorExpression();
        }

        return new BoundBinaryExpression(boundLeft, boundOperator, boundRight);
    }

    private BoundExpression BindCallExpression(CallExpressionSyntax syntax)
    {
        if (syntax.Arguments.Count == 1 && LookupType(syntax.Identifier.Text) is TypeSymbol type)
            return BindConversion(syntax.Arguments[0], type, allowExplicit: true);

        var boundArguments = ImmutableArray.CreateBuilder<BoundExpression>();

        foreach (var argument in syntax.Arguments)
        {
            var boundArgument = BindExpression(argument);
            boundArguments.Add(boundArgument);
        }

        if (!_scope.TryLookupFunction(syntax.Identifier.Text, out var function))
        {
            _diagostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
            return new BoundErrorExpression();
        }

        if (syntax.Arguments.Count != function.Parameters.Length)
        {
            _diagostics.ReportWrongArgumentCount(syntax.Span, function.Name, function.Parameters.Length, syntax.Arguments.Count);
            return new BoundErrorExpression();
        }

        for (int i = 0; i < syntax.Arguments.Count; i++)
        {
            var argument = boundArguments[i];
            var parameter = function.Parameters[i];

            if (argument.Type != parameter.Type)
            {
                _diagostics.ReportWrongArgumentType(syntax.Arguments[i].Span, function.Name, parameter.Name, parameter.Type, argument.Type);
                return new BoundErrorExpression();
            }
        }

        return new BoundCallExpression(function, boundArguments.ToImmutable());
    }

    private BoundExpression BindConversion(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
    {
        var expression = BindExpression(syntax);
        return BindConversion(syntax.Span, expression, type, allowExplicit);
    }

    private BoundExpression BindConversion(TextSpan diagnosticSpan, BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
    {
        var conversion = Conversion.Classify(expression.Type, type);

        if (!conversion.Exists)
        {
            if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                _diagostics.ReportCannotConvert(diagnosticSpan, expression.Type, type);

            return new BoundErrorExpression();
        }

        if (!allowExplicit && conversion.IsExplicit)
        {
            _diagostics.ReportCannotConvertImplicitly(diagnosticSpan, expression.Type, type);
        }

        if (conversion.IsIdentity)
            return expression;

        return new BoundConversionExpression(type, expression);
    }

    private VariableSymbol BindVariable(SyntaxToken identifier, bool isReadOnly, TypeSymbol type)
    {
        var name = identifier.Text ?? "?";
        var declare = !identifier.IsMissing;
        var variable = _function == null
            ? (VariableSymbol)new GlobalVariableSymbol(name, isReadOnly, type)
            : new LocalVariableSymbol(name, isReadOnly, type);

        if (declare && !_scope.TryDeclareVariable(variable))
            _diagostics.ReportSymbolAlreadyDeclared(identifier.Span, name);

        return variable;
    }

    private TypeSymbol LookupType(string name)
    {
        return name switch
        {
            "bool" => TypeSymbol.Bool,
            "int" => TypeSymbol.Int,
            "string" => TypeSymbol.String,
            _ => null,
        };
    }
}
