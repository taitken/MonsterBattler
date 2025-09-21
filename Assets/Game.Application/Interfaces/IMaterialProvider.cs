using UnityEngine;

namespace Game.Application.Interfaces
{
    public interface IMaterialProvider
    {
        Material GetGlowMaterial();
        Material GetMaterial(string materialName);
    }
}