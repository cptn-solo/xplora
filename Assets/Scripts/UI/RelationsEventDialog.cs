using Assets.Scripts.Data;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class RelationsEventDialog : MonoBehaviour, IEventDialog<RelationsEventInfo>
    {
        private RaidService raidService;

        [SerializeField] private Image srcImage;
        [SerializeField] private Image tgtImage;
        [SerializeField] private TextMeshProUGUI eventTitle;
        [SerializeField] private HeroRelationsScorePanel scorePanel;
        [SerializeField] private Transform kindsTransform;
        [SerializeField] private GameObject kindPrefab;
        
        [SerializeField] private Button[] actionButtons;

        [Inject]
        public void Construct(RaidService raidService)
        {
            this.raidService = raidService;
            this.raidService.RegisterEventDialog(this);

            gameObject.SetActive(false);
        }

        private void OnActionButtonClick(int idx)
        {
            raidService.OnEventAction<WorldEventInfo>(idx);
        }

        private void Awake()
        {
            for (int i = 0; i < actionButtons.Length; i++)
            {
                var idx = i;
                UnityAction call = () => OnActionButtonClick(idx);
                actionButtons[i].onClick.AddListener(call);
            }
        }

        private void OnDestroy()
        {
            foreach (var button in actionButtons)
                button.onClick.RemoveAllListeners();

            raidService.UnregisterEventDialog(this);
        }

        #region IEventDialog members

        public void Dismiss()
        {
            gameObject.SetActive(false);
        }

        public void SetEventInfo(RelationsEventInfo info)
        {
            srcImage.sprite = Resources.Load<Sprite>(info.SrcIconName);
            tgtImage.sprite = Resources.Load<Sprite>(info.TgtIconName);

            eventTitle.text = info.EventTitle;

            for (int i = 0; i < actionButtons.Length; i++)
            {
                var button = actionButtons[i];
                if (info.ActionTitles.Length > i)
                {
                    button.gameObject.SetActive(true);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = info.ActionTitles[i];
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }

            gameObject.SetActive(true);
        }

        #endregion
    }
}