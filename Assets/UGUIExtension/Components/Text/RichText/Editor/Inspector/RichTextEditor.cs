using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(RichText), true)]
public class RichTextEditor : UnityEditor.UI.TextEditor
{
    SerializedProperty m_SpriteAsset;
    SerializedProperty mRichTextSpriteRender;


    protected override void OnEnable()
    {
        base.OnEnable();
        m_SpriteAsset = serializedObject.FindProperty("m_SpriteAsset");
        mRichTextSpriteRender = serializedObject.FindProperty("mRichTextSpriteRender");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(m_SpriteAsset);
        EditorGUILayout.PropertyField(mRichTextSpriteRender);

        serializedObject.ApplyModifiedProperties();
    }
}
