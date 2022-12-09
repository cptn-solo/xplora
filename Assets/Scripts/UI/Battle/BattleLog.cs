using Assets.Scripts.UI.Data;
using System;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Battle
{
    public class BattleLog : MonoBehaviour
    {
        [Inject] private readonly BattleManagementService battleManager;

        [SerializeField] private Transform contentTransform;
        [SerializeField] private GameObject logRecordPrefab;
        
        private bool initialized;
        private Canvas canvas;
        
        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();

            battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
            battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
            battleManager.OnTurnEvent += BattleManager_OnTurnEvent;
            initialized = true;
        }
        private void ClearLogRecords()
        {
            if (contentTransform.childCount > 0)
                for (int i = contentTransform.childCount - 1; i >= 0 ; i--)
                    Destroy(contentTransform.GetChild(i).gameObject);
        }

        private void AddLogRecord(string text)
        {
            var record = Instantiate(logRecordPrefab).GetComponent<TextMeshProUGUI>();
            record.text = text;
            record.transform.localScale = canvas.transform.localScale;
            record.transform.SetParent(contentTransform);
            record.transform.localPosition = Vector3.zero;
            record.transform.SetAsFirstSibling();
        }

        private void BattleManager_OnTurnEvent(BattleTurnInfo arg0)
        {
            AddLogRecord(arg0.ToString());
        }

        private void BattleManager_OnRoundEvent(BattleRoundInfo arg0)
        {
            AddLogRecord(arg0.ToString());
        }

        private void BattleManager_OnBattleEvent(BattleInfo arg0)
        {
            if (arg0.State == BattleState.BattleStarted)
                ClearLogRecords();

            AddLogRecord(arg0.ToString());
        }

        private void OnEnable()
        {
            if (initialized)
            {
                battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
                battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
                battleManager.OnTurnEvent += BattleManager_OnTurnEvent;
            }
        }

        private void OnDisable()
        {
            if (initialized)
            {
                battleManager.OnBattleEvent -= BattleManager_OnBattleEvent;
                battleManager.OnRoundEvent -= BattleManager_OnRoundEvent;
                battleManager.OnTurnEvent -= BattleManager_OnTurnEvent;
            }
        }
    }
}