namespace Mkcmp.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // Statements
    BlockStatement,
    VariableDeclaration,
    IfStatement,
    ExpressionStatement,

    // Epressions
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
}

