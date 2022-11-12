using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Units
    {
        private Hero selectedHero;
        private readonly Dictionary<Hero, RaidMember> heroCards = new();

        public GameObject ItemForHero(Hero hero)
        {
            RaidMember rm = null;

            if (heroCards.TryGetValue(hero, out var raidmember))
            {
                rm = raidmember;
            }
            else
            {
                var card = Instantiate(heroPrefab).transform;
                rm = card.GetComponent<RaidMember>();

                rm.OnItemDropped += RaidMember_OnItemDropped;
                rm.Validator = (Transform cargo) => cargo.GetComponent<InventoryItem>() != null;

                var actionButton = card.GetComponent<UIActionButton>();
                actionButton.OnActionButtonClick += HeroSelected;

                heroCards[hero] = rm;
            }

            rm.Hero = hero;

            return rm.gameObject;
        }

        private void RaidMember_OnItemDropped(Hero hero, InventoryItem inventoryItem)
        {
            teamManager.CommitAssetTransfer(hero.Inventory, -1, hero);
            ShowHeroInventory(selectedHero);
            ShowTeamInventory(team);
        }

        private void HeroSelected(Actions action, Transform actionTransform)
        {
            var raidMemeber = actionTransform.GetComponent<RaidMember>();
            Debug.Log($"Hero from line #{raidMemeber.Hero} selected");
            selectedHero = raidMemeber.Hero;

            ShowHeroInventory(selectedHero);

            foreach (var card in heroCards)
                card.Value.Selected = card.Key.Equals(selectedHero);
        }

        private void ShowTeamBatleUnits(Team team)
        {
            foreach (var hero in team.FrontLine)
                playerFrontSlots[hero.Key].Put(
                    hero.Value.HeroType == HeroType.NA ? null :
                    ItemForHero(hero.Value).transform);

            foreach (var hero in team.BackLine)
                playerBackSlots[hero.Key].Put(
                    hero.Value.HeroType == HeroType.NA ? null :
                    ItemForHero(hero.Value).transform);

        }
    }
}