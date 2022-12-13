using System;
using System.Buffers;
using System.Buffers.Text;
using System.Threading.Tasks;

namespace FluentSocket;

public sealed class Base64Layer : Layer
{
    public static readonly Base64Layer Instance = new();

    private readonly MemoryPool<byte> _pool;

    public Base64Layer(MemoryPool<byte> pool = null)
    {
        _pool = pool ?? MemoryPool<byte>.Shared;
    }

    public override async ValueTask SendAsync(SendDelegate next, Memory<byte> data)
    {
        var maxLength = Base64.GetMaxEncodedToUtf8Length(data.Length);

        using var owner = _pool.Rent(maxLength);

        Base64.EncodeToUtf8(data.Span, owner.Memory.Span, out _, out var bytesWritten, isFinalBlock: true);

        await next(owner.Memory.Slice(0, bytesWritten));
    }

    public override async ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
    {
        var maxLength = Base64.GetMaxDecodedFromUtf8Length(data.Length);

        if (maxLength > data.Length)
        {
            using var owner = _pool.Rent(maxLength);

            Base64.DecodeFromUtf8(data.Span, owner.Memory.Span, out _, out var bytesWritten, isFinalBlock: true);

            await next(owner.Memory.Slice(0, bytesWritten));
        }
        else
        {
            Base64.DecodeFromUtf8(data.Span, data.Span, out _, out var bytesWritten, isFinalBlock: true);

            await next(data.Slice(0, bytesWritten));
        }
    }
}
