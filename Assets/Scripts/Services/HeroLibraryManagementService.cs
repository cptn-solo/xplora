using Assets.Scripts.UI.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public partial class HeroLibraryManagementService : MonoBehaviour
    {
        private GoogleSheetReader libraryMetadata;
        private HeroesLibrary library = HeroesLibrary.EmptyLibrary();
        public HeroesLibrary Library => library;

        private const int heroesNumber = 8;
        private const string heroesIconsPath = "Heroes/Icons";

        public void LoadData()
        {
            libraryMetadata = new GoogleSheetReader();
            var list = libraryMetadata.GetSheetRange("'Герои'!A1:I19");
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