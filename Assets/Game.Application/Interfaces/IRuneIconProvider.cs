using Game.Domain.Enums;
using UnityEngine;

namespace Game.Application.Interfaces
{
    public interface IRuneIconProvider
    {
        Sprite GetRuneSprite(RuneType runeType);
    }
}