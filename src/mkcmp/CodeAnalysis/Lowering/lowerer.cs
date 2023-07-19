using System.Collections.Immutable;
using Mkcmp.CodeAnalysis.Binding;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;

namespace Mkcmp.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private int _labelCount;

    private Lowerer()
    {
    }

    private BoundLabel GenerateLabel()
    {
        var name = $"Label{++_labelCount}";
        return new BoundLabel(name);
    }

    public static BoundBlockStatement Lower(BoundStatement statement)
    {
        var lowerer = new Lowerer();
        var result = lowerer.RewriteStatement(statement);
        return Flatten(result);
    }

    private static BoundBlockStatement Flatten(BoundStatement statement)
    {
        var builder = ImmutableArray.CreateBuilder<BoundStatement>();
        var stack = new Stack<BoundStatement>();
        stack.Push(statement);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current is BoundBlockStatement block)
            {
                foreach (var s in block.Statements.Reverse())
                    stack.Push(s);
            }
            else
            {
                builder.Add(current);
            }
        }

        return new BoundBlockStatement(builder.ToImmutable());
    }

    protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
    {
        if (node.ElseStatement == null)
        {
            var endLabel = GenerateLabel();
            var gotoFalse = new BoundConditionalGoToStatement(endLabel, node.Condition, false);
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
                gotoFalse,
                node.ThenStatement,
                endLabelStatement));

            return RewriteStatement(result);
        }
        else
        {
            var endLabel = GenerateLabel();
            var elseLabel = GenerateLabel();

            var gotoFalse = new BoundConditionalGoToStatement(elseLabel, node.Condition, false);
            var gotoEndStatement = new BoundGoToStatement(endLabel);
            var elseLabelStatement = new BoundLabelStatement(elseLabel);
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var result = new BoundBlockStatement(
                ImmutableArray.Create<BoundStatement>(
                    gotoFalse,
                    node.ThenStatement,
                    gotoEndStatement,
                    elseLabelStatement,
                    node.ElseStatement,
                    endLabelStatement
                )
            );
            return RewriteStatement(result);
        }
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
    {
        var continueLabel = GenerateLabel();
        var checkLabel = GenerateLabel();

        var gotoTrue = new BoundConditionalGoToStatement(continueLabel, node.Condition);
        var gotoCheck = new BoundGoToStatement(checkLabel);
        var continueLabelStatement = new BoundLabelStatement(continueLabel);
        var checkLabelStatement = new BoundLabelStatement(checkLabel);

        var result = new BoundBlockStatement(
            ImmutableArray.Create<BoundStatement>(
                gotoCheck,
                continueLabelStatement,
                node.Body,
                checkLabelStatement,
                gotoTrue
            )
        );
        return RewriteStatement(result);
    }

    protected override BoundStatement RewriteDoWhileStatement(BoundDoWhileStatement node)
    {
        var continueLabel = GenerateLabel();

        var gotoTrue = new BoundConditionalGoToStatement(continueLabel, node.Condition);
        var continueLabelStatement = new BoundLabelStatement(continueLabel);

        var result = new BoundBlockStatement(
            ImmutableArray.Create<BoundStatement>(
                continueLabelStatement,
                node.Body,
                gotoTrue
            )
        );
        return RewriteStatement(result);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement node)
    {
        var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.LowerBound);
        var variableExpression = new BoundVariableExpression(node.Variable);
        var upperBoundSymbol = new LocalVariableSymbol("upperBound", true, TypeSymbol.Int);
        var upperBoundDeclaration = new BoundVariableDeclaration(upperBoundSymbol, node.UpperBound);
        var op = node.RangeKeyword.Kind == SyntaxKind.ToKeyword ? SyntaxKind.LessToken : SyntaxKind.LessOrEqualsToken;

        var condition = new BoundBinaryExpression(
            variableExpression,
            BoundBinaryOperator.Bind(op, TypeSymbol.Int, TypeSymbol.Int),
            new BoundVariableExpression(upperBoundSymbol));

        var increment = new BoundExpressionStatement(
            new BoundAssignmentExpression(
                node.Variable,
                new BoundBinaryExpression(
                    variableExpression,
                    BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                    new BoundLiteralExpression(1))));

        var whileBody = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(node.Body, increment));
        var whileStatement = new BoundWhileStatement(condition, whileBody);

        var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(
            variableDeclaration,
            upperBoundDeclaration,
            whileStatement));

        return RewriteStatement(result);
    }

}
