using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Battle
{
    using HeroDict = Dictionary<int, Hero>;
    using AssetDict = Dictionary<int, Asset>;
    public partial class BattleScreen // Inventory Slot Delegate
    {
        private event UnityAction<Hero> OnHeroMoved;
        private event UnityAction<Hero> OnHeroUpdated;

        private void InitInventorySlotDelegates()
        {
            OnHeroMoved += SlotDelegate_HeroMoved;
            OnHeroUpdated += SlotDelegate_HeroUpdated;
            
            slotDelegate.Pool = PooledItem;
            slotDelegate.Validator = CheckSlot;
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                if (s is BattleLineSlot bls)
                {
                    var dict = DictForBattleSlot(bls);
                    heroTransfer.Begin(dict, s.SlotIndex);
                    bls.Hero = Hero.Default;
                    Rollback = () => bls.Hero = dict[s.SlotIndex];
                }
                else if (s is AssetInventorySlot ais)
                {
                    var dict = DictForAssetSlot(ais);
                    assetTransfer.Begin(dict, s.SlotIndex);
                    ais.Asset = default;
                    Rollback = () => ais.Asset = dict[s.SlotIndex];
                }
            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var success = false;
                if (s is BattleLineSlot bls)
                {
                    var dict = DictForBattleSlot(bls);
                    success = heroTransfer.Commit(dict, s.SlotIndex);
                    bls.Hero = success ? dict[s.SlotIndex] : Hero.Default;

                    OnHeroMoved?.Invoke(bls.Hero);
                }
                else if (s is AssetInventorySlot ais)
                {
                    var dict = DictForAssetSlot(ais);
                    success = assetTransfer.Commit(dict, s.SlotIndex);
                    ais.Asset = success ? dict[s.SlotIndex] : default;

                    OnHeroUpdated?.Invoke(selectedHero);
                }

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            slotDelegate.TransferCleanup = (UIItemSlot s) =>
            {
                if (assetTransfer.TransferAsset.AssetType != AssetType.NA ||
                    heroTransfer.TransferHero.HeroType != HeroType.NA)
                    slotDelegate.TransferAbort(s);
            };
            slotDelegate.TransferAbort = (UIItemSlot s) =>
            {
                var success = s is BattleLineSlot bls ? heroTransfer.Abort() : assetTransfer.Abort();

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }

        private HeroDict DictForBattleSlot(BattleLineSlot bls)
        {
            var playerTeamId = battleManager.PlayerTeam.Id;
            var team = bls.TeamId == playerTeamId ? battleManager.PlayerTeam : battleManager.EnemyTeam;
            var frontSlots = bls.TeamId == playerTeamId ? playerFrontSlots : enemyFrontSlots;
            var dict = frontSlots.Contains(bls) ? team.FrontLine : team.BackLine;
            return dict;
        }
        private AssetDict DictForAssetSlot(AssetInventorySlot s)
        {
            if (s is TeamInventorySlot tis)
                return tis.TeamId == battleManager.PlayerTeam.Id ?
                    battleManager.PlayerTeam.Inventory :
                    battleManager.EnemyTeam.Inventory;

            return s is HeroDefenceSlot ? selectedHero.Defence :
                                    s is HeroAttackSlot ? selectedHero.Attack :
                                    selectedHero.Inventory;
        }

        private void SlotDelegate_HeroUpdated(Hero hero) =>
            RaidMemberForHero(hero).Hero = hero;

        private RaidMember RaidMemberForHero(Hero hero)
        {
            var slots = (hero.TeamId == battleManager.PlayerTeam.Id) ?
                playerFrontSlots.Concat(playerBackSlots) :
                enemyFrontSlots.Concat(enemyBackSlots);

            var rm = slots.Where(x => x.RaidMember.Hero.Id == hero.Id)
                .Select(x => x.RaidMember).FirstOrDefault();

            return rm;
        }

        private void SlotDelegate_HeroMoved(Hero hero) =>
            SyncHeroCardSelectionWithHero();
    }
}

