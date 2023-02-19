using System;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{

    public class BattlePrepareTurnSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<DraftTag> draftPool;
        private readonly EcsPoolInject<BattleTurnInfo> turnPool;
        private readonly EcsPoolInject<BattleRoundInfo> roundPool;

        private readonly EcsFilterInject<Inc<DraftTag, BattleTurnInfo>> filter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {                
                PrepareTurn(entity);
                draftPool.Value.Del(entity);
            }

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

        internal void PrepareTurn(int turnEntity)
        {
            if (!battleService.Value.RoundEntity.Unpack(out var world, out var roundEntity))
                throw new Exception("No Round");

            ref var roundInfo = ref roundPool.Value.Get(roundEntity);
            ref var turnInfo = ref turnPool.Value.Get(turnEntity);
            
            var roundSlot = roundInfo.QueuedHeroes[0];

            if (!roundSlot.HeroInstancePackedEntity.Unpack(out _, out var heroInstanceEntity))
                throw new Exception("No Hero instance");

            var heroInstanceRefPool = world.GetPool<HeroInstanceRefComp>();
            ref var heroInstanceRef = ref heroInstanceRefPool.Get(heroInstanceEntity);

            var effectsPool = world.GetPool<EffectsComp>();
            ref var effectsComp = ref effectsPool.Get(heroInstanceEntity);

            var attacker = heroInstanceRef;
            var heroConfigRefPool = world.GetPool<HeroConfigRefComp>();
            ref var attackerConfigRef = ref heroConfigRefPool.Get(heroInstanceEntity);

            if (!attackerConfigRef.HeroConfigPackedEntity.Unpack(out var libWorld, out var heroConfigEntity))
                throw new Exception("No Hero Config");

            var heroConfigPool = libWorld.GetPool<Hero>();
            ref var attackerCofig = ref heroConfigPool.Get(heroConfigEntity);

            if (effectsComp.SkipTurnActive)
            {
                BattleTurnInfo.Update(ref turnInfo, turnInfo.Turn, attackerCofig,
                    0, effectsComp.ActiveEffects.Keys.ToArray());
                turnInfo.State = TurnState.TurnSkipped;
            }
            else
            {
                var attackTeam = roundSlot.TeamId;
                ref var battleInfo = ref battleService.Value.CurrentBattle;

                var targetEntity = -1;
                Hero targetConfig = default;

                var ranged = world.GetPool<RangedTag>().Has(heroInstanceEntity);
                if (ranged)
                {
                    var filter = battleInfo.PlayerTeam.Id == attackTeam ?
                        TeamTagFilter<EnemyTeamTag>(world) :
                        TeamTagFilter<PlayerTeamTag>(world);
                    var targets = filter.GetRawEntities();
                    targetEntity = targets.Length > 0 ?
                        targets[Random.Range(0, targets.Length)] :
                        -1;
                }
                else
                {
                    var filterFront = battleInfo.PlayerTeam.Id == attackTeam ?
                        TeamTagFilter<EnemyTeamTag, FrontlineTag>(world) :
                        TeamTagFilter<PlayerTeamTag, FrontlineTag>(world);

                    var filterBack = battleInfo.PlayerTeam.Id == attackTeam ?
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
                    ref var targetConfigRef = ref heroConfigRefPool.Get(targetEntity);

                    if (!targetConfigRef.HeroConfigPackedEntity.Unpack(out _, out var targetConfigEntity))
                        throw new Exception("No Hero Config");

                    ref var targetConfigTemp = ref heroConfigPool.Get(targetConfigEntity);

                    targetConfig = targetConfigTemp;
                }

                BattleTurnInfo.Update(ref turnInfo, turnInfo.Turn, attackerCofig, targetConfig,
                    0, effectsComp.ActiveEffects.Keys.ToArray(), null);

                turnInfo.State = targetEntity == -1 ?
                    TurnState.NoTargets : TurnState.TurnPrepared;
            }
        }
    }
}
