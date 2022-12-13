using System;
using System.IO.Hashing;

namespace FluentSocket;

public sealed class Crc32Layer : ChecksumLayer
{
    public static readonly Crc32Layer Instance = new();

    private Crc32Layer()
        : base(sizeof(uint))
    {
    }

    protected override void Hash(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        Crc32.Hash(source, destination);
    }
}
