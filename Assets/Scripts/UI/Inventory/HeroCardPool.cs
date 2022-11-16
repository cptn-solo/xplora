using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Library;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public class HeroCardPool : MonoBehaviour
    {
        [SerializeField] private GameObject heroCardPrefab;
        private readonly Dictionary<int, List<HeroCard>> heroCardPool = new();
        private HeroCard CreateCachedHeroCard(Hero hero, List<HeroCard> cachedItems, Vector3 scale)
        {
            HeroCard heroCard = Instantiate(heroCardPrefab).GetComponent<HeroCard>();
            heroCard.Hero = hero;
            heroCard.gameObject.SetActive(false);
            heroCard.transform.localScale = scale;
            heroCard.transform.SetParent(transform);
            cachedItems.Add(heroCard);
            return heroCard;
        }
        internal HeroCard GetHeroCard(Hero hero, Vector3 scale)
        {
            HeroCard heroCard = null;

            if (!heroCardPool.TryGetValue(hero.Id, out List<HeroCard> cachedItems))
            {
                cachedItems = new();
                heroCard = CreateCachedHeroCard(hero, cachedItems, scale);
                heroCardPool[hero.Id] = cachedItems;
            }

            if (hero.HeroType != HeroType.NA &&
                cachedItems.Where(x => !x.isActiveAndEnabled).FirstOrDefault() is HeroCard instance)
            {
                heroCard = instance;
            }
            else
            {
                heroCard = CreateCachedHeroCard(hero, cachedItems, scale);
            }

            heroCard.gameObject.SetActive(hero.HeroType != HeroType.NA);

            return heroCard;

        }
    }
}