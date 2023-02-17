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
        private event UnityAction<Hero> OnHeroMoved;

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

                heroTransfer.Begin(bls.Hero, pos);

                Rollback = () => bls.Hero = libManager.HeroAtPosition(pos);
                bls.Hero = Hero.Default;

            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var success = false;
                var bls = s as LibrarySlot;
                HeroPosition pos = s is TeamMemberSlot pts ?
                    pts.Position :
                    new(-1, BattleLine.NA, s.SlotIndex);

                success = heroTransfer.Commit(pos, out var hero);

                bls.Hero = hero;

                if (success)
                {
                    libManager.MoveHero(hero, pos);
                    OnHeroMoved?.Invoke(bls.Hero);
                }

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            slotDelegate.TransferCleanup = (UIItemSlot s) =>
            {
                if (heroTransfer.TransferHero.HeroType != HeroType.NA)
                    slotDelegate.TransferAbort(s);
            };
            slotDelegate.TransferAbort = (UIItemSlot s) =>
            {
                var success = false;
                if (s is LibrarySlot bls)
                    success = heroTransfer.Abort();

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }

        private Transform PooledItem(Transform placeholder)
        {
            if (placeholder == null) // create and bind a new card
            {
                var placeholderCard = cardPool.GetHeroCard(
                    Hero.Default, canvas.transform.localScale)
                    .transform.GetComponent<HeroCard>();
                BindHeroCard(placeholderCard); //placeholders are just filled with data on cargo drop
                return placeholderCard.transform;
            }
            else // grab a card from the pool for display purposes
            {
                var placeholderCard = placeholder
                    .transform.GetComponent<HeroCard>();
                var hero = placeholderCard.Hero;
                var card = cardPool.GetHeroCard(hero, canvas.transform.localScale);
                return card.transform;
            }
        }

        private void SlotDelegate_HeroMoved(Hero hero)
        {
            SyncHeroCardSelectionWithHero();
            SyncWorldAndButton();
        }
    }
}