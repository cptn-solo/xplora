using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Units
    {
        private Hero selectedHero;

        public GameObject ItemForHero(Hero hero)
        {

            RaidMember card = assetPool.GetHeroCard(hero);
            card.Hero = hero;

            if (hero.HeroType == HeroType.NA)
                BindHeroCard(card); //placeholders are just filled with data on cargo drop

            return card.gameObject;
        }

        private void BindHeroCard(RaidMember heroCard)
        {
            heroCard.DelegateProvider = heroDelegate;

            var actionButton = heroCard.GetComponent<UIActionButton>();
            actionButton.OnActionButtonClick += HeroSelected;
        }


        private void HeroSelected(Actions action, Transform actionTransform)
        {
            var raidMemeber = actionTransform.GetComponent<RaidMember>();
            Debug.Log($"Hero from line #{raidMemeber.Hero} selected");
            
            selectedHero = raidMemeber.Hero;            
            SyncHeroCardSelectionWithHero();
            ShowHeroInventory(selectedHero);

        }

        private void SyncHeroCardSelectionWithHero()
        {
            foreach (var card in playerFrontSlots.Select(x => x.RaidMember).ToArray())
                card.Selected = card.Hero.Equals(selectedHero);

            foreach (var card in playerBackSlots.Select(x => x.RaidMember).ToArray())
                card.Selected = card.Hero.Equals(selectedHero);
        }

        private void ShowTeamBatleUnits(Team team)
        {
            foreach (var hero in team.FrontLine)
                playerFrontSlots[hero.Key].Hero = hero.Value;

            foreach (var hero in team.BackLine)
                playerBackSlots[hero.Key].Hero = hero.Value;

        }
    }
}