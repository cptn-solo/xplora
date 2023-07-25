using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Just resets relations effects container before next turn brings new effects there
    /// </summary>
    public class BattleSceneRelationEffectResetVisualSystem :
        BattleSceneVisualSystem<RelEffectResetVisualsInfo>
    {
        private readonly EcsPoolInject<ItemsContainerRef<RelationEffectInfo>> effectsViewRefPool = default;

        protected override void AssignVisualizer(int entity, RelEffectResetVisualsInfo visualInfo, EcsWorld world, int viewEntity)
        {
            if (!visualInfo.SubjectEntity.Unpack(out _, out var subjectEntity))
                throw new Exception("Stale effect subject entity");

            if (!effectsViewRefPool.Value.Has(subjectEntity))
                throw new Exception("No view for effect animation");

            ref var viewRef = ref effectsViewRefPool.Value.Get(subjectEntity);
            viewRef.Container.Reset();

            base.AssignVisualizer(entity, visualInfo, world, viewEntity);
        }
    }
}
