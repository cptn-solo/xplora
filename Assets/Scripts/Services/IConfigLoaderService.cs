namespace Assets.Scripts.Services
{
    public interface IConfigLoaderService
    {
        public bool DataAvailable { get; }
        public void InitConfigLoading();
        public void NotifyIfAllDataAvailable();
        public void LoadCachedData();
        public void LoadRemoteData();
    }

}

