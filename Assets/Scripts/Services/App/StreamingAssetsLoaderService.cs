using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Services.App
{
    public class StreamingAssetsLoaderService : MonoBehaviour
    {
        public delegate void LoaderDelegate(string data);

        public void LoadData(string assetFileName, LoaderDelegate callback) =>
            StartCoroutine(LoadStreamingAsset(assetFileName, callback));

        // Use this for initialization
        private IEnumerator LoadStreamingAsset(string fileName, LoaderDelegate callback = null) 
        {
            string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
            string results = "";

            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                var www = UnityWebRequest.Get(filePath);

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // Show results as text
                    Debug.Log(www.downloadHandler.text);

                    // Or retrieve results as binary data
                    results = www.downloadHandler.text;
                }
            }
            else
                results = System.IO.File.ReadAllText(filePath);

            callback?.Invoke(results);
        }
    }
}