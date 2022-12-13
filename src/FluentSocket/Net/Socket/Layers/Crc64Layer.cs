using System;
using System.IO.Hashing;

namespace FluentSocket;

public sealed class Crc64Layer : ChecksumLayer
{
    public static readonly Crc64Layer Instance = new();

    private Crc64Layer()
        : base(sizeof(ulong))
    {
    }

    protected override void Hash(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        Crc64.Hash(source, destination);
    }
}
