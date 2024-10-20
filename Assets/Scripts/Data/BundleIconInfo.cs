﻿using UnityEngine;

namespace Assets.Scripts.Data
{
    public struct BundleIconInfo : IContainableItemInfo<int>
    {
        public int Id { get; set; }
        public BundleIcon Icon;
        public Color IconColor;
    }
}