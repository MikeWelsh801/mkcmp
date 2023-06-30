using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Mkcmp;

internal abstract class Repl
{
    private List<string> _submissionHistory = new();
    private int _submissionHistoryIndex;
    private bool _done;

    public void Run()
    {
        while (true)
        {
            var text = EditSubmission();
            if (string.IsNullOrEmpty(text))
                return;

            if (!text.Contains(Environment.NewLine) && text.StartsWith("#"))
                EvaluateMetaCommand(text);
            else
                EvaluateSubmission(text);

            _submissionHistory.Add(text);
            _submissionHistoryIndex = 0;
        }
    }

    private sealed class SubmissionView
    {
        private readonly ObservableCollection<string> _submissionDocument;
        private readonly int _cursorTop;
        private int _renderedLineCount;
        private int _currentLineIndex;
        private int _currentCharacter;

        public SubmissionView(ObservableCollection<string> submissionDocument)
        {
            _submissionDocument = submissionDocument;
            _submissionDocument.CollectionChanged += SubmissionDocumentChanged;
            _cursorTop = Console.CursorTop;
            Render();
        }

        private void SubmissionDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            Console.CursorVisible = false;
            var lineCount = 0;

            foreach (var line in _submissionDocument)
            {
                Console.SetCursorPosition(0, _cursorTop + lineCount);
                Console.ForegroundColor = ConsoleColor.Green;

                if (lineCount == 0)
                    Console.Write("» ");
                else
                    Console.Write("· ");

                Console.ResetColor();
                Console.Write(line);
                Console.WriteLine(new string(' ', Console.WindowWidth - line.Length));

                lineCount++;
            }

            for (int i = 0; i < _renderedLineCount - lineCount; i++)
            {
                Console.SetCursorPosition(0, _cursorTop + lineCount + i);
                Console.WriteLine(new string(' ', Console.WindowWidth));
            }

            //var numberOfBlankLines = _renderedLineCount - lineCount;
            //if (numberOfBlankLines > 0)
            //{
            //    var blankLine = new string(' ', Console.WindowWidth);
            //    while (numberOfBlankLines > 0)
            //    {
            //        Console.WriteLine(blankLine);
            //    }
            //}

            _renderedLineCount = lineCount;
            Console.CursorVisible = true;
            UpdateCursorPosition();
        }

        private void UpdateCursorPosition()
        {
            Console.CursorTop = _cursorTop + CurrentLine;
            Console.CursorLeft = 2 + CurrentCharacter;
        }

        public int CurrentLine
        {
            get => _currentLineIndex;
            set
            {
                if (_currentLineIndex != value)
                {
                    _currentLineIndex = value;

                    _currentCharacter = Math.Min(
                        _currentCharacter,
                        _submissionDocument[_currentLineIndex].Length);
                    UpdateCursorPosition();
                }
            }
        }

