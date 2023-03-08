using Assets.Scripts.Services;
using Assets.Scripts.ECS;
using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using Zenject;
using Assets.Scripts.ECS.Data;

public class HeroDetailsHover : BaseEntityView<SelectedTag<Hero>>
{
    [SerializeField] private TextMeshProUGUI heroNameText;

    [Inject]
    public void Construct(HeroLibraryService libraryService)
    {
        libraryService.RegisterEntityView(this);
    }    

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
}
