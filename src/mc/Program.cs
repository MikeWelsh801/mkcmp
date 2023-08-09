using Mkcmp.CodeAnalysis;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;

namespace Mkcmp;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.Error.WriteLine("usage: mc <source-path>");
            return;
        }

        if (args.Length > 1)
        {
            Console.Error.WriteLine("error: only one path supported right now");
            return;
        }
        var path = args.Single();

        var text = File.ReadAllText(path);
        var _syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(_syntaxTree);
        compilation.Evaluate(new Dictionary<VariableSymbol, object>());
    }
}
