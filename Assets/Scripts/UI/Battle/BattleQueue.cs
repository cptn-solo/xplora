using Assets.Scripts.UI.Data;
using System.Collections.Generic;
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

        internal void CompleteTurn()
        {
        }

        internal void PrepareRound()
        {
            var allBattleHeroes = battleManager.AllBattleHeroes.Select(x => x.Value);
            var activeHeroes = allBattleHeroes.Where(x => x.HealthCurrent > 0);
            var orderedHeroes = activeHeroes.OrderByDescending(x => x.Speed);

            Dictionary<int, List<Hero>> speedSlots = new();
            foreach (var hero in orderedHeroes)
            {
                if (speedSlots.TryGetValue(hero.Speed, out var slots))
                    slots.Add(hero);
                else
                    speedSlots[hero.Speed] = new List<Hero>() { hero };
            }

            var speedKeys = speedSlots.Keys.OrderByDescending(x => x);
            var queuedHeroes = new Hero[orderedHeroes.Count()];
            
            var idx = -1;
            foreach (var speed in speedKeys)
            {                
                var slots = speedSlots[speed];
                while (slots.Count() > 0)
                {
                    var choosenIdx = slots.Count() == 1 ? 0 : Random.Range(0, slots.Count());
                    queuedHeroes[++idx] = slots[choosenIdx];
                    slots.RemoveAt(choosenIdx);
                }
            }

            LayoutHeroes(queuedHeroes);

            battleManager.ResetBattle = false;
        }

        private void LayoutHeroes(Hero[] heroes)
        {
            foreach (var slot in combinedSlots)
                slot.QueueMember.Hero = Hero.Default;

            for (int idx = 0; idx < heroes.Length; idx++)
                combinedSlots[idx].QueueMember.Hero = heroes[idx];
        }

        internal void Toggle(bool toggle)
        {
            battleQueuePanel.gameObject.SetActive(toggle);
            if (!slotsInitialized)
                InitSlots();
        }

        public void InitSlots()
        {
            var slots = battleQueuePanel.GetComponentsInChildren<BattleQueueSlot>(true);
            foreach (var slot in slots)
                slot.InitQueueMember();

            combinedSlots = slots
                .OrderBy(x => x.QueueIndex)
                .ToArray();

            slotsInitialized = true;

        }
    }
}