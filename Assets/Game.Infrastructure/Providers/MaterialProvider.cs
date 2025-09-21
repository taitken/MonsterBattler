using Game.Application.Interfaces;
using Game.Infrastructure.ScriptableObjects;
using UnityEngine;

namespace Game.Infrastructure.Providers
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly MaterialDatabase _database;

        public MaterialProvider(MaterialDatabase database)
        {
            _database = database;
        }

        public Material GetGlowMaterial()
        {
            return _database.GetGlowMaterial();
        }

        public Material GetMaterial(string materialName)
        {
            return _database.GetMaterial(materialName);
        }
    }
}