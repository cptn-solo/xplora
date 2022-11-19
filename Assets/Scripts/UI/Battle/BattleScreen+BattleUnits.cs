using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Units
    {
        private Hero selectedHero;

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
            var slots = playerFrontSlots
                .Concat(playerBackSlots)
                .Concat(enemyFrontSlots)
                .Concat(enemyBackSlots);

            foreach (var card in slots.Select(x => x.RaidMember).ToArray())
                card.Selected = card.Hero.Equals(selectedHero);
        }

        private void ShowTeamBatleUnits(Team team)
        {
            var frontSlots = (team.Id == battleManager.PlayerTeam.Id) ?
                playerFrontSlots : enemyFrontSlots;
            var backSlots = (team.Id == battleManager.PlayerTeam.Id) ?
                playerBackSlots : enemyBackSlots;

            foreach (var hero in team.FrontLine)
                frontSlots[hero.Key].Hero = hero.Value;

            foreach (var hero in team.BackLine)
                backSlots[hero.Key].Hero = hero.Value;

        }
    }
}