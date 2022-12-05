using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public class BattleQueue : MonoBehaviour
    {
        [Inject] private readonly BattleManagementService battleManager;

        [SerializeField] private RectTransform battleQueuePanel;

        private BattleQueueSlot[] combinedSlots;

        private bool slotsInitialized;

        internal void CompleteTurn(Hero[] heroes)
        {
            // TODO: ???
            LayoutHeroes(heroes);
        }
        internal void ResetQueue()
        {
            foreach (var slot in combinedSlots)
                slot.QueueMember.Hero = Hero.Default;
        }

        internal void LayoutHeroes(Hero[] heroes)
        {
            ResetQueue();

            for (int idx = 0; idx < heroes.Length; idx++)
                combinedSlots[idx].QueueMember.Hero = heroes[idx];
        }

        internal void Toggle(bool toggle)
        {
            battleQueuePanel.gameObject.SetActive(toggle);
            if (!slotsInitialized)
                InitSlots();
        }

        internal void InitSlots()
        {
            var slots = battleQueuePanel.GetComponentsInChildren<BattleQueueSlot>(true);
            foreach (var slot in slots)
                slot.InitQueueMember();

            combinedSlots = slots
                .OrderBy(x => x.QueueIndex)
                .ToArray();

            slotsInitialized = true;
        }

        internal void UpdateHero(Hero hero)
        {
            foreach (var slot in combinedSlots)
            {
                if (slot.QueueMember.Hero.Id == hero.Id)
                    slot.QueueMember.Hero = hero;
            }
        }
    }
}