#if UNITY_EDITOR
using Assets.Scripts.Services.ConfigDataManagement.Parsers;
using Assets.Scripts.Data;
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
        else if (GUILayout.Button("Load Terrain Attributes (Google)"))
        {
            LoadTerrainAttributes();
        }
        else if (GUILayout.Button("Load Terrain Events (Google)"))
        {
            LoadTerrainEvents();
        }
    }

    private void LoadHeroesFromGoogleSheet()
    {        
        var loader = new HeroesConfigLoader(HeroesLibrary.EmptyLibrary(), () => { });
        loader.LoadGoogleData();

        Debug.Log("The function LoadHeroesFromGoogleSheet ran.");
    }

    private void LoadDamageTypesFromGoogleSheet()
    {
        var loader = new DamageTypesConfigLoader(DamageTypesLibrary.EmptyLibrary(), () => { });
        loader.LoadGoogleData();

        Debug.Log("The function LoadDamageTypesFromGoogleSheet ran.");
    }

    private void LoadTerrainAttributes()
    {
        var loader = new TerrainAttributesConfigLoader(TerrainAttributesLibrary.EmptyLibrary(), () => { });
        loader.LoadGoogleData();

        Debug.Log("The function LoadTerrainAttributes ran.");
    }

    private void LoadTerrainEvents()
    {
        var loader = new TerrainEventsConfigLoader(TerrainEventLibrary.EmptyLibrary(), () => { });
        loader.LoadGoogleData();

        Debug.Log("The function LoadTerrainEvents ran.");
    }

}
#endif
