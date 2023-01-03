using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using UnityEngine;

public class Unit : MonoBehaviour
{
    private UnitAnimation unitAnimation;

    private void Awake()
    {
        unitAnimation = GetComponentInChildren<UnitAnimation>();
    }

    public void SetHero(Hero hero)
    {
        unitAnimation.SetHero(hero);
    }
}
