using Assets.Scripts.Data;
using Assets.Scripts.UI.Inventory;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Library
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public partial class LibraryScreen // Slot Delegate 
    {
        private event UnityAction OnHeroMoved;
        private SlotDelegateProvider slotDelegate = default;
        private readonly HeroTransfer heroTransfer = new();

        delegate void TransferRollback();
        TransferRollback Rollback { get; set; }// initialised on transaction start


        private void InitSlotDelegates()
        {
            OnHeroMoved += SlotDelegate_HeroMoved;

            slotDelegate.TransferEnabled = (UIItemSlot s) => true;
            slotDelegate.Validator = (UIItemSlot s) => s.IsEmpty;
            slotDelegate.Pool = (UIItemSlot s) =>
            {
                if (s is not LibrarySlot libSlot)
                    return null;

                return PooledItem(libSlot.HeroCard != null ? libSlot.HeroCard.transform : null);
            };
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                var bls = s as LibrarySlot;
                HeroPosition pos = s is TeamMemberSlot pts ? 
                    pts.Position :
                    new(-1, BattleLine.NA, s.SlotIndex);

                var packed = bls.HeroCard.PackedEntity.Value;

                Rollback = () => libManager.MoveHero(packed, pos);

                heroTransfer.Begin(bls.HeroCard.PackedEntity.Value, pos);

                bls.Reset();

            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var bls = s as LibrarySlot;
                HeroPosition pos = s is TeamMemberSlot pts ?
                    pts.Position :
                    new(-1, BattleLine.NA, s.SlotIndex);
                
                if (heroTransfer.Commit(pos, out var hero) is bool success &&
                    success)
                {
                    libManager.MoveHero(hero, pos);
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
                if (heroTransfer.Abort() is bool success &&
                    success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }

        private Transform PooledItem(Transform placeholder)
        {
            if (placeholder == null)
                return null;

            var card = placeholder.GetComponent<HeroCard>();
            return cardPool.Pooled(card).transform;
        }

        private void SlotDelegate_HeroMoved()
        {
            SyncWorldAndButton();
        }
    }
}