using Game.Applcation.DTOs;
using Game.Application.Interfaces;
using Game.Application.Messaging;
using Game.Core;
using Game.Domain.Enums;


namespace Game.Application.Handlers
{
    public class EnterRoomHandler : ICommandHandler<EnterRoomCommand>
    {
        private readonly IEventBus _bus;
        private readonly INavigationService _nav;

        public EnterRoomHandler()
        {
            _bus = ServiceLocator.Get<IEventBus>();
            _nav = ServiceLocator.Get<INavigationService>();
        }

        public void Handle(EnterRoomCommand command)
        {
            _nav.SetPayload(GameScene.BattleScene, new BattlePayload(command.RoomId));
            _bus.Publish(new LoadSceneCommand(GameScene.BattleScene));
        }
    }
}