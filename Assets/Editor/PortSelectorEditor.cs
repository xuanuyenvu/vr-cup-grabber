using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PortSelector))]
public class PortSelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PortSelector portSelector = (PortSelector)target;

        DrawDefaultInspector(); // Vẫn hiển thị odorController nếu cần gán

        GUILayout.Space(10);
        GUILayout.Label("Serial Port Control", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Port List"))
        {
            portSelector.RefreshPortList();
        }

        if (portSelector.portNames.Count > 0)
        {
            portSelector.selectedPortIndex = EditorGUILayout.Popup("Select Port", portSelector.selectedPortIndex, portSelector.portNames.ToArray());

            if (GUILayout.Button("Connect"))
            {
                portSelector.Connect();
            }

            if (GUILayout.Button("Disconnect"))
            {
                portSelector.Disconnect();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No COM ports found. Click 'Refresh' to update.", MessageType.Info);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}