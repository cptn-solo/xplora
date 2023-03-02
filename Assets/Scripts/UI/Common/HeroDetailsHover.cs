using Assets.Scripts.ECS;
using Assets.Scripts.UI.Common;
using Assets.Scripts.Data;
using Assets.Scripts.Battle;
using TMPro;
using UnityEngine;
using Assets.Scripts;

public class HeroDetailsHover : BaseEntityView<Hero>
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

    private void OnDestroy()
    {
        OnGameObjectDestroy();
    }
    #region IEntityView

    public override void UpdateData()
    {
        Hero = DataLoader(PackedEntity != null ? PackedEntity.Value : null);
    }

    #endregion

}
