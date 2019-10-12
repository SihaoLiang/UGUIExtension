using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;

public class RichTextAssetGenWindow : EditorWindow
{
    public enum PackingType
    {
        TexturePack = 0,
        UnityPack
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

    [MenuItem("RichText/RichText Asset Creator")]
    public static void ShowArtFontAssetGenWindow()
    {
        RichTextAssetGenWindow window = EditorWindow.GetWindow<RichTextAssetGenWindow>();
        window.titleContent = new GUIContent("RichTextAsset Creator");
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
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Space(5);
        AtlasPacking = (PackingType)EditorGUILayout.EnumPopup("打包方式", AtlasPacking);

        if (AtlasPacking == PackingType.TexturePack)
        {
            TexturePackGUI();
        }
        else
        {
            UnityPackGUI();
        }

        GUILayout.EndVertical();
    }


    /// <summary>
    /// 利用Unity打包的图集
    /// </summary>
    void UnityPackGUI()
    {
        EditorGUI.BeginChangeCheck();
        SettingAsset =
            (EditorGUILayout.ObjectField("图集信息", this.SettingAsset, typeof(UnityPackSetting), false,
                new GUILayoutOption[0]) as UnityPackSetting);
        if (EditorGUI.EndChangeCheck())
        {
            this.ConsoleContent = string.Empty;
        }

        if (SettingAsset == null)
        {
            return;
        }

        TextureSource = SettingAsset.Atlas as Texture2D;
        RichTextAnimateDic = new Dictionary<string, RichTextAnimate>();
        RichTextSpriteInfoList = CreateSpriteFromUnityPackSetting(SettingAsset);

        GUILayout.Space(5f);
        GUI.enabled = (this.RichTextSpriteInfoList != null);
        if (GUILayout.Button("Save Sprite Asset", new GUILayoutOption[0]))
        {
            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("Save Sprite Asset File", SettingAsset.OutputAbsolutelyPath, SettingAsset.Name, "asset");
            if (text.Length == 0)
            {
                return;
            }
            this.SaveSpriteAsset(text);
        }

        GUILayout.Space(5f);
        GUILayout.BeginVertical("box", new GUILayoutOption[]
        {
            GUILayout.Height(60f)
        });

        EditorGUILayout.LabelField(this.ConsoleContent, LabelStyle, new GUILayoutOption[0]);
        GUILayout.EndVertical();

        GUILayout.Space(5f);
    }


    private List<TexturePackSprite> CreateSpriteFromUnityPackSetting(UnityPackSetting setting)
    {
        List<TexturePackSprite> list = new List<TexturePackSprite>();
        for (int i = 0; i < setting.TexturePackSprite.Count; i++)
        {
            UnityPackSprite frame = setting.TexturePackSprite[i];

            TexturePackSprite spriteData = new TexturePackSprite();
            spriteData.id = i;
            spriteData.name = frame.filename;
            spriteData.hashCode = spriteData.name.GetHashCode();
            int num = spriteData.name.IndexOf('-');

            // int ascii;
            int pos = spriteData.name.LastIndexOf("_");

            if (pos == -1)
                Debug.LogError("图集命名不合法，不存在 '_'");

            string key = spriteData.name.Substring(0, pos);
            if (!RichTextAnimateDic.ContainsKey(key))
            {
                RichTextAnimate richTextAnimate = new RichTextAnimate();
                RichTextAnimateDic.Add(key, richTextAnimate);
            }

            //fontData.key = key;
            spriteData.animatGroup = key;
            spriteData.x = frame.x;
            spriteData.y = (float)this.TextureSource.height - (frame.y + frame.height);
            spriteData.width = frame.width;
            spriteData.height = frame.height;
            spriteData.pivot = frame.pivot;
            spriteData.rotated = frame.rotated;
            spriteData.xAdvance = spriteData.width;
            spriteData.scale = 1f;
            spriteData.xOffset = 0f - spriteData.width * spriteData.pivot.x;
            spriteData.yOffset = spriteData.height - spriteData.height * spriteData.pivot.y;

            Rect texCoords = new Rect(frame.x / (float)TextureSource.width, frame.y / (float)TextureSource.height, frame.width / (float)TextureSource.width, frame.height / (float)TextureSource.height);
            Sprite sprite = Sprite.Create((Texture2D)TextureSource, texCoords, frame.pivot);
            spriteData.sprite = sprite;

            list.Add(spriteData);

            RichTextAnimateDic[key].animateName = key;
            RichTextAnimateDic[key].frameCount++;
            RichTextAnimateDic[key].spriteList.Add(spriteData);
        }
        return list;
    }



