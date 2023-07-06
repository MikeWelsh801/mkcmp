using System.Collections;
using System.Collections.Immutable;

namespace Mkcmp.CodeAnalysis.Syntax;

public sealed class SeparatedSyntaxList<T> : IEnumerable<T>
    where T : SyntaxNode
{
    private readonly ImmutableArray<SyntaxNode> _separatorsAndNodes;

    public SeparatedSyntaxList(ImmutableArray<SyntaxNode> separatorsAndNodes)
    {
        _separatorsAndNodes = separatorsAndNodes;
    }

    public int Count => (_separatorsAndNodes.Length + 1) / 2;

    public T this[int index] => (T) _separatorsAndNodes[index * 2];

    public SyntaxToken GetSeparator(int index) => (SyntaxToken) _separatorsAndNodes[index * 2 + 1];

    public ImmutableArray<SyntaxNode> GetWithSeparators() => _separatorsAndNodes;

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


