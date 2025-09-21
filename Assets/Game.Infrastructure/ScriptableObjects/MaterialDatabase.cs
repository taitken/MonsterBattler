using System.Linq;
using Game.Infrastructure.DTOs;
using UnityEngine;

namespace Game.Infrastructure.ScriptableObjects
{
    /// <summary>
    /// Database containing material mappings for visual effects.
    /// </summary>
    [CreateAssetMenu(fileName = "MaterialDatabase", menuName = "App/Databases/MaterialDatabase")]
    public class MaterialDatabase : ScriptableObject
    {
        [SerializeField] private Material _glowMaterial;
        [SerializeField] private MaterialData[] _materials;

        public Material GetGlowMaterial()
        {
            return _glowMaterial;
        }

        public Material GetMaterial(string materialName)
        {
            return _materials.FirstOrDefault(m => m.name == materialName)?.material;
        }
    }
}