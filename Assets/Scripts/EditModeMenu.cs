#if UNITY_EDITOR
using Newtonsoft.Json;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditModeMenu : EditorWindow
{

    [MenuItem("Window/XPLORA Data Utilities")]
    public static void ShowWindow()
    {
        GetWindow<EditModeMenu>("XPLORA Data Utilities");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Load Heroes (Google)"))
        {
            LoadHeroesFromGoogleSheet();
        }
    }

    private void LoadHeroesFromGoogleSheet()
    {
        Debug.Log("The function ran.");
        var libraryMetadata = new GoogleSheetReader();
        var list = libraryMetadata.GetSheetRange("'Герои'!A1:I31");
        
        var serialized = JsonConvert.SerializeObject(list);
        File.WriteAllText(Application.streamingAssetsPath + "/Heroes.json", serialized);

    }
}
#endif
