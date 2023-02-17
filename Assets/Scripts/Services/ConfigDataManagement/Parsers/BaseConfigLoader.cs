using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Services.ConfigDataManagement.Parsers
{
    public class BaseConfigLoader
    {
        public delegate void DataDelegate();
        
        public DataDelegate OnDataAvailable;

        protected bool dataAvailable = false;
        
        internal delegate object ValueGetter(int row, int cell);

        public BaseConfigLoader(DataDelegate dataDelegate) =>
            OnDataAvailable = dataDelegate;

        public BaseConfigLoader() { }

        public bool DataAvailable => dataAvailable;

        protected virtual string RangeString => null;
        protected virtual string ConfigName => null; //without extension

        public string ConfigFileName => $"{ConfigName}.json";

        private void NotifyDataAvailable()
        {
            dataAvailable = true;
            OnDataAvailable?.Invoke();
        }

        public void LoadGoogleData()
        {
            IList<IList<object>> list = null;

#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR

            var libraryMetadata = new GoogleSheetReader();
            list = libraryMetadata.GetSheetRange(RangeString);
            
            var serialized = JsonConvert.SerializeObject(list);
            File.WriteAllText(Application.streamingAssetsPath + $"/{ConfigFileName}", serialized);

#endif
            ProcessList(list);
            
            NotifyDataAvailable();
        }

        public void ProcessSerializedString(string serialized)
        {
            IList<IList<object>> list = JsonConvert.DeserializeObject<string[][]>(serialized);
            
            ProcessList(list);
            
            NotifyDataAvailable();
        }

        protected virtual void ProcessList(IList<IList<object>> list) { }
    }
}