using System.Runtime.InteropServices;

namespace IsATeletypewriter;

/// <summary>
/// The Linux implementation of <see cref="IIsATeletypewriter"/>
/// </summary>
public partial class LinuxIsATeletypewriter : IIsATeletypewriter
{
    /// <inheritdoc /> 
    public bool IsTerminal(nint fd)
    {
        var termios = new termios();
        _ = ioctl(fd.ToInt32(), TCGETS, ref termios);
        return Marshal.GetLastPInvokeError() == 0;
    }

    /// <inheritdoc /> 
    public bool IsCygwinTerminal(nint fd) => false;

    private const string LibC = "libc";

    [LibraryImport(LibC, SetLastError = true)]
    internal static partial int ioctl(int fd, int cmd, ref termios argp);

    private const int TCGETS = 0x5401;
}
