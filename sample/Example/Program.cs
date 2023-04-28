using IsATeletypewriter;

var stdout = Console.OpenStandardOutput();
var fd = stdout.GetFileDescriptor();

if (SystemIsATeletypewriter.Instance.IsCygwinTerminal(fd))
{
    Console.WriteLine("Is Cygwin/MSYS2 Terminal");
}
else if (SystemIsATeletypewriter.Instance.IsTerminal(fd))
{
    Console.WriteLine("Is Terminal");
}
else
{
    Console.WriteLine("Is Not Terminal");
}
