using System.Collections.Immutable;
using System.Text;
using Mkcmp.CodeAnalysis.Text;

namespace mkcmp.Tests.CodeAnalysis;

internal sealed class AnnotatedText
{
    public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
    {
        Text = text;
        Spans = spans;
    }

    public string Text { get; }
    public ImmutableArray<TextSpan> Spans { get; }

    public static AnnotatedText Parse(string text)
    {
        text = Unindent(text);

        var textBuilder = new StringBuilder();
        var spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
        var startStack = new Stack<int>();

        var position = 0;

        foreach (var c in text)
        {
            if (c == '[')
            {
                startStack.Push(position);
            }
            else if (c == ']')
            {
                if (startStack.Count == 0)
                    throw new ArgumentException("Too many ']' int text", nameof(text));

                var start = startStack.Pop();
                var end = position;
                var span = TextSpan.FromBounds(start, end);
                spanBuilder.Add(span);
            }
            else
            {
                position++;
                textBuilder.Append(c);
            }
        }

        if (startStack.Count != 0)
            throw new ArgumentException("Missing ']' int text", nameof(text));

        return new AnnotatedText(textBuilder.ToString(), spanBuilder.ToImmutableArray());

    }

    public static string[] UnindentLines(string text)
    {
        var lines = new List<string>();
        using var stringReader = new StringReader(text);

        string currLine;
        while ((currLine = stringReader.ReadLine()) != null)
            lines.Add(currLine);

        var minIndentation = int.MaxValue;
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            if (line.Trim().Length == 0)
            {
                lines[i] = string.Empty;
                continue;
            }

            var indentation = line.Length - line.TrimStart().Length;
            minIndentation = Math.Min(indentation, minIndentation);
        }

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Length == 0)
                continue;

            lines[i] = lines[i].Substring(minIndentation);
        }

        while (lines.Count > 0 && lines[0].Length == 0)
            lines.RemoveAt(0);

        while (lines.Count > 0 && lines[lines.Count - 1].Length == 0)
            lines.RemoveAt(lines.Count - 1);

        return lines.ToArray();
    }

    private static string Unindent(string text)
    {
        return string.Join(Environment.NewLine, UnindentLines(text));
    }
}



