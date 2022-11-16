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
                if (library.Heroes.Where(x => x.Value.Id == id).FirstOrDefault() is KeyValuePair<int, Hero> h &&
                    h.Value.HeroType != HeroType.NA)
                {
                    var hero = h.Value;
                    hero.Name = heroName;
                    library.Heroes[h.Key] = hero;
                }
                else
                {                    
                    library.GiveHero(Hero.EmptyHero(id, heroName, iconName));
                }
            }

        }
    }
}