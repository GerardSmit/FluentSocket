namespace FluentSocket.Tests.Messages;

public partial class MovePlayerMessage : IServerMessage
{
    public int X { get; set; }

    public int Y { get; set; }
}
