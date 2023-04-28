using System.Runtime.InteropServices;

namespace IsATeletypewriter;

/// <summary>
/// The Unix implementation of <see cref="IIsATeletypewriter"/>
/// </summary>
public partial class UnixIsATeletypewriter : IIsATeletypewriter
{
    /// <inheritdoc /> 
    public bool IsTerminal(nint fd)
    {
        var termios = new termios();
        _ = ioctl(fd.ToInt32(), TIOCGETA, ref termios);
        return Marshal.GetLastPInvokeError() == 0;
    }

    /// <inheritdoc /> 
    public bool IsCygwinTerminal(nint fd) => false;
    
    private const string LibC = "libc";

    [LibraryImport(LibC, SetLastError = true)]
    internal static partial int ioctl(int fd, int cmd, ref termios argp);

    private const int TIOCGETA = 0x402c7413;
}
