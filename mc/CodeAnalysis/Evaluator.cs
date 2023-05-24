namespace Mkcmp.CodeAnalysis
{
    public sealed class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int? Evaluate()
        {
            return EvaluateEpression(_root);
        }

        private int? EvaluateEpression(ExpressionSyntax node)
        {
            // BinaryExpression
            // NumberExpression

            if (node is LiteralExpressionSyntax n)
                return (int?)n.LiteralToken.Value;

            if (node is BinaryExpressionSytax b)
            {
                var left = EvaluateEpression(b.Left);
                var right = EvaluateEpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.PlusToken)
                    return left + right;
                else if (b.OperatorToken.Kind == SyntaxKind.MinusToken)
                    return left - right;
                else if (b.OperatorToken.Kind == SyntaxKind.StarToken)
                    return left * right;
                else if (b.OperatorToken.Kind == SyntaxKind.SlashToken)
                    return left / right;
                else
                    throw new Exception($"Unexpected binary operator {b.OperatorToken.Kind}");
            }

            if (node is ParenthesizedExpressionSyntax p)
                return EvaluateEpression(p.Espression);


            throw new Exception($"Unexpected node {node.Kind}");
        }
    }
}
