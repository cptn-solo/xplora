using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;
using Leopotam.EcsLite;
using Assets.Scripts.UI.Library;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleScreen // Battle Units
    {
        private EcsPackedEntityWithWorld? selectedHero = null;

        private void BindHeroCard(BattleUnit heroCard)
        {
            heroCard.DelegateProvider = heroDelegate;

            var actionButton = heroCard.GetComponent<UIActionButton>();
            actionButton.OnActionButtonClick += HeroSelected;
        }


        private void HeroSelected(Actions action, Transform actionTransform)
        {
            var raidMemeber = actionTransform.GetComponent<BattleUnit>();
            Debug.Log($"Hero from line #{raidMemeber.Hero} selected");
            
            selectedHero = raidMemeber.PackedEntity.Value;            
            SyncHeroCardSelectionWithHero();

        }
        
        private void SyncHeroCardSelectionWithHero()
        {
            foreach (var slots in new[] { playerFrontSlots, playerBackSlots, enemyFrontSlots, enemyBackSlots })
                foreach (var card in slots
                    .Where(x => x.BattleUnit != null)
                    .Select(x => x.BattleUnit)
                    .ToArray())
                    card.Selected = card.PackedEntity.Equals(selectedHero);
        }

    }
}