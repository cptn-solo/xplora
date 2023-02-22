using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;
using Leopotam.EcsLite;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Units
    {
        private EcsPackedEntityWithWorld selectedHero;

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
            
            selectedHero = raidMemeber.HeroInstanceEntity.Value;            
            SyncHeroCardSelectionWithHero();

        }

        private void SyncHeroCardSelectionWithHero()
        {
            var slots = playerFrontSlots
                .Concat(playerBackSlots)
                .Concat(enemyFrontSlots)
                .Concat(enemyBackSlots);

            foreach (var card in slots.Select(x => x.RaidMember).ToArray())
                card.Selected = card.HeroInstanceEntity.Equals(selectedHero);
        }
    }
}