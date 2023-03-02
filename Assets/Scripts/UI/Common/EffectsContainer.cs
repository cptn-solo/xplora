using Assets.Scripts.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public class EffectsContainer : MonoBehaviour
    {
        private Dictionary<DamageEffect, EffectIcon> effectIcons = new();

        private void Awake()
        {
            foreach (var icon in GetComponentsInChildren<EffectIcon>(true))
                effectIcons.Add(icon.DamageEffect, icon);
        }
        public void FlashEffect(DamageEffect effect)
        {
            if (effectIcons.TryGetValue(effect, out var icon))
                StartCoroutine(FlashActiveEffectCoroutine(icon));
        }

        public void SetEffects(Dictionary<DamageEffect, int> effects)
        {
            foreach (var icon in effectIcons)
            {
                if (effects != null && effects.ContainsKey(icon.Key))
                {
                    icon.Value.gameObject.SetActive(true);
                    icon.Value.SetTurnsCount(effects[icon.Key]);
                }
                else
                {
                    icon.Value.SetTurnsCount(0);
                    icon.Value.gameObject.SetActive(false);
                }
            }

        }

        private IEnumerator FlashActiveEffectCoroutine(EffectIcon icon)
        {
            icon.gameObject.SetActive(true);

            var origScale = icon.transform.localScale;
            var scale = origScale;
            var sec = 0f;
            while (sec <= 1f)
            {
                icon.transform.localScale = scale;

                sec += Time.deltaTime * 2f; // .5 sec flash to 2x size
                scale = origScale * (1 + sec);
                scale.z = 1f;

                yield return null;
            }
            icon.transform.localScale = origScale;
        }
    }
}