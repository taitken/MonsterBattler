using UnityEngine;

namespace System
{
    public abstract class BaseObjectModel : BaseModel
    {
        public event Action OnModelUpdated;
        private IMonoObject _view;

        public void SetView(IMonoObject view)
        {
            _view = view;
        }

        public TView GetView<TView>() where TView : MonoBehaviour
        {
            return _view?.AsMonoBehaviour() as TView;
        }
    }
}