using Assets.Scripts.UI.Data;
using Assets.Scripts.UI.Inventory;
using UnityEngine;

namespace Assets.Scripts.UI.Library
{
    public class LibrarySlot : UIItemSlot
    {
        private Hero hero;
        public Hero Hero
        {
            get => hero;
            set
            {
                hero = value;
                if (transform.childCount == 0)
                    return;

                heroCard.Hero = hero;
            }
        }

        public int TeamId { get; set; }

        private HeroCard heroCard;
        public HeroCard HeroCard => heroCard;
        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);
            heroCard = itemTransform.GetComponent<HeroCard>();
        }
    }
}