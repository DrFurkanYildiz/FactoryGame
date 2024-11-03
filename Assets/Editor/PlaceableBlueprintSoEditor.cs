using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PlaceableBlueprintSo))]
    public class PlaceableBlueprintSoEditor : UnityEditor.Editor
    {
        private SerializedProperty grid;
        private SerializedProperty type;
        private SerializedProperty height;
        private SerializedProperty width;
        private SerializedProperty prefab;
        private SerializedProperty visual;
        int elementLength;

        private SerializedProperty isMachine;
        private SerializedProperty itemRecipeSo;

        private void OnEnable()
        {
            grid = serializedObject.FindProperty("grid");
            type = serializedObject.FindProperty("type");
            height = serializedObject.FindProperty("height");
            width = serializedObject.FindProperty("width");
            prefab = serializedObject.FindProperty("prefab");
            visual = serializedObject.FindProperty("visual");

            isMachine = serializedObject.FindProperty("isMachine");
            itemRecipeSo = serializedObject.FindProperty("itemRecipeSo");

            elementLength = Enum.GetValues(typeof(Elements)).Length;

            var script = (PlaceableBlueprintSo)target;
            if (script.grid == null || script.grid.Length != script.height ||
                script.grid[0]?.values.Length != script.width)
                script.ResetGrid();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var script = (PlaceableBlueprintSo)target;

            // Grid size inputs
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(height);
            EditorGUILayout.PropertyField(width);
            EditorGUILayout.PropertyField(prefab);
            EditorGUILayout.PropertyField(visual);


            EditorGUILayout.PropertyField(isMachine);
            if (isMachine.boolValue) EditorGUILayout.PropertyField(itemRecipeSo);
            else script.itemRecipeSo = null;

            GUILayout.Space(10);

            if (GUILayout.Button("Reset Grid", GUILayout.Height(30), GUILayout.Width(200)))
            {
                script.ResetGrid();
            }

            GUILayout.Space(20);
            DrawGrid();


            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid()
        {
            GUILayout.BeginVertical();
            var script = (PlaceableBlueprintSo)target;

            if (script.grid == null || script.grid.Length != script.height ||
                script.grid[0]?.values.Length != script.width)
            {
                script.ResetGrid();
                return;
            }

            for (int i = 0; i < script.height; i++)
            {
                GUILayout.BeginHorizontal();
                SerializedProperty row = grid.GetArrayElementAtIndex(i).FindPropertyRelative("values");

                for (int j = 0; j < script.width; j++)
                {
                    var cell = row.GetArrayElementAtIndex(j);
                    Elements element = (Elements)cell.intValue;
                    Texture2D icon = GetIcon(element);
                    string cord = j + "," + (script.height - 1 - i);

                    // Draw button
                    GUILayout.BeginVertical(GUILayout.Width(50));
                    if (GUILayout.Button(icon, GUILayout.Width(50), GUILayout.Height(50)))
                    {
                        cell.intValue = NextIndex(cell.intValue);
                    }

                    // Draw coordinates label below the button
                    GUILayout.TextArea(cord, GUILayout.Width(25), GUILayout.Height(15));
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private int NextIndex(int index)
        {
            return (++index) % elementLength;
        }

        private Texture2D GetIcon(Elements element)
        {
            switch (element)
            {
                case Elements.Cross:
                    return Resources.Load<Texture2D>("CrossIcon");
                case Elements.Input:
                    return Resources.Load<Texture2D>("InputIcon");
                case Elements.Output:
                    return Resources.Load<Texture2D>("OutputIcon");
                default:
                    return Texture2D.grayTexture; // Default or empty
            }
        }
    }
}