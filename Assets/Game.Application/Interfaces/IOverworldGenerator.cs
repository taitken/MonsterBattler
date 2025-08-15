using Game.Domain.Entities.Overworld;

namespace Game.Application.Interfaces
{
    public interface IOverworldGenerator
    {
        OverworldEntity GenerateOverworld();
    }
}