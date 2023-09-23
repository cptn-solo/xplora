using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{

    public class RelationsEventDialog : BaseEntityView<RelationsEventInfo>, IEventDialog<RelationsEventInfo>
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
            this.raidService.RegisterEntityView(this);

            gameObject.SetActive(false);
        }

        private void OnActionButtonClick(int idx)
        {
            raidService.OnEventAction<RelationsEventInfo>(idx);
        }

        protected override void OnBeforeAwake()
        {
            for (int i = 0; i < actionButtons.Length; i++)
            {
                var idx = i;
                UnityAction call = () => OnActionButtonClick(idx);
                actionButtons[i].onClick.AddListener(call);
            }
        }

        protected override void OnBeforeDestroy()
        {
            base.OnBeforeDestroy();

            foreach (var button in actionButtons)
                button.onClick.RemoveAllListeners();            
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

            scorePanel.SetInfo(info.ScoreInfo);

            foreach (var child in kindsTransform.GetComponentsInChildren<HeroKindPanel>())
                Destroy(child.gameObject);
            
            var canvas = GetComponentInParent<Canvas>();

            foreach (var item in info.EventItems)
            {
                var card = Instantiate(kindPrefab).GetComponent<HeroKindPanel>();
                card.transform.localScale = canvas.transform.localScale;
                card.transform.SetParent(kindsTransform);
                card.transform.localRotation = Quaternion.identity;
                card.transform.localPosition = Vector3.zero;

                card.SetInfo(item);
            }

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