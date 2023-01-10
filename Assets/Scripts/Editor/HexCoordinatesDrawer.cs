using Assets.Scripts.World.HexMap;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexCoordinates))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position, SerializedProperty property, GUIContent label
    )
    {
        #region precode for labels i guess
        // For this particular purpose, the LABEL text must be CLEAR. (?)
        label.text = "Coordinates";

        // Using BeginProperty / EndProperty on the parent property means that (?)
        // prefab override logic works on the entire property. (?)
        EditorGUI.BeginProperty(position, label, property);

        // Draw label (?)
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        //// Don't make child fields be indented (?)
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;
        #endregion

        HexCoordinates coordinates = new(
            property.FindPropertyRelative("x").intValue,
            property.FindPropertyRelative("z").intValue
        );
        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());

        #region postcode for labels I guess
        // Set indent back to what it was (?)
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
        #endregion
    }
}
