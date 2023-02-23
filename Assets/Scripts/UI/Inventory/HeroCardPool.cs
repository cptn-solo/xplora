using Assets.Scripts.Data;
using Assets.Scripts.UI.Library;
using Leopotam.EcsLite;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{

    public delegate void CardBinder(HeroCard card);

    public class HeroCardPool : MonoBehaviour
    {
        [SerializeField] private GameObject heroCardPrefab;
        [SerializeField] private GameObject heroDetailsPrefab;

        private Canvas canvas;

        public CardBinder CardBinder { get; set; }

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public HeroCard CreateHeroCard(
            EcsPackedEntityWithWorld? heroInstance)
        {
            HeroCard heroCard = Instantiate(heroCardPrefab).GetComponent<HeroCard>();
            heroCard.transform.localScale = canvas.transform.localScale;
            heroCard.transform.SetParent(transform);
            heroCard.PackedEntity = heroInstance;

            CardBinder?.Invoke(heroCard);

            heroCard.gameObject.SetActive(false);

            return heroCard;
        }
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

        public HeroCard Pooled(HeroCard card)
        {
            card.transform.SetParent(transform);
            return card;
        }
    }
}