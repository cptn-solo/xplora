using UnityEngine;
using Zenject;

namespace Assets.Scripts.UI.Common
{
    public partial class BaseItemsContainer<T, I, F> : BaseItemsContainer<T, I>
        where T : struct, IContainableItemInfo<int>
        where I : MonoBehaviour, IContainableItem<T>
        where F : PlaceholderFactory<I>
    {
        [Inject]
        protected DiContainer Container;

        protected override I Spawn()
        {
            var item = Container.Resolve<F>().Create();
            return item;
        }
    }
}