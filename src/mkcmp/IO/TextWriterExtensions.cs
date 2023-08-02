using System.CodeDom.Compiler;

namespace Mkcmp.IO;

internal static class TextWriterExtensions
{

    public static bool IsConsoleOut(this TextWriter writer)
    {
        if (writer == Console.Out)
            return true;
        
        if (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut())
            return true;

        return false;
    }

    public static void SetForeground(this TextWriter writer, ConsoleColor color)
    {
        if (writer.IsConsoleOut())
            Console.ForegroundColor = color;
    }

    public static void ResetColor(this TextWriter writer)
    {
        if (writer.IsConsoleOut())
            Console.ResetColor();
    }

    public static void WriteKeyword(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Cyan);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteBool(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkYellow);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteOperator(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Magenta);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteNumber(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkCyan);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteFun(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Blue);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteIdentifier(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Yellow);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WriteString(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.Green);
        writer.Write(text);
        writer.ResetColor();
    }

    public static void WritePunctuation(this TextWriter writer, string text)
    {
        writer.SetForeground(ConsoleColor.DarkGray);
        writer.Write(text);
        writer.ResetColor();
    }
}
