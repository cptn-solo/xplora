using Assets.Scripts.UI.Common;
using Assets.Scripts.Data;
using Assets.Scripts.Battle;
using TMPro;
using UnityEngine;
using Leopotam.EcsLite;
using Assets.Scripts;

public class HeroDetailsHover : MonoBehaviour, IEntityView<Hero>
{
    [SerializeField] private TextMeshProUGUI heroNameText;
    [SerializeField] private BarsContainer barsContainer;

    private Hero hero;
    public Hero Hero
    {
        get => hero;
        set
        {
            hero = value;
            if (hero.HeroType == HeroType.NA)
            {
                this.gameObject.SetActive(false);
                return;
            }

            this.gameObject.SetActive(true);

            heroNameText.text = hero.Name;

            barsContainer.SetData(hero.BarsInfo);
        }
    }


    private void Start()
    {
        Hero = Hero.Default;
    }

    #region IEntityView

    public EcsPackedEntityWithWorld? PackedEntity { get; set; }

    public DataLoadDelegate<Hero> DataLoader { get; set; }

    public void Update()
    {
        Hero = DataLoader(PackedEntity.Value);
    }


    public Transform Transform => transform;

    #endregion

}
