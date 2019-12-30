using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RichTextSpriteAsset))]
[CanEditMultipleObjects]
public class RichTextSpriteAssetEditor : Editor
{
    private SerializedProperty m_SpriteAtlas;
    private SerializedProperty m_Material;

    private List<string> groupOption;
    private Dictionary<string, List<SerializedProperty>> fontSpriteListDic;
    int m_SelectGroup = 0;
    private string m_SearchText;
    private List<TexturePackSprite> Sprites = new List<TexturePackSprite>();
    private Vector2 m_Scroll;
    private int GridHeight = 130;
    public Dictionary<string, Texture2D> SpriteDic = new Dictionary<string, Texture2D>();
    private List<string> m_AnimateGroup = new List<string>();
    private RichTextSpriteAsset m_RichTextSpriteAsset;
    public Dictionary<string, List<Texture2D>> m_AnimateDic = new Dictionary<string, List<Texture2D>>();
    private const int Col = 10;

    private Vector2 m_ScrollAnimate;


    public void OnEnable()
    {
        this.m_SpriteAtlas = serializedObject.FindProperty("m_SpriteSheet");
        this.m_Material = serializedObject.FindProperty("m_Material");
        m_RichTextSpriteAsset = target as RichTextSpriteAsset;
        Texture2D tex2D = m_RichTextSpriteAsset.spriteSheet as Texture2D;

        if (m_RichTextSpriteAsset.animateList != null)
        {
            if (m_RichTextSpriteAsset.animateList.Count >= m_SelectGroup)
            {
                m_AnimateDic.Clear();
                m_AnimateGroup.Clear();
                for (int i = 0; i < m_RichTextSpriteAsset.animateList.Count; i++)
                {
                    RichTextAnimate textAnimate = m_RichTextSpriteAsset.animateList[i];
                    m_AnimateGroup.Add(textAnimate.animateName);
                    if (!m_AnimateDic.ContainsKey(textAnimate.animateName))
                    {
                        List<Texture2D> tempList = new List<Texture2D>(textAnimate.frameCount);
                        m_AnimateDic.Add(textAnimate.animateName, tempList);
                    }

                    for (int index = 0; index < textAnimate.spriteList.Count; index++)
                    {
                        TexturePackSprite texturePackSprite = textAnimate.spriteList[index];

                        Texture2D tex = new Texture2D((int)texturePackSprite.width, (int)texturePackSprite.height);
                        Color[] colors = tex2D.GetPixels((int)texturePackSprite.x, (int)texturePackSprite.y, (int)texturePackSprite.width, (int)texturePackSprite.height);
                        tex.SetPixels(0, 0, (int)texturePackSprite.width, (int)texturePackSprite.height, colors);
                        tex.Apply();
                        m_AnimateDic[texturePackSprite.animatGroup].Add(tex);
                    }
                }
            }
        }
    }


