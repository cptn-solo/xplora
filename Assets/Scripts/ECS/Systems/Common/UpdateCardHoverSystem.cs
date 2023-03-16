using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite.Di;
using UnityEngine;

namespace Assets.Scripts.ECS.Systems
{
    public class UpdateCardHoverSystem
    {
        protected readonly EcsPoolInject<EntityViewRef<CameraTag>> cameraRefPool = default;

        protected readonly EcsFilterInject<
            Inc<EntityViewRef<CameraTag>
                >> cameraFilter = default;

        protected void PositionHoverView(Transform hostTransform, Transform hoverTransform)
        {
            foreach (var cameraEntity in cameraFilter.Value)
            {
                ref var cameraRef = ref cameraRefPool.Value.Get(cameraEntity);
                var camView = (ICameraView)cameraRef.EntityView;
                var relativePos = camView.GetViewPosition(hostTransform.position);
                var offset = relativePos.y < 0.3 ? Vector3.up * 2f : Vector3.up * -2f;

                hoverTransform.position = hostTransform.position + offset;
            }
        }

    }
}