using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
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
                return s.IsEmpty;
            };
            slotDelegate.TransferStart = (UIItemSlot s, Transform t) =>
            {
                if (s is LibrarySlot bls)
                {
                    var dict = DictForSlot(s);
                    heroTransfer.Begin(dict, s.SlotIndex);
                    bls.Hero = Hero.Default;
                    Rollback = () => bls.Hero = dict[s.SlotIndex];
                }
            };
            slotDelegate.TransferEnd = (UIItemSlot s) =>
            {
                var success = false;
                if (s is LibrarySlot bls)
                {
                    var dict = DictForSlot(s);
                    success = heroTransfer.Commit(dict, s.SlotIndex, BattleLine.NA);

                    if (success)
                    {
                        var hero = dict[s.SlotIndex];

                        hero.TeamId = bls.TeamId;
                        bls.Hero = hero;
                        dict[s.SlotIndex] = hero;
                    }
                    else
                    {
                        bls.Hero = Hero.Default;
                    }

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
                {
                    var dict = DictForSlot(s);
                    success = heroTransfer.Abort();
                }

                if (success)
                    Rollback?.Invoke();

                Rollback = null;

                return success;
            };
        }

        private Dictionary<int, Hero> DictForSlot(UIItemSlot s)
        {
            if (s is PlayerTeamSlot)
                return library.PlayerTeam;
            else if (s is EnemyTeamSlot)
                return library.EnemyTeam;
            else return library.Heroes;
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