using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(BMFontText), true)]
[CanEditMultipleObjects]
public class BMFontTextEditor : Editor {

    SerializedProperty m_Text;
    SerializedProperty m_Color;
    SerializedProperty m_Material;
    SerializedProperty m_RaycastTarget;
    SerializedProperty m_FontAsset;
    SerializedProperty m_Gap;
    int selectGroup = 0;
    bool markAsChange = false;
    private void OnEnable()
    {
        m_Text = serializedObject.FindProperty("m_Text");
        m_FontAsset = serializedObject.FindProperty("m_FontAsset");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        m_Gap = serializedObject.FindProperty("m_Gap");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Text);
        EditorGUILayout.PropertyField(m_Color);
        EditorGUILayout.PropertyField(m_Material);
        EditorGUILayout.PropertyField(m_RaycastTarget);


        BMFontText bmFont = target as BMFontText;
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_FontAsset);


        GUI.changed = false;
        if (m_FontAsset.objectReferenceValue != null)
        {
            if (bmFont.m_FontGroups.Contains(bmFont.m_CurrentGroup))
                selectGroup = bmFont.m_FontGroups.IndexOf(bmFont.m_CurrentGroup);

            selectGroup = EditorGUILayout.Popup("Font Group", selectGroup, bmFont.m_FontGroups.ToArray());
            if (GUI.changed && bmFont.m_FontGroups.Count > selectGroup)
            {
                bmFont.m_CurrentGroup = bmFont.m_FontGroups[selectGroup];
                bmFont.SetAllDirty();
            }
        }

        EditorGUILayout.PropertyField(m_Gap);

        serializedObject.ApplyModifiedProperties();

        
    }

}
