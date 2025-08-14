
using System;

namespace Game.Application.Messaging
{
    public readonly struct EnterRoomCommand : ICommand
    {
        public readonly Guid RoomId;
        public EnterRoomCommand(Guid roomId) => RoomId = roomId;
    }
}