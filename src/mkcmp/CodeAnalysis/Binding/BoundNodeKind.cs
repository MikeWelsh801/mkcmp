namespace Mkcmp.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // Statements
    BlockStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    ForStatement,
    LabelStatement,
    GoToStatement,
    ConditionalGoToStatement,
    ExpressionStatement,

    // Expressions
    ErrorExpression,
    LiteralExpression,
    VariableExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    CallExpression,
}

