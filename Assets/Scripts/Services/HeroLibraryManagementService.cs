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
        public Team PlayerTeam => library.PlayerTeam;
        public Team EnemyTeam => library.EnemyTeam;


        private const int heroesNumber = 8;
        private const string heroesIconsPath = "Heroes/Icons";
       
        [SerializeField] private List<List<object>> heroesRawData;

        public event UnityAction<string> OnDataLoaded;
        public event UnityAction OnDataAvailable;

        private bool dataAvailable = false;
        public bool DataAvailable => dataAvailable;

        private void Awake()
        {
            OnDataLoaded += HeroLibraryManagementService_OnDataLoaded;
        }

        public bool PrepareTeamsForBattle(out Hero[] playerHeroes, out Hero[] enemyHeroes)
        {
            ResetHealthCurrent();
            playerHeroes = library.TeamHeroes(PlayerTeam.Id, true);
            enemyHeroes = library.TeamHeroes(EnemyTeam.Id, true);
            
            if (playerHeroes.Length > 0 && enemyHeroes.Length > 0)
                return true;

            return false;
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
            list = libraryMetadata.GetSheetRange("'Герои'!A1:I31");

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

            var sndAttack = list[21]; // Герой атакует
            var sndDodged = list[22]; // Герой увернулся(дамаг не был наложен)
            var sndHit = list[23]; // Герой принял обычный удар
            var sndStunned = list[24]; // Герой принял или находится Оглушение
            var sndBleeding = list[25]; // Герой принял или находится Кровотечение
            var sndPierced = list[26]; // Герой принял Пробитие брони
            var sndBurning = list[27]; // Герой принял или находится Горение
            var sndFreezed = list[28]; // Герой принял или находится Заморозка
            var sndCritHit = list[29]; // Герой принял Критический удар
            var sndDied = list[30]; // Герой умер

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
            
            dataAvailable = true;
            OnDataAvailable?.Invoke();

            bool UpdateHero(Hero oldHero, int cellNumber)
            {
                var hero = oldHero;

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
                hero.AttackType = ParseAttackType((string)attackTypes[cellNumber]);
                hero.DamageType = ParseDamageType((string)damageTypes[cellNumber]);
                hero.ResistBleedRate = ParseAbsoluteValue((string)resistBleedRates[cellNumber]);
                hero.ResistPoisonRate = ParseAbsoluteValue((string)resistPoisonRates[cellNumber]);
                hero.ResistStunRate = ParseAbsoluteValue((string)resistStunRates[cellNumber]);
                hero.ResistBurnRate = ParseAbsoluteValue((string)resistBurnRates[cellNumber]);
                hero.ResistFrostRate = ParseAbsoluteValue((string)resistFrostRates[cellNumber]);
                hero.ResistFlushRate = ParseAbsoluteValue((string)resistFlushRates[cellNumber]);

                hero.SndAttack = ParseSoundFileValue((string)sndAttack[cellNumber]);
                hero.SndDodged = ParseSoundFileValue((string)sndDodged[cellNumber]);
                hero.SndHit = ParseSoundFileValue((string)sndHit[cellNumber]);
                hero.SndStunned = ParseSoundFileValue((string)sndStunned[cellNumber]);
                hero.SndBleeding = ParseSoundFileValue((string)sndBleeding[cellNumber]);
                hero.SndPierced = ParseSoundFileValue((string)sndPierced[cellNumber]);
                hero.SndBurning = ParseSoundFileValue((string)sndBurning[cellNumber]);
                hero.SndFreezed = ParseSoundFileValue((string)sndFreezed[cellNumber]);
                hero.SndCritHit = ParseSoundFileValue((string)sndCritHit[cellNumber]);
                hero.SndDied = ParseSoundFileValue((string)sndDied[cellNumber]);

                library.UpdateHero(hero);

                return true;
            }
        }
        private DamageType ParseDamageType(string rawValue)
        {
            try
            {
                var rawValues = rawValue.Replace(" ", "").ToLower();
                return rawValues switch
                {
                    "Силовой" => DamageType.Force,
                    "Режущий" => DamageType.Cut,
                    "Колющий" => DamageType.Pierce,
                    "Огненный" => DamageType.Burn,
                    "Замораживающий" => DamageType.Frost,
                    _ => DamageType.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseDamageType [{rawValue}] Exception: {ex.Message}");
                return DamageType.NA;
            }
        }
        private AttackType ParseAttackType(string rawValue)
        {
            try
            {
                var rawValues = rawValue.Replace(" ", "").ToLower();
                return rawValues switch
                {
                    "дистанционная" => AttackType.Ranged,
                    "ближняя" => AttackType.Melee,
                    "магическая" => AttackType.Magic,
                    _ => AttackType.NA
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseAttackType [{rawValue}] Exception: {ex.Message}");
                return AttackType.NA;
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
        private static string ParseSoundFileValue(string rawValue)
        {
            try
            {
                var rawValues = rawValue.ToLower().Replace(".mp3", "").Replace(" ", "");
                return rawValues.Trim();
            }
            catch (Exception ex)
            {
                Debug.LogError($"ParseSoundFileValue [{rawValue}] Exception: {ex.Message}");
                return "nosound";
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

        internal void ResetTeams()
        {
            library = Library.ResetTeamAssets();
            Library.ResetHealthCurrent();

        }
    }
}