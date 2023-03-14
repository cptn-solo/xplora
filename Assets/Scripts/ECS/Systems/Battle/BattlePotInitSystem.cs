using Assets.Scripts.Services;
using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace Assets.Scripts.ECS.Systems
{
    public class BattlePotInitSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<BattleInfo> battleInfoPool = default;
        private readonly EcsPoolInject<HPComp> hpPool = default;
        private readonly EcsPoolInject<BattlePotComp> battlePotPool = default;

        private readonly EcsFilterInject<
            Inc<BattleInfo, BattleFieldComp>,
            Exc<DraftTag<BattleInfo>, BattlePotComp>> battleFilter = default;

        private readonly EcsFilterInject<
            Inc<HPComp, EnemyTeamTag>> enemyFilter = default;

        private readonly EcsFilterInject<
            Inc<HPComp, PlayerTeamTag>> playerFilter = default;

        private readonly EcsCustomInject<BattleManagementService> battleService = default;

        public void Run(IEcsSystems systems)
        {
            if (!battleService.Value.BattleEntity.Unpack(out var battleWorld, out var battleEntity))
                return;

            if (battleFilter.Value.GetEntitiesCount() == 0)
                return;

            ref var battleInfo = ref battleInfoPool.Value.Get(battleEntity);

            var playerHP = 0;
            foreach (var entity in playerFilter.Value)
            {
                ref var hpComp = ref hpPool.Value.Get(entity);
                playerHP += hpComp.Value;
            }

            var enemyHP = 0;
            foreach (var entity in enemyFilter.Value)
            {
                ref var hpComp = ref hpPool.Value.Get(entity);
                enemyHP += hpComp.Value;
            }
            var koeff = (float)enemyHP / playerHP;
            var potValue = (int)(koeff * enemyHP);

            ref var pot = ref battlePotPool.Value.Add(battleEntity);
            pot.PotAssets = new Asset[] {
                new Asset(){
                    AssetType = AssetType.Money,
                    Count = potValue
                }
            };
        }
    }
}
