using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.ECS
{
    public class BaseEntityView<T> : MonoBehaviour, IEntityView<T>
    where T : struct
    {
        public virtual T? CurrentData { get; private set; }

        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public DataLoadDelegate<T> DataLoader { get; set; }

        public Transform Transform => transform;

        public virtual void Destroy()
        {
            GameObject.Destroy(gameObject);
        }

        public virtual void UpdateData()
        {
            CurrentData = DataLoader?.Invoke(PackedEntity);
        }
    }
}
