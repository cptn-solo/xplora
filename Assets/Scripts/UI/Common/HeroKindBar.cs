using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Common
{
    public class HeroKindBar : BaseContainableItem<HeroKindBarInfo>
    {
        public class Factory : PlaceholderFactory<HeroKindBar> { }

        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private BarWithTitle kindBar;

        protected override void ApplyInfoValues(HeroKindBarInfo info)
        {
            titleText.text = info.ItemTitle;
            kindBar.SetInfo(info.BarInfo);
        }
    }
}