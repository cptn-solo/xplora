using System;

namespace Assets.Scripts.Data
{
    public class RelationEventTriggerRange : Tuple<IntRange, float, float>
    {
        /// <summary>
        /// Applies to RSD in range
        /// </summary>
        public IntRange RSDRange => Item1;

        /// <summary>
        /// Event spawn trigger rate basis
        /// </summary>
        public float RateBasis => Item2;

        /// <summary>
        /// Event spawn trigger rate factor. This value should be multiplied to the difference between 
        /// actual hero RSD value and RateBasis. In other words, RateFactor is a value to be added for
        /// each RSD extra over the RateBasis
        /// </summary>
        public float RateFactor => Item3;

        public RelationEventTriggerRange(IntRange rsdRange, float rateBasis, float rateFactor) : 
            base(rsdRange, rateBasis, rateFactor)
        {
        }
    }
}