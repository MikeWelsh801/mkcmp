namespace Mkcmp.CodeAnalysis.Syntax;

public enum SyntaxKind
{
    // tokens
    BadToken,
    EndOfFileToken,
    WhitespaceToken,
    NumberToken,
    StringToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    BangToken,
    EqualsToken,
    GreaterToken,
    LessToken,
    TildeToken,
    HatToken,
    AmpersandToken,
    AmpersandAmpersandToken,
    PipeToken,
    PipePipeToken,
    BangEqualsToken,
    EqualsEqualsToken,
    GreaterOrEqualsToken,
    LessOrEqualsToken,
    OpenParenToken,
    CloseParenToken,
    OpenBraceToken,
    CloseBraceToken,
    CommaToken,
    IdentifierToken,

    // Keywords
    ElseKeyword,
    FalseKeyword,
    ForKeyword,
    IfKeyword,
    InKeyword,
    LetKeyword,
    TrueKeyword,
    VarKeyword,
    WhileKeyword,
    DoKeyword,
    ThroughKeyword,
    ToKeyword,

    // Nodes
    CompilationUnit,
    ElseClause,

    // Statements
    BlockStatement,
    VariableDeclaration,
    IfStatement,
    WhileStatement,
    DoWhileStatement,
    ForStatement,
    ExpressionStatement,

    // Expressions
    LiteralExpression,
    NameExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression,
    AssignmentExpression,
    CallExpression,
}