    /// <summary>
    /// 利用TexturePack打包图集信息
    /// </summary>
    void TexturePackGUI()
    {
        EditorGUI.BeginChangeCheck();
        this.TPTextSheet = (EditorGUILayout.ObjectField("Texture Data(Json)", this.TPTextSheet, typeof(TextAsset), false, new GUILayoutOption[0]) as TextAsset);
        this.TextureSource = (EditorGUILayout.ObjectField("Texture Atlas", this.TextureSource, typeof(Texture2D), false, new GUILayoutOption[0]) as Texture2D);
        if (EditorGUI.EndChangeCheck())
        {
            this.ConsoleContent = string.Empty;
        }

        if (TPTextSheet == null || TextureSource == null)
        {
            return;
        }

        GUILayout.Space(10f);
        if (GUILayout.Button("Create Sprite Asset", new GUILayoutOption[0]))
        {
            this.ConsoleContent = string.Empty;
            TP.TexturePackJsonData spriteDataObject = JsonUtility.FromJson<TP.TexturePackJsonData>(this.TPTextSheet.text);
            if (spriteDataObject != null && spriteDataObject.frames != null && spriteDataObject.frames.Count > 0)
            {
                int count = spriteDataObject.frames.Count;
                this.ConsoleContent = "<b>Import Results</b>\n-----------------------------\n";
                string str = this.ConsoleContent;
                this.ConsoleContent = string.Concat(new object[]
                {
                            str,
                            "<color=#C0ffff><b>",
                            count,
                            "</b></color> Sprites were imported from file."
                });
            }
            RichTextAnimateDic = new Dictionary<string, RichTextAnimate>();
            RichTextSpriteInfoList = CreateSpriteInfoList(spriteDataObject);
        }

        GUILayout.Space(5f);
        GUI.enabled = (this.RichTextSpriteInfoList != null);
        if (GUILayout.Button("Save Sprite Asset", new GUILayoutOption[0]))
        {
            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(this.TPTextSheet)).DirectoryName, this.TPTextSheet.name, "asset");
            if (text.Length == 0)
            {
                return;
            }
            this.SaveSpriteAsset(text);
        }

        GUILayout.Space(5f);
        GUILayout.BeginVertical("box", new GUILayoutOption[]
        {
            GUILayout.Height(60f)
        });

        EditorGUILayout.LabelField(this.ConsoleContent, LabelStyle, new GUILayoutOption[0]);
        GUILayout.EndVertical();

        GUILayout.Space(5f);
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
            int num = spriteData.name.IndexOf('-');

            // int ascii;

            int pos = spriteData.name.LastIndexOf("_");

            if (pos == -1)
                Debug.LogError("图集命名不合法，不存在 '_'");

            string key = spriteData.name.Substring(0, pos);
            if (!RichTextAnimateDic.ContainsKey(key))
            {
                RichTextAnimate richTextAnimate = new RichTextAnimate();
                RichTextAnimateDic.Add(key, richTextAnimate);
            }

            //fontData.key = key;
            spriteData.animatGroup = key;
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
            Rect texCoords;


            texCoords = new Rect(frames[i].frame.x / (float)TextureSource.width, frames[i].frame.y / (float)TextureSource.height, frames[i].frame.w / (float)TextureSource.width, frames[i].frame.h / (float)TextureSource.height);

            Sprite sprite = Sprite.Create((Texture2D)TextureSource, texCoords, frames[i].pivot);
            spriteData.sprite = sprite;

            list.Add(spriteData);

            RichTextAnimateDic[key].animateName = key;
            RichTextAnimateDic[key].frameCount++;
            RichTextAnimateDic[key].spriteList.Add(spriteData);
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
        if (this.RichTextAnimateDic != null && this.RichTextAnimateDic.Count > 0)
        {
            foreach (var keyValue in RichTextAnimateDic)
            {
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
