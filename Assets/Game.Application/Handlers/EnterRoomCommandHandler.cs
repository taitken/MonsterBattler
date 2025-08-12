using Game.Application.Messaging;
using Game.Core;
using Game.Domain.Enums;
using UnityEngine;


namespace Game.Application.Handlers
{

public class EnterRoomHandler : ICommandHandler<EnterRoomCommand>
{
    private readonly IEventBus _bus;

    public EnterRoomHandler()
    {
        Debug.Log("EnterRoomHandler created");
        _bus = ServiceLocator.Get<IEventBus>();
    }

    public void Handle(EnterRoomCommand command)
    {
        _bus.Publish(new LoadSceneCommand(GameScene.BattleScene));
    }
}
}