using Assets.Scripts.UI.Battle;
using UnityEngine;
using Leopotam.EcsLite;

namespace Assets.Scripts.UI.Inventory
{
    public class AssetPool : MonoBehaviour
    {
        [SerializeField] private GameObject heroPrefab;
        [SerializeField] private GameObject overlayPrefab;

        [SerializeField] private Transform overlayParent;
        
        public RaidMember CreateRaidMember(
            EcsPackedEntityWithWorld heroInstance, Vector3 scale)
        {
            RaidMember heroCard = Instantiate(heroPrefab).GetComponent<RaidMember>();
            heroCard.transform.localScale = scale;
            heroCard.transform.SetParent(transform);

            Overlay overlay = Instantiate(overlayPrefab).GetComponent<Overlay>();
            overlay.transform.localScale = scale;
            overlay.transform.SetParent(overlayParent);
            var ha = heroCard.GetComponentInChildren<HeroAnimation>();
            ha.SetOverlay(overlay);

            heroCard.PackedEntity = heroInstance;
            heroCard.gameObject.SetActive(false);

            return heroCard;
        }       
    }
}