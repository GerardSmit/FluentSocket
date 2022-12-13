using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public sealed class WebSocketSource : SocketSourceBase
{
    private readonly WebSocket _socket;

    public WebSocketSource(WebSocket socket)
    {
        _socket = socket;
    }

    public override ValueTask SendAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        return new ValueTask(_socket.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken));
    }

#if NET7_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public override ValueTask SendAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        return _socket.SendAsync(data, WebSocketMessageType.Binary, true, cancellationToken);
    }
#endif

    public override async ValueTask<int> ReceiveAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        var result = await _socket.ReceiveAsync(data, cancellationToken);

        return result.Count;
    }

#if NET7_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public override async ValueTask<int> ReceiveAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        var result = await _socket.ReceiveAsync(data, cancellationToken);

        return result.Count;
    }
#endif
}
