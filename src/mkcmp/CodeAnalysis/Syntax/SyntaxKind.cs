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
    ColonToken,
    CommaToken,
    IdentifierToken,

    // Keywords
    ElseKeyword,
    FalseKeyword,
    ForKeyword,
    FunctionKeyword,
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
    GlobalStatement,
    FunctionDeclaration,
    ElseClause,
    TypeClause,
    Parameter,

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

