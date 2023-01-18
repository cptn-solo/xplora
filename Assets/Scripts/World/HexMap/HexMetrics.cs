using UnityEngine;

namespace Assets.Scripts.World.HexMap
{
    public static class HexMetrics
    {

        public const float outerRadius = 10f;

        /// <summary>
        /// Inner radius of a hex cell.
        /// </summary>
        public const float innerRadius = outerRadius * outerToInner;

        /// <summary>
        /// Ratio of outer to inner radius of a hex cell.
        /// </summary>
        public const float outerToInner = 0.866025404f;

        /// <summary>
        /// Ratio of inner to outer radius of a hex cell.
        /// </summary>
        public const float innerToOuter = 1f / outerToInner;

        public static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius)
        };
    }
}