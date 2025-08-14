using System;
using UnityEngine;

namespace Game.Presentation.Services
{
    public interface IViewRegistryService
    {
        public void Register(Guid id, MonoBehaviour view);
        public void Unregister(Guid id);
        public bool TryGet<T>(Guid id, out T view);
    }
}