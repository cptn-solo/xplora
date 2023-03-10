using Assets.Scripts.Data;
using Leopotam.EcsLite;
using System;
using Assets.Scripts.UI.Data;
using Assets.Scripts.ECS.Data;

namespace Assets.Scripts.Services
{
    public partial class HeroLibraryService : BaseEcsService
    {
        private MenuNavigationService menuNavigationService;
        private BattleManagementService battleManagementService;

        public DamageTypesLibrary DamageTypesLibrary => damageTypesLib;

        public ref Team PlayerTeam => ref GetEcsPlayerTeam();
        public ref Team EnemyTeam => ref GetEcsEnemyTeam();

        public EcsPackedEntityWithWorld[] PlayerHeroes => GetEcsTeamHeroes(PlayerTeamEntity);
        public EcsPackedEntityWithWorld[] EnemyHeroes => GetEcsTeamHeroes(EnemyTeamEntity);
        public EcsPackedEntityWithWorld[] NonPlayerTeamHeroes => GetEcsNotInTeamHeroes(PlayerTeamEntity, true);
        public EcsPackedEntityWithWorld[] EnemyDomainHeroes => GetEcsEnemyDomainHeroes();

        public void Init(
            MenuNavigationService menuNavigationService,
            BattleManagementService battleManagementService)
        {
            StartEcsContext();
            InitConfigLoading();

            this.menuNavigationService = menuNavigationService;
            menuNavigationService.OnBeforeNavigateToScreen += MenuNavigationService_OnBeforeNavigateToScreen;

            this.battleManagementService = battleManagementService;
            battleManagementService.OnBattleComplete += BattleManagementService_OnBattleComplete;
        }

        private void MenuNavigationService_OnBeforeNavigateToScreen(
            Screens current, Screens prev)
        {
        }

        private void BattleManagementService_OnBattleComplete(bool won, Asset[] pot)
        {
            if (!won)
                return;

            if (!PlayerTeamEntity.Unpack(out var world, out var entity))
                return;
            
            var buffer = ListPool<Asset>.Get();
            ref var team = ref PlayerTeam;

            buffer.AddRange(team.Assets);

            foreach (var trophy in pot)
            {
                if (buffer.FindIndex(x => x.AssetType == trophy.AssetType) is var idx &&
                    idx >= 0)
                {
                    var val = buffer[idx];
                    val.Count += trophy.Count;
                    buffer[idx] = val;
                }
                else
                {
                    buffer.Add(new Asset()
                    {
                        AssetType = trophy.AssetType,
                        Count = trophy.Count
                    });
                }
            }

            team.Assets = buffer.ToArray();

            ListPool<Asset>.Add(buffer);

            var pool = ecsWorld.GetPool<UpdateAssetBalanceTag>();
            if (!pool.Has(entity))
                pool.Add(entity);
        }

        private void OnDestroy()
        {
            StopEcsContext();
            menuNavigationService.OnBeforeNavigateToScreen -= MenuNavigationService_OnBeforeNavigateToScreen;
            battleManagementService.OnBattleComplete -= BattleManagementService_OnBattleComplete;
        }

        internal EcsPackedEntityWithWorld? HeroAtPosition(Tuple<int, BattleLine, int> position) =>
            GetEcsHeroAtPosition(position);

        internal void MoveHero(EcsPackedEntityWithWorld hero, Tuple<int, BattleLine, int> pos) =>
            MoveEcsHeroToPosition(hero, pos);
    }
}