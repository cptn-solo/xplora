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
        if (GUILayout.Button("Run Function"))
        {
            FunctionToRun();
        }
    }

    private void FunctionToRun()
    {
        Debug.Log("The function ran.");
        var libraryMetadata = new GoogleSheetReader();
        var list = libraryMetadata.GetSheetRange("'Герои'!A1:I19");
        
        var serialized = JsonConvert.SerializeObject(list);
        File.WriteAllText(Application.dataPath + "/Heroes.json", serialized);

    }
}
#endif
