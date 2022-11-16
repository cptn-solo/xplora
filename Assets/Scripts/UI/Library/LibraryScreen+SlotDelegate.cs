using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.UI.Library
{
    public partial class LibraryScreen // Slot Delegate 
    {
        private event UnityAction<Hero> OnHeroMoved;
        private event UnityAction<Hero> OnHeroUpdated;

        private void InitSlotDelegates()
        {
            OnHeroMoved += SlotDelegate_HeroMoved;
            OnHeroUpdated += SlotDelegate_HeroUpdated;

            slotDelegate.Pool = (UIItemSlot s) =>
            {
                if (s is not LibrarySlot libSlot)
                    return null;

                return PooledItem(libSlot.HeroCard != null ? libSlot.HeroCard.transform : null);
            };
            slotDelegate.Validator = (UIItemSlot s) => {
                return true;
            };
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                if (s is LibrarySlot bls)
                {
                    var dict = library.Heroes;
                    libManager.BeginHeroTransfer(dict, s.SlotIndex);
                    bls.Hero = Hero.Default;
                    Rollback = () => bls.Hero = dict[s.SlotIndex];
                }
            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var success = false;
                if (s is LibrarySlot bls)
                {
                    var dict = library.Heroes;
                    success = libManager.CommitHeroTransfer(dict, s.SlotIndex);
                    bls.Hero = success ? dict[s.SlotIndex] : Hero.Default;

                    OnHeroMoved?.Invoke(bls.Hero);
                }

                if (!success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
            slotDelegate.TransferCleanup = (UIItemSlot s) =>
            {
                if (libManager.TransferHero.HeroType != HeroType.NA)
                    slotDelegate.TransferAbort(s);
            };
            slotDelegate.TransferAbort = (UIItemSlot s) =>
            {
                var success = false;
                if (s is LibrarySlot bls)
                {
                    var dict = library.Heroes;
                    success = libManager.AbortHeroTransfer();
                }

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }

        private Transform PooledItem(Transform sample = null)
        {
            if (sample == null)
                sample = ItemForHero(Hero.Default).transform;

            var sampleCard = sample.GetComponent<HeroCard>();
            var card = cardPool.GetHeroCard(sampleCard.Hero);
            return card.transform;

        }


        private void SlotDelegate_HeroUpdated(Hero hero) =>
            CardForHero(hero).Hero = hero;

        private HeroCard CardForHero(Hero hero)
        {
            var card = librarySlots.Where(x => x.HeroCard.Hero.Id == hero.Id).Select(x => x.HeroCard).FirstOrDefault();

            return card;
        }

        private void SlotDelegate_HeroMoved(Hero hero) =>
            SyncHeroCardSelectionWithHero();
    }
}