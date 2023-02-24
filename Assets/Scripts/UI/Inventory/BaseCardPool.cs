using Leopotam.EcsLite;
using UnityEngine;

namespace Assets.Scripts.UI.Inventory
{
    public delegate void CardBinder<T>(T card);

    public class BaseCardPool<T, E> : MonoBehaviour
        where T: MonoBehaviour, ITransform, IEntityView<E>
    {
        [SerializeField] protected GameObject cardPrefab;

        protected Canvas canvas;

        public CardBinder<T> CardBinder { get; set; }

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public T Pooled(T card)
        {
            card.Transform.SetParent(transform);
            return card;
        }

        public T CreateCard(
            EcsPackedEntityWithWorld? packedInstanceEntity)
        {
            T card = Instantiate(cardPrefab).GetComponent<T>();
            card.Transform.localScale = canvas.transform.localScale;
            card.Transform.SetParent(transform);
            card.PackedEntity = packedInstanceEntity;

            CardBinder?.Invoke(card);

            card.Transform.gameObject.SetActive(false);

            return card;
        }

    }
}