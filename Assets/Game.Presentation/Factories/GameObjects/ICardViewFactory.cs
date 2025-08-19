using Game.Domain.Entities.Abilities;
using Game.Presentation.GameObjects.Card;
using UnityEngine;

namespace Game.Presentation.GameObjects.Factories
{
    public interface ICardViewFactory
    {
        CardView Create(AbilityCard model, Vector3 spawnPoint);
    }
}