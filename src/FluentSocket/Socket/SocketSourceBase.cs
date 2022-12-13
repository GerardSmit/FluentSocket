using System;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public abstract class SocketSourceBase
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ArraySegment<byte> ToArraySegment(ReadOnlyMemory<byte> memory)
    {
        return MemoryMarshal.TryGetArray(memory, out var segment)
            ? segment
            : new ArraySegment<byte>(memory.ToArray());
    }

    public virtual ValueTask SendAsync(Memory<byte> data, CancellationToken cancellationToken = default)
    {
        return SendAsync(ToArraySegment(data), cancellationToken);
    }

    public abstract ValueTask SendAsync(ArraySegment<byte> data, CancellationToken cancellationToken = default);

    public virtual ValueTask<int> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return ReceiveAsync(ToArraySegment(buffer), cancellationToken);
    }

    public abstract ValueTask<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken = default);

    public static implicit operator SocketSourceBase(Socket socket) => new SocketSource(socket);

    public static implicit operator SocketSourceBase(WebSocket socket) => new WebSocketSource(socket);

    public static implicit operator SocketSourceBase(Stream socket) => new StreamSource(socket);
}
