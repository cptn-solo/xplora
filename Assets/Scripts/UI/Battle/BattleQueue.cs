using Assets.Scripts.Data;
using Assets.Scripts.Services;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public class BattleQueue : MonoBehaviour
    {
        [Inject] private readonly HeroLibraryService libraryService;

        [SerializeField] private RectTransform battleQueuePanel;

        private BattleQueueSlot[] combinedSlots;

        private bool slotsInitialized;

        internal void ResetQueue()
        {
            var emptyHero = Hero.Default;
            foreach (var slot in combinedSlots)
            {
                slot.QueueMember.Hero = emptyHero;
            }
        }

        internal void LayoutHeroes(RoundSlotInfo[] heroes)
        {
            ResetQueue();

            for (int idx = 0; idx < Mathf.Min(heroes.Length, combinedSlots.Length); idx++)
            {
                var queueMember = combinedSlots[idx].QueueMember;
                var slotInfo = heroes[idx];
                queueMember.Hero = libraryService.Library.HeroById(slotInfo.HeroId);
            }
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
            var emptyHero = Hero.Default;
            foreach (var slot in slots)
                slot.InitQueueMember(emptyHero);

            combinedSlots = slots
                .OrderBy(x => x.QueueIndex)
                .ToArray();

            slotsInitialized = true;
        }
    }
}