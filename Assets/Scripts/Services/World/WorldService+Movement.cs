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
using Assets.Scripts.Services.Data;
using Assets.Scripts.ECS.Data;

namespace Assets.Scripts.Services
{
    public partial class WorldService
    {
        private HexCoordinates? currentAim;
        private HexDirection hexDir;

        private void PlayerUnit_OnArrivedToCoordinates(HexCoordinates coordinates, Unit unit)
        {
            var cellId = CellIndexResolver(coordinates);

            if (currentAim.HasValue && currentAim.Value.Equals(coordinates))
                SetAimToCoordinates(null);

            if (!InitiateBattle(cellId))
            {
                UpdateEcsPlayerCellId(cellId);
                SetAimByHexDir(); // will try to continue move to direction set earlier
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
            if (!TryGetPlayerUnit(out var unit, out var cellId))
                return false;

            var coord = CellCoordinatesResolver(cellId);
            var distance = coord.DistanceTo(coordinates);

            if (distance > 0 && distance <= unit.WorldRange)
                return true;

            return false;
        }

        public void SetAimToCoordinates(HexCoordinates? coordinates)
        {
            currentAim = coordinates;
            CoordHoverer?.Invoke(coordinates);
        }

        public void ProcessTargetCoordinatesSelection(HexCoordinates? coordinates = null)
        {
            if (coordinates == null || !CheckIfReachable(coordinates.Value))
                coordinates = currentAim;

            if (coordinates == null)
                return;

            if (!CheckIfReachable(coordinates.Value))
                return;

            CoordSelector?.Invoke(coordinates);

            //TODO: add a decision button or such
            ProcessMoveToHexCoordinates(coordinates.Value);
        }        

        public void ProcessMoveToHexCoordinates(HexCoordinates coordinates)
        {
            if (!TryGetPlayerUnit(out var unit, out var cellId))
                return;

            unit.SetMoveTargetCoordinates(coordinates);
            unit.MoveToTargetCoordinates();
        }

        public void ProcessDirectionSelection(Vector3 direction)
        {
            hexDir = HexDirection.NA;

            if (!TryGetPlayerUnit(out var unit, out var _))


            if (currentAim != null)
            {
                //NB: valid moves in comments below are
                //relative to currently aimed (w yellow border) cell,
                //but hexDir assigned with already converted direction
                //related to the player's position
                if (currentAim.Value.X == unit.CurrentCoord.X)
                {
                    if (currentAim.Value.Z > unit.CurrentCoord.Z)
                    {
                        //from NE valid moves are left (W) and down (SE)
                        if (direction.x < 0)
                            hexDir = HexDirection.NW;
                        else if (direction.x > 0 || direction.z < 0)
                            hexDir = HexDirection.E;
                    }
                    else if (currentAim.Value.Z < unit.CurrentCoord.Z)
                    {
                        //from SW valid moves are right (E) and up (NW)
                        if (direction.x > 0)
                            hexDir = HexDirection.SE;
                        else if (direction.x < 0 || direction.z > 0)
                            hexDir = HexDirection.W;
                    }
                    else
                    {
                        //error, aimed active position
                    }
                }
                else if (currentAim.Value.Y == unit.CurrentCoord.Y)
                {
                    if (currentAim.Value.Z > unit.CurrentCoord.Z)
                    {
                        //from NW valid moves are right (E) and down (SW)
                        if (direction.x > 0)
                            hexDir = HexDirection.NE;
                        else if (direction.x < 0 || direction.z < 0)
                            hexDir = HexDirection.W;
                    }
                    else if (currentAim.Value.Z < unit.CurrentCoord.Z)
                    {
                        //from SE valid moves are left (W) and up (NE)
                        if (direction.x < 0)
                            hexDir = HexDirection.SW;
                        else if (direction.x > 0 || direction.z > 0)
                            hexDir = HexDirection.E;
                    }
                    else
                    {
                        //error, aimed active position
                    }
                }
                else if (currentAim.Value.Z == unit.CurrentCoord.Z)
                {
                    if (currentAim.Value.X > unit.CurrentCoord.X)
                    {
                        //from E valid moves are up (NW), down (SW) or left (W)
                        if (direction.x < 0)
                            hexDir = HexDirection.W;
                        else if (direction.z > 0)
                            hexDir = HexDirection.NE;
                        else if (direction.z < 0)
                            hexDir = HexDirection.SE;
                    }
                    else if (currentAim.Value.X < unit.CurrentCoord.X)
                    {
                        //from W valid moves are up (NE), down (SE) or right (E)
                        if (direction.x > 0)
                            hexDir = HexDirection.E;
                        else if (direction.z > 0)
                            hexDir = HexDirection.NW;
                        else if (direction.z < 0)
                            hexDir = HexDirection.SW;
                    }
                    else
                    {
                        //error, aimed active position
                    }
                }
            }
            else
            {
                //relative to player coordinate
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
                    hexDir = HexDirection.NE; // change to NW if player positioned leftside
                else if (direction.z < 0)
                    hexDir = HexDirection.SE; // change to SW if player positioned leftside
            }

            Debug.Log($"ProcessDirectionSelection {hexDir}");

            SetAimByHexDir();

        }

        public void ResetHexDir()
        {
            hexDir = HexDirection.NA;
        }

        public void SetAimByHexDir()
        {
            if (hexDir != HexDirection.NA &&
                TryGetPlayerUnit(out var unit, out var _))
            {
                // TODO: decide on move rules etc.
                var startPoint = unit.CurrentCoord;
                var targetCoord = CoordResolver(startPoint, hexDir);

                if (CheckIfReachable(targetCoord))
                    SetAimToCoordinates(targetCoord);
            }

        }

    }
}