using System;
using System.Buffers;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FluentSocket;

public class PacketChannelLayer : Layer
{
    private readonly ChannelWriter<Packet> _writer;
    private readonly MemoryPool<byte> _memoryPool;

    public PacketChannelLayer(ChannelWriter<Packet> writer, MemoryPool<byte> memoryPool = null)
    {
        _writer = writer;
        _memoryPool = memoryPool ?? MemoryPool<byte>.Shared;
    }

    public override ValueTask SendAsync(SendDelegate next, Memory<byte> data)
    {
        return next(data);
    }

    public override async ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
    {
        var owner = _memoryPool.Rent(data.Length);
        var memory = owner.Memory;
        data.CopyTo(memory);

        var packet = new Packet(memory, owner);
        await _writer.WriteAsync(packet);

        await next(data);
    }
}
