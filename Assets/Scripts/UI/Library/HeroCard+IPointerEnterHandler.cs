using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Library
{
    public partial class HeroCard : IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData) =>
            EcsService.RequestDetailsHover(PackedEntity);

        public void OnPointerExit(PointerEventData eventData) =>
            EcsService.DismissDetailsHover(PackedEntity);
    }
}