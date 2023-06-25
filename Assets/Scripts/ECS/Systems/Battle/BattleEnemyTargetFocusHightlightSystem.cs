using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleEnemyTargetFocusHightlightSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<EffectFocusComp> pool = default;        
        private readonly EcsPoolInject<TransformRef<AimTargetTag>> aimIconPool = default;
        private readonly EcsPoolInject<EntityViewRef<BarsAndEffectsInfo>> viewRefPool = default;
        
        private readonly EcsFilterInject<
            Inc<EffectFocusComp>, 
            Exc<TransformRef<AimTargetTag>>
            > filter = default;
        
        private readonly EcsCustomInject<BattleManagementService> battleManagementService = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var focusComp = ref pool.Value.Get(entity);
                if (!focusComp.Focused.Unpack(out var world, out var focusedEntity))
                    throw new Exception("Stale Focused entity");

                if (!viewRefPool.Value.Has(focusedEntity))
                    continue;
                
                ref var viewRef = ref viewRefPool.Value.Get(focusedEntity);
                var anchor = viewRef.EntityView.Transform;

                var icon = battleManagementService.Value.BundleIconFactory.Create();
                icon.transform.SetParent(anchor);
                icon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                icon.transform.localScale = Vector3.one;

                var rt = (RectTransform)anchor.transform;
                var iconRt = (RectTransform)icon.transform;
                float randomOffset = Random.Range(-.2f, .2f);
                iconRt.anchoredPosition = rt.rect.center * randomOffset + Vector2.up * rt.rect.height;
                
                icon.SetInfo(new BundleIconInfo
                {
                    Icon = BundleIcon.AimTarget,
                    IconColor = focusComp.EffectKey.RelationsEffectType == RelationsEffectType.AlgoRevenge ? 
                        Color.blue : Color.Lerp(Color.red, Color.yellow, .5f),
                });


                ref var aimIconRef = ref aimIconPool.Value.Add(entity);
                aimIconRef.Transform = icon.transform;
            }
        }
    }
}
