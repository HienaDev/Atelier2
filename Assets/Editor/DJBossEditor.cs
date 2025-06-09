#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


[CustomEditor(typeof(DJBoss))]
public class DJBossEditor : Editor
{
    private SerializedProperty attackPatternsProperty;
    private SerializedProperty easyAttackPatternsProperty;
    private SerializedProperty normalAttackPatternsProperty;
    private SerializedProperty usePatterns;
    private bool showPatterns = true;

    void OnEnable()
    {
        attackPatternsProperty = serializedObject.FindProperty("attackPatterns");
        easyAttackPatternsProperty = serializedObject.FindProperty("easyAttackPatterns");
        normalAttackPatternsProperty = serializedObject.FindProperty("normalAttackPatterns");
        usePatterns = serializedObject.FindProperty("usePatterns");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(serializedObject, new string[3] { "attackPatterns", "easyAttackPatterns", "normalAttackPatterns" });

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Attack Pattern System", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(usePatterns, new GUIContent("Use Pattern Mode"));

        EditorGUILayout.Space(5);
        showPatterns = EditorGUILayout.Foldout(showPatterns, "Attack Patterns", true, EditorStyles.foldoutHeader);

        if (showPatterns)
        {
            EditorGUI.indentLevel++;

            DrawPatternsList(attackPatternsProperty, "Main Attack Patterns");
            EditorGUILayout.Space(10);
            DrawPatternsList(easyAttackPatternsProperty, "Easy Attack Patterns");
            EditorGUILayout.Space(10);
            DrawPatternsList(normalAttackPatternsProperty, "Normal Attack Patterns");

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPatternsList(SerializedProperty patternsProperty, string label)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        // Add/Remove pattern buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Pattern", GUILayout.Height(25)))
        {
            int newIndex = patternsProperty.arraySize;
            patternsProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty newPattern = patternsProperty.GetArrayElementAtIndex(newIndex);
            SerializedProperty patternName = newPattern.FindPropertyRelative("patternName");
            SerializedProperty attacks = newPattern.FindPropertyRelative("attacks");
            SerializedProperty patternDelay = newPattern.FindPropertyRelative("patternDelay");

            patternName.stringValue = $"New Pattern {newIndex + 1}";
            patternDelay.floatValue = 0f;
            attacks.ClearArray();
        }

        if (patternsProperty.arraySize > 0 && GUILayout.Button("Remove Last Pattern", GUILayout.Height(25)))
        {
            patternsProperty.DeleteArrayElementAtIndex(patternsProperty.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Draw each pattern
        for (int i = 0; i < patternsProperty.arraySize; i++)
        {
            DrawPattern(patternsProperty, i);
            EditorGUILayout.Space(10);
        }
    }

    private void DrawPattern(SerializedProperty patternsProperty, int patternIndex)
    {
        SerializedProperty pattern = patternsProperty.GetArrayElementAtIndex(patternIndex);
        SerializedProperty patternName = pattern.FindPropertyRelative("patternName");
        SerializedProperty attacks = pattern.FindPropertyRelative("attacks");
        SerializedProperty patternDelay = pattern.FindPropertyRelative("patternDelay");

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        string displayName = string.IsNullOrEmpty(patternName.stringValue) ? $"Pattern {patternIndex + 1}" : patternName.stringValue;
        EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel);

        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
        {
            patternsProperty.DeleteArrayElementAtIndex(patternIndex);
            return;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(patternName, new GUIContent("Pattern Name"));
        EditorGUILayout.PropertyField(patternDelay, new GUIContent("Pattern Delay"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Attack Commands:", EditorStyles.boldLabel);

        // Add/Remove attack buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Attack", GUILayout.Height(20)))
        {
            AddNewAttackCommand(attacks);
        }

        if (attacks.arraySize > 0 && GUILayout.Button("Remove Last", GUILayout.Height(20)))
        {
            attacks.DeleteArrayElementAtIndex(attacks.arraySize - 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUI.indentLevel++;
        for (int j = 0; j < attacks.arraySize; j++)
        {
            DrawAttackCommand(attacks.GetArrayElementAtIndex(j), j);
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
    }


    private void DrawAttackCommand(SerializedProperty attackCommand, int index)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Attack {index + 1}", EditorStyles.boldLabel, GUILayout.Width(80));

        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
        {
            // Get the parent array property path by removing the array index part
            string propertyPath = attackCommand.propertyPath;
            string parentPath = propertyPath.Substring(0, propertyPath.LastIndexOf(".Array.data["));
            SerializedProperty parentArray = serializedObject.FindProperty(parentPath);

            if (parentArray != null && parentArray.isArray)
            {
                // Record undo before making changes
                Undo.RecordObject(serializedObject.targetObject, "Remove Attack Command");

                // Delete the array element
                parentArray.DeleteArrayElementAtIndex(index);

                // Apply changes immediately
                serializedObject.ApplyModifiedProperties();

                // End the GUI groups we started
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                // Exit early since we've deleted this element
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        // Only try to access properties if we didn't delete this element
        try
        {
            SerializedProperty attackType = attackCommand.FindPropertyRelative("attackType");
            SerializedProperty targetColumns = attackCommand.FindPropertyRelative("targetColumns");
            SerializedProperty delay = attackCommand.FindPropertyRelative("delay");
            SerializedProperty numberOfShots = attackCommand.FindPropertyRelative("numberOfShots");
            SerializedProperty delayBetweenCommands = attackCommand.FindPropertyRelative("delayBetweenCommands");

            // Attack type
            EditorGUILayout.PropertyField(attackType, new GUIContent("Attack Type"));

            // Only show number of shots for column attacks
            if (attackType.enumValueIndex == (int)DJBoss.AttackType.ColumnAttack)
            {
                EditorGUILayout.PropertyField(numberOfShots, new GUIContent("Number of Shots"));
            }

            // Delays
            EditorGUILayout.PropertyField(delay, new GUIContent("Initial Delay (seconds)"));
            EditorGUILayout.PropertyField(delayBetweenCommands, new GUIContent("Delay After Command"));

            // Target columns with visual toggles
            EditorGUILayout.LabelField("Target Columns:", EditorStyles.boldLabel);
            DrawColumnSelector(targetColumns);
        }
        catch
        {
            // If we get an exception, the property was probably deleted
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }

    private void DrawColumnSelector(SerializedProperty targetColumns)
    {
        // Get current selected columns
        HashSet<int> selectedColumns = new HashSet<int>();
        for (int i = 0; i < targetColumns.arraySize; i++)
        {
            selectedColumns.Add(targetColumns.GetArrayElementAtIndex(i).intValue);
        }

        EditorGUILayout.BeginHorizontal();

        // Create toggle buttons for each column (0-3)
        bool[] columnToggles = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            columnToggles[i] = selectedColumns.Contains(i);
        }

        EditorGUILayout.LabelField("Columns:", GUILayout.Width(60));

        for (int i = 0; i < 4; i++)
        {
            bool newValue = GUILayout.Toggle(columnToggles[i], $"{i}", "Button", GUILayout.Width(30));

            if (newValue != columnToggles[i])
            {
                if (newValue)
                {
                    // Add column
                    selectedColumns.Add(i);
                }
                else
                {
                    // Remove column
                    selectedColumns.Remove(i);
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        // Update the SerializedProperty
        targetColumns.ClearArray();
        var sortedColumns = selectedColumns.OrderBy(x => x).ToList();
        for (int i = 0; i < sortedColumns.Count; i++)
        {
            targetColumns.InsertArrayElementAtIndex(i);
            targetColumns.GetArrayElementAtIndex(i).intValue = sortedColumns[i];
        }

        // Show selected columns info
        if (selectedColumns.Count > 0)
        {
            string columnsText = string.Join(", ", sortedColumns);
            EditorGUILayout.LabelField($"Selected: {columnsText}", EditorStyles.helpBox);

            if (selectedColumns.Count > 1)
            {
                EditorGUILayout.LabelField("⚡ Multiple columns = Double Slam effect!", EditorStyles.centeredGreyMiniLabel);
            }
        }
        else
        {
            EditorGUILayout.LabelField("No columns selected", EditorStyles.helpBox);
        }
    }

    private void AddNewPattern()
    {
        int newIndex = attackPatternsProperty.arraySize;
        attackPatternsProperty.InsertArrayElementAtIndex(newIndex);

        SerializedProperty newPattern = attackPatternsProperty.GetArrayElementAtIndex(newIndex);
        SerializedProperty patternName = newPattern.FindPropertyRelative("patternName");
        SerializedProperty attacks = newPattern.FindPropertyRelative("attacks");
        SerializedProperty patternDelay = newPattern.FindPropertyRelative("patternDelay");

        patternName.stringValue = $"New Pattern {newIndex + 1}";
        patternDelay.floatValue = 0f;
        attacks.ClearArray();
    }

    private void AddNewAttackCommand(SerializedProperty attacks)
    {
        int newIndex = attacks.arraySize;
        attacks.InsertArrayElementAtIndex(newIndex);

        SerializedProperty newAttack = attacks.GetArrayElementAtIndex(newIndex);
        SerializedProperty attackType = newAttack.FindPropertyRelative("attackType");
        SerializedProperty targetColumns = newAttack.FindPropertyRelative("targetColumns");
        SerializedProperty numberOfShots = newAttack.FindPropertyRelative("numberOfShots");
        SerializedProperty delay = newAttack.FindPropertyRelative("delay");
        SerializedProperty delayBetweenCommands = newAttack.FindPropertyRelative("delayBetweenCommands"); // New

        attackType.enumValueIndex = 1; // Default to ColumnAttack
        targetColumns.ClearArray();
        numberOfShots.intValue = 3; // Default number of shots
        delay.floatValue = 0f;
        delayBetweenCommands.floatValue = 0f; // New: Initialize to 0
    }
}
#endif