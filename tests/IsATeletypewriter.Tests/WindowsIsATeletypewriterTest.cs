using FluentAssertions;

namespace IsATeletypewriter.Tests;

public class WindowsIsATeletypewriterTest
{
    private readonly WindowsIsATeletypewriter _tty = new();

    [Fact]
    public void IsCygwinTerminal_Should_ReturnFalse_When_TERM_PROGRAMIsNotMinTty()
    {
        Environment.SetEnvironmentVariable("TERM_PROGRAM", "notmintty");
        var stdout = Console.OpenStandardOutput();
        _tty.IsCygwinTerminal(stdout.GetFileDescriptor())
            .Should().BeFalse();
    }
}
