using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{
    public partial class BagedIconsContainer : MonoBehaviour
    {
        [SerializeField] private GameObject iconPrefab;

        private Dictionary<BundleIcon, IconWithBage> icons = new();

        public void SetIconInfo(BagedIconInfo info)
        {
            if (icons.TryGetValue(info.Icon, out var iconWithBage))
                UpdateIcon(iconWithBage, info);
            else
                AddIcon(info);
        }

        public void RemoveIcon(BundleIcon iconCode)
        {
            if (icons.TryGetValue(iconCode, out var icon))
            {
                GameObject.Destroy(icon.gameObject);
                icons.Remove(iconCode);
            }
        }

        public void ResetIcons()
        {
            foreach (var item in icons)
                GameObject.Destroy(item.Value.gameObject);

            icons.Clear();
        }

        private void AddIcon(BagedIconInfo info)
        {
            var canvas = GetComponentInParent<Canvas>();
            var icon = Instantiate(iconPrefab).GetComponent<IconWithBage>();
            icon.gameObject.transform.localScale = canvas.transform.localScale;
            icon.gameObject.transform.SetParent(transform);
            icon.gameObject.transform.localPosition = Vector3.zero;
            icon.gameObject.transform.localRotation = Quaternion.identity;

            icons.Add(info.Icon, icon);

            UpdateIcon(icon, info);
        }

        private void UpdateIcon(IconWithBage icon, BagedIconInfo info)
        {
            icon.SetIconByCode(info.Icon);
            icon.SetBadgeText(info.BadgeText);
            icon.SetIconColor(info.Icon.IconMaterial() == BundleIconMaterial.Font ?
                info.IconColor : Color.white);
        }

        private void OnDestroy()
        {
            OnGameObjectDestroy();
        }
    }
}