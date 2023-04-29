using Assets.Scripts.Data;
using Assets.Scripts.ECS.Data;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using System;
using Color = UnityEngine.Color;

namespace Assets.Scripts.ECS.Systems
{
    public class BattleShowRelationEffectsSystem : IEcsRunSystem
    {
        private readonly EcsPoolInject<ItemsContainerRef<RelationEffectInfo>> pool = default;
        private readonly EcsPoolInject<HeroInstanceOriginRefComp> instanceOriginRefPool = default;
        private readonly EcsPoolInject<RelationEffectsComp> relEffectsPool = default;

        private readonly EcsFilterInject<
            Inc<ItemsContainerRef<RelationEffectInfo>,
                RelationEffectsComp,
                HeroInstanceOriginRefComp,
                NameValueComp<IconTag>,
                UpdateTag<RelationEffectInfo>>
            > filter = default;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in filter.Value)
            {
                ref var originRef = ref instanceOriginRefPool.Value.Get(entity);
                if (!originRef.Packed.Unpack(out var origWorld, out var origEntity))
                    throw new Exception("No origin for hero");

                ref var relationEffects = ref relEffectsPool.Value.Get(entity);
                var buffer = ListPool<RelationEffectInfo>.Get();

                var heroIconPool = origWorld.GetPool<NameValueComp<IconTag>>();
                foreach (var item in relationEffects.CurrentEffects)
                {
                    if (!item.Value.EffectSource.Unpack(origWorld, out var effectSourceOrigEntity))
                        continue;

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
