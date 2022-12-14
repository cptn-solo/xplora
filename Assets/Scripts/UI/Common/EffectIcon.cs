using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectIcon : MonoBehaviour
{
    [SerializeField] private DamageEffect effectType;

    public DamageEffect DamageEffect => effectType;
}
