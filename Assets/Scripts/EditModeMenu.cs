#if UNITY_EDITOR
using Assets.Scripts.Services;
using Assets.Scripts.Services.ConfigDataManagement.Parsers;
using Assets.Scripts.UI.Data;
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
        else if (GUILayout.Button("Load Damage Types (Google)"))
        {
            LoadDamageTypesFromGoogleSheet();
        }
    }

    private void LoadHeroesFromGoogleSheet()
    {
        Debug.Log("The function LoadHeroesFromGoogleSheet ran.");
        
        var loader = new HeroesConfigLoader(HeroesLibrary.EmptyLibrary(), () => { });
        loader.LoadGoogleData();        
    }

    private void LoadDamageTypesFromGoogleSheet()
    {
        Debug.Log("The function LoadDamageTypesFromGoogleSheet ran.");

        var loader = new DamageTypesConfigLoader(DamageTypesLibrary.EmptyLibrary(), () => { });
        loader.LoadGoogleData();
    }

}
#endif
