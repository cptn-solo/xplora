using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class WorldEventDialog : BaseEntityView<WorldEventInfo>, IEventDialog<WorldEventInfo>
    {

        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI eventTitle;
        [SerializeField] private TextMeshProUGUI eventText;

        [SerializeField] private Button[] actionButtons;
        
        private RaidService raidService;

        [Inject]
        public void Construct(RaidService raidService)
        {
            this.raidService = raidService;

            raidService.RegisterEntityView(this);

            gameObject.SetActive(false);
        }

        private void OnActionButtonClick(int idx)
        {
            raidService.OnEventAction<WorldEventInfo>(idx);
        }

        protected override void OnBeforeAwake()
        {
            for (int i = 0; i < actionButtons.Length; i++)
            {
                var idx = i;
                void call() => OnActionButtonClick(idx);
                actionButtons[i].onClick.AddListener(call);
            }
        }

        protected override void OnBeforeDestroy()
        {
            foreach (var button in actionButtons)
                button.onClick.RemoveAllListeners();
        }

        #region IEventDialog members

        public void Dismiss()
        {
            gameObject.SetActive(false);
        }

        public void SetEventInfo(WorldEventInfo info)
        {
            var icon = Resources.Load<Sprite>(info.IconName);
            iconImage.sprite = icon;

            eventTitle.text = info.EventTitle;
            eventText.text = info.EventText;

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