using System;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public class SocketSourceLayered : SocketSourceBase
{
    private readonly SendDelegate _send;
    private readonly ReceiveDelegate _receive;
    private readonly SocketSourceBase _source;

    public SocketSourceLayered(SendDelegate send, ReceiveDelegate receive, SocketSourceBase source)
    {
        _send = send;
        _receive = receive;
        _source = source;
    }

    public override ValueTask SendAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        return _send(data);
    }

    public override ValueTask SendAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        return SendAsync(data.AsMemory(), cancellationToken);
    }

    public override async ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var length = await _source.ReceiveAsync(buffer, cancellationToken);
        await _receive(buffer.Slice(0, length));
        return length;
    }

    public override ValueTask<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
    {
        return ReceiveAsync(buffer.AsMemory(), cancellationToken);
    }
}
