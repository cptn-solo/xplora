using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueExpiredRelationEffectsSystem : BaseEcsSystem
    {
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;

        private readonly EcsFilterInject<Inc<BattleRoundInfo, GarbageTag>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var roundInfo = ref roundInfoPool.Value.Get(entity);
         
                // for both battle and raid worlds and remove expired effects:
                DequeueRelationEffects(systems.GetWorld(), roundInfo.Round);
            }
        }

        private void DequeueRelationEffects(EcsWorld world, int round)
        {
            var buff = ListPool<RelationEffectKey>.Get();
            var filter = world.Filter<RelationEffectsComp>().End();
            var pool = world.GetPool<RelationEffectsComp>();
            foreach (var entity in filter)
            {
                ref var relEffect = ref pool.Get(entity);
                
                foreach (var item in relEffect.CurrentEffects)
                    if (item.Value.EndRound <= round)
                        buff.Add(item.Key);

                foreach (var item in buff)
                {
                    var effect = relEffect.CurrentEffects[item];
                    if (effect.EffectP2PEntity.Unpack(out var origWorld, out var p2pEntity))
                        origWorld.IncrementIntValue<RelationEffectsCountTag>(-1, p2pEntity);

                    relEffect.CurrentEffects.Remove(item);
                }

                buff.Clear();
            }
            
            ListPool<RelationEffectKey>.Add(buff);
        }
    }
}
