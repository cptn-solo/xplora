using System.Collections;
using Assets.Scripts.Data;
using Assets.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class ConfigLoader : MonoBehaviour
    {
        private TextMeshProUGUI text;

        [SerializeField] private Button closeButton;
        public event UnityAction OnCloseButtonPressed;

        private HeroLibraryService heroLibraryService;
        private WorldService worldService;
        private RaidService raidService;
        private bool heroServiceFinished;
        private bool worldServiceFinished;
        private bool raidServiceFinished;

        [Inject]
        public void Construct(
            HeroLibraryService heroLibraryService,
            WorldService worldService,
            RaidService raidService)
        {
            this.heroLibraryService = heroLibraryService;
            this.worldService = worldService;
            this.raidService = raidService;

            Initialize();
        }

        private void Initialize()
        {
        }

        private void Awake()
        {
            text = GetComponentInChildren<TextMeshProUGUI>();
            closeButton.onClick.AddListener(Close);

            ClearLogRecords();
        }

        private void OnEnable()
        {
            heroLibraryService.OnDataAvailable += HeroLibraryService_OnDataAvailable;
            worldService.OnDataAvailable += WorldService_OnDataAvailable;
            raidService.OnDataAvailable += RaidService_OnDataAvailable;

            closeButton.interactable = false;

            StartCoroutine(AutoStartConfigLoading());
        }

        private void OnDisable()
        {
            heroLibraryService.OnDataAvailable -= HeroLibraryService_OnDataAvailable;
            worldService.OnDataAvailable -= WorldService_OnDataAvailable;
            raidService.OnDataAvailable -= RaidService_OnDataAvailable;

            ClearLogRecords();
            StopAllCoroutines();
        }
        private void HeroLibraryService_OnDataAvailable() =>
            heroServiceFinished = true;

        private void WorldService_OnDataAvailable() =>
            worldServiceFinished = true;

        private void RaidService_OnDataAvailable() =>
            raidServiceFinished = true;

        private IEnumerator AutoStartConfigLoading()
        {
            heroServiceFinished = false;
            worldServiceFinished = false;
            raidServiceFinished = false;

            yield return new WaitForSecondsRealtime(1f);

            EnqueueText($"Config loading initialized...");
            EnqueueText($"");

            yield return new WaitForSecondsRealtime(1f);

            heroLibraryService.LoadRemoteData();

            while (!heroServiceFinished)
                yield return null;

            EnqueueText($"Hero configs and Damage configs loaded");
            EnqueueText($"");

            yield return null;

            worldService.StopWorld();
            worldService.LoadRemoteData();

            while (!worldServiceFinished)
                yield return null;

            EnqueueText($"World Attributes and POI configs loaded");
            EnqueueText($"");

            yield return null;

            raidService.LoadRemoteData();

            while (!worldServiceFinished)
                yield return null;

            EnqueueText($"Enemy and Enemy team member spawn rules loaded");
            EnqueueText($"");
            EnqueueText($"");

            yield return new WaitForSecondsRealtime(1f);

            EnqueueText($"All loaded!");

            yield return null;

            closeButton.interactable = true;

            StartCoroutine(AutoCloseCoroutine());
        }

        private IEnumerator AutoCloseCoroutine()
        {
            yield return new WaitForSecondsRealtime(2f);

            Close();
        }

        public void Close() =>
            OnCloseButtonPressed?.Invoke();

        private void ClearLogRecords() =>
            text.text = "";

        private void EnqueueText(string v) =>
            text.text += $"\n{v}";

        private void OnDestroy()
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }
}