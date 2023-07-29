using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    /// <summary>
    /// Just resets focus effect icon container before next turn brings new effects there
    /// </summary>
    public class BattleSceneRelationEffectFocusResetVisualSystem :
        BattleSceneVisualSystem<RelationEffectsFocusResetInfo>
    {
        private readonly EcsPoolInject<TransformRef<AimTargetTag>> aimIconPool = default;
        
        protected override void AssignVisualizer(int entity, RelationEffectsFocusResetInfo visualInfo, EcsWorld world, int viewEntity)
        {
            if (!aimIconPool.Value.Has(viewEntity))
                throw new Exception("No view for focus animation");

            ref var viewRef = ref aimIconPool.Value.Get(viewEntity);
            GameObject.Destroy(viewRef.Transform.gameObject);

            aimIconPool.Value.Del(viewEntity);

            base.AssignVisualizer(entity, visualInfo, world, viewEntity);
        }
    }
}
