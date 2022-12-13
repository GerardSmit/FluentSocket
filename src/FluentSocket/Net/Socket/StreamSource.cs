using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public class StreamSource : SocketSourceBase
{
    private readonly Stream _stream;

    public StreamSource(Stream stream)
    {
        _stream = stream;
    }

    public override ValueTask SendAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default)
    {
        return new ValueTask(_stream.WriteAsync(data.Array!, data.Offset, data.Count, cancellationToken));
    }

#if NET7_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public override ValueTask SendAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        return _stream.WriteAsync(data, cancellationToken);
    }
#endif

    public override ValueTask<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default)
    {
        return new ValueTask<int>(_stream.ReadAsync(buffer.Array!, buffer.Offset, buffer.Count, cancellationToken));
    }

#if NET7_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    public override ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _stream.ReadAsync(buffer, cancellationToken);
    }
#endif
}
