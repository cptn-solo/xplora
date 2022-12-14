using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public class EffectsContainer : MonoBehaviour
    {
        private EffectIcon[] effectIcons;
        private DamageEffect[] effects;

        public void SetEffects(List<DamageEffectInfo> effects)
        {
            this.effects = effects != null && effects.Count > 0 ?
                effects.Select(x => x.Effect).ToArray() :
                null;
            UpdateView();
        }

        private void UpdateView()
        {
            if (effectIcons != null)
                foreach (var icon in effectIcons)
                    icon.gameObject.SetActive(effects != null && effects.Contains(icon.DamageEffect));
        }

        void Start()
        {
            effectIcons = GetComponentsInChildren<EffectIcon>();
            UpdateView();
        }

    }
}