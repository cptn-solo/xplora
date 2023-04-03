using Assets.Scripts.Data;
using Assets.Scripts.ECS;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using TMPro;
using UnityEngine;
using Zenject;

public class TeamMemberHover : BaseEntityView<HoverTag<TeamMemberInfo>>
{
    [SerializeField] protected TextMeshProUGUI heroNameText;

    [Inject]
    public void Construct(RaidService service) =>
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

    protected override void OnBeforeDestroy()
    {
        base.OnBeforeDestroy();
    }
    protected override void OnBeforeStart() =>
        HeroName = null;
}
