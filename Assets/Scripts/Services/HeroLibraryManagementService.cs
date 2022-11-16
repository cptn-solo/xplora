using Assets.Scripts.UI.Data;
using UnityEngine;

namespace Assets.Scripts
{
    public partial class HeroLibraryManagementService : MonoBehaviour
    {
        private GoogleSheetReader libraryMetadata;
        private HeroesLibrary library = HeroesLibrary.EmptyLibrary();
        public HeroesLibrary Library => library;

        public void LoadData()
        {
            libraryMetadata = new GoogleSheetReader();
            var list = libraryMetadata.GetSheetRange("'Герои'!A1:I19");
            Debug.Log(list);
        }
    }
}