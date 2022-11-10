using Assets.Scripts.UI.Inventory;
using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI.Data
{

    public struct Team : IEntity
    {
        private int id;
        private string name;
        private Dictionary<int, Asset> inventory; // index, item

        public int Id => id;
        public string Name => name;
        public Dictionary<int, Asset> Inventory => inventory; // index, item

        public static Team EmptyTeam(int id, string name)
        {
            Team team = default;
            team.id = id;
            team.name = name;
            team.inventory = DefaultTeamInventory();

            return team;
        }

        public int GiveAsset(Asset asset, int index = -1) =>
            inventory.PutAsset(asset, index);

        public Asset TakeAsset(AssetType assetType, int count)
        {
            throw new System.NotImplementedException();
        }
        public Asset TakeAsset(int index, int count)
        {
            throw new System.NotImplementedException();
        }

        private static Dictionary<int, Asset> DefaultTeamInventory() => new() {
            {0, default}, {1, default}, {2, default}, {3, default}, {4, default},
            {5, default}, {6, default}, {7, default}, {8, default}, {9, default},
            {10, default}, {11, default}, {12, default}, {13, default}, {14, default},
        };
    }


}