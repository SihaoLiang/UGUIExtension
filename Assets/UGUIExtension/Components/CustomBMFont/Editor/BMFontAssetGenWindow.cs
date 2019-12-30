using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;

public class BMFontAssetGenWindow : EditorWindow
{
    public enum PackingType
    {
        TexturePack = 0,
        UnityPack,
        Custom
    }

    private string m_ConsoleContent;

    private Texture2D m_TextureSource;
    private TextAsset m_TPTextSheet;
    GUIStyle m_LabelStyle;

    private PackingType AtlasPacking;

    List<BMFontSprite> m_FontSpriteInfoList = new List<BMFontSprite>();
    BMFontAsset m_FontAsset;

    [MenuItem("图集工具/生成艺术字图集")]
    public static void ShowArtFontAssetGenWindow()
    {
        BMFontAssetGenWindow window = EditorWindow.GetWindow<BMFontAssetGenWindow>();
        window.titleContent = new GUIContent("生成艺术字图集");
        window.Focus();
    }

    private void OnEnable()
    {
        m_ConsoleContent = string.Empty;
        m_TextureSource = null;
        m_TPTextSheet = null;
        m_FontAsset = null;
        InitGUIStyle();
    }


    void InitGUIStyle()
    {
        m_LabelStyle = new GUIStyle();
        m_LabelStyle.fontStyle = FontStyle.Bold;
        m_LabelStyle.fontSize = 16;
        m_LabelStyle.richText = true;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Space(5);

        if (AtlasPacking == PackingType.TexturePack)
        {
            EditorGUILayout.HelpBox("使用外部插件Texture Packer 打包的图集(Json)生成可用于富文本的图集数据", MessageType.Info);
        }
        else if (AtlasPacking == PackingType.Custom)
        {
            EditorGUILayout.HelpBox("使用自定义打包的图集生成可用于富文本的图集数据", MessageType.Info);
        }
        else if (AtlasPacking == PackingType.UnityPack)
        {
            EditorGUILayout.HelpBox("使用Unity Sprite Edtor打包的图集生成可用于富文本的图集数据", MessageType.Info);
        }


        AtlasPacking = (PackingType)EditorGUILayout.EnumPopup("生成方式", AtlasPacking);
        if (AtlasPacking == PackingType.TexturePack)
        {
            TexturePackGUI();
        }
        else if (AtlasPacking == PackingType.Custom)
        {
            //CustomPackGUI();
        }
        else if (AtlasPacking == PackingType.UnityPack)
        {
            //UnityPackGUI();
        }

        if (m_FontAsset != null)
        {
            this.m_ConsoleContent = string.Concat(new object[]
            {
                "生成富文本图集成功\n",
                string.Format("本次生成精灵数量：{0}\n", m_FontAsset.spriteInfoList.Count),
                //string.Format("本次生成动画数量：{0}\n", m_FontAsset.animateList.Count),
            });

        }
        else
        {
            this.m_ConsoleContent = string.Concat(new object[]
            {
                "未生成图集",
            });

        }


        GUILayout.Space(5f);
        GUILayout.BeginVertical("box", new GUILayoutOption[]
        {
            GUILayout.Height(80)
        });



        EditorGUILayout.LabelField(this.m_ConsoleContent, m_LabelStyle, new GUILayoutOption[0]);
        GUILayout.EndVertical();
        GUILayout.Space(5f);
        GUILayout.EndVertical();


        //EditorGUI.BeginChangeCheck();
        //this.m_TPTextSheet = (EditorGUILayout.ObjectField("Texture Data(Json)", this.m_TPTextSheet, typeof(TextAsset), false, new GUILayoutOption[0]) as TextAsset);
        //this.m_TextureSource = (EditorGUILayout.ObjectField("Texture Atlas", this.m_TextureSource, typeof(Texture2D), false, new GUILayoutOption[0]) as Texture2D);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    this.m_ConsoleContent = string.Empty;
        //}

        //if (m_TPTextSheet == null || m_TextureSource == null)
        //{
        //    GUILayout.EndVertical();
        //    return;
        //}

        //GUILayout.Space(10f);
        //if (GUILayout.Button("Create Sprite Asset", new GUILayoutOption[0]))
        //{
        //    this.m_ConsoleContent = string.Empty;
        //    TP.TexturePackJsonData spriteDataObject = JsonUtility.FromJson<TP.TexturePackJsonData>(this.m_TPTextSheet.text);
        //    if (spriteDataObject != null && spriteDataObject.frames != null && spriteDataObject.frames.Count > 0)
        //    {
        //        int count = spriteDataObject.frames.Count;
        //        this.m_ConsoleContent = "<b>Import Results</b>\n-----------------------------\n";
        //        string str = this.m_ConsoleContent;
        //        this.m_ConsoleContent = string.Concat(new object[]
        //        {
        //                    str,
        //                    "<color=#C0ffff><b>",
        //                    count,
        //                    "</b></color> Sprites were imported from file."
        //        });
        //    }
        //    m_FontSpriteInfoList = CreateSpriteInfoList(spriteDataObject);
        //}

        //GUILayout.Space(5f);
        //GUI.enabled = (this.m_FontSpriteInfoList != null);
        //if (GUILayout.Button("Save Sprite Asset", new GUILayoutOption[0]))
        //{
        //    string text = string.Empty;
        //    text = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(this.m_TPTextSheet)).DirectoryName, this.m_TPTextSheet.name, "asset");
        //    if (text.Length == 0)
        //    {
        //        return;
        //    }
        //    this.SaveSpriteAsset(text);
        //}

        //GUILayout.Space(5f);
        //GUILayout.BeginVertical("box", new GUILayoutOption[]
        //{
        //    GUILayout.Height(60f)
        //});

        //EditorGUILayout.LabelField(this.m_ConsoleContent, m_LabelStyle, new GUILayoutOption[0]);
        //GUILayout.EndVertical();

        //GUILayout.Space(5f);
        //GUILayout.EndVertical();
    }


