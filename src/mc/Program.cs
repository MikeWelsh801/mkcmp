using Mkcmp.CodeAnalysis;
using Mkcmp.CodeAnalysis.Symbols;
using Mkcmp.CodeAnalysis.Syntax;
using Mkcmp.IO;

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

        var paths = GetFilePaths(args);
        var syntaxTrees = new List<SyntaxTree>();
        var hasErrors = false;

        foreach (var path in paths)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"error: file '{path}' doesn't exist");
                hasErrors = true;
                continue;
            }
            var syntaxTree = SyntaxTree.Load(path);
            syntaxTrees.Add(syntaxTree);
        }

        if (hasErrors)
            return;

        var compilation = new Compilation(syntaxTrees.ToArray());
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        if (result.Diagnostics.Any())
            Console.Error.WriteDiagnostics(result.Diagnostics);
        else if (result.Value != null)
            Console.WriteLine(result.Value);
    }

    private static IEnumerable<string> GetFilePaths(IEnumerable<string> args)
    {
        var result = new SortedSet<string>();

        foreach (var path in args)
        {
            if (Directory.Exists(path))
                result.UnionWith(Directory.EnumerateFiles(path, "*.mw", SearchOption.AllDirectories));
            else
                result.Add(path);
        }
        return result;
    }
}
