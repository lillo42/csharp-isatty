namespace IsATeletypewriter;

/// <summary>
/// The <see cref="IIsATeletypewriter"/> for all other OS.                                                         
/// </summary>
public class OtherIsATeletypewriter : IIsATeletypewriter
{
    /// <inheritdoc /> 
    public bool IsTerminal(IntPtr fd) => false;

    /// <inheritdoc /> 
    public bool IsCygwinTerminal(IntPtr fd) => false;
}
