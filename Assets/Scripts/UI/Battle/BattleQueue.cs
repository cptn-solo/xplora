using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Battle
{
    public class BattleQueue : MonoBehaviour
    {
       [SerializeField] private RectTransform battleQueuePanel;

        private BattleQueueSlot[] combinedSlots;

        private bool slotsInitialized;

        internal void ResetQueue()
        {
            foreach (var slot in combinedSlots)
                slot.QueueMember.Hero = Hero.Default;
        }

        internal void LayoutHeroes(Hero[] heroes)
        {
            ResetQueue();

            for (int idx = 0; idx < Mathf.Min(heroes.Length, combinedSlots.Length); idx++)
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