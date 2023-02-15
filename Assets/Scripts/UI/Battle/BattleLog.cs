using Assets.Scripts.Services;
using Assets.Scripts.Data;
using System.Collections;
using System.Collections.Generic;
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

        private readonly List<string> logRecordTexts = new();
        private bool clearLogActive;
        private bool enqueLogActive;

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
            if (!clearLogActive)
                StartCoroutine(ClearLogCoroutine());
        }
        private void EnqueueText(string v)
        {
            logRecordTexts.Add(v);
            if (!enqueLogActive)
                StartCoroutine(AddLogRecordCoroutine());

        }

        private IEnumerator AddLogRecordCoroutine()
        {
            enqueLogActive = true;

            while (clearLogActive)
                yield return null;

            while (logRecordTexts.Count > 0)
            {
                AddLogRecord(logRecordTexts[0]);
                logRecordTexts.RemoveAt(0);

                yield return null;
            }

            yield return null;

            enqueLogActive = false;
        }

        private IEnumerator ClearLogCoroutine()
        {
            clearLogActive = true;
            
            if (logRecordTexts.Count > 0)
            {
                logRecordTexts.Clear();
                yield return null;
            }

            if (contentTransform.childCount > 0)
                for (int i = contentTransform.childCount - 1; i >= 0; i--)
                {
                    Destroy(contentTransform.GetChild(i).gameObject);
                    yield return null;
                }
            
            yield return null;

            clearLogActive = false;
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

        private void BattleManager_OnTurnEvent(BattleTurnInfo turnInfo, BattleRoundInfo roundInfo, BattleInfo battleInfo)
        {
            EnqueueText(turnInfo.ToString());
        }

        private void BattleManager_OnRoundEvent(BattleRoundInfo roundInfo, BattleInfo battleInfo)
        {
            EnqueueText(roundInfo.ToString());
        }

        private void BattleManager_OnBattleEvent(BattleInfo battleInfo)
        {
            if (battleInfo.State == BattleState.BattleStarted ||
                battleInfo.State == BattleState.PrepareTeams)
                ClearLogRecords();

            EnqueueText(battleInfo.ToString());
        }

        private void OnEnable()
        {
            if (initialized)
            {
                battleManager.OnBattleEvent += BattleManager_OnBattleEvent;
                battleManager.OnRoundEvent += BattleManager_OnRoundEvent;
                battleManager.OnTurnEvent += BattleManager_OnTurnEvent;

                ClearLogRecords();
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
            clearLogActive = false;
            enqueLogActive = false;
        }
    }
}