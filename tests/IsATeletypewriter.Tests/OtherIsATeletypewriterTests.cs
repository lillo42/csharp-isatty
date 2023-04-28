using FluentAssertions;

namespace IsATeletypewriter.Tests;

public class OtherIsATeletypewriterTests
{
    private readonly OtherIsATeletypewriter _tty = new ();
    [Fact] 
    public void IsTerminal_Should_Always_ReturnFalse()
    {
        var b = Console.Out;
        var a = Console.OpenStandardOutput();
        _tty.IsTerminal(0).Should().BeFalse();
    }
    
    [Fact]
    public void IsCygwinTerminal_Should_Always_ReturnFalse()
    {
        _tty.IsCygwinTerminal(0).Should().BeFalse();
    }
}
