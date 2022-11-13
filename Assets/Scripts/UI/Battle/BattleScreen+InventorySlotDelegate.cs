using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Inventory Slot Delegate
    {
        private void InitInventorySlotDelegates()
        {
            slotDelegate.Pool = PooledItem;
            slotDelegate.Validator = CheckSlot;
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                if (s is BattleLineSlot bls)
                {
                    var dict = playerFrontSlots.Contains(s) ? team.FrontLine : team.BackLine;
                    teamManager.BeginHeroTransfer(dict, s.SlotIndex);
                    bls.Hero = Hero.Default;
                    Rollback = () => bls.Hero = dict[s.SlotIndex];
                }
                else if (s is AssetInventorySlot ais)
                {
                    var dict = GetAssetDictForSlot(s);
                    teamManager.BeginAssetTransfer(dict, s.SlotIndex);
                    ais.Asset = default;
                    Rollback = () => ais.Asset = dict[s.SlotIndex];
                }
            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var success = false;
                if (s is BattleLineSlot bls)
                {
                    var dict = playerFrontSlots.Contains(s) ? team.FrontLine : team.BackLine;
                    success = teamManager.CommitHeroTransfer(dict, s.SlotIndex);
                    bls.Hero = success ? dict[s.SlotIndex] : Hero.Default;
                }
                else if (s is AssetInventorySlot ais)
                {
                    var dict = GetAssetDictForSlot(s);
                    success = teamManager.CommitAssetTransfer(dict, s.SlotIndex);
                    ais.Asset = success ? dict[s.SlotIndex] : default;
                }

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            slotDelegate.TransferCleanup = (UIItemSlot s) =>
            {
                if (teamManager.TransferAsset.AssetType != AssetType.NA ||
                    teamManager.TransferHero.HeroType != HeroType.NA)
                    slotDelegate.TransferAbort(s);
            };
            slotDelegate.TransferAbort = (UIItemSlot s) =>
            {
                var success = false;
                if (s is BattleLineSlot bls)
                {
                    var dict = playerFrontSlots.Contains(s) ? team.FrontLine : team.BackLine;
                    success = teamManager.AbortHeroTransfer();
                }
                else
                {
                    var dict = GetAssetDictForSlot(s);
                    success = teamManager.AbortAssetTransfer();
                }

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }
    }
}

