using UnityEngine;
using Leopotam.EcsLite;

namespace Assets.Scripts.Services
{
    public partial class BaseEcsService : MonoBehaviour
    {
        protected EcsWorld ecsWorld { get; set; }

        public EcsWorld EcsWorld => ecsWorld;

        protected IEcsSystems ecsRunSystems { get; set; }
        protected IEcsSystems ecsInitSystems { get; set; }                        
    }

}