    /// <summary>
    /// 利用TexturePack打包图集信息
    /// </summary>
    void TexturePackGUI()
    {
        EditorGUI.BeginChangeCheck();
        this.m_TPTextSheet = (EditorGUILayout.ObjectField("TP数据(Json)", this.m_TPTextSheet, typeof(TextAsset), false, new GUILayoutOption[0]) as TextAsset);
        GUILayout.Space(5);

        this.m_TextureSource = (EditorGUILayout.ObjectField("TP合图", this.m_TextureSource, typeof(Texture2D), false, new GUILayoutOption[0]) as Texture2D);
        if (EditorGUI.EndChangeCheck())
        {
            this.m_ConsoleContent = string.Empty;
        }

        if (m_TPTextSheet == null || m_TextureSource == null)
        {
            return;
        }

        GUILayout.Space(10f);

        if (GUILayout.Button("生成", new GUILayoutOption[0]))
        {

            this.m_ConsoleContent = string.Empty;
            TP.TexturePackJsonData spriteDataObject = JsonUtility.FromJson<TP.TexturePackJsonData>(this.m_TPTextSheet.text);

            m_FontSpriteInfoList = CreateSpriteInfoList(spriteDataObject);

            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("保存富文本数据", new FileInfo(AssetDatabase.GetAssetPath(this.m_TPTextSheet)).DirectoryName, this.m_TPTextSheet.name, "asset");
            if (text.Length == 0)
            {
                return;
            }
            this.SaveSpriteAsset(text);
        }
    }

    private List<BMFontSprite> CreateSpriteInfoList(TP.TexturePackJsonData spriteDataObject)
    {
        List<TP.SpriteData> frames = spriteDataObject.frames;
        List<BMFontSprite> list = new List<BMFontSprite>();
        for (int i = 0; i < frames.Count; i++)
        {
            BMFontSprite fontData = new BMFontSprite();
            fontData.id = i;
            fontData.name = Path.GetFileNameWithoutExtension(frames[i].filename);
            fontData.hashCode = fontData.name.GetHashCode();
            int num = fontData.name.IndexOf('-');

            // int ascii;
            string key = string.Empty;
            int pos = fontData.name.LastIndexOf("_");

            if (pos == -1)
            {
                key = fontData.name;
                pos = fontData.name.Length;
                //Debug.LogError("图集命名不合法，不存在 '_'");
            }
            else
            {
                key = fontData.name.Substring(pos + 1, fontData.name.Length - pos - 1);
            }

            fontData.key = key;
            fontData.group = fontData.name.Substring(0, pos);
            fontData.x = frames[i].frame.x;
            fontData.y = (float)this.m_TextureSource.height - (frames[i].frame.y + frames[i].frame.h);
            fontData.width = frames[i].frame.w;
            fontData.height = frames[i].frame.h;
            fontData.pivot = frames[i].pivot;
            fontData.xAdvance = fontData.width;
            fontData.scale = 1f;
            fontData.xOffset = 0f - fontData.width * fontData.pivot.x;
            fontData.yOffset = fontData.height - fontData.height * fontData.pivot.y;
            list.Add(fontData);
        }
        return list;
    }

    private void SaveSpriteAsset(string filePath)
    {
        filePath = filePath.Substring(0, filePath.Length - 6);
        string dataPath = Application.dataPath;
        if (filePath.IndexOf(dataPath, StringComparison.InvariantCultureIgnoreCase) == -1)
        {
            Debug.LogError("You're saving the font asset in a directory outside of this project folder. This is not supported. Please select a directory under \"" + dataPath + "\"");
            return;
        }
        string path = filePath.Substring(dataPath.Length - 6);
        string directoryName = Path.GetDirectoryName(path);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string str = directoryName + "/" + fileNameWithoutExtension;

        if (!File.Exists(filePath + ".asset"))
        {
            this.m_FontAsset = ScriptableObject.CreateInstance<BMFontAsset>();
            AssetDatabase.CreateAsset(this.m_FontAsset, str + ".asset");
        }
        else
        {
            this.m_FontAsset = AssetDatabase.LoadAssetAtPath<BMFontAsset>(str + ".asset");
            if (this.m_FontAsset == null)
            {
                this.m_FontAsset = ScriptableObject.CreateInstance<BMFontAsset>();
                AssetDatabase.CreateAsset(this.m_FontAsset, str + ".asset");
            }
        }

        this.m_FontAsset.hashCode = this.m_FontAsset.name.GetHashCode();
        this.m_FontAsset.spriteSheet = this.m_TextureSource;
        this.m_FontAsset.spriteInfoList = this.m_FontSpriteInfoList;
       // AssetDatabase.CreateAsset(this.m_FontAsset, str + ".asset");


        AddDefaultMaterial(this.m_FontAsset);

        EditorUtility.SetDirty(this.m_FontAsset);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        if (Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

    }

    private static void AddDefaultMaterial(BMFontAsset spriteAsset)
    {
        Shader shader = Shader.Find("UI/Default");
        Material material = new Material(shader);

        material.SetTexture("_MainTex", spriteAsset.spriteSheet);
        spriteAsset.material = material;
        material.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(material, spriteAsset);
    }

    private void SetEditorWindowSize()
    {
        Vector2 minSize = this.minSize;
        this.minSize = new Vector2(Mathf.Max(230f, minSize.x), Mathf.Max(300f, minSize.y));
    }
}
