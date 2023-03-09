using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public static class HexCoordinatesExtensions
    {
        /// <summary>
        /// Determine distance between this and another set of coordinates.
        /// Takes <see cref="HexMetrics.Wrapping"/> into account.
        /// </summary>
        /// <param name="other">Coordinate to determine distance to.</param>
        /// <returns>Distance in cells.</returns>
        public static int DistanceTo(this HexCoordinates me, HexCoordinates other)
        {
            int xy =
                (me.X < other.X ? other.X - me.X : me.X - other.X) +
                (me.Y < other.Y ? other.Y - me.Y : me.Y - other.Y);

            return (xy + (me.Z < other.Z ? other.Z - me.Z : me.Z - other.Z)) / 2;
        }

        public static int CellIndexForCoordinates(this HexCoordinates coordinates,
            int width)
        {
            int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
            return index;
        }

        public static HexCoordinates[] RangeFromCoordinates(this HexCoordinates coord,
            int range, Vector4 frame, out int[] cellIndexes)
        {
            var buffer = ListPool<HexCoordinates>.Get();
            var bufferIdx = ListPool<int>.Get();
            for (var z = coord.Z - range; z < coord.Z + range + 1; z++)
            {
                for (var x = coord.HexX - range; x < coord.HexX + range + 1; x++)
                {
                    var offsetX = x - z / 2 - ((z & 1) == 0 ? 0f : 0.5f);
                    var refCoord = new HexCoordinates((int)offsetX, z);

                    if (refCoord.HexX < frame.w || refCoord.HexX > frame.x)
                        continue;

                    if (refCoord.HexZ < frame.z || refCoord.HexZ > frame.y)
                        continue;

                    if (refCoord.DistanceTo(coord) > range)
                        continue;

                    buffer.Add(refCoord);
                    bufferIdx.Add(refCoord.CellIndexForCoordinates((int)frame.x));
                }
            }
            var retval = buffer.ToArray();
            cellIndexes = bufferIdx.ToArray();

            ListPool<HexCoordinates>.Add(buffer);
            ListPool<int>.Add(bufferIdx);

            return retval;
        }
    }
}