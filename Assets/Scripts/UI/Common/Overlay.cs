using Assets.Scripts.ECS;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Leopotam.EcsLite;

public class Overlay : BaseEntityView<BarsAndEffectsInfo>
{
    [SerializeField] private EffectsContainer effectsContainer;
    [SerializeField] private Image piercedImage;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI effectText;
    [SerializeField] private BarsContainer barsContainer;

    private Transform anchor;
    private Vector3 prevAnchorScale;
    private bool destroyed;

    public void Attach(Transform anchor)
    {
        this.anchor = anchor;
        this.gameObject.SetActive(true);
        ResetOverlayInfo();
    }

    internal void FlashEffect(DamageEffect damageEffect) =>
        effectsContainer.FlashEffect(damageEffect);

    internal void ResetOverlayInfo()
    {
        if (destroyed)
            return;

        piercedImage.gameObject.SetActive(false);
        damageText.text = "";
        effectText.text = "";
    }
    protected override void OnBeforeDestroy()
    {
        base.OnBeforeDestroy();

        destroyed = true;
        anchor = null;
    }

    private void Update()
    {
        if (anchor == null)
            return;

        transform.position = anchor.transform.position;

        if (prevAnchorScale != Vector3.zero &&
            prevAnchorScale.x != anchor.transform.localScale.x)
            transform.localScale *= anchor.transform.localScale.x / prevAnchorScale.x;

        prevAnchorScale = anchor.localScale;

    }

    internal void SetOverlayInfo(TurnStageInfo info)
    {
        piercedImage.gameObject.SetActive(info.IsPierced);
        damageText.text = info.Damage > 0 ? $"{info.Damage}" : "";
        effectText.text = info.EffectText;

        if (info.Effect != DamageEffect.NA)
            effectsContainer.FlashEffect(info.Effect);
    }

    public void SetBarsEndEffectsInfo(
        BarInfo[] barsInfoBattle = null,
        Dictionary<DamageEffect, int> effects = null)
    {
        if (destroyed)
            return;

        barsContainer.gameObject.SetActive(barsInfoBattle != null);
        barsContainer.SetInfo(barsInfoBattle??new BarInfo[0]);

        effectsContainer.gameObject.SetActive(effects != null);
        effectsContainer.SetEffects(effects??new());
    }

    internal void ResetBarsAndEffects()
    {
        if (destroyed)
            return;

        barsContainer.gameObject.SetActive(false);
        effectsContainer.gameObject.SetActive(false);
    }

    #region IEntityView

    public override void UpdateData()
    {
        var data = DataLoader(PackedEntity.Value);
        SetBarsEndEffectsInfo(data.BarsInfoBattle, data.ActiveEffects);
    }

    public override void Visualize<V>(V visualInfo, EcsPackedEntityWithWorld visualEntity)
    {
        if (visualInfo is EffectsBarVisualsInfo ebv)
        {
            effectsContainer.SetEffects(ebv.ActiveEffects);
        }
        else if (visualInfo is HealthBarVisualsInfo hbv)
        {
            barsContainer.SetInfo(hbv.BarsInfoBattle);
        }
        else if (visualInfo is DeathVisualsInfo dv)
        {
            ResetBarsAndEffects();
        }
        
        base.Visualize(visualInfo, visualEntity);
    }


    #endregion
}
