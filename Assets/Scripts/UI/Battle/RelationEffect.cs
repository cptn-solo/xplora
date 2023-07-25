using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Battle
{
    public class RelationEffect : BaseContainableItem<RelationEffectInfo> 
    {
        [SerializeField] private Image heroIcon;
        [SerializeField] private Image effectIcon;
        [SerializeField] private TextMeshProUGUI effectText;
        [SerializeField] private Transform movableCard;

        protected override Image IconImage => effectIcon;
        public override Transform MovableCard => movableCard;

        protected override void ApplyInfoValues(RelationEffectInfo info)
        {
            heroIcon.sprite = SpriteForResourceName(info.HeroIcon);

            ResolveIcon(effectIcon, info.EffectIcon);
            effectIcon.color = info.EffectIcon.IconMaterial() == BundleIconMaterial.Font ?
            info.EffectIconColor : Color.white;

            effectText.text = info.EffectText;
        }
    }

}
