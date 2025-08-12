
namespace Game.Application.Messaging
{
    public readonly struct EnterRoomCommand : ICommand
    {
        public readonly string RoomId;
        public EnterRoomCommand(string roomId) => RoomId = roomId;
    }
}