using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public sealed class XorEncryptLayer : Layer
{
    private readonly ReadOnlyMemory<byte> _byteKey;

    public XorEncryptLayer(ReadOnlyMemory<byte> byteKey)
    {
        _byteKey = byteKey;
    }

    public override ValueTask SendAsync(SendDelegate next, Memory<byte> data)
    {
        Encrypt(data.Span);
        return next(data);
    }

    public override ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
    {
        Decrypt(data.Span);
        return next(data);
    }

    private void Encrypt(Span<byte> span)
    {
        var key = _byteKey.Span;

        for (var i = 0; i < span.Length; i++)
        {
            span[i] = (byte)(span[i] ^ key[i % key.Length]);
        }
    }

    private void Decrypt(Span<byte> span)
    {
        var key = _byteKey.Span;

        for (var i = 0; i < span.Length; i++)
        {
            span[i] = (byte)(span[i] ^ key[i % _byteKey.Length]);
        }
    }
}
