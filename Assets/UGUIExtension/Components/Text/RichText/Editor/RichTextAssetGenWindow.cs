using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;
using UnityEngine.U2D;

public class RichTextAssetGenWindow : EditorWindow
{
    public enum PackingType
    {
        TexturePack = 0,
        UnityPack,
        Custom
    }

    //原图
    Texture2D TextureSource;
    //TP描述文件
    TextAsset TPTextSheet;

    //所有的纹理数据
    List<TexturePackSprite> RichTextSpriteInfoList = new List<TexturePackSprite>();

    //动态表情
    Dictionary<string, RichTextAnimate> RichTextAnimateDic = new Dictionary<string, RichTextAnimate>();

    //导出的资源
    RichTextSpriteAsset RichTextAsset;

    //打包类型
    private PackingType AtlasPacking = PackingType.TexturePack;


    private UnityPackSetting SettingAsset;

    //输出信息
    string ConsoleContent;
    //字体样式
    GUIStyle LabelStyle;
    GUIStyle TitleLabelStyle;

    [MenuItem("图集工具/生成富文本图集")]
    public static void ShowArtFontAssetGenWindow()
    {
        RichTextAssetGenWindow window = EditorWindow.GetWindow<RichTextAssetGenWindow>();
        window.titleContent = new GUIContent("富文本图集");
        window.Focus();
    }

    private void OnEnable()
    {
        ConsoleContent = string.Empty;
        TextureSource = null;
        TPTextSheet = null;
        RichTextAsset = null;
        InitGUIStyle();
    }

    void InitGUIStyle()
    {
        LabelStyle = new GUIStyle();
        LabelStyle.fontSize = 16;
        LabelStyle.richText = true;


        TitleLabelStyle = new GUIStyle("IN TitleText");
        TitleLabelStyle.fontSize = 30;
        TitleLabelStyle.richText = true;

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
            CustomPackGUI();
        }
        else if (AtlasPacking == PackingType.UnityPack)
        {
            UnityPackGUI();
        }

        if (RichTextAsset != null)
        {
            this.ConsoleContent = string.Concat(new object[]
            {
                "生成富文本图集成功\n",
                string.Format("本次生成精灵数量：{0}\n", RichTextAsset.spriteInfoList.Count),
                string.Format("本次生成动画数量：{0}\n", RichTextAsset.animateList.Count),
            });

        }
        else
        {
            this.ConsoleContent = string.Concat(new object[]
            {
                "未生成富文本图集",
            });

        }


        GUILayout.Space(5f);
        GUILayout.BeginVertical("box", new GUILayoutOption[]
        {
            GUILayout.Height(80)
        });



