#if NET462 || NETSTANDARD2_0
using System;
using System.Text;

namespace FluentSocket;

internal static class EncodingExtensions
{
    public static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        fixed (char* charsPtr = &chars.GetPinnableReference())
        fixed (byte* bytesPtr = &bytes.GetPinnableReference())
        {
            return encoding.GetBytes(charsPtr, chars.Length, bytesPtr, bytes.Length);
        }
    }

    public static unsafe string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
    {
        fixed (byte* bytesPtr = &bytes.GetPinnableReference())
        {
            return encoding.GetString(bytesPtr, bytes.Length);
        }
    }
}
#endif
