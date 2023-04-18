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

        protected override Image IconImage => effectIcon;

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
