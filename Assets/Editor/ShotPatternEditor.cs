#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShotPattern))]
public class ShotPatternEditor : PropertyDrawer
{
    private const float CellSize = 20f;
    private const float Spacing = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Get the serialized grid array
        SerializedProperty gridProp = property.FindPropertyRelative("_grid");

        // Initialize if empty
        if (gridProp.arraySize != 25)
        {
            gridProp.arraySize = 25;
            for (int i = 0; i < 25; i++)
                gridProp.GetArrayElementAtIndex(i).intValue = 0;
        }

        // Draw grid
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                int index = y * 5 + x;
                SerializedProperty cellProp = gridProp.GetArrayElementAtIndex(index);

                Rect cellRect = new Rect(
                    position.x + x * (CellSize + Spacing),
                    position.y + y * (CellSize + Spacing),
                    CellSize,
                    CellSize
                );

                EditorGUI.BeginChangeCheck();
                int newValue = EditorGUI.IntField(cellRect, cellProp.intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    cellProp.intValue = Mathf.Max(0, newValue);
                }
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 5 * (CellSize + Spacing) + EditorGUIUtility.singleLineHeight;
    }
}
#endif