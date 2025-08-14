
using Game.Core;
using Game.Domain;
using Game.Presentation.Services;
using UnityEngine;

namespace Game.Presentation
{
    public abstract class MonoObject<ModelType> : MonoBehaviour, IMonoObject where ModelType : BaseEntity
    {
        public ModelType model;
        protected IViewRegistryService _viewRegistry;
        public virtual void Bind(ModelType model)
        {
            this.model = model;
            OnModelBound();
        }

        public BaseEntity GetModel() => model;

        public MonoBehaviour AsMonoBehaviour() => this;

        protected abstract void OnModelBound();
    }
}
