using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(BMFontMultText), true)]
[CanEditMultipleObjects]
public class BMFontMultTextEditor : Editor
{

    SerializedProperty m_TextGroupList;
    SerializedProperty m_Color;
    SerializedProperty m_Material;
    SerializedProperty m_RaycastTarget;
    SerializedProperty m_FontAsset;
    SerializedProperty m_Gap;
    int selectGroup = 0;
    bool markAsChange = false;

    private void OnEnable()
    {
        m_TextGroupList = serializedObject.FindProperty("m_FontDataList");
        m_FontAsset = serializedObject.FindProperty("m_FontAsset");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        m_Gap = serializedObject.FindProperty("m_Gap");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        BMFontMultText fontText = target as BMFontMultText;
        EditorGUILayout.Space();

        int removeKey = -1;
        List<FontData> fontDataList = fontText.fontDataList;
        for (int i = 0; i < fontDataList.Count; i++)
        {
            EditorGUILayout.BeginVertical((GUIStyle)"SelectionRect");

            EditorGUILayout.BeginHorizontal();
            bool status = FontAssetEditor.DrawTextToggle(string.Format("序列：{0}", i + 1));
            if (GUILayout.Button("", (GUIStyle)"OL Minus", GUILayout.Width(20)))
                removeKey = i;
            EditorGUILayout.EndHorizontal();


            if (status)
            {
                fontDataList[i].text = EditorGUILayout.TextArea(fontDataList[i].m_Text, GUILayout.Height(50));

                int index = 0;
                if (fontText.m_FontGroups.Contains(fontDataList[i].group))
                    index = fontText.m_FontGroups.IndexOf(fontDataList[i].group);

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 50;
                index = EditorGUILayout.Popup("Group", index, fontText.m_FontGroups.ToArray(), GUILayout.Width(200));
                EditorGUILayout.Space();
                EditorGUIUtility.labelWidth = 50;
                fontDataList[i].offest = EditorGUILayout.Vector2Field("Offest", fontDataList[i].offest, GUILayout.Width(200));

                EditorGUILayout.EndHorizontal();

                if (fontText.m_FontGroups.Count > selectGroup)
                {
                    fontDataList[i].group = fontText.m_FontGroups[index];
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (removeKey != -1 && fontDataList.Count > removeKey)
        {
            fontDataList.RemoveAt(removeKey);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("添 加"))
        {
            fontText.AddFontGroup();
        }

        if (GUILayout.Button("删 除"))
        {
            if (fontDataList.Count > 0)
                fontDataList.RemoveAt(fontDataList.Count - 1);
        }

        EditorGUILayout.EndHorizontal();


        if (GUI.changed)
        {
            fontText.SetAllDirty();
            EditorUtility.SetDirty(target);
            GUI.changed = false;
        }

      

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_Material);
        EditorGUILayout.PropertyField(m_RaycastTarget);
        EditorGUILayout.PropertyField(m_FontAsset);
        EditorGUILayout.PropertyField(m_Gap);

        serializedObject.ApplyModifiedProperties();
    }
}
