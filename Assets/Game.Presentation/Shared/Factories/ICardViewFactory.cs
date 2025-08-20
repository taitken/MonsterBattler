using Game.Domain.Entities.Abilities;
using Game.Presentation.Shared.Views;
using UnityEngine;

namespace Game.Presentation.Shared.Factories
{
    public interface ICardViewFactory
    {
        CardView Create(AbilityCard model, Vector3 spawnPoint);
    }
}