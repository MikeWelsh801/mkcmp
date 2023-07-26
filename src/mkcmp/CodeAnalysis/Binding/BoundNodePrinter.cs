using System.CodeDom.Compiler;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;
using Mkcmp.IO;

namespace Mkcmp.CodeAnalysis.Binding;

internal static class BoundNodePrinter
{
    public static void WriteTo(this BoundNode node, TextWriter writer)
    {
        if (writer is IndentedTextWriter iw)
            WriteTo(node, iw);
        else
            WriteTo(node, new IndentedTextWriter(writer));
    }

    public static void WriteTo(this BoundNode node, IndentedTextWriter writer)
    {
        switch (node.Kind)
        {
            case BoundNodeKind.BlockStatement:
                WriteBlockStatement((BoundBlockStatement)node, writer);
                break;
            case BoundNodeKind.VariableDeclaration:
                WriteVariableDeclaration((BoundVariableDeclaration)node, writer);
                break;
            case BoundNodeKind.IfStatement:
                WriteIfStatement((BoundIfStatement)node, writer);
                break;
            case BoundNodeKind.WhileStatement:
                WriteWhileStatement((BoundWhileStatement)node, writer);
                break;
            case BoundNodeKind.DoWhileStatement:
                WriteDoWhileStatement((BoundDoWhileStatement)node, writer);
                break;
            case BoundNodeKind.ForStatement:
                WriteForStatement((BoundForStatement)node, writer);
                break;
            case BoundNodeKind.LabelStatement:
                WriteLabelStatement((BoundLabelStatement)node, writer);
                break;
            case BoundNodeKind.GoToStatement:
                WriteGoToStatement((BoundGoToStatement)node, writer);
                break;
            case BoundNodeKind.ConditionalGoToStatement:
                WriteConditionalGoToStatement((BoundConditionalGoToStatement)node, writer);
                break;
            case BoundNodeKind.ExpressionStatement:
                WriteExpressionStatement((BoundExpressionStatement)node, writer);
                break;
            case BoundNodeKind.ErrorExpression:
                WriteErrorExpression((BoundErrorExpression)node, writer);
                break;
            case BoundNodeKind.LiteralExpression:
                WriteLiteralExpression((BoundLiteralExpression)node, writer);
                break;
            case BoundNodeKind.VariableExpression:
                WriteVariableExpression((BoundVariableExpression)node, writer);
                break;
            case BoundNodeKind.AssignmentExpression:
                WriteAssignmentExpression((BoundAssignmentExpression)node, writer);
                break;
            case BoundNodeKind.UnaryExpression:
                WriteUnaryExpression((BoundUnaryExpression)node, writer);
                break;
            case BoundNodeKind.BinaryExpression:
                WriteBinaryExpression((BoundBinaryExpression)node, writer);
                break;
            case BoundNodeKind.CallExpression:
                WriteCallExpression((BoundCallExpression)node, writer);
                break;
            case BoundNodeKind.ConversionExpression:
                WriteConversionExpression((BoundConversionExpression)node, writer);
                break;
            default:
                throw new Exception($"Unexpected node {node.Kind}.");
        }
    }

    private static void WriteNestedStatement(this IndentedTextWriter writer, BoundStatement node)
    {
        var needsIndentation = !(node is BoundBlockStatement);

        if (needsIndentation)
            writer.Indent++;
        node.WriteTo(writer);
        if (needsIndentation)
            writer.Indent--;
    }

