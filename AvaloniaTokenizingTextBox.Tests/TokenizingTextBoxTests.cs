using Avalonia.Data;
using AvaloniaTokenizingTextBox.Controls;
using Xunit;
using Xunit.Abstractions;

namespace AvaloniaTokenizingTextBox.Tests;

public class TokenizingTextBoxTests
{
    private readonly ITestOutputHelper _output;

    public TokenizingTextBoxTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DefaultBindingMode_Should_Be_TwoWay()
    {
        Assert.Equal(BindingMode.TwoWay, TokenizingTextBox.TextProperty.GetMetadata(typeof(TokenizingTextBox)).DefaultBindingMode);
    }

    [Fact]
    public void SelectableTokens_Should_Filter_With_Contains_Until_Space()
    {
        var selectableTokens = new List<string>
        {
            "test@gmail.com",
            "test1@gmail.com",
            "anothertest@gmail.com",
            "what@gmail.com",
            "test fake@gmail.com"
        };

        var filtered = selectableTokens.Where(x => x.Contains("test")).ToList();

        _output.WriteLine(string.Join(',', filtered));
    }
}