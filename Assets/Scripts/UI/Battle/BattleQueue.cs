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

        private BattleQueueSlot[] combinedSlots = new BattleQueueSlot[0];

        private bool slotsInitialized;

        internal void ResetQueue()
        {
            foreach (var slot in combinedSlots)
                slot.QueueMember.Hero = null;
        }

        internal void LayoutHeroes(RoundSlotInfo[] heroes)
        {
            ResetQueue();

            for (int idx = 0; idx < Mathf.Min(heroes.Length, combinedSlots.Length); idx++)
            {
                var queueMember = combinedSlots[idx].QueueMember;
                queueMember.Hero = heroes[idx];
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
            foreach (var slot in slots)
                slot.InitQueueMember(null);

            combinedSlots = slots
                .OrderBy(x => x.QueueIndex)
                .ToArray();

            slotsInitialized = true;
        }        
    }
}