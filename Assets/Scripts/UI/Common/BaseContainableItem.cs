using System.Collections;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public partial class BaseContainableItem<T> : MonoBehaviour, IContainableItem<T>
        where T : struct
    {
        protected RectTransform rectTransform;

        protected T? itemInfo = null;
        protected bool initialized;

        protected void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            OnAwake();
            
            InitIconUtils(); // requires icon so after implementation had a chance to initialize it
        }

        protected virtual void OnAwake() { }
        protected virtual void ApplyInfoValues(T info) { }

        protected void Start()
        {
            initialized = true;

            // if an item (bar) was just spawned it can't read it's layout values (size of its parent)
            // so we need to wait a frame to hange to start layout based calculations
            StartCoroutine(ApplyItemInfo());
        }

        protected IEnumerator ApplyItemInfo()
        {
            yield return null;
            
            if (itemInfo != null)
                SetInfo(itemInfo.Value);             
        }

        public void SetInfo(T info)
        {
            itemInfo = info;

            if (!initialized)
                return;

            ApplyInfoValues(info);
        }
    }
}