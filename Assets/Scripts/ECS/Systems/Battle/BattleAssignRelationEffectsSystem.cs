using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleAssignRelationEffectsSystem<T> : BaseEcsSystem
        where T : struct, IPackedWithWorldRef
    {
        protected virtual RelationSubjectState SubjectState => 
            RelationSubjectState.Attacking;

        protected readonly EcsWorldInject ecsWorld;
        
        protected readonly EcsPoolInject<T> pool = default;
        protected readonly EcsPoolInject<RelEffectProbeComp> relEffectProbePool = default;
        protected readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool = default;
        protected readonly EcsPoolInject<HeroInstanceOriginRef> heroInstanceOriginRefPool = default;

        protected readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo, T>> filter = default;

        public override void RunIfActive(IEcsSystems systems)
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
                
                var origHeroConfigRefPool = origWorld.GetPool<HeroConfigRef>();
                
                ref var tgtHeroConfigRef = ref origHeroConfigRefPool.Get(effectTargetOrig);

                var matrixFilter = origWorld.Filter<RelationsMatrixComp>().End();
                var matrixPool = origWorld.GetPool<RelationsMatrixComp>();

                foreach (var matrixEntity in matrixFilter)
                {
                    ref var matrixComp = ref matrixPool.Get(matrixEntity);
                    foreach (var item in matrixComp.Matrix)
                    {
                        // matching only one side of the key to avoid duplicates
                        if (!item.Key.Item2.EqualsTo(effTargetOriginRef.Packed))
                            continue;

                        if (!item.Key.Item1.Unpack(out _, out var effectSourceOrig))
                            throw new Exception("Stale effect source");

                        ref var srcHeroConfigRef = ref origHeroConfigRefPool.Get(effectSourceOrig);

                        var relEffectProbeEntity = ecsWorld.Value.NewEntity();
                        ref var probeComp = ref relEffectProbePool.Value.Add(relEffectProbeEntity);
                        probeComp.SourceOrigPacked = item.Key.Item1;
                        probeComp.SourceConfigRefPacked = srcHeroConfigRef.Packed;
                        probeComp.TargetOrigPacked = item.Key.Item2;
                        probeComp.TargetConfigRefPacked = tgtHeroConfigRef.Packed;
                        probeComp.SubjectState = SubjectState;
                        probeComp.P2PEntityPacked = item.Value;
                        probeComp.TurnEntity = systems.GetWorld().PackEntity(entity);
                    }
                }

            }
        }

        
    }
}
