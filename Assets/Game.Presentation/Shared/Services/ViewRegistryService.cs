using System;
using System.Collections.Generic;
using Game.Presentation.Core.Interfaces;
using UnityEngine;

namespace Game.Presentation.Services
{
    public sealed class ViewRegistryService : IViewRegistryService
    {
        private readonly Dictionary<Guid, MonoBehaviour> _views = new();

        public void Register(Guid id, MonoBehaviour view)
        {
            _views[id] = view;
        }
        public void Unregister(Guid id)
        {
            _views.Remove(id);
        }
        public bool TryGet<T>(Guid id, out T view)
        {
            if (_views.TryGetValue(id, out var obj) && obj is T tView)
            {
                view = tView;
                return true;
            }
            view = default(T);
            return false;
        }
    }
}