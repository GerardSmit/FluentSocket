using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public sealed class SocketSource : SocketSourceBase
{
    private readonly Socket _socket;

    public SocketSource(Socket socket)
    {
        _socket = socket;
    }

    public override ValueTask SendAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        return new ValueTask(_socket.SendAsync(data, SocketFlags.None));
    }

#if NET7_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public override async ValueTask SendAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        await _socket.SendAsync(data, SocketFlags.None, cancellationToken);
    }
#endif

    public override ValueTask<int> ReceiveAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        return new ValueTask<int>(_socket.ReceiveAsync(data, SocketFlags.None));
    }

#if NET7_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public override ValueTask<int> ReceiveAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        return _socket.ReceiveAsync(data, SocketFlags.None, cancellationToken);
    }
#endif
}
