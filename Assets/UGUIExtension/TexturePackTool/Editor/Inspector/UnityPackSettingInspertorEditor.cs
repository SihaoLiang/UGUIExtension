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
    private string SearchText;
    private List<UnityPackSprite> Sprites = new List<UnityPackSprite>();
    private int TotalSpriteCount = 0;
    private int GridHeight = 130;
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

    public void OnDisable()
    {
        Sprites.Clear();
        SpriteDic.Clear();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(Name, NameContent, true);
        EditorGUILayout.PropertyField(Atlas, AtlasContent, true);

        UnityPackSetting setting = target as UnityPackSetting;
        if (GUILayout.Button("生成图集精灵"))
        {
            TexturePackTool.SetupSpriteMetaData(setting);
        }

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


        if (TexturePackTool.DrawHeader(string.Format("图集精灵({0})",TotalSpriteCount), true))
        {
            Texture2D tex2D = setting.Atlas as Texture2D;

            GUILayout.BeginHorizontal("box");
            SearchText = EditorGUILayout.TextField("", SearchText, "SearchTextField");
            TotalSpriteCount = setting.TexturePackSprite.Count;

            if (string.IsNullOrEmpty(SearchText))
            {
                GUILayout.Label("", "SearchCancelButtonEmpty");
            }
            else
            {
                if (GUILayout.Button("", "SearchCancelButton"))
                {
                    SearchText = string.Empty;
                }
            }


            Sprites.Clear();

            for (int i = 0; i < setting.TexturePackSprite.Count; i++)
            {
                UnityPackSprite uSprite = setting.TexturePackSprite[i];

                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (!uSprite.filename.ToString().Contains(SearchText) && !i.ToString().Contains(SearchText))
                        continue;
                }

                Sprites.Add(uSprite);
            }

            GUILayout.EndHorizontal();
            Scroll = TexturePackTool.BeginScrollViewEx(Scroll, Sprites.Count, GridHeight, 10, 2,
            delegate (int index)
            {
                GUILayout.BeginVertical("box", GUILayout.Height(GridHeight));
                UnityPackSprite uSprite = Sprites[index];

                Texture2D tex = null;

                if (SpriteDic.ContainsKey(uSprite.filename))
                {
                    tex = SpriteDic[uSprite.filename];
                }
                else
                {
                    tex = new Texture2D((int)uSprite.width, (int)uSprite.height);
                    Color[] colors = tex2D.GetPixels((int)uSprite.x, (int)uSprite.y, (int)uSprite.width, (int)uSprite.height);
                    tex.SetPixels(0, 0, (int)uSprite.width, (int)uSprite.height, colors);

                    tex = TexturePackTool.ScaleTextureBilinear(tex, 2);
                    tex.Apply();
                    SpriteDic.Add(uSprite.filename, tex);
                }

                GUILayout.Label("序号：" + index.ToString(), new GUIStyle("AppToolbar"));
                GUILayout.Label("图片名称：" + uSprite.filename, new GUIStyle("BoldLabel"));

                GUILayout.BeginHorizontal();
                GUILayout.Label(tex, GUILayout.Width(80), GUILayout.Height(80));
                EditorGUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                GUILayout.Label("纹理位置(uv)", GUILayout.Width(80));
                EditorGUILayout.FloatField(uSprite.x);
                EditorGUILayout.FloatField(uSprite.y);
                GUILayout.EndHorizontal();

                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                GUILayout.Label("纹理尺寸", GUILayout.Width(80));
                EditorGUILayout.FloatField(uSprite.width);
                EditorGUILayout.FloatField(uSprite.height);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);

                GUILayout.BeginHorizontal();
                GUILayout.Label("中心点", GUILayout.Width(80));
                EditorGUILayout.FloatField(uSprite.pivot.x);
                EditorGUILayout.FloatField(uSprite.pivot.y);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);

                GUILayout.BeginHorizontal();
                GUILayout.Label("九宫格(LBRT)", GUILayout.Width(80));
                EditorGUILayout.Vector4Field("",uSprite.border);
                GUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(2);

            }
            );

         
        }



        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }
}
