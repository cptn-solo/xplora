using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Assets.Scripts.ECS
{
    public class BaseEntityButton<T, S> : MonoBehaviour, IEntityButton<T> 
        where T : struct
        where S : IEcsService
    {
        private event UnityAction OnButtonClick;
        private Button button;

        public event Action<Transform> OnEntityButtonClick;

        protected Button Button => button;

        [Inject]
        public void Construct(S service)
        {
            EcsService = service;
            EcsService.RegisterEntityButtonRef<T>(this);
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            OnButtonClick += UIEntityButton_OnButtonClick;
        }
        private void UIEntityButton_OnButtonClick()
        {
            if (PackedEntity.HasValue && PackedEntity.Value.Unpack(out var world, out var entity))
            {
                var pool = world.GetPool<EntityButtonClickedTag>();
                if (!pool.Has(entity))
                    pool.Add(entity);
            }
            
            OnEntityButtonClick?.Invoke(transform);

        }

        private void OnEnable() =>
            button.onClick.AddListener(OnButtonClick);

        private void OnDisable() =>
            button.onClick.RemoveListener(OnButtonClick);

        private void OnDestroy() =>
            OnGameObjectDestroy();

        internal void SetEnabled(bool v)
        {
            button.interactable = v;
            enabled = v;
        }

        #region ITransform<T>

        public Transform Transform => transform;

        public void OnGameObjectDestroy()
        {
            if (PackedEntity == null || !PackedEntity.Value.Unpack(out var world, out var entity))
                return; //throw new Exception("No Entity for Entity view");

            EcsService.UnregisterEntityButtonRef<T>(this);
        }

        #endregion

        #region IEntityButton<T>

        public EcsPackedEntityWithWorld? PackedEntity { get; set; }
        public IEcsService EcsService { get; set; }
       
        public void Toggle(bool toggle)
        {
            gameObject.SetActive(toggle);
        }

        
        #endregion
    }

}

