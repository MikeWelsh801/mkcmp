using Mkcmp.CodeAnalysis;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;
using Mkcmp.IO;

namespace Mkcmp;

internal sealed class MkRepl : Repl
{
    private Compilation? _previous = null;
    private bool _showTree = false;
    private bool _showProgram = false;
    private readonly Dictionary<VariableSymbol, object> _variables = new();

    protected override void RenderLine(string line)
    {
        var tokens = SyntaxTree.ParseTokens(line);
        foreach (SyntaxToken token in tokens)
            PrintToken(token);
    }

    private void PrintToken(SyntaxToken token)
    {
        switch (token.Kind)
        {
            // keywords
            case SyntaxKind.BreakKeyword:
            case SyntaxKind.ContinueKeyword:
            case SyntaxKind.IfKeyword:
            case SyntaxKind.ElseKeyword:
            case SyntaxKind.WhileKeyword:
            case SyntaxKind.DoKeyword:
            case SyntaxKind.ForKeyword:
            case SyntaxKind.LetKeyword:
            case SyntaxKind.VarKeyword:
            case SyntaxKind.ColonToken:
                Console.ForegroundColor = ConsoleColor.Cyan;
                break;
            case SyntaxKind.TrueKeyword:
            case SyntaxKind.FalseKeyword:
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                break;
            case SyntaxKind.InKeyword:
            case SyntaxKind.ToKeyword:
            case SyntaxKind.ThroughKeyword:
            case SyntaxKind.EqualsToken:
            case SyntaxKind.EqualsEqualsToken:
            case SyntaxKind.BangToken:
            case SyntaxKind.BangEqualsToken:
            case SyntaxKind.LessToken:
            case SyntaxKind.LessOrEqualsToken:
            case SyntaxKind.GreaterToken:
            case SyntaxKind.GreaterOrEqualsToken:
            case SyntaxKind.PlusToken:
            case SyntaxKind.MinusToken:
            case SyntaxKind.StarToken:
            case SyntaxKind.SlashToken:
            case SyntaxKind.AmpersandToken:
            case SyntaxKind.AmpersandAmpersandToken:
            case SyntaxKind.PipeToken:
            case SyntaxKind.PipePipeToken:
                Console.ForegroundColor = ConsoleColor.Magenta;
                break;
            case SyntaxKind.NumberToken:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                break;
            case SyntaxKind.FunctionKeyword:
            case SyntaxKind.ReturnKeyword:
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            case SyntaxKind.IdentifierToken:
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case SyntaxKind.StringToken:
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            default:
                break;
        }
        Console.Write(token.Text);
        Console.ResetColor();
    }

    protected override void EvaluateMetaCommand(string input)
    {
        switch (input)
        {
            case "#showTree":
                _showTree = !_showTree;
                Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
                break;
            case "#showProgram":
                _showProgram = !_showProgram;
                Console.WriteLine(_showProgram ? "Showing bound tree." : "Not showing bound tree.");
                break;
            case "#cls":
                Console.Clear();
                break;
            case "#reset":
                _previous = null;
                break;
            default:
                base.EvaluateMetaCommand(input);
                break;
        }
    }

    protected override bool IsCompleteSubmission(string text)
    {
        if (string.IsNullOrEmpty(text))
            return true;

        var lastTwoLinesAreBlank = text.Split(Environment.NewLine)
                                       .Reverse()
                                       .TakeWhile(s => string.IsNullOrEmpty(s))
                                       .Take(2)
                                       .Count() == 2;
        if (lastTwoLinesAreBlank)
            return true;

        var syntaxTree = SyntaxTree.Parse(text);
        if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
            return false;

        return true;
    }

    protected override void EvaluateSubmission(string text)
    {
        var _syntaxTree = SyntaxTree.Parse(text);

        var compilation = _previous == null
                            ? new Compilation(_syntaxTree)
                            : _previous.ContinueWith(_syntaxTree);

        if (_showTree)
            _syntaxTree.Root.WriteTo(Console.Out);

        if (_showProgram)
            compilation.EmitTree(Console.Out);

        var result = compilation.Evaluate(_variables);

        if (!result.Diagnostics.Any())
        {
            if (result.Value != null)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }
            _previous = compilation;
        }
        else
        {
            Console.Out.WriteDiagnostics(result.Diagnostics, _syntaxTree);
        }
    }

}

