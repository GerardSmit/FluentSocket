#if NET462
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FluentSocket;

internal static class SocketExtensions
{
    public static Task<int> SendAsync(this Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags)
    {
        var state = new TaskCompletionSource<int>(socket);

        socket.BeginSend(buffer.Array!, buffer.Offset, buffer.Count, socketFlags, static iar =>
        {
            var asyncState = (TaskCompletionSource<int>) iar.AsyncState!;
            try
            {
                asyncState.TrySetResult(((Socket) asyncState.Task.AsyncState!).EndSend(iar));
            }
            catch (Exception ex)
            {
                asyncState.TrySetException(ex);
            }
        }, state);

        return state.Task;
    }

    public static Task<int> ReceiveAsync(this Socket socket, ArraySegment<byte> buffer, SocketFlags socketFlags)
    {
        var state = new TaskCompletionSource<int>(socket);

        socket.BeginReceive(buffer.Array!, buffer.Offset, buffer.Count, socketFlags, static iar =>
        {
            var asyncState = (TaskCompletionSource<int>) iar.AsyncState!;
            try
            {
                asyncState.TrySetResult(((Socket) asyncState.Task.AsyncState!).EndReceive(iar));
            }
            catch (Exception ex)
            {
                asyncState.TrySetException(ex);
            }
        }, state);

        return state.Task;
    }
}
#endif
