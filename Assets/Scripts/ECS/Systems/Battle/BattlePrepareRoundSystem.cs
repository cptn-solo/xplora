using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattlePrepareRoundSystem : IEcsRunSystem
    {
        private const int minRoundsQueue = 4;

        private readonly EcsWorldInject ecsWorld;

        private readonly EcsPoolInject<HeroInstanceRefComp> heroInstanceRefPool;
        private readonly EcsPoolInject<PlayerTeamTag> playerTeamTagPool;
        private readonly EcsPoolInject<NameComp> namePool;
        private readonly EcsPoolInject<SpeedComp> speedPool;

        private readonly EcsFilterInject<Inc<BattleRoundInfo>> roundInfoFilter;

        private readonly EcsCustomInject<BattleManagementService> battleService;

        public void Run(IEcsSystems systems)
        {
            if (battleService.Value.CurrentBattle.State != BattleState.BattleInProgress)
                return;

            if (GetEcsRoundsCount() is var count && count < minRoundsQueue)
            {
                EnqueueEcsRound();
            }
        }

        private void EnqueueEcsRound()
        {
            ref var battleInfo = ref battleService.Value.CurrentBattle;

            var roundEntity = ecsWorld.Value.NewEntity();

            var roundPool = ecsWorld.Value.GetPool<BattleRoundInfo>();

            ref var roundInfo = ref roundPool.Add(roundEntity);
            roundInfo.Round = ++battleInfo.LastRoundNumber;

            PrepareRound(ref roundInfo, ref battleInfo);

            battleInfo.LastRoundNumber = roundInfo.Round;
        }

        private int GetEcsLastRoundNumber()
        {
            ref var battleInfo = ref battleService.Value.CurrentBattle;
            return battleInfo.LastRoundNumber;
        }

        private int GetEcsRoundsCount()
        {
            var filter = roundInfoFilter.Value;
            return filter.GetEntitiesCount();
        }
        ///     Enqueue heroes 
        /// </summary>
        /// <param name="info">Draft round</param>
        /// <returns>Round with heroes</returns>
        internal void PrepareRound(ref BattleRoundInfo info, ref BattleInfo battleInfo)
        {
            info.State = RoundState.PrepareRound;

            var buffer = ListPool<RoundSlotInfo>.Get();

            foreach (var heroInstance in battleService.Value.PlayerHeroes.Concat(
                battleService.Value.EnemyHeroes))
            {
                if (!heroInstance.HeroInstancePackedEntity.Unpack(
                    out var world, out var heroInstanceEntity))
                    continue;

                ref var heroInstanceRef = ref heroInstanceRefPool.Value.Get(heroInstanceEntity);

                RoundSlotInfo slotInfo = new RoundSlotInfo()
                {
                    HeroInstancePackedEntity = heroInstance.HeroInstancePackedEntity,
                    HeroName = namePool.Value.Get(heroInstanceEntity).Name,
                    Speed = speedPool.Value.Get(heroInstanceEntity).Speed,
                    TeamId = playerTeamTagPool.Value.Has(heroInstanceEntity) ?
                        battleInfo.PlayerTeam.Id :
                        battleInfo.EnemyTeam.Id
                };
                buffer.Add(slotInfo);
            }

            var orderedHeroes = buffer.OrderByDescending(x => x.Speed);
            ListPool<RoundSlotInfo>.Add(buffer);

            Dictionary<int, List<RoundSlotInfo>> speedSlots = new();
            foreach (var hero in orderedHeroes)
            {
                if (speedSlots.TryGetValue(hero.Speed, out var slots))
                    slots.Add(hero);
                else
                    speedSlots[hero.Speed] = new List<RoundSlotInfo>() { hero };
            }

            var speedKeys = speedSlots.Keys.OrderByDescending(x => x);

            var queue = ListPool<RoundSlotInfo>.Get();

            foreach (var speed in speedKeys)
            {
                var slots = speedSlots[speed];
                while (slots.Count() > 0)
                {
                    var choosenIdx = slots.Count() == 1 ? 0 : Random.Range(0, slots.Count());
                    queue.Add(slots[choosenIdx]);
                    slots.RemoveAt(choosenIdx);
                }
            }

            info.QueuedHeroes = queue.ToArray();

            ListPool<RoundSlotInfo>.Add(queue);

            info.State = RoundState.RoundPrepared;
        }

    }
}
