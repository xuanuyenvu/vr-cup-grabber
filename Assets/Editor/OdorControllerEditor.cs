using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(OdorController))]
public class OdorControllerEditor : Editor
{
    private int smellDuration = 1000;
    private int tasteDuration = 1000;
    private int tasteIntensity = 100;

    private Dictionary<OdorType, bool> smellSelection = new();
    private Dictionary<OdorType, bool> tasteSelection = new();

    private void OnEnable()
    {
        foreach (OdorType type in System.Enum.GetValues(typeof(OdorType)))
        {
            if (!smellSelection.ContainsKey(type))
                smellSelection[type] = false;
            if (!tasteSelection.ContainsKey(type))
                tasteSelection[type] = false;
        }
    }

    public override void OnInspectorGUI()
    {
        OdorController controller = (OdorController)target;

        DrawDefaultInspector(); // hiện các public field gán từ ngoài (nếu cần)

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connection Status:", controller.IsConnected ? "Connected" : "Not Connected");

        EditorGUILayout.Space(10);
        DrawSmellSection(controller);
        EditorGUILayout.Space(15);
        DrawTasteSection(controller);
        EditorGUILayout.Space();
    }

    private void DrawSmellSection(OdorController controller)
    {
        EditorGUILayout.LabelField("Smell Layer", EditorStyles.boldLabel);

        smellDuration = EditorGUILayout.IntField("Duration (ms)", smellDuration);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            foreach (var key in new List<OdorType>(smellSelection.Keys))
                smellSelection[key] = true;
        }
        if (GUILayout.Button("Deselect All"))
        {
            foreach (var key in new List<OdorType>(smellSelection.Keys))
                smellSelection[key] = false;
        }
        EditorGUILayout.EndHorizontal();

        foreach (OdorType type in System.Enum.GetValues(typeof(OdorType)))
        {
            smellSelection[type] = EditorGUILayout.ToggleLeft(type.ToString(), smellSelection[type]);
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Activate Selected Smells"))
        {
            var selected = GetSelectedOdorTypes(smellSelection);
            controller.smellAPI?.ActivateSelectedSmells(selected, smellDuration);
        }

        if (GUILayout.Button("Deactivate Selected Smells"))
        {
            var selected = GetSelectedOdorTypes(smellSelection);
            controller.smellAPI?.DeactivateSelectedSmells(selected);
        }
    }

    private void DrawTasteSection(OdorController controller)
    {
        EditorGUILayout.LabelField("Taste Layer", EditorStyles.boldLabel);

        tasteDuration = EditorGUILayout.IntField("Duration (ms)", tasteDuration);
        tasteIntensity = EditorGUILayout.IntSlider("Intensity", tasteIntensity, 0, 100);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            foreach (var key in new List<OdorType>(tasteSelection.Keys))
                tasteSelection[key] = true;
        }
        if (GUILayout.Button("Deselect All"))
        {
            foreach (var key in new List<OdorType>(tasteSelection.Keys))
                tasteSelection[key] = false;
        }
        EditorGUILayout.EndHorizontal();

        foreach (OdorType type in System.Enum.GetValues(typeof(OdorType)))
        {
            tasteSelection[type] = EditorGUILayout.ToggleLeft(type.ToString(), tasteSelection[type]);
        }

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Activate Selected Tastes"))
        {
            var selected = GetSelectedOdorTypes(tasteSelection);
            controller.tasteAPI?.ActivateSelectedTastes(selected, tasteDuration, tasteIntensity);
        }

        if (GUILayout.Button("Deactivate Selected Tastes"))
        {
            var selected = GetSelectedOdorTypes(tasteSelection);
            controller.tasteAPI?.DeactivateSelectedTastes(selected);
        }
    }

    private List<OdorType> GetSelectedOdorTypes(Dictionary<OdorType, bool> dict)
    {
        var result = new List<OdorType>();
        foreach (var pair in dict)
        {
            if (pair.Value)
                result.Add(pair.Key);
        }
        return result;
    }
}
