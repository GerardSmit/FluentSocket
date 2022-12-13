using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

namespace FluentSocket;

public abstract class Layer
{
    public abstract ValueTask SendAsync(SendDelegate next, Memory<byte> data);

    public abstract ValueTask ReceiveAsync(ReceiveDelegate next, Memory<byte> data);
}

public readonly struct ReceiveRequest : IDisposable
{
    private readonly IDisposable _owner;

    public ReceiveRequest(Memory<byte> memory, IDisposable owner)
    {
        _owner = owner;
        Memory = memory;
    }

    public Memory<byte> Memory { get; }

    public void Dispose()
    {
        _owner?.Dispose();
    }
}

public delegate ValueTask ReceiveDelegate(Memory<byte> data);

public delegate ValueTask SendDelegate(Memory<byte> data);
