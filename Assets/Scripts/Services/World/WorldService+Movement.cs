using Assets.Scripts.UI.Data;
using Assets.Scripts.World;
using Assets.Scripts.World.HexMap;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Services
{
    public partial class WorldService
    {
        private HexCoordinates? currentAim;

        private void PlayerUnit_OnArrivedToCoordinates(HexCoordinates coordinates, Unit unit)
        {
            var cellId = CellIndexResolver(coordinates);
            if (worldObjects.FirstOrDefault(x => x.CellIndex == cellId) is WorldObject obj &&
                obj.ObjectType == WorldObjectType.Unit &&
                obj.Unit != null &&
                obj.Hero.HeroType != HeroType.NA)
            {
                InitiateBattle(obj);
            }
            else
            {
                worldObjects.Remove(player);
                
                player.CellIndex = cellId;
                
                worldObjects.Add(player);
            }
        }

        /// <summary>
        /// Checks if a cell for the specified coordinates is nearby the
        /// cell where player is located rn
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public bool CheckIfReachable(HexCoordinates coordinates)
        {
            var coord = CellCoordinatesResolver(player.CellIndex);
            if (coord.DistanceTo(coordinates) <= player.Unit.WorldRange)
                return true;

            return false;
        }

        public void SetAimToCoordinates(HexCoordinates? coordinates)
        {
            currentAim = coordinates;
            CoordHoverer?.Invoke(coordinates);
        }

        public void ProcessTargetCoordinatesSelection(HexCoordinates coordinates)
        {
            CoordSelector?.Invoke(coordinates);

            //TODO: add a decision button or such
            ProcessMoveToHexCoordinates(coordinates);
        }

        public void ProcessMoveToHexCoordinates(HexCoordinates coordinates)
        {
            player.Unit.SetMoveTargetCoordinates(coordinates);
            player.Unit.MoveToTargetCoordinates();
        }

        public void ProcessDirectionSelection(Vector3 direction)
        {
            HexDirection hexDir;
            if (direction.x > 0 && direction.z > 0)
                hexDir = HexDirection.NE;
            else if (direction.x > 0 && direction.z < 0)
                hexDir = HexDirection.SE;
            else if (direction.x > 0)
                hexDir = HexDirection.E;
            else if (direction.x < 0 && direction.z > 0)
                hexDir = HexDirection.NW;
            else if (direction.x < 0 && direction.z < 0)
                hexDir = HexDirection.SW;
            else if (direction.x < 0)
                hexDir = HexDirection.W;
            else if (direction.z > 0)
                hexDir = HexDirection.W;
            else
                hexDir = HexDirection.NA; // can't move to south or north or default should be set

            if (hexDir != HexDirection.NA)
            {
                // TODO: decide on move rules etc.
                var startPoint = currentAim ?? player.Unit.CurrentCoord;
                var targetCoord = CoordResolver(startPoint, hexDir);

                if (CheckIfReachable(targetCoord))
                    SetAimToCoordinates(targetCoord);

                //ProcessTargetCoordinatesSelection(targetCoord);
            }

            // var targetPos = playerUnit.transform.position + direction;            
            //playerUnit.MoveTo(targetPos);
        }

    }
}