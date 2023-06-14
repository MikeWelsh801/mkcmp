using Mkcmp.CodeAnalysis;
using Mkcmp.CodeAnalysis.Syntax;

namespace mkcmp.Tests.CodeAnalysis;

public class EvaluationTests
{
    [Theory]
    [InlineData("1", 1)]
    [InlineData("-1", -1)]
    [InlineData("+1", 1)]
    [InlineData("14 + 12", 26)]
    [InlineData("12 - 3", 9)]
    [InlineData("4 * 2", 8)]
    [InlineData("9 / 3", 3)]
    [InlineData("(10)", 10)]
    [InlineData("12 == 3", false)]
    [InlineData("3 == 3", true)]
    [InlineData("12 != 3", true)]
    [InlineData("12 != 12", false)]
    [InlineData("12 < 3", false)]
    [InlineData("12 <= 12", true)]
    [InlineData("12 > 3", true)]
    [InlineData("-12 >= 3", false)]
    [InlineData("true == false", false)]
    [InlineData("true == true", true)]
    [InlineData("true != true", false)]
    [InlineData("true != false", true)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("!true", false)]
    [InlineData("!false", true)]
    [InlineData("{ var a = 0 (a = 10) * a }", 100)]
    [InlineData("{ var a = 0 if a == 0 a = 10 a }", 10)]
    [InlineData("{ var a = 0 if a == 2 a = 10 a }", 0)]
    [InlineData("{ var a = 0 if a == 0 a = 10 else a = 5 a }", 10)]
    [InlineData("{ var a = 0 if a == 2 a = 10 else a = 5 a }", 5)]
    public void Test_Expression_Evaluation_Result(string text, object expectedValue)
    {
        AssertValue(text, expectedValue);
    }

    [Fact]
    public void Evaluator_VariabableDeclaration_Reports_Redeclaration()
    {
        var text = @"
            {
                var x = 10
                var y = 100
                {
                    var x = 10
                }
                var [x] = 5
            }
        ";

        var diagnostics = @"
            Variable 'x' is already declared.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Name_Reports_Undefined()
    {
        var text = @"[x] + 10";

        var diagnostics = @"
            Variable 'x' doesn't exist.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Assignment_Reports_Undefined()
    {
        var text = @"[x] = 10";

        var diagnostics = @"
            Variable 'x' doesn't exist.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Unary_Reports_Undefined()
    {
        var text = @"[+]true";

        var diagnostics = @"
                Unary operator '+' is not defined for type 'System.Boolean'.
            ";

        AssertDiagnostics(text, diagnostics);
    }

    [Fact]
    public void Evaluator_Assigned_Reports_CannotConvert()
    {
        var text = @"12 [+] true";
        var diagnostics = @"
            Binary operator '+' is not defined for types 'System.Int32' and 'System.Boolean'.
        ";

        AssertDiagnostics(text, diagnostics);
    }

    private static void AssertValue(string text, object expectedValue)
    {
        var syntaxTree = SyntaxTree.Parse(text);
        var compilation = new Compilation(syntaxTree);
        var variables = new Dictionary<VariableSymbol, object>();
        var result = compilation.Evaluate(variables);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedValue, result.Value);
    }

    private static void AssertDiagnostics(string text, string diagnosticText)
    {
        var annotateText = AnnotatedText.Parse(text);
        var syntaxTree = SyntaxTree.Parse(annotateText.Text);
        var compilation = new Compilation(syntaxTree);
        var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

        var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

        if (annotateText.Spans.Length != expectedDiagnostics.Length)
            throw new Exception("ERROR: Must mark as many spans as there are expected diagnostics.");

        Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

        for (var i = 0; i < expectedDiagnostics.Length; i++)
        {
            var expectedMessage = expectedDiagnostics[i];
            var actualMessage = result.Diagnostics[i].Message;

            Assert.Equal(expectedMessage, actualMessage);

            var expectedSpan = annotateText.Spans[i];
            var actualSpan = result.Diagnostics[i].Span;

            Assert.Equal(expectedSpan, actualSpan);
        }
    }
}
