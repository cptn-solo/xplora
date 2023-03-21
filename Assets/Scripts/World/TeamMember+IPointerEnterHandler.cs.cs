using UnityEngine.EventSystems;

namespace Assets.Scripts.World
{
    public partial class TeamMember : IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData) =>
            EcsService.RequestDetailsHover(PackedEntity);

        public void OnPointerExit(PointerEventData eventData) =>
            EcsService.DismissDetailsHover(PackedEntity);
    }
}