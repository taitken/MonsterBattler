using System;
using System.Collections.Generic;
using GameControllers.Services;
using Item.Models;
using ObjectComponents;
using UI.Services;
using Zenject;

namespace UnityEngine
{
    public abstract class MonoBaseObject : MonoBehaviour2
    {
        protected IItemObjectService itemObjectService;
        protected IUiPanelService uiPanelService;
        protected IList<GameObjectComponent> goComponents;

        [Inject]
        public void Construct(IItemObjectService _itemService,
                                IUiPanelService _uiPanelService)
        {
            this.itemObjectService = _itemService;
            this.uiPanelService = _uiPanelService;
            this.goComponents = new List<GameObjectComponent>();
        }
        public abstract BaseObjectModel GetBaseObjectModel();

        // Not to be overridden. Use Awake() instead.
        public void Awake()
        {
            BaseObjectModel test = this.GetBaseObjectModel();
            if (this.GetBaseObjectModel() != null)
            {
                ObjectHitPointsComponent hitPointsComponent = this.GetBaseObjectModel().GetObjectComponent<ObjectHitPointsComponent>();
                if (hitPointsComponent != null)
                {
                    hitPointsComponent.OnZeroHitPoints(this.Destroy);
                }
            }
        }

        public virtual void OnSelect()
        {

        }
        public T GetObjectComponent<T>() where T : GameObjectComponent
        {
            for (int i = 0; i < this.goComponents.Count; i++)
            {
                if (this.goComponents[i] is T)
                {
                    return this.goComponents[i] as T;
                }
            }
            return null;
        }
        protected override void BeforeDeath()
        {
            ObjectCompositionComponent oc = this.GetBaseObjectModel().GetObjectComponent<ObjectCompositionComponent>();
            ObjectStorageComponent os = this.GetBaseObjectModel().GetObjectComponent<ObjectStorageComponent>();
            if (oc != null)
            {
                oc.GetComposition().ForEach(item =>
                {
                    if (item.itemType != eItemType.OrganicMass)
                    {
                        this.itemObjectService.AddItemToWorld(new ItemObjectModel(this.GetBaseObjectModel().position, item, ItemObjectModel.eItemState.OnGround, true));
                    }
                });
            }
            if (os != null)
            {
                os.GetItems().ForEach(item =>
                {
                    item.itemState = ItemObjectModel.eItemState.OnGround;
                    this.itemObjectService.onItemPickupOrDropTrigger.Set(item);
                });
            }
            if (this.GetBaseObjectModel() != null) this.uiPanelService.RemoveContext(this.GetBaseObjectModel().ID);
            if (this.GetBaseObjectModel() != null) this.uiPanelService.RemovePanelsForObject(this.GetBaseObjectModel().ID);
        }
    }
}