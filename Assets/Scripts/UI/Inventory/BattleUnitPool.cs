using Assets.Scripts.UI.Battle;
using UnityEngine;
using Leopotam.EcsLite;
using Assets.Scripts.Data;

namespace Assets.Scripts.UI.Inventory
{
    public class BattleUnitPool : BaseCardPool<BattleUnit, Hero>
    {        
        public BattleUnit CreateBattleUnit(
            EcsPackedEntityWithWorld heroInstance)
        {
            BattleUnit heroCard = base.CreateCard(heroInstance);

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