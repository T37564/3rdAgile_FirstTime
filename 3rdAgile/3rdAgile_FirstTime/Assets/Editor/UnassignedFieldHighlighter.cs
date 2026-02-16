#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Reflection;

[InitializeOnLoad]
public static class UnassignedFieldHighlighter
{
    static UnassignedFieldHighlighter()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
    }

    private static void OnGUI(int instanceID, Rect selectionRect)
    {
        var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        if (HasUnassignedSerializedField(go))
        {
            var color = new Color(0.85f, 0.3f, 0.3f, 0.25f);

            // 選択中は少し弱める
            if (Selection.activeInstanceID == instanceID)
                color.a = 0.15f;

            EditorGUI.DrawRect(selectionRect, color);
        }
    }

    private static bool HasUnassignedSerializedField(GameObject go)
    {
        var components = go.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            if (comp == null) continue; // Missing Script 対策

            var fields = comp.GetType().GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

            foreach (var field in fields)
            {
                // public か [SerializeField]
                bool isSerialized =
                    field.IsPublic ||
                    field.GetCustomAttribute<SerializeField>() != null;

                if (!isSerialized) continue;

                // UnityEngine.Object 派生のみ
                if (!typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    continue;

                var value = field.GetValue(comp) as UnityEngine.Object;

                if (value == null)
                {
                    return true; // 未アタッチ発見
                }
            }
        }

        return false;
    }
}
#endif