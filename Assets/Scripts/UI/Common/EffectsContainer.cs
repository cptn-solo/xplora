using Assets.Scripts.UI.Data;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public class EffectsContainer : MonoBehaviour
    {
        private EffectIcon[] effectIcons;
        private DamageEffect[] effects;

        public void SetEffects(DamageEffect[] effects)
        {
            this.effects = effects;
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