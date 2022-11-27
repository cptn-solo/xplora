using Assets.Scripts.UI.Data;
using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public class BattleQueue : MonoBehaviour
    {
        [Inject] private BattleManagementService battleManager;

        [SerializeField] private RectTransform battleQueuePanel;

        private BattleQueueSlot[] playerSlots;
        private BattleQueueSlot[] enemySlots;

        private bool slotsInitialized;

        internal void CompleteTurn()
        {
        }

        internal void Prepare()
        {
            PrepareTeam(battleManager.PlayerTeam);
            PrepareTeam(battleManager.EnemyTeam);

            battleManager.ResetBattle = false;
        }

        private void PrepareTeam(Team team)
        {
            var idx = -1;
            var slots = team.Id == 0 ? playerSlots : enemySlots;

            foreach (var slot in slots)
            {
                slot.QueueMember.Hero = Hero.Default;
            }

            foreach (var hero in
                team.FrontLine.Concat(
                team.BackLine)
                .Where(x => x.Value.HeroType != HeroType.NA)
                .Select(x => x.Value))
            {
                slots[++idx].QueueMember.Hero = hero;
            }
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

            playerSlots = slots
                .Where(x => x.TeamId == battleManager.PlayerTeam.Id)
                .OrderBy(x => x.QueueIndex)
                .ToArray();
            enemySlots = slots
                .Where(x => x.TeamId == battleManager.EnemyTeam.Id)
                .OrderBy(x => x.QueueIndex)
                .ToArray();

            slotsInitialized = true;

        }
    }
}