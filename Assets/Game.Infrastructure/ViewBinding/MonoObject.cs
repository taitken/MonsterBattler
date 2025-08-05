using System;
using UnityEngine;

public abstract class MonoObject<ModelType> : MonoBehaviour, IMonoObject where ModelType : BaseObjectModel
{
    public ModelType model;

    public virtual void Bind(ModelType model)
    {
        this.model = model;
        model.SetView(this);
        OnModelBound();
    }

    public BaseObjectModel GetModel() => model;

    public MonoBehaviour AsMonoBehaviour() => this;

    protected abstract void OnModelBound();

    protected virtual void BeforeDeath() { }

    public void Destroy()
    {
        this.BeforeDeath();
        Destroy(gameObject);
    }

    protected static IServiceType Inject<IServiceType>()
    {
        return ServiceContainer.Instance.Resolve<IServiceType>();
    }
}