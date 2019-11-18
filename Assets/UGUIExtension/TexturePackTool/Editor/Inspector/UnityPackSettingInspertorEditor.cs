using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UnityPackSetting), true)]
[CanEditMultipleObjects]
public class UnityPackSettingInspertorEditor : Editor
{
    private SerializedProperty Name;
    private SerializedProperty Atlas;

    private SerializedProperty OutputAbsolutelyPath;
    private SerializedProperty OutputPath;
    private SerializedProperty MaxSize;

    private SerializedProperty Width;
    private SerializedProperty Height;

    private SerializedProperty Padding;
    private SerializedProperty TrimAlpha;
    private SerializedProperty TrimSimilar;
    private SerializedProperty ForeSquare;


    private GUIContent NameContent;
    private GUIContent AtlasContent;
    private GUIContent OutputAbsolutelyPathContent;
    private GUIContent OutputPathContent;

    private GUIContent WidthContent;
    private GUIContent HeightContent;
    private GUIContent PaddingContent;
    private GUIContent TrimAlphaContent;
    private GUIContent TrimSimilarContent;
    private GUIContent ForeSquareContent;

    private Vector2 Scroll;

    public Dictionary<string, Texture2D> SpriteDic = new Dictionary<string, Texture2D>();
    public void OnEnable()
    {
        Name = serializedObject.FindProperty("Name");
        Atlas = serializedObject.FindProperty("Atlas");
        OutputAbsolutelyPath = serializedObject.FindProperty("OutputAbsolutelyPath");
        OutputPath = serializedObject.FindProperty("OutputPath");
        MaxSize = serializedObject.FindProperty("MaxSize");

        Width = serializedObject.FindProperty("Width");
        Height = serializedObject.FindProperty("Height");


        Padding = serializedObject.FindProperty("Padding");
        TrimAlpha = serializedObject.FindProperty("TrimAlpha");

        TrimSimilar = serializedObject.FindProperty("TrimSimilar");
        TrimAlpha = serializedObject.FindProperty("TrimAlpha");
        ForeSquare = serializedObject.FindProperty("ForeSquare");

        NameContent = new GUIContent("图集名称");
        AtlasContent = new GUIContent("图集");
        OutputAbsolutelyPathContent = new GUIContent("全路径");
        OutputPathContent = new GUIContent("导出路径");

        WidthContent = new GUIContent("宽");
        HeightContent = new GUIContent("高");

        PaddingContent = new GUIContent("间隙");
        TrimAlphaContent = new GUIContent("去除透明");
        TrimSimilarContent = new GUIContent("去除重复");
        ForeSquareContent = new GUIContent("强制正方形");
        SpriteDic.Clear();
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(Name, NameContent, true);
        EditorGUILayout.PropertyField(Atlas, AtlasContent, true);
        GUI.enabled = false;


        if (TexturePackTool.DrawHeader("图集设置（请勿更改）", true))
        {

            EditorGUILayout.PropertyField(Width, WidthContent, true);
            EditorGUILayout.PropertyField(Height, HeightContent, true);
            EditorGUILayout.PropertyField(Padding, PaddingContent, true);
            EditorGUILayout.PropertyField(TrimAlpha, TrimAlphaContent, true);
            EditorGUILayout.PropertyField(TrimSimilar, TrimSimilarContent, true);
            EditorGUILayout.PropertyField(ForeSquare, ForeSquareContent, true);
        }

        GUI.enabled = true;


        if (TexturePackTool.DrawHeader("图集精灵", true))
        {
            UnityPackSetting setting = target as UnityPackSetting;
            Texture2D tex2D = setting.Atlas as Texture2D;

            Scroll = TexturePackTool.BeginScrollViewEx(Scroll, setting.TexturePackSprite.Count, 150, 10, 2,
            delegate (int index)
            {
                GUILayout.BeginVertical("box", GUILayout.Height(150));
                UnityPackSprite uSprite = setting.TexturePackSprite[index];
                GUILayout.TextField(uSprite.filename);


                Texture2D tex = null;
        
                tex = new Texture2D((int)uSprite.width,(int)uSprite.height);
                Color[] colors = tex2D.GetPixels((int)uSprite.x, (int)uSprite.y, (int)uSprite.width, (int)uSprite.height);
                tex.SetPixels(0,0, (int)uSprite.width, (int)uSprite.height, colors);
            
                GUILayout.TextField(index.ToString());
                EditorGUI.DrawPreviewTexture(new Rect(350, 50 + index * (150 + 4), 80, 80), tex);


                GUILayout.EndVertical();
                GUILayout.Space(2);

            }
            );
        }



        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
}
