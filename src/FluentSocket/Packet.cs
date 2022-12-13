using System;

namespace FluentSocket;

public readonly struct Packet : IDisposable
{
    private readonly IDisposable _memoryOwner;
    private readonly ReadOnlyMemory<byte> _memory;

    public Packet(ReadOnlyMemory<byte> memory, IDisposable memoryOwner)
    {
        _memoryOwner = memoryOwner;
        _memory = memory;
    }

    public ReadOnlyMemory<byte> Memory => _memory;

    public ReadOnlySpan<byte> Span => _memory.Span;

    public void Dispose() => _memoryOwner?.Dispose();
}
