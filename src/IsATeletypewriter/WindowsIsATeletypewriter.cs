using System.Runtime.InteropServices;

namespace IsATeletypewriter;

/// <summary>
/// The Windows implementation of <see cref="IIsATeletypewriter"/>
/// </summary>
public partial class WindowsIsATeletypewriter : IIsATeletypewriter
{
    /// <inheritdoc /> 
    public bool IsTerminal(nint fd)
    {
        if (!GetConsoleMode(fd, out _))
        {
            return false;
        }

        return Marshal.GetLastPInvokeError() == 0;
    }

    /// <inheritdoc /> 
    public bool IsCygwinTerminal(nint fd)
        => Environment.GetEnvironmentVariable("TERM_PROGRAM") == "mintty" && IsTerminal(fd);

    private const string Kernel32 = "Kernel32.dll";

    [LibraryImport(Kernel32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetConsoleMode(nint hConsoleHandle, out uint lpMode);
}
