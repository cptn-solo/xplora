﻿using Assets.Scripts.Data;
using Assets.Scripts.Battle;
using Assets.Scripts.UI.Inventory;
using System;
using UnityEngine;

namespace Assets.Scripts.UI.Library
{
    using HeroPosition = Tuple<int, BattleLine, int>;

    public class LibrarySlot : UIItemSlot, IHeroPosition, ITransform
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

        #region IHeroPosition

        public HeroPosition Position { get; internal set; }

        public override void Put(Transform itemTransform)
        {
            base.Put(itemTransform);

            if (itemTransform == null)
                return;

            heroCard = itemTransform.GetComponent<HeroCard>();
        }

        public void Reset()
        {
            heroCard = null;
        }

        #endregion

        #region ITransform

        public Transform Transform => transform;

        #endregion


    }

}