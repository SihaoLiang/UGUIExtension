using UnityEditor;

[CustomEditor(typeof(RichText), true)]
public class RichTextEditor : UnityEditor.UI.TextEditor
{
    SerializedProperty mRichTextSpriteRender;


    protected override void OnEnable()
    {
        base.OnEnable();
        mRichTextSpriteRender = serializedObject.FindProperty("m_RichTextSpriteRender");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();

        EditorGUILayout.PropertyField(mRichTextSpriteRender);

        serializedObject.ApplyModifiedProperties();
    }
}
