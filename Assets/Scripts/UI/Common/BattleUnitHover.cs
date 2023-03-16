using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using TMPro;
using UnityEngine;
using Zenject;

public class BattleUnitHover : BaseEntityView<HoverTag<Hero>>
{
    [SerializeField] protected TextMeshProUGUI heroNameText;

    [Inject]
    public void Construct(BattleManagementService service) =>
        service.RegisterEntityView(this);

    public string HeroName
    {
        get => heroNameText.text;
        set
        {
            if (value == null)
            {
                this.gameObject.SetActive(false);
                return;
            }

            this.gameObject.SetActive(true);

            heroNameText.text = value;

        }
    }

    protected override void OnBeforeStart() =>
        HeroName = null;
}
