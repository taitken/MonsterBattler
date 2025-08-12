using Game.Domain;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Interface for MonoBehaviour objects that bind to a model.
    /// </summary>
    public interface IMonoObject
    {
        BaseEntity GetModel();
        MonoBehaviour AsMonoBehaviour();
    }
}