# csharp-isatty

isatty for c#,  It is heavily inspired by the Go library [go-isatty](https://github.com/mattn/go-isatty).

## Usage

```csharp
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
```

## License

MIT

