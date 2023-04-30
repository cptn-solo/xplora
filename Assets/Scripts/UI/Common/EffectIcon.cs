using Assets.Scripts.Data;
using TMPro;
using UnityEngine;

public class EffectIcon : MonoBehaviour
{
    [SerializeField] private DamageEffect effectType;

    private TextMeshProUGUI turnsCount;
    public DamageEffect DamageEffect => effectType;

    public void SetTurnsCount(int count)
    {
        turnsCount.text = count > 0 ? $"{count}" : "";
    }

    private void Awake()
    {
        turnsCount = GetComponentInChildren<TextMeshProUGUI>();
    }
}
