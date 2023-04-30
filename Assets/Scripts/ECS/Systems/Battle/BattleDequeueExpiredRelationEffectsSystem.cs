using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleDequeueExpiredRelationEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<UpdateTag<RelationEffectInfo>> updatePool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;

        private readonly EcsFilterInject<Inc<RelationEffectsComp>> currentEffectFilter = default;
        private readonly EcsFilterInject<Inc<BattleRoundInfo, GarbageTag>> filter = default;

        private readonly EcsCustomInject<RaidService> raidService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var roundInfo = ref roundInfoPool.Value.Get(entity);
         
                // for both battle and raid worlds and remove expired effects:
                DequeueRelationEffects(systems.GetWorld(), roundInfo.Round);

                if (raidService.Value.EcsWorld != null)
                    DequeueRelationEffects(raidService.Value.EcsWorld, roundInfo.Round);

                // enqueue view update to reflect changes (if any)
                foreach (var effectsEntity in currentEffectFilter.Value)
                    if (!updatePool.Value.Has(effectsEntity))
                        updatePool.Value.Add(effectsEntity);                    

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
                    relEffect.CurrentEffects.Remove(item);

                buff.Clear();
            }
            
            ListPool<RelationEffectKey>.Add(buff);
        }

    }
}
