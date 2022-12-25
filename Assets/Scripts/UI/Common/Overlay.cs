using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Overlay : MonoBehaviour
{
    [SerializeField] private EffectsContainer effectsContainer;
    [SerializeField] private Image piercedImage;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private BarsContainer barsContainer;

    private Transform anchor;
    public void Attach(Transform anchor) =>
        this.anchor = anchor;

    internal void FlashEffect(DamageEffect damageEffect) =>
        effectsContainer.FlashEffect(damageEffect);

    internal void ResetOverlayInfo()
    {
        piercedImage.gameObject.SetActive(false);
        damageText.text = "";
        effectText.text = "";
    }

    private void Update()
    {
        if (anchor != null)
            transform.position = anchor.transform.position;
    }

    internal void SetOverlayInfo(TurnStageInfo info)
    {
        piercedImage.gameObject.SetActive(info.IsPierced);
        damageText.text = info.Damage > 0 ? $"{info.Damage}" : "";
        effectText.text = info.EffectText;

        if (info.Effect != DamageEffect.NA)
            effectsContainer.FlashEffect(info.Effect);
    }

    internal void SetBarsEndEffectsInfo(
        List<BarInfo> barsInfoBattle,
        Dictionary<DamageEffect, int> effects)
    {
        barsContainer.gameObject.SetActive(true);
        barsContainer.SetData(barsInfoBattle);

        effectsContainer.gameObject.SetActive(true);
        effectsContainer.SetEffects(effects);
    }

    internal void ResetBarsAndEffects()
    {
        barsContainer.gameObject.SetActive(false);
        effectsContainer.gameObject.SetActive(false);
    }
}
