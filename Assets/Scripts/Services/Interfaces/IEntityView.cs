using Leopotam.EcsLite;

namespace Assets.Scripts
{
    public delegate T DataLoadDelegate<T>(EcsPackedEntityWithWorld entity);

    public interface IEntityView<T> : ITransform //RaidMember
    {
        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public DataLoadDelegate<T> DataLoader { get; set; }
        public void Update();
    }

}