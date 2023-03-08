﻿using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateUnitOverlaySystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<PowerComp> powerPool;
        /// <summary>
        /// BarInfo shouldn't be messed up with BarSInfo wich is parent empty
        /// entity view that now contains only bars child
        /// </summary>
        private readonly EcsPoolInject<ItemsContainerRef<BarInfo>> overlayPool;

        private readonly EcsFilterInject<
            Inc<UpdateTag, ItemsContainerRef<BarInfo>, PowerComp>> updateFilter;


        public void Run(IEcsSystems systems)
        {
            foreach (var entity in updateFilter.Value)
            {
                ref var powerComp = ref powerPool.Value.Get(entity);
                ref var overlayRef = ref overlayPool.Value.Get(entity);

                overlayRef.Container.SetItems(powerComp.BarsInfo);
            }

        }
    }
}