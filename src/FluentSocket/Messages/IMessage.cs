namespace FluentSocket;

public interface IMessage
{
    void Write(ref MessageWriter writer);

    void Read(ref MessageReader reader);
}

public interface IUnionMessage<T>
{
#if NET7_0_OR_GREATER
    static abstract void Write(ref MessageWriter writer, T value);

    static abstract T Read(ref MessageReader reader);
#endif
}
