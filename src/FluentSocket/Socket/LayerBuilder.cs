using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentSocket.Protocol;

namespace FluentSocket;

public static class SocketBuilderExtensions
{
    public static LayerBuilder LayerBuilder(this SocketSourceBase source) => new(source);

    public static LayerBuilder LayerBuilder(this Socket source) => new(source);

    public static LayerBuilder LayerBuilder(this WebSocket source) => new(source);

    public static LayerBuilder LayerBuilder(this Stream source) => new(source);
}

public sealed class LayerBuilder
{
    private readonly List<Layer> _layers = new();
    private readonly SocketSourceBase _socket;
    private MemoryPool<byte> _memoryPool;

    public LayerBuilder(SocketSourceBase socket)
    {
        _socket = socket;
    }

    public LayerBuilder AddLayer(Layer layer)
    {
        _layers.Add(layer);
        return this;
    }

    public LayerBuilder AddLayer<T>()
        where T : Layer, new()
    {
        return AddLayer(new T());
    }

    public LayerBuilder AddCrc32() => AddLayer(Crc32Layer.Instance);

    public LayerBuilder AddCrc64() => AddLayer(Crc64Layer.Instance);

    public LayerBuilder XorEncrypt(ReadOnlyMemory<byte> key) => AddLayer(new XorEncryptLayer(key));

    public LayerBuilder PacketLength(LengthType lengthType, MemoryPool<byte> memoryPool = null)
        => AddLayer(new PacketLengthPrefix(lengthType, memoryPool));

    public LayerBuilder PacketChannel(ChannelWriter<Packet> writer, MemoryPool<byte> pool = null)
        => AddLayer(new PacketChannelLayer(writer, pool));

    public LayerBuilder PacketChannel(out ChannelReader<Packet> writer, MemoryPool<byte> pool = null)
    {
        var channel = Channel.CreateUnbounded<Packet>();
        writer = channel.Reader;
        return PacketChannel(channel.Writer, pool);
    }

    public LayerBuilder MessageChannel<T>(ChannelWriter<T> writer, IMessageProtocol protocol = null)
        where T : IMessage, new()
        => AddLayer(new MessageChannelLayer<T>(writer, protocol ?? LittleEndianProtocol.Utf8));

    public LayerBuilder MessageChannel<T>(out ChannelReader<T> writer, IMessageProtocol protocol = null)
        where T : IMessage, new()
    {
        var channel = Channel.CreateUnbounded<T>();
        writer = channel.Reader;
        return MessageChannel(channel.Writer, protocol);
    }

    public LayerBuilder WithMemoryPool(MemoryPool<byte> memoryPool)
    {
        _memoryPool = memoryPool;
        return this;
    }

    public LayerBuilder Base64Encode() => AddLayer(Base64Layer.Instance);

    public SocketSourceBase Build()
    {
        if (_layers.Count == 0)
        {
            return _socket;
        }

        SendDelegate send = data => _socket.SendAsync(data);
        ReceiveDelegate receive = _ => default;

        for (var i = _layers.Count - 1; i >= 0; i--)
        {
            var layer = _layers[i];

            var sendDelegate = send;
            var receiveDelegate = receive;

            send = data => layer.SendAsync(sendDelegate, data);
            receive = buffer => layer.ReceiveAsync(receiveDelegate, buffer);
        }

        return new SocketSourceLayered(send, receive, _socket);
    }

    public async Task ListenAsync(Memory<byte> buffer)
    {
        var socket = Build();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer);

            if (result == 0)
            {
                break;
            }
        }
    }

    public async Task ListenAsync(int bufferSize)
    {
        using var buffer = (_memoryPool ?? MemoryPool<byte>.Shared).Rent(bufferSize);

        await ListenAsync(buffer.Memory);
    }

    public async Task ListenAsync()
    {
        await ListenAsync(4096);
    }
}
