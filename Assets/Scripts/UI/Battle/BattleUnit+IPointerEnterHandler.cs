using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.Battle
{
    public partial class BattleUnit : IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData) =>
            EcsService.RequestDetailsHover(PackedEntity);

        public void OnPointerExit(PointerEventData eventData) =>
            EcsService.DismissDetailsHover(PackedEntity);
    }
}