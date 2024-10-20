﻿using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class GarbageCollectorSystem : BaseEcsSystem
    {
        private readonly EcsWorldInject ecsWorld = default;

        private readonly EcsFilterInject<Inc<GarbageTag>> garbageTagFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entityToDestroy in garbageTagFilter.Value)
                ecsWorld.Value.DelEntity(entityToDestroy);
        }
    }
}