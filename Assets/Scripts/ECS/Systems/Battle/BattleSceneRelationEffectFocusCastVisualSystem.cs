using Assets.Scripts.ECS.Data;
using Assets.Scripts.Services;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleSceneRelationEffectFocusCastVisualSystem :
        BattleSceneVisualSystem<RelationEffectsFocusCastInfo>
    {
        private readonly EcsPoolInject<TransformRef<AimTargetTag>> aimIconPool = default;

        private readonly EcsCustomInject<BattleManagementService> battleManagementService = default;

        protected override void AssignVisualizer(int entity, RelationEffectsFocusCastInfo visualInfo, EcsWorld world, int viewEntity)
        {
            if (!visualInfo.SubjectEntity.Unpack(out _, out var subjectEntity))
                throw new Exception("Stale effect subject entity");

            var anchor = visualInfo.TargetTransform;

            if (!aimIconPool.Value.Has(subjectEntity))
                aimIconPool.Value.Add(subjectEntity);

            ref var aimIconRef = ref aimIconPool.Value.Get(subjectEntity);

            BundleIconHost icon = aimIconRef.Transform == null ? 
                battleManagementService.Value.BundleIconFactory.Create() :
                aimIconRef.Transform.GetComponent<BundleIconHost>();

            icon.transform.SetParent(anchor);
            icon.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            icon.transform.localScale = Vector3.one;

            var rt = (RectTransform)anchor.transform;
            var iconRt = (RectTransform)icon.transform;
            float randomOffset = UnityEngine.Random.Range(-.2f, .2f);
            iconRt.anchoredPosition = rt.rect.center * randomOffset;

            icon.SetInfoAnimated(visualInfo.FocusInfo, visualInfo.SourceTransform);

            aimIconRef.Transform = icon.transform;

            // TODO: remove after the animation implementation
            base.AssignVisualizer(entity, visualInfo, world, viewEntity);
        }
    }
}
