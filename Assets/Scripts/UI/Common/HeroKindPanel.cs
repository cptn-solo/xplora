using Assets.Scripts.Data;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Common
{

    public class HeroKindPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private BarWithTitle source;
        [SerializeField] private BarWithTitle target;

        public void SetInfo(RelationEventItemInfo info)
        {
            titleText.text = info.ItemTitle;
            source.SetInfo(info.SrcBarInfo);
            target.SetInfo(info.TgtBarInfo);
        }
    }
}