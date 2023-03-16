using Assets.Scripts.ECS;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Inventory
{
    public delegate void CardBinder<T>(T card);

    public class BaseCardPool<T, E> : BaseCardPool<IEcsService, T, E>
        where T : MonoBehaviour, ITransform, IEntityView<E>
        where E : struct
    {

    }

    public class BaseCardPool<S, T, E> : MonoBehaviour
        where T: MonoBehaviour, ITransform, IEntityView<E>
        where E: struct
        where S: IEcsService
    {
        [SerializeField] protected GameObject cardPrefab;

        protected Canvas canvas;

        public CardBinder<T> CardBinder { get; set; }

        protected S ecsService;


        [Inject]
        public void Construct(S ecsService)
        {
            this.ecsService = ecsService;
            ecsService.RegisterEntityViewFactory(CreateCard);
        }

        private void OnDestroy()
        {
            ecsService.UnregisterEntityViewFactory<E>();
        }

        public T Pooled(T card)
        {
            card.Transform.SetParent(transform);
            return card;
        }

        public T CreateCard(
            EcsPackedEntityWithWorld? packedInstanceEntity)
        {
            var canvas = GetComponentInParent<Canvas>();

            T card = Instantiate(cardPrefab).GetComponent<T>();
            card.Transform.localScale = canvas.transform.localScale;
            card.Transform.SetParent(transform);
            card.Transform.localRotation = Quaternion.identity;
            card.PackedEntity = packedInstanceEntity;
            card.EcsService = ecsService;

            CardBinder?.Invoke(card);

            return card;
        }

    }
}