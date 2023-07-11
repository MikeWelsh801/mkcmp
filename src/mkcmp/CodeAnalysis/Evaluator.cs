using Mkcmp.CodeAnalysis.Binding;
using Mkcmp.CodeAnalysis.Symbols;

namespace Mkcmp.CodeAnalysis;

internal sealed class Evaluator
{
    private readonly BoundBlockStatement _root;
    private readonly Dictionary<VariableSymbol, object> _variables;

    private object _lastValue;

    public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
    {
        _root = root;
        _variables = variables;
    }

    public object Evaluate()
    {
        var labelToIndex = new Dictionary<BoundLabel, int>();

        for (int i = 0; i < _root.Statements.Length; i++)
        {
            if (_root.Statements[i] is BoundLabelStatement l)
                labelToIndex.Add(l.Label, i + 1);
        }

        var index = 0;
        while (index < _root.Statements.Length)
        {
            var s = _root.Statements[index];

            switch (s.Kind)
            {
                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration((BoundVariableDeclaration)s);
                    index++;
                    break;
                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement((BoundExpressionStatement)s);
                    index++;
                    break;
                case BoundNodeKind.GoToStatement:
                    var gs = (BoundGoToStatement)s;
                    index = labelToIndex[gs.Label];
                    break;
                case BoundNodeKind.ConditionalGoToStatement:
                    var cgs = (BoundConditionalGoToStatement)s;
                    var condition = (bool)EvaluateExpression(cgs.Condition);

                    if (condition == cgs.JumpIfTrue)
                        index = labelToIndex[cgs.Label];
                    else
                        index++;
                    break;
                case BoundNodeKind.LabelStatement:
                    index++;
                    break;
                default:
                    throw new Exception($"Unexpected node {s.Kind}");
            }
        }

        return _lastValue;
    }

    private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
    {
        var value = EvaluateExpression(node.Initializer);
        _variables[node.Variable] = value;
        _lastValue = value;
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement node)
    {
        _lastValue = EvaluateExpression(node.Expression);
    }

    private object EvaluateExpression(BoundExpression node)
    {
        return node.Kind switch
        {
            BoundNodeKind.LiteralExpression =>
                EvaluateLiteralExpression((BoundLiteralExpression)node),
            BoundNodeKind.VariableExpression =>
                EvaluateVariableExpression((BoundVariableExpression)node),
            BoundNodeKind.AssignmentExpression =>
                EvaluateAssignmentExpression((BoundAssignmentExpression)node),
            BoundNodeKind.UnaryExpression =>
                EvaluateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression =>
                EvaluateBinaryExpression((BoundBinaryExpression)node),
            BoundNodeKind.CallExpression =>
                EvaluateCallExpression((BoundCallExpression)node),
            _ => throw new Exception($"Unexpected node {node.Kind}")
        };
    }

    private object EvaluateLiteralExpression(BoundLiteralExpression n)
    {
        return n.Value;
    }

    private object EvaluateVariableExpression(BoundVariableExpression v)
    {
        return _variables[v.Variable];
    }

    private object EvaluateAssignmentExpression(BoundAssignmentExpression a)
    {
        var value = EvaluateExpression(a.Expression);
        _variables[a.Variable] = value;
        return value;
    }

    private object EvaluateUnaryExpression(BoundUnaryExpression u)
    {
        var operand = EvaluateExpression(u.Operand);

        return u.Op.Kind switch
        {
            BoundUnaryOperatorKind.Identity => (int)operand,
            BoundUnaryOperatorKind.Negation => -(int)operand,
            BoundUnaryOperatorKind.LogicalNegation => !(bool)operand,
            BoundUnaryOperatorKind.OnesComplement => ~(int)operand,
            _ => throw new Exception($"Unexpected unary operator {u.Op}")
        };
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        return b.Op.Kind switch
        {
            BoundBinaryOperatorKind.Addition when (b.Type == TypeSymbol.Int) => (int)left + (int)right,
            BoundBinaryOperatorKind.Addition when (b.Type == TypeSymbol.String) => (string)left + (string)right,
            BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
            BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
            BoundBinaryOperatorKind.Division => (int)left / (int)right,
            BoundBinaryOperatorKind.BitwiseAnd when (b.Type == TypeSymbol.Int) => (int)left & (int)right,
            BoundBinaryOperatorKind.BitwiseAnd when (b.Type == TypeSymbol.Bool) => (bool)left & (bool)right,
            BoundBinaryOperatorKind.BitwiseOr when (b.Type == TypeSymbol.Int) => (int)left | (int)right,
            BoundBinaryOperatorKind.BitwiseOr when (b.Type == TypeSymbol.Bool) => (bool)left | (bool)right,
            BoundBinaryOperatorKind.BitwiseXor when (b.Type == TypeSymbol.Int) => (int)left ^ (int)right,
            BoundBinaryOperatorKind.BitwiseXor when (b.Type == TypeSymbol.Bool) => (bool)left ^ (bool)right,
            BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
            BoundBinaryOperatorKind.Equals => Equals(left, right),
            BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
            BoundBinaryOperatorKind.Less => (int)left < (int)right,
            BoundBinaryOperatorKind.LessOrEqual => (int)left <= (int)right,
            BoundBinaryOperatorKind.Greater => (int)left > (int)right,
            BoundBinaryOperatorKind.GreaterOrEqual => (int)left >= (int)right,
            _ => throw new Exception($"Unexpected binary operator {b.Op}")
        };
    }

    private object EvaluateCallExpression(BoundCallExpression node)
    {
        if (node.Function == BuiltinFunctions.Input)
        {
            return Console.ReadLine();
        }
        else if (node.Function == BuiltinFunctions.Print)
        {
            var message = (string)EvaluateExpression(node.Arguments[0]);
            Console.WriteLine(message);
            return null;
        }
        else
        {
            throw new Exception($"Unexpected function '{node.Function.Name}'.");
        }

    }
}
