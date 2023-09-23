using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.Services;
using Assets.Scripts.UI.Common;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.UI
{
    public class RelationsEventToast : BaseEntityView<RelationsEventToastInfo>, IEventDialog<RelationsEventToastInfo>
    {
        private RaidService raidService;

        [SerializeField] private Image srcImage;
        [SerializeField] private Image tgtImage;
        [SerializeField] private HeroRelationsScorePanel scorePanel;
        
        [Inject]
        public void Construct(RaidService raidService)
        {
            this.raidService = raidService;
            this.raidService.RegisterEntityView(this);

            gameObject.SetActive(false);
        }

        #region IEventDialog members

        public void Dismiss()
        {
            gameObject.SetActive(false);
        }

        public void SetEventInfo(RelationsEventToastInfo info)
        {
            srcImage.sprite = Resources.Load<Sprite>(info.SrcIconName);
            tgtImage.sprite = Resources.Load<Sprite>(info.TgtIconName);

            scorePanel.SetInfo(info.ScoreInfo);

            gameObject.SetActive(true);
        }

        #endregion
    }
}