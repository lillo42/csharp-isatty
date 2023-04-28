using System.Runtime.InteropServices;

namespace IsATeletypewriter;

/// <summary>
/// The static instance of <see cref="IIsATeletypewriter"/>.
/// </summary>
public static class SystemIsATeletypewriter
{
    /// <summary>
    /// The current implementation of <see cref="IIsATeletypewriter"/> based on OS.
    /// </summary>
    public static IIsATeletypewriter Instance { get; }

    static SystemIsATeletypewriter()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Instance = new WindowsIsATeletypewriter();
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Instance = new LinuxIsATeletypewriter();
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Instance = new UnixIsATeletypewriter();
        }
        else
        {
            Instance = new OtherIsATeletypewriter();
        }
    }
}
