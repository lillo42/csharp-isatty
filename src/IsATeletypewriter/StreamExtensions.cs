using System.Reflection;

namespace IsATeletypewriter;

/// <summary>
/// Extensions for <see cref="Stream"/>
/// </summary>
public static class StreamExtensions
{
    private static readonly Dictionary<Type, Func<Stream, nint>?> s_propertyAccess = new();

    /// <summary>
    /// Gets the file descriptor for the given <paramref name="stream"/>
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/>.</param>
    /// <returns>The <see cref="nint"/> related with <paramref name="stream"/>.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="stream"/> is <see lang="null"/>.</exception>
    public static nint GetFileDescriptor(this Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        if (stream == Stream.Null)
        {
            return nint.Zero;
        }

        if (stream is FileStream fileStream)
        {
            return fileStream.SafeFileHandle.DangerousGetHandle();
        }

        var propertyAccess = GetOrCreateHandlerAccess(stream);
        if (propertyAccess is not null)
        {
            return propertyAccess(stream);
        }

        return nint.Zero;
    }

    private static Func<Stream, nint>? GetOrCreateHandlerAccess(Stream stream)
    {
        var type = stream.GetType();
        if (s_propertyAccess.TryGetValue(type, out var value))
        {
            return value;
        }

        var field = type.GetField("_handle", BindingFlags.Instance | BindingFlags.NonPublic);
        if (field is null || field.FieldType != typeof(nint))
        {
            return null;
        }
        
        return s_propertyAccess[type] = s => (nint)field.GetValue(s)!;
    }
}