    private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, BoundExpression expression)
    {
        if (expression is BoundUnaryExpression unary)
            writer.WriteNestedExpression(parentPrecedence, SyntaxFacts.GetUnaryOperatorPrecedence(unary.Op.SyntaxKind), unary);
        else if (expression is BoundBinaryExpression binary)
            writer.WriteNestedExpression(parentPrecedence, SyntaxFacts.GetBinaryOperatorPrecedence(binary.Op.SyntaxKind), binary);
        else
            expression.WriteTo(writer);
    }

    private static void WriteNestedExpression(this IndentedTextWriter writer, int parentPrecedence, int currentPrecedence, BoundExpression expression)
    {
        bool needsParenthesis = parentPrecedence >= currentPrecedence;
        if (needsParenthesis)
            writer.WritePunctuation("(");
        expression.WriteTo(writer);
        if (needsParenthesis)
            writer.WritePunctuation(")");
    }

    private static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer)
    {
        writer.Write("{");
        writer.WriteLine();
        writer.Indent++;

        foreach (var statement in node.Statements)
            statement.WriteTo(writer);

        writer.Indent--;
        writer.Write("}");
        writer.WriteLine();
    }

    private static void WriteVariableDeclaration(BoundVariableDeclaration node, IndentedTextWriter writer)
    {
        writer.WriteKeyword(node.Variable.IsReadOnly ? "let " : "var ");
        writer.WriteIdentifier(node.Variable.Name);
        writer.WriteOperator(" = ");
        node.Initializer.WriteTo(writer);
        writer.WriteLine();
    }


    private static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("if ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();

        writer.WriteNestedStatement(node.ThenStatement);

        if (node.ElseStatement != null)
        {
            writer.WriteKeyword("else");
            writer.WriteLine();
            writer.WriteNestedStatement(node.ElseStatement);
        }
    }

    private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("while ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatement(node.Body);

    }

    private static void WriteDoWhileStatement(BoundDoWhileStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("do ");
        writer.WriteLine();
        writer.WriteNestedStatement(node.Body);
        writer.WriteKeyword("while ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();

    }

    private static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("for ");
        writer.WriteIdentifier(node.Variable.Name);
        writer.WriteOperator(" in ");
        node.LowerBound.WriteTo(writer);
        writer.WriteOperator(node.RangeKeyword.Text);
        node.UpperBound.WriteTo(writer);
        writer.WriteLine();
        writer.WriteNestedStatement(node.Body);
    }

    private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer)
    {
        var unindent = writer.Indent > 0;
        if (unindent)
            writer.Indent--;

        writer.WriteFun(node.Label.Name);
        writer.WritePunctuation(":");
        writer.WriteLine();

        if (unindent)
            writer.Indent++;
    }

    private static void WriteGoToStatement(BoundGoToStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(node.Label.Name);
        writer.WriteLine();
    }

    private static void WriteConditionalGoToStatement(BoundConditionalGoToStatement node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("goto ");
        writer.WriteIdentifier(node.Label.Name);
        writer.WriteKeyword(node.JumpIfTrue ? " if " : " unless ");
        node.Condition.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
    {
        node.Expression.WriteTo(writer);
        writer.WriteLine();
    }

    private static void WriteErrorExpression(BoundErrorExpression node, IndentedTextWriter writer)
    {
        writer.WriteKeyword("?");
    }

    private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer)
    {
        string value = node.Value.ToString();

        if (node.Type == TypeSymbol.Bool)
            writer.WriteBool(value);
        else if (node.Type == TypeSymbol.Int)
            writer.WriteNumber(value);
        else if (node.Type == TypeSymbol.String)
        {
            value = "\"" + value.Replace("\"", "\"\"") + "\"";
            writer.WriteString(value);
        }
        else
            throw new Exception($"Unexpected type {node.Type}");
    }

    private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer)
    {
        writer.WriteIdentifier(node.Variable.Name);
    }

    private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
    {
        writer.WriteIdentifier(node.Variable.Name);
        writer.WriteOperator(" = ");
        node.Expression.WriteTo(writer);
    }

    private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer)
    {
        string op = SyntaxFacts.GetText(node.Op.SyntaxKind);
        int precedence = SyntaxFacts.GetUnaryOperatorPrecedence(node.Op.SyntaxKind);

        writer.WriteOperator(op);
        writer.WriteNestedExpression(precedence, node.Operand);
    }

    private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer)
    {
        var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
        int precedence = SyntaxFacts.GetBinaryOperatorPrecedence(node.Op.SyntaxKind);

        writer.WriteNestedExpression(precedence, node.Left);
        writer.Write(" ");
        writer.WriteOperator(op);
        writer.Write(" ");
        writer.WriteNestedExpression(precedence, node.Right);
    }

    private static void WriteCallExpression(BoundCallExpression node, IndentedTextWriter writer)
    {
        writer.WriteIdentifier(node.Function.Name);
        writer.WritePunctuation("(");

        var isFirst = true;
        foreach (var arg in node.Arguments)
        {
            if (isFirst)
                isFirst = false;
            else
                writer.WritePunctuation(", ");

            arg.WriteTo(writer);
        }
        writer.WritePunctuation(")");
    }

    private static void WriteConversionExpression(BoundConversionExpression node, IndentedTextWriter writer)
    {
        writer.WriteIdentifier(node.Type.Name);
        writer.WritePunctuation("(");
        node.Expression.WriteTo(writer);
        writer.WritePunctuation(")");

    }
}