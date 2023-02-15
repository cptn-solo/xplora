using Assets.Scripts.UI.Common;
using Assets.Scripts.Data;
using TMPro;
using UnityEngine;

public class HeroDetailsHover : MonoBehaviour
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


}
