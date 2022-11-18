using Assets.Scripts.UI.Data;

#if !PLATFORM_STANDALONE_WIN && !UNITY_EDITOR

using Newtonsoft.Json;
using System.IO;

#endif

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public partial class HeroLibraryManagementService : MonoBehaviour
    {
        private HeroesLibrary library = HeroesLibrary.EmptyLibrary();
        public HeroesLibrary Library => library;

        private const int heroesNumber = 8;
        private const string heroesIconsPath = "Heroes/Icons";

        [SerializeField] private List<List<object>> heroesRawData;
        public void LoadData()
        {
            IList<IList<object>> list = null;

#if PLATFORM_STANDALONE_WIN || UNITY_EDITOR
            var libraryMetadata = new GoogleSheetReader();
            list = libraryMetadata.GetSheetRange("'Герои'!A1:I19");
#else
            var serialized = File.ReadAllText(Application.dataPath + "/Heroes.json");
            list = JsonConvert.DeserializeObject<string[][]>(serialized);
#endif

            if (list.Count < 3)
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

            IEnumerable<KeyValuePair<K, T>> ExistingItem<T, K>(Dictionary<K, T> dict, K id) where T: IIdentifiable<K>
            {
                return dict.Where(x => x.Value.Id.Equals(id));
            }
        }
    }
}