using Assets.Scripts.Services;
using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public class BattleLog : MonoBehaviour
    {
        private BattleManagementService battleManager;
        private RectTransform rt;

        private float initialHeight;
        private float currentHeight;

        [Inject]
        public void Construct(BattleManagementService battleManager)
        {
            this.battleManager = battleManager;
            Initialize();
        }

        private void Initialize()
        {
            rt = GetComponent<RectTransform>();

            battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent += BattleManager_OnTurnEvent;
        }

        private TextMeshProUGUI text;        

        private void Awake() =>
            text = GetComponentInChildren<TextMeshProUGUI>();

        private void Start()
        {
            initialHeight = rt.rect.height;
            currentHeight = initialHeight;
        }

        private void ClearLogRecords() => 
            text.text = "";

        private void EnqueueText(string v) =>
            text.text += $"\n{v}";

        private void BattleManager_OnTurnEvent(BattleTurnInfo turnInfo,
            BattleRoundInfo roundInfo, BattleInfo battleInfo) =>
            EnqueueText(turnInfo.ToString());

        private void BattleManager_OnRoundEvent(BattleRoundInfo roundInfo,
            BattleInfo battleInfo) =>
            EnqueueText(roundInfo.ToString());

        private void BattleManager_OnBattleEvent(BattleInfo battleInfo)
        {
            if (battleInfo.State == BattleState.BattleStarted ||
                battleInfo.State == BattleState.PrepareTeams)
                ClearLogRecords();

            EnqueueText(battleInfo.ToString());
        }

        private void OnDestroy()
        {
            battleManager.OnBattleEvent -= BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent -= BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent -= BattleManager_OnTurnEvent;
        }

        internal void TogglePanelSize()
        {
            currentHeight =
                initialHeight == currentHeight ?
                initialHeight * 3 : initialHeight;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, currentHeight);
        }
    }
}