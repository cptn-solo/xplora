﻿using System;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattlePrepareRevengeTurnsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<BattleRoundInfo> roundInfoPool = default;
        private readonly EcsPoolInject<RetiredTag> retiredTagPool = default;
        private readonly EcsPoolInject<PrepareRevengeComp> revengePool = default;
        private readonly EcsPoolInject<IntValueComp<SpeedTag>> speedPool = default;
        private readonly EcsPoolInject<NameValueComp<NameTag>> namePool = default;
        private readonly EcsPoolInject<NameValueComp<IconTag>> iconNamePool = default;
        private readonly EcsPoolInject<NameValueComp<IdleSpriteTag>> idleSpriteNamePool = default;

        private readonly EcsFilterInject<Inc<PrepareRevengeComp>> revengeFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            if (revengeFilter.Value.GetEntitiesCount() <= 0) 
                return;

            if (!battleService.Value.BattleEntity.Unpack(out var world, out var battleEntity))
                throw new Exception("No battle");

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            if (!battleService.Value.RoundEntity.Unpack(out _, out var roundentity))
                throw new Exception($"No Round");

            ref var roundInfo = ref roundInfoPool.Value.Get(roundentity); 
            
            var buffer = ListPool<RoundSlotInfo>.Get();
            buffer.AddRange(roundInfo.QueuedHeroes);

            foreach (var entity in revengeFilter.Value)
            {
                ref var revengeComp = ref revengePool.Value.Get(entity);
                var packedRevenger = revengeComp.RevengeBy;
                if (!revengeComp.RevengeBy.Unpack(out _, out var revengerEntity))
                    throw new Exception($"Stale revenger entity");

                if (retiredTagPool.Value.Has(revengerEntity))
                    continue; // died hero can't revenge

                var idx = buffer.FindIndex(x => x.HeroInstancePackedEntity.EqualsTo(packedRevenger));
                if (idx < 0)
                {
                    RoundSlotInfo slotInfo = new ()
                    {
                        HeroInstancePackedEntity = revengeComp.RevengeBy,
                        HeroName = namePool.Value.Get(revengerEntity).Name,
                        Speed = speedPool.Value.Get(revengerEntity).Value,
                        TeamId = battleInfo.PlayerTeam.Id,
                        IconName = iconNamePool.Value.Get(revengerEntity).Name,
                        IdleSpriteName = idleSpriteNamePool.Value.Get(revengerEntity).Name,
                    };
                    buffer.Insert(0, slotInfo);
                }
                else if (idx > 0)
                {
                    var slotInfo = buffer[idx];
                    buffer.RemoveAt(idx);
                    buffer.Insert(0, slotInfo);
                }
            }

            roundInfo.QueuedHeroes = buffer.ToArray();
        
            ListPool<RoundSlotInfo>.Add(buffer);
        }
    }
}
