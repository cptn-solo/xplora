using Assets.Scripts.Data;
using Assets.Scripts.Battle;
using Assets.Scripts.UI.Inventory;
using System;
using UnityEngine;

namespace Assets.Scripts.UI.Library
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class LibrarySlot : UIItemSlot, IHeroPosition
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

        private HeroCard heroCard;
        public HeroCard HeroCard => heroCard;

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);
            heroCard = itemTransform.GetComponent<HeroCard>();
        }

        #region IHeroPosition

        public HeroPosition Position { get; internal set; }

        public IHeroInstanceEntity Unit => HeroCard;

        public IBarsAndEffects UnitStateView => null;

        #endregion

    }

}