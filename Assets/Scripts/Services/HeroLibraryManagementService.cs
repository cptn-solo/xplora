using Assets.Scripts.UI.Data;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;

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
            var damageMinMax = list[4];
            var defenceRates = list[5];
            var accuracyRates = list[6];
            var dodgeRates = list[7];
            var healths = list[8];
            var speeds = list[9];
            var criticalHitRates = list[10];
            var attackTypes = list[11];
            var damageTypes = list[12];
            var resistBleedRates = list[13];
            var resistPoisonRates = list[14];
            var resistStunRates = list[15];
            var resistBurnRates = list[16];
            var resistFrostRates = list[17];
            var resistFlushRates = list[18];

            for (int col = 0; col < heroesNumber; col++)
            {
                var cellNumber = col + 1; // 1st column is used for headers
                var id = col;
                var heroName = (string)names[cellNumber];
                var typeName = (string)heroTypes[cellNumber];

                var iconName = $"{heroesIconsPath}/{typeName}";

                if (library.HeroById(id) is Hero hero && hero.HeroType != HeroType.NA)
                {
                    UpdateHero(hero, cellNumber);
                }
                else if(library.GiveHero(Hero.EmptyHero(id, heroName, iconName)))
                { 
                    UpdateHero(library.HeroById(id), cellNumber);
                }
            }

            OnDataAvailable?.Invoke();

            bool UpdateHero(Hero hero, int cellNumber)
            {
                hero.Name = (string)names[cellNumber];

                ParseAbsoluteRangeValue((string)damageMinMax[cellNumber], out int minVal, out int maxVal);
                hero.DamageMin = minVal;
                hero.DamageMax = maxVal;

                hero.DefenceRate = ParseRateValue((string)defenceRates[cellNumber]);
                hero.AccuracyRate = ParseRateValue((string)accuracyRates[cellNumber]);
                hero.DodgeRate = ParseRateValue((string)dodgeRates[cellNumber]);
                hero.Health = ParseAbsoluteValue((string)healths[cellNumber]);
                hero.Speed = ParseAbsoluteValue((string)speeds[cellNumber]);
                hero.CriticalHitRate = ParseRateValue((string)criticalHitRates[cellNumber]);

                hero.ResistBleedRate = ParseAbsoluteValue((string)resistBleedRates[cellNumber]);
                hero.ResistPoisonRate = ParseAbsoluteValue((string)resistPoisonRates[cellNumber]);
                hero.ResistStunRate = ParseAbsoluteValue((string)resistStunRates[cellNumber]);
                hero.ResistBurnRate = ParseAbsoluteValue((string)resistBurnRates[cellNumber]);
                hero.ResistFrostRate = ParseAbsoluteValue((string)resistFrostRates[cellNumber]);
                hero.ResistFlushRate = ParseAbsoluteValue((string)resistFlushRates[cellNumber]);
                
                return true;
            }

            IEnumerable<KeyValuePair<K, T>> ExistingItem<T, K>(Dictionary<K, T> dict, K id) where T : IIdentifiable<K>
            {
                return dict.Where(x => x.Value.Id.Equals(id));
            }
        }

        private static void ParseAbsoluteRangeValue(string rawValue, out int minVal, out int maxVal)
        {
            try
            {
                var rawValues = rawValue.Replace(" ", "").Split('-');
                minVal = int.Parse(rawValues[0], System.Globalization.NumberStyles.None);
                maxVal = int.Parse(rawValues[1], System.Globalization.NumberStyles.None);
            }
            catch (Exception ex)
            {
                minVal = 0;
                maxVal = 0;
                Debug.LogError($"ParseAbsoluteRangeValue [{rawValue}] Exception: {ex.Message}");
            }
        }

        private static int ParseRateValue(string rawValue)
        {
            try
            {
                var rawValues = rawValue.Replace("%", "").Replace(" ", "");
                return int.Parse(rawValues, System.Globalization.NumberStyles.None);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseRateValue [{rawValue}] Exception: {ex.Message}");
                return 0;
            }
        }

        private static int ParseAbsoluteValue(string rawValue)
        {
            try
            {
                var rawValues = rawValue.Replace("%", "").Replace(" ", "");
                return int.Parse(rawValues, System.Globalization.NumberStyles.None);
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseAbsoluteValue [{rawValue}] Exception: {ex.Message}");
                return 0;
            }
        }

        internal void ResetHealthCurrent() =>
            library.ResetHealthCurrent();
    }
}