using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public static class HexCoordinatesExtensions
    {
        public static HexCoordinates[] RangeFromCoordinates(this HexCoordinates coord, int range, Vector4 frame)
        {
            var buffer = ListPool<HexCoordinates>.Get();
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
                }
            }
            var retval = buffer.ToArray();
            ListPool<HexCoordinates>.Add(buffer);

            return retval;
        }
    }
}