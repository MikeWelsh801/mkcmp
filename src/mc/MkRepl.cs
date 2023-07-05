using Mkcmp.CodeAnalysis;
using Mkcmp.CodeAnalysis.Syntax;
using Mkcmp.CodeAnalysis.Text;

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
            case SyntaxKind.IfKeyword:
            case SyntaxKind.ElseKeyword:
            case SyntaxKind.WhileKeyword:
            case SyntaxKind.ForKeyword:
            case SyntaxKind.LetKeyword:
            case SyntaxKind.VarKeyword:
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
            case SyntaxKind.IdentifierToken:
                break;
            case SyntaxKind.NumberToken:
                Console.ForegroundColor = ConsoleColor.DarkCyan;
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

        var syntaxTree = SyntaxTree.Parse(text);
        if (syntaxTree.Diagnostics.Any())
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
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(result.Value);
            Console.ResetColor();
            _previous = compilation;
        }
        else
        {
            foreach (var diagnostic in result.Diagnostics)
            {
                var lineIndex = _syntaxTree.Text.GetLineIndex(diagnostic.Span.Start);
                var lineNumber = lineIndex + 1;
                var line = _syntaxTree.Text.Lines[lineIndex];
                var character = diagnostic.Span.Start - line.Start + 1;

                Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"({lineNumber}, {character}): ");
                Console.WriteLine(diagnostic);
                Console.ResetColor();

                var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                var prefix = _syntaxTree.Text.ToString(prefixSpan);
                var error = _syntaxTree.Text.ToString(diagnostic.Span);
                var suffix = _syntaxTree.Text.ToString(suffixSpan);

                Console.Write("    ");
                Console.Write(prefix);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(error);
                Console.ResetColor();

                Console.Write(suffix);

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }

}

