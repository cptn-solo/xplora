using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Color = UnityEngine.Color;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleShowRelationEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<RelationEffectInfo>> pool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;

        private readonly EcsFilterInject<
            Inc<ItemsContainerRef<RelationEffectInfo>,
                RelationEffectsComp,
                UpdateTag<RelationEffectInfo>>
            > filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var relationEffects = ref relEffectsPool.Value.Get(entity);
                var buffer = ListPool<RelationEffectInfo>.Get();

                foreach (var item in relationEffects.CurrentEffects)
                {
                    if (!item.Value.EffectSource.Unpack(out var origWorld, out var effectSourceOrigEntity))
                        continue;

                    var heroIconPool = origWorld.GetPool<NameValueComp<IconTag>>();
                    ref var heroIcon = ref heroIconPool.Get(effectSourceOrigEntity);
                    var effectType = item.Value.Rule.EffectType;
                    var info = new RelationEffectInfo()
                    {
                        Id = (int)effectType,
                        EffectText = $"{item.Value.Rule.Description} ({item.Value.UsageLeft})",
                        HeroIcon = heroIcon.Name,
                        EffectIcon = effectType.EffectIcon(),
                        EffectIconColor = Color.yellow,
                    };

                    buffer.Add(info);
                }

                ref var viewRef = ref pool.Value.Get(entity);
                viewRef.Container.SetInfo(buffer.ToArray());

                ListPool<RelationEffectInfo>.Add(buffer);
            }
        }
    }
}
