using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI
{
    public class HandlePanelVisibility : MonoBehaviour, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private bool mouseIsOver = false;
        public event UnityAction OnClickedOutside; 

        private void OnEnable()
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
        public void OnDeselect(BaseEventData eventData)
        {
            //Close the Window on Deselect only if a click occurred outside this panel
            if (!mouseIsOver)
                OnClickedOutside?.Invoke();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseIsOver = true;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseIsOver = false;
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }
}