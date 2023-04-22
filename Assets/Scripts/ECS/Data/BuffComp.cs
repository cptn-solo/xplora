using Assets.Scripts.Data;
using Color = UnityEngine.Color;

namespace Assets.Scripts.ECS.Data
{
    public struct BuffComp<T> : IIntValue, IIdentifiable<int>
    {
        public int Id { get; set; }

        public int Value { get; set; }
        public int Usages { get; set; }
        public Color IconColor { get; set; }
        public BundleIcon Icon { get; set; }

        public void Add(int b) =>
            Value += b;

        public void Combine(int b) =>
            Value *= b;

        public void Combine(float b) =>
            Value = (int)(Value * b);
    }

}


