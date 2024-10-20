﻿using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Assets.Scripts.World.HexMap
{
    [System.Serializable]
    public struct HexCoordinates
    {
        [SerializeField]
        private int x, z;

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Z
        {
            get
            {
                return z;
            }
        }
        /// <summary>
        /// X position in hex space,
        /// where the distance between cell centers of east-west neighbors is one unit.
        /// </summary>
        public float HexX => X + Z / 2 + ((Z & 1) == 0 ? 0f : 0.5f);

        /// <summary>
        /// Z position in hex space,
        /// where the distance between cell centers of east-west neighbors is one unit.
        /// </summary>
        public float HexZ => Z * HexMetrics.outerToInner;

        public HexCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }
        public int Y
        {
            get
            {
                return -X - Z;
            }
        }

        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinates(x - z / 2, z);
        }
        public static HexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / (HexMetrics.innerRadius * 2f);
            float y = -x;

            float offset = position.z / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;
            
            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);
            
            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }
            return new HexCoordinates(iX, iZ);
        }

        public Vector3 ToPosition()
        {
            Vector3 position = default;

            position.x = (x + z * 0.5f) * (HexMetrics.innerRadius * 2f); // hex
            position.y = 0f;
            position.z = z * (HexMetrics.outerRadius * 1.5f);

            return position;
        }
        public override string ToString()
        {
            return "(" +
                X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
        }

        public string ToStringOnSeparateLines()
        {
            return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is HexCoordinates coord)
                return X == coord.X && Z == coord.Z && Y == coord.Y;
            else return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Z, Y);
        }
    }
}