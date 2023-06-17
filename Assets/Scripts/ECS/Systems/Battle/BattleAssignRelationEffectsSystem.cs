using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleAssignRelationEffectsSystem<T> : IEcsRunSystem
        where T : struct, IPackedWithWorldRef
    {
        protected virtual RelationSubjectState SubjectState => 
            RelationSubjectState.Attacking;

        protected readonly EcsWorldInject ecsWorld;
        
        protected readonly EcsPoolInject<T> pool = default;
        protected readonly EcsPoolInject<RelEffectProbeComp> relEffectProbePool = default;
        protected readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        protected readonly EcsPoolInject<HeroInstanceOriginRef> heroInstanceOriginRefPool = default;
        protected readonly EcsPoolInject<HeroConfigRef> heroConfigRefPool = default;

        protected readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo, T>> filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var heroInstanceRef = ref pool.Value.Get(entity);
                if (!heroInstanceRef.Packed.Unpack(out _, out var effectTargetEntity))
                    continue;

                if (!playerTeamTagPool.Value.Has(effectTargetEntity))
                    continue;

                ref var effTargetOriginRef = ref heroInstanceOriginRefPool.Value.Get(effectTargetEntity);
                if (!effTargetOriginRef.Packed.Unpack(out var origWorld, out var effectTargetOrig))
                    continue;

                ref var heroConfigRef = ref heroConfigRefPool.Value.Get(effectTargetEntity);

                var matrixFilter = origWorld.Filter<RelationsMatrixComp>().End();
                var matrixPool = origWorld.GetPool<RelationsMatrixComp>();

                foreach (var matrixEntity in matrixFilter)
                {
                    ref var matrixComp = ref matrixPool.Get(matrixEntity);
                    foreach (var item in matrixComp.Matrix)
                    {
                        // matching only one side of the key to avoid duplicates
                        if (!item.Key.Item1.EqualsTo(effTargetOriginRef.Packed))
                            continue;

                        var relEffectProbeEntity = ecsWorld.Value.NewEntity();
                        ref var probeComp = ref relEffectProbePool.Value.Add(relEffectProbeEntity);
                        probeComp.TargetOrigPacked = item.Key.Item1;
                        probeComp.SourceOrigPacked = item.Key.Item2;
                        probeComp.TargetConfigRefPacked = heroConfigRef.Packed;
                        probeComp.P2PEntityPacked = item.Value;
                        probeComp.SubjectState = SubjectState;
                        probeComp.TurnEntity = systems.GetWorld().PackEntity(entity);
                    }
                }

            }
        }

        
    }
}