        EditorGUILayout.LabelField(this.ConsoleContent, LabelStyle, new GUILayoutOption[0]);
        GUILayout.EndVertical();
        GUILayout.Space(5f);
        GUILayout.EndVertical();
    }


    /// <summary>
    /// 利用Unity打包的图集
    /// </summary>
    void UnityPackGUI()
    {
        EditorGUI.BeginChangeCheck();
        TextureSource = (EditorGUILayout.ObjectField("图集", this.TextureSource, typeof(Texture2D), false,
            new GUILayoutOption[0]) as Texture2D);
        if (EditorGUI.EndChangeCheck())
        {
            this.ConsoleContent = string.Empty;
        }

        if (TextureSource == null)
        {
            return;
        }


        GUILayout.Space(5f);
        if (GUILayout.Button("生成", new GUILayoutOption[0]))
        {
            TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(TextureSource)) as TextureImporter;

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                ShowNotification(new GUIContent("SpriteImportMode not equal SpriteImportMode.Multiple"));
                return; ;
            }


            RichTextAnimateDic = new Dictionary<string, RichTextAnimate>();
            RichTextSpriteInfoList = CreateSpriteFromUnityPackSetting(importer);


            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(this.TextureSource)).DirectoryName, this.TextureSource.name, "asset");
            if (text.Length == 0)
            {
                return;
            }
            this.SaveSpriteAsset(text);
        }

        GUILayout.Space(5f);
    }


    /// <summary>
    /// 利用Unity打包的图集
    /// </summary>
    void CustomPackGUI()
    {
        EditorGUI.BeginChangeCheck();
        SettingAsset = (EditorGUILayout.ObjectField("图集信息", this.SettingAsset, typeof(UnityPackSetting), false,
                new GUILayoutOption[0]) as UnityPackSetting);
        if (EditorGUI.EndChangeCheck())
        {
            this.ConsoleContent = string.Empty;
        }

        if (SettingAsset == null)
        {
            return;
        }


        if (GUILayout.Button("生成", new GUILayoutOption[0]))
        {
            TextureSource = SettingAsset.Atlas as Texture2D;
            RichTextAnimateDic = new Dictionary<string, RichTextAnimate>();
            RichTextSpriteInfoList = CreateSpriteFromCustomPackSetting(SettingAsset);


            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("保存富文本数据", SettingAsset.OutputAbsolutelyPath, SettingAsset.Name, "asset");
            if (text.Length == 0)
            {
                return;
            }
            this.SaveSpriteAsset(text);
        }

        GUILayout.Space(5f);
    }


    private List<TexturePackSprite> CreateSpriteFromCustomPackSetting(UnityPackSetting setting)
    {
        List<TexturePackSprite> list = new List<TexturePackSprite>();
        RichTextAnimateDic.Clear();
        for (int i = 0; i < setting.TexturePackSprite.Count; i++)
        {
            UnityPackSprite frame = setting.TexturePackSprite[i];

            TexturePackSprite spriteData = new TexturePackSprite();
            spriteData.id = i;
            spriteData.name = frame.filename;
            spriteData.hashCode = spriteData.name.GetHashCode();

            spriteData.x = frame.x;
            spriteData.y = frame.y;
            spriteData.width = frame.width;
            spriteData.height = frame.height;
            spriteData.pivot = frame.pivot;
            spriteData.rotated = frame.rotated;
            spriteData.xAdvance = spriteData.width;
            spriteData.scale = 1f;
            spriteData.xOffset = 0f - spriteData.width * spriteData.pivot.x;
            spriteData.yOffset = spriteData.height - spriteData.height * spriteData.pivot.y;

            list.Add(spriteData);

            int pos = spriteData.name.LastIndexOf("_");

            if (pos > 0)
            {
                string key = spriteData.name.Substring(0, pos);
                string aniIndex = spriteData.name.Substring(pos + 1, spriteData.name.Length - pos - 1);

                if (!RichTextAnimateDic.ContainsKey(key))
                {
                    RichTextAnimate richTextAnimate = new RichTextAnimate();
                    RichTextAnimateDic.Add(key, richTextAnimate);
                }

                spriteData.animatGroup = key;
                spriteData.animatIndex = int.Parse(aniIndex);

                RichTextAnimateDic[key].animateName = key;
                RichTextAnimateDic[key].frameCount++;
                RichTextAnimateDic[key].spriteList.Add(spriteData);
            }
        }

        return list;
    }


    private List<TexturePackSprite> CreateSpriteFromUnityPackSetting(TextureImporter importer)
    {
        List<TexturePackSprite> list = new List<TexturePackSprite>();
        RichTextAnimateDic.Clear();
        for (int i = 0; i < importer.spritesheet.Length; i++)
        {
            SpriteMetaData frame = importer.spritesheet[i];

            TexturePackSprite spriteData = new TexturePackSprite();
            spriteData.id = i;
            spriteData.name = frame.name;
            spriteData.hashCode = spriteData.name.GetHashCode();


            spriteData.x = frame.rect.x;
            spriteData.y = frame.rect.y;
            spriteData.width = frame.rect.width;
            spriteData.height = frame.rect.height;
            spriteData.pivot = frame.pivot;
            spriteData.rotated = false;
            spriteData.xAdvance = spriteData.width;
            spriteData.scale = 1f;
            spriteData.xOffset = 0f - spriteData.width * spriteData.pivot.x;
            spriteData.yOffset = spriteData.height - spriteData.height * spriteData.pivot.y;

            list.Add(spriteData);

            int pos = spriteData.name.LastIndexOf("_");

            if (pos > 0)
            {
                string key = spriteData.name.Substring(0, pos);
                string aniIndex = spriteData.name.Substring(pos + 1, spriteData.name.Length - pos - 1);

                if (!RichTextAnimateDic.ContainsKey(key))
                {
                    RichTextAnimate richTextAnimate = new RichTextAnimate();
                    RichTextAnimateDic.Add(key, richTextAnimate);
                }

                spriteData.animatGroup = key;
                spriteData.animatIndex = int.Parse(aniIndex);


                RichTextAnimateDic[key].animateName = key;
                RichTextAnimateDic[key].frameCount++;
                RichTextAnimateDic[key].spriteList.Add(spriteData);
            }
        }
        return list;
    }



    /// <summary>
    /// 利用TexturePack打包图集信息
    /// </summary>
    void TexturePackGUI()
    {
        EditorGUI.BeginChangeCheck();
        this.TPTextSheet = (EditorGUILayout.ObjectField("TP数据(Json)", this.TPTextSheet, typeof(TextAsset), false, new GUILayoutOption[0]) as TextAsset);
        GUILayout.Space(5);

        this.TextureSource = (EditorGUILayout.ObjectField("TP合图", this.TextureSource, typeof(Texture2D), false, new GUILayoutOption[0]) as Texture2D);
        if (EditorGUI.EndChangeCheck())
        {
            this.ConsoleContent = string.Empty;
        }

        if (TPTextSheet == null || TextureSource == null)
        {
            return;
        }

        GUILayout.Space(10f);

        if (GUILayout.Button("生成", new GUILayoutOption[0]))
        {

            this.ConsoleContent = string.Empty;
            TP.TexturePackJsonData spriteDataObject = JsonUtility.FromJson<TP.TexturePackJsonData>(this.TPTextSheet.text);

            RichTextAnimateDic = new Dictionary<string, RichTextAnimate>();
            RichTextSpriteInfoList = CreateSpriteInfoList(spriteDataObject);

            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("保存富文本数据", new FileInfo(AssetDatabase.GetAssetPath(this.TPTextSheet)).DirectoryName, this.TPTextSheet.name, "asset");
            if (text.Length == 0)
            {
                return;
            }
            this.SaveSpriteAsset(text);
        }
    }

    private List<TexturePackSprite> CreateSpriteInfoList(TP.TexturePackJsonData spriteDataObject)
    {
        List<TP.SpriteData> frames = spriteDataObject.frames;
        List<TexturePackSprite> list = new List<TexturePackSprite>();
        for (int i = 0; i < frames.Count; i++)
        {
            TexturePackSprite spriteData = new TexturePackSprite();
            spriteData.id = i;
            spriteData.name = Path.GetFileNameWithoutExtension(frames[i].filename);
            spriteData.hashCode = spriteData.name.GetHashCode();

            spriteData.x = frames[i].frame.x;
            spriteData.y = (float)this.TextureSource.height - (frames[i].frame.y + frames[i].frame.h);
            spriteData.width = frames[i].frame.w;
            spriteData.height = frames[i].frame.h;
            spriteData.pivot = frames[i].pivot;
            spriteData.rotated = frames[i].rotated;
            spriteData.xAdvance = spriteData.width;
            spriteData.scale = 1f;
            spriteData.xOffset = 0f - spriteData.width * spriteData.pivot.x;
            spriteData.yOffset = spriteData.height - spriteData.height * spriteData.pivot.y;

            list.Add(spriteData);

            int pos = spriteData.name.LastIndexOf("_");

            if (pos > 0)
            {
                string key = spriteData.name.Substring(0, pos);
                string aniIndex = spriteData.name.Substring(pos + 1, spriteData.name.Length - pos - 1);

                if (!RichTextAnimateDic.ContainsKey(key))
                {
                    RichTextAnimate richTextAnimate = new RichTextAnimate();
                    RichTextAnimateDic.Add(key, richTextAnimate);
                }

                //动画
                spriteData.animatGroup = key;
                spriteData.animatIndex = int.Parse(aniIndex);


                RichTextAnimateDic[key].animateName = key;
                RichTextAnimateDic[key].frameCount++;
                RichTextAnimateDic[key].spriteList.Add(spriteData);
            }



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
            this.RichTextAsset = ScriptableObject.CreateInstance<RichTextSpriteAsset>();
            AssetDatabase.CreateAsset(this.RichTextAsset, str + ".asset");
        }
        else
        {
            this.RichTextAsset = AssetDatabase.LoadAssetAtPath<RichTextSpriteAsset>(str + ".asset");
            if (this.RichTextAsset == null)
            {
                this.RichTextAsset = ScriptableObject.CreateInstance<RichTextSpriteAsset>();
                AssetDatabase.CreateAsset(this.RichTextAsset, str + ".asset");
            }
        }

        this.RichTextAsset.hashCode = this.RichTextAsset.name.GetHashCode();
        this.RichTextAsset.spriteSheet = this.TextureSource;
        this.RichTextAsset.spriteInfoList = this.RichTextSpriteInfoList;
        this.RichTextAsset.animateList.Clear();
        if (this.RichTextAnimateDic != null && this.RichTextAnimateDic.Count > 0)
        {
            foreach (var keyValue in RichTextAnimateDic)
            {
                keyValue.Value.m_SpriteList.Sort((a, b) =>
                {
                    if (a.animatIndex == b.animatIndex) return 0;
                    return a.animatIndex > b.animatIndex ? 1 : -1;
                });
                this.RichTextAsset.animateList.Add(keyValue.Value);
            }
        }

        AddDefaultMaterial(this.RichTextAsset);

        EditorUtility.SetDirty(this.RichTextAsset);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        if (Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        this.RichTextAnimateDic = null;
        this.RichTextSpriteInfoList = null;
    }

    private static void AddDefaultMaterial(RichTextSpriteAsset spriteAsset)
    {
        Shader shader = Shader.Find("UI/Default");
        Material material = new Material(shader);

        material.SetTexture("_MainTex", spriteAsset.spriteSheet);
        spriteAsset.material = material;
        material.hideFlags = HideFlags.HideInHierarchy;
        AssetDatabase.AddObjectToAsset(material, spriteAsset);
    }
}
