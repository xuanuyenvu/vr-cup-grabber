// using UnityEngine;
// using UnityEditor;

// [CustomEditor(typeof(GameObject), true)] // Áp dụng cho mọi GameObject
// public class ObjectSizeInEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         DrawDefaultInspector(); // Hiển thị Inspector mặc định

//         GameObject targetObject = (GameObject)target; // Lấy GameObject đang được chọn

//         if (GUILayout.Button("Calculate Size"))
//         {
//             CalculateSize(targetObject);
//         }

//         // Hiển thị kích thước trong Inspector
//         Renderer renderer = targetObject.GetComponent<Renderer>();
//         if (renderer != null)
//         {
//             Vector3 size = renderer.bounds.size;
//             EditorGUILayout.LabelField("Size (X, Y, Z)", $"{size.x}, {size.y}, {size.z}");
//         }
//     }

//     void CalculateSize(GameObject obj)
//     {
//         Renderer renderer = obj.GetComponent<Renderer>();
//         if (renderer != null)
//         {
//             Vector3 size = renderer.bounds.size;
//             Debug.Log($"Kích thước của {obj.name}: Chiều dài (X): {size.x}, Chiều rộng (Z): {size.z}, Chiều cao (Y): {size.y}");
//         }
//         else
//         {
//             Debug.LogError($"{obj.name} không có Renderer!");
//         }
//     }
// }