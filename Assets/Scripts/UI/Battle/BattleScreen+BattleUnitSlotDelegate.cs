using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Inventory;
using Assets.Scripts.UI.Library;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Battle
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class BattleScreen // Battle Unit Slot Delegate
    {
        private event UnityAction OnHeroMoved;

        [SerializeField] private BattleUnitPool battleUnitPool;

        private void InitBattleUnitSlotDelegates()
        {
            OnHeroMoved += SlotDelegate_HeroMoved;
            
            slotDelegate.TransferEnabled = (UIItemSlot s) =>
                battleManager.CurrentBattle.State != BattleState.BattleInProgress;
            slotDelegate.Pool = (UIItemSlot s) => {
                if (s is not BattleLineSlot slot)
                    return null;

                return PooledItem(slot.BattleUnit != null ? slot.BattleUnit.transform : null);
            };
            slotDelegate.Validator = CheckSlot;
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                var bls = s as BattleLineSlot;
                HeroPosition pos = bls.Position;

                var packed = bls.BattleUnit.PackedEntity.Value;

                Rollback = () => battleManager.MoveHero(packed, pos);

                heroTransfer.Begin(bls.BattleUnit.PackedEntity.Value, pos);

                bls.Reset();

            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var bls = s as BattleLineSlot;
                HeroPosition pos = bls.Position;

                if (heroTransfer.Commit(pos, out var hero) is bool success)
                {
                    battleManager.MoveHero(hero, pos);
                    OnHeroMoved?.Invoke();
                }
                else Rollback?.Invoke();

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
                if (heroTransfer.Abort() is bool success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }

        private Transform PooledItem(Transform placeholder)
        {
            if (placeholder == null)
                return null;

            var card = placeholder.GetComponent<BattleUnit>();
            return battleUnitPool.Pooled(card).transform;
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

        private BattleUnit BattleUnitForPosition(Tuple<int, BattleLine, int> position)
        {
            var slots = (position.Item1 == playerTeamId) ?
                playerFrontSlots.Concat(playerBackSlots) :
                enemyFrontSlots.Concat(enemyBackSlots);

            var rm = slots.Where(x => x.Position.Equals(position))
                .Select(x => x.BattleUnit).FirstOrDefault();

            return rm;

        }
    }
}

