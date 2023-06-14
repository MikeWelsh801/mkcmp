namespace Mkcmp.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    // tokens
    BadToken,
    EndOfFileToken,
    WhitespaceToken,
    NumberToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    BangToken,
    EqualsToken,
    GreaterToken,
    LessToken,
    AmpersandAmpersandToken,
    PipePipeToken,
    BangEqualsToken,
    EqualsEqualsToken,
    GreaterOrEqualsToken,
    LessOrEqualsToken,
    OpenParenToken,
    CloseParenToken,
    OpenBraceToken,
    CloseBraceToken,
    IdentifierToken,

    // Keywords
    ElseKeyword,
    FalseKeyword,
    IfKeyword,
    LetKeyword,
    TrueKeyword,
    VarKeyword,

    // Nodes
    CompilationUnit,
    ElseClause,

    // Statements
    BlockStatement,
    VariableDeclaration,
    IfStatement,
    ExpressionStatement,

    // Expressions
    LiteralExpression,
    NameExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
}

