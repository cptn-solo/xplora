using Assets.Scripts.Data;
using Assets.Scripts.UI.Library;
using Leopotam.EcsLite;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public class HeroCardPool : BaseCardPool<HeroCard, Hero>
    {
        [SerializeField] private GameObject heroDetailsPrefab;

        public HeroDetailsHover CreateHeroDetailsHoverCard(
            EcsPackedEntityWithWorld? heroInstance)
        {
            HeroDetailsHover heroCard = Instantiate(heroDetailsPrefab).GetComponent<HeroDetailsHover>();
            heroCard.transform.localScale = canvas.transform.localScale;
            heroCard.transform.SetParent(transform);
            heroCard.PackedEntity = heroInstance;
            heroCard.gameObject.SetActive(false);
            return heroCard;
        }
    }
}