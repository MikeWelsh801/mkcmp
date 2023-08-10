﻿using Mkcmp.CodeAnalysis;
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

        if (args.Length > 1)
        {
            Console.Error.WriteLine("error: only one path supported right now");
            return;
        }
        var path = args.Single();

        var text = File.ReadAllText(path);
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        if (result.Diagnostics.Any())
            Console.Error.WriteDiagnostics(result.Diagnostics, syntaxTree);
        else if (result.Value != null)
            Console.WriteLine(result.Value);
    }
}
