using System;
using System.Buffers;
using System.Threading.Tasks;

namespace FluentSocket;

public abstract class ChecksumLayer : Layer
{
    private readonly int _checksumSize;

    protected ChecksumLayer(int checksumSize)
    {
        _checksumSize = checksumSize;
    }

    protected abstract void Hash(ReadOnlySpan<byte> source, Span<byte> destination);

    public override async ValueTask SendAsync(SendDelegate next, Memory<byte> data)
    {
        var length = data.Length + _checksumSize;
        using var owner = MemoryPool<byte>.Shared.Rent(length);

        var buffer = owner.Memory.Slice(0, length);

        data.CopyTo(buffer.Slice(_checksumSize, data.Length));
        Hash(data.Span, buffer.Span.Slice(0, _checksumSize));

        await next(buffer);
    }

    public override ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
    {
        if (!ValidateChecksum(data.Span))
        {
            return default;
        }

        return next(data.Slice(_checksumSize));
    }

    private bool ValidateChecksum(ReadOnlySpan<byte> buffer)
    {
        Span<byte> checksum = stackalloc byte[_checksumSize];
        Hash(buffer.Slice(_checksumSize), checksum);
        return checksum.SequenceEqual(buffer.Slice(0, _checksumSize));
    }
}
