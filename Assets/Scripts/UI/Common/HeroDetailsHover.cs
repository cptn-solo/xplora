using Assets.Scripts.Services;
using Assets.Scripts.ECS;
using Assets.Scripts.Data;
using TMPro;
using UnityEngine;
using Zenject;
using Assets.Scripts.ECS.Data;

public class HeroDetailsHover : BaseEntityView<HoverTag<Hero>>
{
    [SerializeField] protected TextMeshProUGUI heroNameText;

    [Inject]
    public void Construct(HeroLibraryService service)
    {
        service.RegisterEntityView(this);
    }    

    protected Hero hero;
    public Hero? Hero
    {
        get => hero;
        set
        {
            if (value == null)
            {
                this.gameObject.SetActive(false);
                hero = default;
                return;
            }

            hero = value.Value;

            this.gameObject.SetActive(true);

            heroNameText.text = hero.Name;

        }
    }


    protected override void OnBeforeStart() =>
        Hero = null;
}
