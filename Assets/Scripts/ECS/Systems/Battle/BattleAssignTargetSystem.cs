using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{

    public class BattleAssignTargetSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<TargetRef> targetRefPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnPool;
        private readonly EcsPoolInject<AttackerRef> attackerRefPool;
        private readonly EcsPoolInject<RangedTag> rangedTagPool;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool;
        private readonly EcsPoolInject<HeroConfigRefComp> heroConfigRefPool;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>, Exc<SkippedTag>> filter;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
                AssignTarget(entity);
        }

        private void AssignTarget(int turnEntity)
        {
            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            ref var attackerRef = ref attackerRefPool.Value.Get(turnEntity);

            if (!attackerRef.HeroInstancePackedEntity.Unpack(out var world, out var attackerInstanceEntity))
                throw new Exception("No Attacker");

            var isPlayerTeam = playerTeamTagPool.Value.Has(attackerInstanceEntity);

            var targetEntity = -1;
            Hero targetConfig = default;

            var ranged = rangedTagPool.Value.Has(attackerInstanceEntity);
            if (ranged)
            {
                var filter = isPlayerTeam ?
                    TeamTagFilter<EnemyTeamTag>(world) :
                    TeamTagFilter<PlayerTeamTag>(world);
                var targets = filter.GetRawEntities();
                targetEntity = targets.Length > 0 ?
                    targets[Random.Range(0, targets.Length)] :
                    -1;
            }
            else
            {
                var filterFront = isPlayerTeam ?
                    TeamTagFilter<EnemyTeamTag, FrontlineTag>(world) :
                    TeamTagFilter<PlayerTeamTag, FrontlineTag>(world);

                var filterBack = isPlayerTeam ?
                    TeamTagFilter<EnemyTeamTag, BacklineTag>(world) :
                    TeamTagFilter<PlayerTeamTag, BacklineTag>(world);

                var frontTargets = filterFront.GetRawEntities();
                var backTargets = filterBack.GetRawEntities();

                // TODO: consider range (not yet imported/parsed)

                targetEntity = frontTargets.Length > 0 ?
                    frontTargets[Random.Range(0, frontTargets.Length)] :
                    backTargets.Length > 0 ?
                    backTargets[Random.Range(0, backTargets.Length)] :
                    -1;
            }

            if (targetEntity != -1)
            {
                ref var targetConfigRef = ref heroConfigRefPool.Value.Get(targetEntity);

                if (!targetConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var targetConfigEntity))
                    throw new Exception("No Hero Config");

                var heroConfigPool = libWorld.GetPool<Hero>();
                ref var targetConfigTemp = ref heroConfigPool.Get(targetConfigEntity);

                targetConfig = targetConfigTemp;
            }

            turnInfo.Target = targetConfig;
            turnInfo.State = targetEntity == -1 ?
                TurnState.NoTargets : TurnState.TurnPrepared;
        }

        private EcsFilter TeamTagFilter<T, L>(EcsWorld world) where T : struct where L : struct
        {
            return world.Filter<T>()
                        .Inc<L>()
                        .Exc<DeadTag>().End();
        }
        private EcsFilter TeamTagFilter<T>(EcsWorld world) where T : struct
        {
            return world.Filter<T>()
                        .Exc<DeadTag>().End();
        }


    }
}
