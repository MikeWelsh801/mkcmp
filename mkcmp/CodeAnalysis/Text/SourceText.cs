using System.Collections.Immutable;

namespace Mkcmp.CodeAnalysis.Text;

public sealed class SourceText
{
    private readonly string _text;

    private SourceText(string text)
    {
        _text = text;
        Lines = ParseLines(this, text);
    }

    public ImmutableArray<TextLine> Lines { get; }

    public char this[int index] => _text[index];

    public int Length => _text.Length;

    public int GetLineIndex(int position)
    {
        var startPos = 0;
        var endPos = Lines.Length - 1;
        var mid = startPos + (endPos - startPos) / 2;
        var midLine = Lines[mid];

        while (position < midLine.Start || position > midLine.End)
        {
            if (position < midLine.Start)
                endPos = midLine.Start - 1;
            else
                startPos = midLine.End + 1;

            mid = startPos + (endPos - startPos) / 2;
            midLine = Lines[mid];
        }
        return mid;
    }

    private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
    {
        var result = ImmutableArray.CreateBuilder<TextLine>();

        var position = 0;
        var lineStart = 0;
        while (position < text.Length)
        {
            var lineBreakWidth = GetLineBreakWidth(text, position);
            if (lineBreakWidth == 0)
            {
                position++;
            }
            else
            {
                AddLine(result, sourceText, position, lineStart, lineBreakWidth);
                position += lineBreakWidth;
                lineStart = position;
            }
        }

        if (position > lineStart)
            AddLine(result, sourceText, position, lineStart, 0);

        return result.ToImmutable();
    }

    private static void AddLine(ImmutableArray<TextLine>.Builder lines,
                                SourceText sourceText,
                                int position,
                                int lineStart,
                                int lineBreakWidth)
    {
        var lineLength = position - lineStart;
        var lineLengthIncludingLineBreak = lineLength + lineBreakWidth;
        var line = new TextLine(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);
        lines.Add(line);
    }

    private static int GetLineBreakWidth(string text, int position)
    {
        var c = text[position];
        var l = position + 1 >= text.Length ? '\0' : text[position + 1];
        return c switch
        {
            '\r' when l == '\n' => 2,
            '\r' or '\n' => 1,
            _ => 0
        };

    }

    public static SourceText From(string text)
    {
        return new SourceText(text);
    }

    public override string ToString() => _text;

    public string ToString(int start, int length) => _text.Substring(start, length);

    public string ToString(TextSpan span) => ToString(span.Start, span.Length);
}