    public override void OnInspectorGUI()
    {
        base.serializedObject.Update();

        if (TexturePackTool.DrawHeader("基础信息", true))
        {
            EditorGUILayout.PropertyField(this.m_SpriteAtlas, new GUIContent("合图"));
            EditorGUILayout.PropertyField(this.m_Material, new GUIContent("材质球"));
        }

        GUILayout.BeginVertical();

        if (m_RichTextSpriteAsset == null)
            return;

        if (TexturePackTool.DrawHeader(string.Format("图集动画({0})", m_RichTextSpriteAsset.animateList.Count), true))
        {

            if (m_RichTextSpriteAsset.animateList != null)
            {
                EditorGUILayout.BeginVertical("box");

                int col = 0;
                for (int index = 0; index < m_RichTextSpriteAsset.animateList.Count; index++)
                {
                    RichTextAnimate textAnimate = m_RichTextSpriteAsset.animateList[index];
                    string animate = textAnimate.animateName;
                    if (col <= 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                    }

                    GUILayout.Label(m_AnimateDic[animate][0]);
                    col++;

                    if (col >= Col)
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                        col = 0;
                    }

                    if (index == m_RichTextSpriteAsset.animateList.Count - 1)
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical("box");

                m_SelectGroup = EditorGUILayout.Popup("动画组", m_SelectGroup, m_AnimateGroup.ToArray());
                string animateName = m_AnimateGroup[m_SelectGroup];
                RichTextAnimate richTextAnimate = m_RichTextSpriteAsset.GetAnimateListByName(animateName);

                GUILayout.BeginHorizontal();
                GUILayout.Label("帧数", GUILayout.Width(80));
                EditorGUILayout.IntField(richTextAnimate.frameCount);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);


                GUILayout.BeginHorizontal();
                GUILayout.Label("帧率", GUILayout.Width(80));
                EditorGUILayout.IntField(richTextAnimate.frameRate);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);

                m_ScrollAnimate = EditorGUILayout.BeginScrollView(m_ScrollAnimate, GUILayout.Height(80));
                EditorGUILayout.BeginHorizontal("box");
                if (m_AnimateDic.ContainsKey(animateName))
                {
                    List<Texture2D> tempList = m_AnimateDic[animateName];
                    int frameIndex = 0;
                    while (frameIndex < tempList.Count && tempList.Count > 0)
                    {
                        GUILayout.Label(tempList[frameIndex]);
                        frameIndex++;
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                if (GUILayout.Button("复制富文本"))
                {
                    GUIUtility.systemCopyBuffer = string.Format("<quad animate={0} rate={1} />", animateName, richTextAnimate.frameRate);
                }

                EditorGUILayout.EndVertical();

            }
        }


        Texture2D tex2D = m_RichTextSpriteAsset.spriteSheet as Texture2D;
        if (TexturePackTool.DrawHeader(string.Format("图集精灵({0})", m_RichTextSpriteAsset.spriteInfoList.Count), true))
        {

            GUILayout.BeginHorizontal("box");
            m_SearchText = EditorGUILayout.TextField("", m_SearchText, "SearchTextField");

            if (string.IsNullOrEmpty(m_SearchText))
            {
                GUILayout.Label("", "SearchCancelButtonEmpty");
            }
            else
            {
                if (GUILayout.Button("", "SearchCancelButton"))
                {
                    m_SearchText = string.Empty;
                }
            }


            Sprites.Clear();

            for (int i = 0; i < m_RichTextSpriteAsset.spriteInfoList.Count; i++)
            {
                TexturePackSprite uSprite = m_RichTextSpriteAsset.spriteInfoList[i];

                if (!string.IsNullOrEmpty(m_SearchText))
                {
                    if (!uSprite.name.ToString().Contains(m_SearchText) && !i.ToString().Contains(m_SearchText))
                        continue;
                }

                Sprites.Add(uSprite);
            }

            GUILayout.EndHorizontal();
            m_Scroll = TexturePackTool.BeginScrollViewEx(m_Scroll, Sprites.Count, GridHeight, 10, 2,
            delegate (int index)
            {
                GUILayout.BeginVertical("box", GUILayout.Height(GridHeight));
                TexturePackSprite packSprite = Sprites[index];
                Texture2D tex = null;

                if (SpriteDic.ContainsKey(packSprite.name))
                {
                    tex = SpriteDic[packSprite.name];
                }
                else
                {
                    tex = new Texture2D((int)packSprite.width, (int)packSprite.height);
                    Color[] colors = tex2D.GetPixels((int)packSprite.x, (int)packSprite.y, (int)packSprite.width, (int)packSprite.height);
                    tex.SetPixels(0, 0, (int)packSprite.width, (int)packSprite.height, colors);

                    tex = TexturePackTool.ScaleTextureBilinear(tex, 2);
                    tex.Apply();
                    SpriteDic.Add(packSprite.name, tex);
                }

                GUILayout.Label("序号：" + index.ToString(), new GUIStyle("AppToolbar"));
                GUILayout.Label("图片名称：" + packSprite.name, new GUIStyle("BoldLabel"));

                GUILayout.BeginHorizontal();
                GUILayout.Label(tex, GUILayout.Width(80), GUILayout.Height(80));
                EditorGUILayout.BeginVertical("box");

                GUILayout.BeginHorizontal();
                GUILayout.Label("纹理位置(uv)", GUILayout.Width(80));
                EditorGUILayout.FloatField(packSprite.x);
                EditorGUILayout.FloatField(packSprite.y);
                GUILayout.EndHorizontal();

                GUILayout.Space(2);
                GUILayout.BeginHorizontal();
                GUILayout.Label("纹理尺寸", GUILayout.Width(80));
                EditorGUILayout.FloatField(packSprite.width);
                EditorGUILayout.FloatField(packSprite.height);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);

                GUILayout.BeginHorizontal();
                GUILayout.Label("中心点", GUILayout.Width(80));
                EditorGUILayout.FloatField(packSprite.pivot.x);
                EditorGUILayout.FloatField(packSprite.pivot.y);
                GUILayout.EndHorizontal();
                GUILayout.Space(2);

                if (GUILayout.Button("复制富文本"))
                {
                    GUIUtility.systemCopyBuffer = string.Format("<quad sprite={0} />", index);
                }

                EditorGUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(2);


            }
            );
            EditorGUILayout.EndVertical();
            serializedObject.ApplyModifiedProperties();

        }
    }
}