        public int CurrentCharacter
        {
            get => _currentCharacter;
            set
            {
                if (_currentCharacter != value)
                {
                    _currentCharacter = value;
                    UpdateCursorPosition();
                }
            }
        }

    }

    private string EditSubmission()
    {
        _done = false;

        var document = new ObservableCollection<string>() { "" };
        var view = new SubmissionView(document);

        while (!_done)
        {
            var key = Console.ReadKey(true);
            HandleKey(key, document, view);
        }

        Console.WriteLine();

        return string.Join(Environment.NewLine, document);
    }

    private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionView view)
    {
        if (key.Modifiers == default(ConsoleModifiers))
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    HandleEnter(document, view);
                    break;
                case ConsoleKey.Escape:
                    HandleEscape(document, view);
                    break;
                case ConsoleKey.LeftArrow:
                    HandleLeftArrow(document, view);
                    break;
                case ConsoleKey.RightArrow:
                    HandleRightArrow(document, view);
                    break;
                case ConsoleKey.UpArrow:
                    HandleUpArrow(document, view);
                    break;
                case ConsoleKey.DownArrow:
                    HandleDownArrow(document, view);
                    break;
                case ConsoleKey.Backspace:
                    HandleBackspace(document, view);
                    break;
                case ConsoleKey.Delete:
                    HandleDelete(document, view);
                    break;
                case ConsoleKey.Home:
                    HandleHome(document, view);
                    break;
                case ConsoleKey.End:
                    HandleEnd(document, view);
                    break;
                case ConsoleKey.Tab:
                    HandleTab(document, view);
                    break;
                case ConsoleKey.PageUp:
                    HandlePageUp(document, view);
                    break;
                case ConsoleKey.PageDown:
                    HandleDown(document, view);
                    break;
            }
        }
        else if (key.Modifiers == ConsoleModifiers.Control)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    HandleControlEnter(document, view);
                    break;
            }
        }
        if (key.KeyChar >= ' ')
            HandleTyping(document, view, key.KeyChar.ToString());
    }

    private void HandleEscape(ObservableCollection<string> document, SubmissionView view)
    {
        document[view.CurrentLine] = string.Empty;
        view.CurrentCharacter = 0;
    }

    private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
    {
        var submissionText = string.Join(Environment.NewLine, document);
        if (submissionText.StartsWith("#") || IsCompleteSubmission(submissionText))
        {
            _done = true;
            return;
        }

        document.Add(string.Empty);
        view.CurrentCharacter = 0;
        view.CurrentLine++;
    }

    private void HandleControlEnter(ObservableCollection<string> document, SubmissionView view)
    {
        _done = true;
    }

    private void HandleLeftArrow(ObservableCollection<string> document, SubmissionView view)
    {
        if (view.CurrentCharacter > 0)
            view.CurrentCharacter--;
    }

    private void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
    {
        var line = document[view.CurrentLine];
        if (view.CurrentCharacter < line.Length)
            view.CurrentCharacter++;
    }

    private void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
    {
        if (view.CurrentLine > 0)
            view.CurrentLine--;
    }

    private void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
    {
        if (view.CurrentLine < document.Count - 1)
            view.CurrentLine++;
    }

    private void HandleBackspace(ObservableCollection<string> document, SubmissionView view)
    {
        var start = view.CurrentCharacter;
        if (start == 0)
            return;

        var lineIndex = view.CurrentLine;
        var line = document[lineIndex];
        var before = line.Substring(0, start - 1);
        var after = line.Substring(start);
        document[lineIndex] = before + after;
        view.CurrentCharacter--;
    }

    private void HandleDelete(ObservableCollection<string> document, SubmissionView view)
    {
        var start = view.CurrentCharacter;
        var lineIndex = view.CurrentLine;
        var line = document[lineIndex];
        if (start >= line.Length)
            return;

        var before = line.Substring(0, start);
        var after = line.Substring(start + 1);
        document[lineIndex] = before + after;
    }

    private void HandleHome(ObservableCollection<string> document, SubmissionView view)
    {
        view.CurrentCharacter = 0;
    }

    private void HandleEnd(ObservableCollection<string> document, SubmissionView view)
    {
        view.CurrentCharacter = document[view.CurrentLine].Length;
    }

    private void HandleTab(ObservableCollection<string> document, SubmissionView view)
    {
        const int TAB_WIDTH = 4;
        var start = view.CurrentCharacter;
        var remainingSpaces = TAB_WIDTH - start % TAB_WIDTH;
        var line = document[view.CurrentLine];
        document[view.CurrentLine] = line.Insert(start, new string(' ', remainingSpaces));
        view.CurrentCharacter += remainingSpaces;
    }

    private void HandlePageUp(ObservableCollection<string> document, SubmissionView view)
    {
        _submissionHistoryIndex--;
        if (_submissionHistoryIndex < 0)
            _submissionHistoryIndex = _submissionHistory.Count - 1;
        UpdateDocumentFromHistory(document, view);
    }

    private void HandleDown(ObservableCollection<string> document, SubmissionView view)
    {
        _submissionHistoryIndex++;
        if (_submissionHistoryIndex >= _submissionHistory.Count)
            _submissionHistoryIndex = 0;
        UpdateDocumentFromHistory(document, view);
    }

    private void UpdateDocumentFromHistory(ObservableCollection<string> document, SubmissionView view)
    {
        document.Clear();
        var historyItem = _submissionHistory[_submissionHistoryIndex];
        var lines = historyItem.Split(Environment.NewLine);

        foreach (var line in lines)
            document.Add(line);

        view.CurrentLine = document.Count - 1;
        view.CurrentCharacter = document[view.CurrentLine].Length;
    }

    private void HandleTyping(ObservableCollection<string> document, SubmissionView view, string text)
    {
        var lineIndex = view.CurrentLine;
        var start = view.CurrentCharacter;
        document[lineIndex] = document[lineIndex].Insert(start, text);
        view.CurrentCharacter += text.Length;
    }

    protected void ClearHistory()
    {
        _submissionHistory.Clear();
    }

    protected virtual void EvaluateMetaCommand(string input)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Invalid command {input}.");
        Console.ResetColor();
    }

    protected abstract bool IsCompleteSubmission(string text);

    protected abstract void EvaluateSubmission(string text);
}

