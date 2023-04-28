namespace IsATeletypewriter;

/// <summary>
/// The isatty functions test whether a file descriptor refers to a terminal.
/// </summary>
public interface IIsATeletypewriter
{
    /// <summary>
    /// True if the file descriptor is terminal.
    /// </summary>
    /// <param name="fd">The file description.</param>
    /// <returns>
    /// True if the file descriptor is terminal.
    /// </returns>
    bool IsTerminal(nint fd);

    /// <summary>
    /// True if the file descriptor is a cygwin or msys2 terminal.
    /// </summary>
    /// <param name="fd">The file description.</param>
    /// <returns>
    /// True if the file descriptor is a cygwin or msys2 terminal.
    /// </returns>
    bool IsCygwinTerminal(nint fd);
}
