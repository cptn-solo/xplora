using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Assets.Scripts
{
    public class HeroLibraryManagementService : MonoBehaviour
    {
        private HeroesLibrary library = HeroesLibrary.EmptyLibrary();
        public HeroesLibrary Library => library;

        private const int heroesNumber = 8;
        private const string heroesIconsPath = "Heroes/Icons";
       
        [SerializeField] private List<List<object>> heroesRawData;

        public event UnityAction<string> OnDataLoaded;
        public event UnityAction OnDataAvailable;

        private void Awake()
        {
            OnDataLoaded += HeroLibraryManagementService_OnDataLoaded;
        }

        private void HeroLibraryManagementService_OnDataLoaded(string serialized)
        {
            ProcessHeroesSerializedString(serialized);
        }

        private IEnumerator LoadStreamingAsset(string fileName)
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

            OnDataLoaded?.Invoke(results);
        }

        public void LoadData()
        {
            StartCoroutine(LoadStreamingAsset("Heroes.json"));
        }

        public void LoadGoogleData()
        {
            IList<IList<object>> list = null;

#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR

            var libraryMetadata = new GoogleSheetReader();
            list = libraryMetadata.GetSheetRange("'Герои'!A1:I19");

#endif
            ProcessHeroesList(list);
        }

        private void ProcessHeroesSerializedString(string serialized)
        {
            IList<IList<object>> list = JsonConvert.DeserializeObject<string[][]>(serialized);
            ProcessHeroesList(list);
        }

        private void ProcessHeroesList(IList<IList<object>> list)
        {
            if (list == null || list.Count < 3)
                return;

            var heroTypes = list[0];
            var names = list[1];

            for (int col = 0; col < heroesNumber; col++)
            {
                var cellNumber = col + 1; // 1st column is used for headers
                var id = col;
                var heroName = (string)names[cellNumber];
                var typeName = (string)heroTypes[cellNumber];

                var iconName = $"{heroesIconsPath}/{typeName}";
                if (!UpdateHero(id, library.Heroes, heroName) &&
                    !UpdateHero(id, library.PlayerTeam, heroName) &&
                    !UpdateHero(id, library.EnemyTeam, heroName))
                {
                    library.GiveHero(Hero.EmptyHero(id, heroName, iconName));
                }
            }

            OnDataAvailable?.Invoke();

            bool UpdateHero(int id, Dictionary<int, Hero> dict, string heroName)
            {
                if (ExistingItem(dict, id).FirstOrDefault() is KeyValuePair<int, Hero> h &&
                    h.Value.HeroType != HeroType.NA)
                {
                    var hero = h.Value;
                    hero.Name = heroName;
                    dict[h.Key] = hero;
                    return true;
                }
                return false;
            }

            IEnumerable<KeyValuePair<K, T>> ExistingItem<T, K>(Dictionary<K, T> dict, K id) where T : IIdentifiable<K>
            {
                return dict.Where(x => x.Value.Id.Equals(id));
            }
        }
    }
}