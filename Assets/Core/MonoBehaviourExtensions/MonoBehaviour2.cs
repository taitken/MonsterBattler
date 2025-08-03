using System;
using System.Collections.Generic;
using GameControllers.Models;
using GameControllers.Services;
using UnityEngine;
using UtilityClasses;
using Zenject;

namespace UnityEngine
{
    public abstract class MonoBehaviour2 : MonoBehaviour
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

        public virtual void OnDrag(DragEventModel dragEvent)
        {

        }

        public virtual void OnDragEnd(DragEventModel dragEvent)
        {

        }

        public void BeforeDestroy(Action callback)
        {
            this.onDeathCallbacks.Add(callback);
        }

        protected virtual void BeforeDeath()
        {

        }

        public void SetMultiTilePosition(Vector3 localPosition)
        {
            SpriteRenderer sr = this.GetComponent<SpriteRenderer>();
            this.transform.position = localPosition + new Vector3(sr.bounds.size.x / 2, sr.bounds.size.y / 2)
            - new Vector3(IEnvironmentService.TILE_WIDTH_PIXELS / 2, IEnvironmentService.TILE_WIDTH_PIXELS / 2);
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

        protected T GetService<T>(IList<IBaseService> _services) where T : IBaseService
        {
            T returnService = default(T);
            try
            {
                returnService = (T)_services.Find(service =>
                {
                    return typeof(T).IsAssignableFrom(service.GetType());
                });
            }
            catch (System.Exception)
            {
                Debug.LogException(new System.Exception("Service type does not exist in the provided service list. Attemped type: " + typeof(T).ToString()));
            }
            return returnService;
        }
    }
}