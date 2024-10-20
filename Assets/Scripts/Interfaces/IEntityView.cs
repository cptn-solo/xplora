﻿using Assets.Scripts.Services;
using Leopotam.EcsLite;

namespace Assets.Scripts
{
    public delegate T DataLoadDelegate<T>(EcsPackedEntityWithWorld? entity);
    public delegate IEntityView<T> EntityViewFactory<T>(EcsPackedEntityWithWorld? packedEntity)
        where T : struct;

    public interface IEntityView : ITransform //BattleUnit, Overlay, etc.
    {
        public void AttachChild<C>(ITransform<C> child) where C : struct;
        public void AttachChild<C>(IDataView<C> child) where C : struct;
        public void AttachChild<C>(IItemsContainer<C> child) where C : struct;
        public void DetachChild<C>(ITransform<C> child) where C : struct;
        public void DetachChild<C>(IDataView<C> child) where C : struct;
        public void DetachChild<C>(IItemsContainer<C> child) where C : struct;
        
        public void Decommision(); // drop entity view ref before destruction
                                   // (important for disabled views)
    }

    public interface IEntityButton<T> : ITransform<T>
    {
        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public IEcsService EcsService { get; set; }
        public void Toggle(bool toggle);
    }

    public interface IEntityView<T> : IEntityView
    {
        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public IEcsService EcsService { get; set; }
        public DataLoadDelegate<T> DataLoader { get; set; }
        public void UpdateData();
        public void Destroy();

        /// <summary>
        /// View is responsible to visualize provided data somehow and remove the visuals
        /// entity when complete
        /// </summary>
        /// <typeparam name="V">type of the visuals info</typeparam>
        /// <param name="visualInfo">visuals to perform</param>
        /// <param name="visualEntity">Should be removed upon complete</param>
        public void Visualize<V>(V visualInfo, EcsPackedEntityWithWorld visualEntity) where V : struct, ISceneVisualsInfo;
    }

}