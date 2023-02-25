using Assets.Scripts;
using Assets.Scripts.Battle;
using Assets.Scripts.Data;
using Assets.Scripts.UI.Common;
using Leopotam.EcsLite;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Overlay : MonoBehaviour, IEntityView<BarsAndEffectsInfo>
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
    private void OnDestroy()
    {
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
        List<BarInfo> barsInfoBattle = null,
        Dictionary<DamageEffect, int> effects = null)
    {
        if (destroyed)
            return;

        barsContainer.gameObject.SetActive(barsInfoBattle != null);
        barsContainer.SetData(barsInfoBattle??new());

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

    public EcsPackedEntityWithWorld? PackedEntity { get; set; }

    public DataLoadDelegate<BarsAndEffectsInfo> DataLoader { get; set; }

    public Transform Transform => transform;

    public void UpdateData()
    {
        var data = DataLoader(PackedEntity.Value);
        SetBarsEndEffectsInfo(data.BarsInfoBattle, data.ActiveEffects);
    }

    public void Destroy()
    {
        GameObject.Destroy(this);
    }

    #endregion
}
