using Mkcmp.CodeAnalysis.Binding;

namespace Mkcmp.CodeAnalysis;

internal sealed class Evaluator
{
    private readonly BoundExpression _root;
    private readonly Dictionary<VariableSymbol, object> _variables;

    public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
    {
        _root = root;
        _variables = variables;
    }

    public object Evaluate()
    {
        return EvaluateExpression(_root);
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
                EvalueateUnaryExpression((BoundUnaryExpression)node),
            BoundNodeKind.BinaryExpression =>
                EvaluateBinaryExpression((BoundBinaryExpression)node),
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

    private object EvalueateUnaryExpression(BoundUnaryExpression u)
    {
        var operand = EvaluateExpression(u.Operand);

        return u.Op.Kind switch
        {
            BoundUnaryOperatorKind.Identity => (int)operand,
            BoundUnaryOperatorKind.Negation => -(int)operand,
            BoundUnaryOperatorKind.LogicalNegation => !(bool)operand,
            _ => throw new Exception($"Unexpected unary operator {u.Op}")
        };
    }

    private object EvaluateBinaryExpression(BoundBinaryExpression b)
    {
        var left = EvaluateExpression(b.Left);
        var right = EvaluateExpression(b.Right);

        return b.Op.Kind switch
        {
            BoundBinaryOperatorKind.Addition => (int)left + (int)right,
            BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
            BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
            BoundBinaryOperatorKind.Division => (int)left / (int)right,
            BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
            BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
            BoundBinaryOperatorKind.Equals => Equals(left, right),
            BoundBinaryOperatorKind.NotEquals => !Equals(left, right),
            _ => throw new Exception($"Unexpected binary operator {b.Op}")
        };
    }
}
