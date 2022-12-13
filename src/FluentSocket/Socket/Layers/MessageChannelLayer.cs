using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentSocket.Protocol;

namespace FluentSocket;

public class MessageChannelLayer<T> : Layer
    where T : IMessage, new()
{
    private readonly ChannelWriter<T> _writer;
    private readonly IMessageProtocol _protocol;

    public MessageChannelLayer(ChannelWriter<T> writer, IMessageProtocol protocol)
    {
        _writer = writer;
        _protocol = protocol;
    }

    public override ValueTask SendAsync(SendDelegate next, Memory<byte> data)
    {
        return next(data);
    }

    public override async ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data)
    {
        var message = ReadMessage(data);
        await _writer.WriteAsync(message);
        await next(data);
    }

    private T ReadMessage(ReadOnlyMemory<byte> data)
    {
        var messageReader = new MessageReader(data.Span, _protocol);

        return messageReader.ReadObject<T>();
    }
}
