using System;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleClearUsedFocusSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;
        private readonly EcsPoolInject<EffectFocusComp> focusPool = default;
        private readonly EcsPoolInject<UsedFocusEntityTag> usedFocusPool = default;
        

        private readonly EcsFilterInject<Inc<UsedFocusEntityTag, EffectFocusComp>> usedFocusFilter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var ufEntity in usedFocusFilter.Value)
            {
                ref var focusComp = ref focusPool.Value.Get(ufEntity);
                if (!focusComp.Actor.Unpack(out var world, out var entity))
                    throw new Exception("Stale Focus Actor");

                // the view itself should be scheduled fo desruction by BattleScheduleRelationEffectFocusResetVisualSystem
                // and then desrtoyed by BattleSceneRelationEffectFocusResetVisualSystem
                //if (aimIconPool.Value.Has(ufEntity))
                //{
                //    ref var iconView = ref aimIconPool.Value.Get(ufEntity);
                //    GameObject.Destroy(iconView.Transform.gameObject);
                //}

                ref var relEffects = ref relEffectsPool.Value.Get(entity);                
                relEffects.RemoveByType(focusComp.EffectKey.RelationsEffectType, out var decrement);
                
                if (decrement != null)
                {
                    foreach (var item in decrement)
                        if (item.Unpack(out var origWorld, out var origEntity))
                            origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, origEntity);
                }

                focusPool.Value.Del(ufEntity);
                usedFocusPool.Value.Del(ufEntity);
            }
        }
    }
}
