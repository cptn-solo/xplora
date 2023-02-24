using Assets.Scripts.UI.Battle;
using UnityEngine;
using Leopotam.EcsLite;
using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Inventory
{
    public class RaidMemberPool : BaseCardPool<RaidMember, Hero>
    {        
        public RaidMember CreateRaidMember(
            EcsPackedEntityWithWorld heroInstance)
        {
            RaidMember heroCard = base.CreateCard(heroInstance);

            //Overlay overlay = Instantiate(overlayPrefab).GetComponent<Overlay>();
            //overlay.transform.localScale = scale;
            //overlay.transform.SetParent(overlayParent);

            //var ha = heroCard.GetComponentInChildren<HeroAnimation>();
            //ha.SetOverlay(overlay);

            //heroCard.PackedEntity = heroInstance;
            //heroCard.gameObject.SetActive(false);

            return heroCard;
        }       
    }
}