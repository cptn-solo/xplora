using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleSceneRelationEffectCastVisualSystem : 
        BattleSceneVisualSystem<RelEffectCastVisualsInfo>
    {
        private readonly EcsPoolInject<ItemsContainerRef<RelationEffectInfo>> effectsViewRefPool = default;

        protected override void AssignVisualizer(int entity, RelEffectCastVisualsInfo visualInfo, EcsWorld world, int viewEntity)
        {
            if (!visualInfo.SubjectEntity.Unpack(out _, out var subjectEntity))
                throw new Exception("Stale effect subject entity");

            if (!effectsViewRefPool.Value.Has(subjectEntity))
                throw new Exception("No view for effect animation");

            ref var viewRef = ref effectsViewRefPool.Value.Get(subjectEntity);            
            viewRef.Container.SetItemInfoAnimatedMove(visualInfo.EffectInfo, visualInfo.SourceTransform);

            // TODO: remove after the animation implementation
            base.AssignVisualizer(entity, visualInfo, world, viewEntity);
        }
    }
}
