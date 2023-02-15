using Assets.Scripts.Data;
using Assets.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class WorldEventDialog : MonoBehaviour, IEventDialog<WorldEventInfo>
    {
        private WorldService worldService;

        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI eventTitle;
        [SerializeField] private TextMeshProUGUI eventText;

        [SerializeField] private Button[] actionButtons;

        [Inject]
        public void Construct(WorldService worldService)
        {
            this.worldService = worldService;
            worldService.RegisterEventDialog(this);

            gameObject.SetActive(false);
        }

        private void OnActionButtonClick(int idx)
        {
            worldService.OnEventAction<WorldEventInfo>(idx);
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

            worldService.UnregisterEventDialog(this);
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