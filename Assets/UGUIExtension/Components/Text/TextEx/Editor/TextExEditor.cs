using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TextExtend;

[CustomEditor(typeof(TextEx), true)]
public class TextExEditor : UnityEditor.UI.TextEditor
{
    SerializedProperty m_SpriteAsset;
    protected override void OnEnable()
    {
        base.OnEnable();
        m_SpriteAsset = serializedObject.FindProperty("m_SpriteAsset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(m_SpriteAsset);
        serializedObject.ApplyModifiedProperties();
    }
}
