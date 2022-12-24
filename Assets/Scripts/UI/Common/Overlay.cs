using Assets.Scripts.UI.Battle;
using Assets.Scripts.UI.Common;
using Assets.Scripts.UI.Data;
using System;
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

    public void Detach()
    {
        this.anchor = null;
        Destroy(this.gameObject);
    }

    internal void SetEffects(DamageEffect[] damageEffects)
    {
        effectsContainer.SetEffects(new DamageEffect[] { });
    }

    internal void ResetOverlayInfo()
    {
        piercedImage.gameObject.SetActive(false);
        damageText.text = "";
        effectText.text = "";

        effectsContainer.SetEffects(new DamageEffect[] { });
    }


    private void Awake()
    {
        //var r = GetComponent<Renderer>();
        //r.sortingLayerName = "Overlay";
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

        effectsContainer.SetEffects(info.Effect != DamageEffect.NA ?
            new DamageEffect[] { info.Effect } :
            new DamageEffect[] { });
    }

    internal void SetBarsInfo(List<BarInfo> barsInfoBattle)
    {
        barsContainer.gameObject.SetActive(true);
        barsContainer.SetData(barsInfoBattle);
    }

    internal void ResetBars()
    {
        barsContainer.gameObject.SetActive(false);
    }
}
