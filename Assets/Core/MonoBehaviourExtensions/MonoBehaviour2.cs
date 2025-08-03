using System;
using System.Collections.Generic;
using UnityEngine;
using UtilityClasses;

namespace UnityEngine
{
    public abstract class MonoObject : MonoBehaviour
    {

        private IList<Action> onDeathCallbacks = new List<Action>();
        private IList<Subscription> subscriptions = new List<Subscription>();


        public virtual void OnClickedByUser()
        {

        }

        public virtual void OnMouseEnter()
        {

        }

        public virtual void OnMouseOver()
        {

        }

        public virtual void OnMouseExit()
        {

        }

        public void BeforeDestroy(Action callback)
        {
            this.onDeathCallbacks.Add(callback);
        }

        protected virtual void BeforeDeath()
        {

        }

        protected void UpdateBoxColliderToFitChildren()
        {
            BoxCollider2D boxCollider2D = this.GetComponent<BoxCollider2D>();
            if (boxCollider2D != null)
            {
                bool hasBounds = false;
                Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

                foreach (Transform child in this.transform)
                {
                    Renderer childRenderer = child.gameObject.GetComponent<Renderer>();
                    if (childRenderer != null)
                    {
                        if (hasBounds)
                        {
                            bounds.Encapsulate(childRenderer.bounds);
                        }
                        else
                        {
                            bounds = childRenderer.bounds;
                            hasBounds = true;
                        }
                    }
                }
                boxCollider2D.offset = bounds.center - this.transform.position;
                boxCollider2D.size = bounds.size;
            }
        }

        public void AddSubscription(Subscription sub)
        {
            this.subscriptions.Add(sub);
        }

        public void Destroy()
        {
            this.subscriptions.ForEach(sub =>
            {
                sub.unsubscribe();
            });
            foreach (Action callback in this.onDeathCallbacks)
            {
                callback();
            }
            this.OnMouseExit();
            this.BeforeDeath();
            Destroy(gameObject);
        }

        protected IServiceType Inject<IServiceType>()
        {
            return ServiceContainer.Instance.Resolve<IServiceType>();
        }
    }
}