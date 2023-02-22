using Assets.Scripts.Data;
using Assets.Scripts.UI.Inventory;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Unit Slot Delegate
    {
        private event UnityAction<Hero> OnHeroMoved;
        private event UnityAction<Hero> OnHeroUpdated;

        private void InitBattleUnitSlotDelegates()
        {
            OnHeroMoved += SlotDelegate_HeroMoved;
            
            slotDelegate.TransferEnabled = (UIItemSlot s) =>
                battleManager.CurrentBattle.State != BattleState.BattleInProgress;
            slotDelegate.Pool = PooledItem;
            slotDelegate.Validator = CheckSlot;
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                if (s is BattleLineSlot bls)
                {
                    heroTransfer.Begin(bls.Unit.HeroInstanceEntity.Value, bls.Position);
                    Rollback = () => bls.Unit.HeroInstanceEntity = battleManager.HeroAtPosition(bls.Position);
                    bls.SetHero(null);

                }
            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var success = false;
                if (s is BattleLineSlot bls)
                {
                    success = heroTransfer.Commit(bls.Position, out var hero);
                    bls.Unit.HeroInstanceEntity = hero;

                    if (success)
                    {
                        battleManager.MoveHero(hero, bls.Position);
                        OnHeroMoved?.Invoke(bls.Hero);
                    }
                }

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            slotDelegate.TransferCleanup = (UIItemSlot s) =>
            {
                if (heroTransfer.TransferHero != null)
                    slotDelegate.TransferAbort(s);
            };
            slotDelegate.TransferAbort = (UIItemSlot s) =>
            {
                var success = heroTransfer.Abort();

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

            if (slot is BattleLineSlot bls)
                return heroTransfer.TransferHero != null &&
                    heroTransfer.TeamId == bls.Position.Item1;
            else
                return false;
        }

        private Transform PooledItem(UIItemSlot slot)
        {
            return slot.transform.childCount == 0 ? null :
                slot.transform.GetChild(0).transform;
        }

        private RaidMember RaidMemberForPosition(Tuple<int, BattleLine, int> position)
        {
            var slots = (position.Item1 == playerTeamId) ?
                playerFrontSlots.Concat(playerBackSlots) :
                enemyFrontSlots.Concat(enemyBackSlots);

            var rm = slots.Where(x => x.Position.Equals(position))
                .Select(x => x.RaidMember).FirstOrDefault();

            return rm;

        }
    }
}

