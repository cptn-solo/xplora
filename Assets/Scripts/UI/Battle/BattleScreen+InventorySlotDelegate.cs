using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Battle
{
    using AssetDict = Dictionary<int, Asset>;
    public partial class BattleScreen // Inventory Slot Delegate
    {
        private event UnityAction<Hero> OnHeroMoved;
        private event UnityAction<Hero> OnHeroUpdated;

        private void InitInventorySlotDelegates()
        {
            OnHeroMoved += SlotDelegate_HeroMoved;
            OnHeroUpdated += SlotDelegate_HeroUpdated;
            
            slotDelegate.TransferEnabled = (UIItemSlot s) =>
                battleManager.CurrentBattle.State != BattleState.BattleInProgress;
            slotDelegate.Pool = PooledItem;
            slotDelegate.Validator = CheckSlot;
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                if (s is BattleLineSlot bls)
                {
                    heroTransfer.Begin(bls.Hero, bls.Position);
                    Rollback = () => bls.Hero = libraryManager.Library.HeroAtPosition(bls.Position);
                    bls.Hero = Hero.Default;

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
                    success = heroTransfer.Commit(bls.Position, out var hero);
                    bls.Hero = hero;

                    if (success)
                    {
                        libraryManager.Library.MoveHero(hero, bls.Position);
                        OnHeroMoved?.Invoke(bls.Hero);
                    }
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

        private bool CheckSlot(UIItemSlot slot)
        {
            if (!slot.IsEmpty)
                return false;

            if (slot is AssetInventorySlot)
            {
                if (slot is HeroAttackSlot)
                    return assetTransfer.TransferAsset.AssetType == AssetType.Attack;
                else if (slot is HeroDefenceSlot)
                    return assetTransfer.TransferAsset.AssetType == AssetType.Defence;
                else
                    return assetTransfer.TransferAsset.AssetType != AssetType.NA;
            }
            else if (slot is BattleLineSlot bls)
                return heroTransfer.TransferHero.HeroType != HeroType.NA &&
                    heroTransfer.TransferHero.TeamId == bls.Position.Item1;
            else
                return false;
        }

        private Transform PooledItem(UIItemSlot slot)
        {
            var sample = slot.transform.childCount == 0 ? null : slot.transform.GetChild(0);
            if (slot is AssetInventorySlot)
            {
                return PooledInventoryItem(sample);
            }
            else if (slot is BattleLineSlot)
            {
                return PooledHeroItem(sample);
            }
            else
                return null;
        }
        private Transform PooledHeroItem(Transform placeholder)
        {
            if (placeholder == null) // create and bind a new card
            {
                var placeholderCard = assetPool.GetRaidMember(
                    Hero.Default, canvas.transform.localScale)
                    .transform.GetComponent<RaidMember>();
                BindHeroCard(placeholderCard); //placeholders are just filled with data on cargo drop
                return placeholderCard.transform;
            }
            else // grab a card from the pool for display purposes
            {
                var placeholderCard = placeholder
                    .transform.GetComponent<RaidMember>();
                var hero = placeholderCard.Hero;
                var card = assetPool.GetRaidMember(hero, canvas.transform.localScale);
                return card.transform;

            }
        }

        private Transform PooledInventoryItem(Transform placeholder)
        {
            if (placeholder == null) // new asset card for placeholder
            {
                var placeholderCard = assetPool.GetAssetCard(
                    default, canvas.transform.localScale)
                    .transform.GetComponent<InventoryItem>();
                return placeholderCard.transform;
            }
            else // grab a card from the pool for display purposes
            {
                var placeholderCard = placeholder
                    .transform.GetComponent<InventoryItem>();
                var asset = placeholderCard.Asset;
                var card = assetPool.GetAssetCard(asset, canvas.transform.localScale);
                return card.transform;
            }
        }

        private AssetDict DictForAssetSlot(AssetInventorySlot s)
        {
            if (s is TeamInventorySlot tis)
                return tis.TeamId == playerTeamId ?
                    libraryManager.PlayerTeam.Inventory :
                    libraryManager.EnemyTeam.Inventory;

            return s is HeroDefenceSlot ? selectedHero.Defence :
                                    s is HeroAttackSlot ? selectedHero.Attack :
                                    selectedHero.Inventory;
        }

        private RaidMember RaidMemberForHero(Hero hero)
        {
            var slots = (hero.TeamId == playerTeamId) ?
                playerFrontSlots.Concat(playerBackSlots) :
                enemyFrontSlots.Concat(enemyBackSlots);

            var rm = slots.Where(x => x.RaidMember.Hero.Id == hero.Id)
                .Select(x => x.RaidMember).FirstOrDefault();

            return rm;
        }
    }
}

