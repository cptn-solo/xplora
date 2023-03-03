namespace Assets.Scripts.Services
{
    public interface IEcsService
    {
        public void RegisterTransformRef<T>(ITransform<T> transformRefOrigin);
        public void UnregisterTransformRef<T>(ITransform transformRefOrigin);
    }

}

