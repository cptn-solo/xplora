#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Assets.Scripts.Services;

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
        else if (GUILayout.Button("Load Terrain POI configs (Google)"))
        {
            LoadTerrainPOIs();
        }
        else if (GUILayout.Button("Load Enemy Spawn rules (Google)"))
        {
            LoadEnemySpawnRules();
        }
    }

    private void LoadHeroesFromGoogleSheet()
    {
        var loader = new HeroesConfigLoader();
        loader.LoadGoogleData();

        Debug.Log("The function LoadHeroesFromGoogleSheet ran.");
    }

    private void LoadDamageTypesFromGoogleSheet()
    {
        var loader = new DamageTypesConfigLoader();
        loader.LoadGoogleData();

        Debug.Log("The function LoadDamageTypesFromGoogleSheet ran.");
    }

    private void LoadTerrainAttributes()
    {
        var loader = new TerrainAttributesConfigLoader();
        loader.LoadGoogleData();

        Debug.Log("The function LoadTerrainAttributes ran.");
    }

    private void LoadTerrainEvents()
    {
        var loader = new TerrainEventsConfigLoader();
        loader.LoadGoogleData();

        Debug.Log("The function LoadTerrainEvents ran.");
    }

    private void LoadTerrainPOIs()
    {
        var loader = new TerrainPOIsConfigLoader();
        loader.LoadGoogleData();

        Debug.Log("The function LoadTerrainPOIs ran.");
    }

    private void LoadEnemySpawnRules()
    {
        var loader = new EnemySpawnRulesConfigLoader();
        loader.LoadGoogleData();

        Debug.Log("The function LoadEnemySpawnRules ran.");
    }

}
#endif
