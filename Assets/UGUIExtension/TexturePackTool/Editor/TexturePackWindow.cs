using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;
using Object = UnityEngine.Object;

public class TexturePackWindow : EditorWindow
{

    public enum PackingType
    {
        TexturePack = 0,
        UnityPack
    }

     
    Texture2D texSource;
    TextAsset texSheet;
    string genConsole;
    GUIStyle labelStyle;

    List<TexturePackSprite> fontSpriteInfoList = new List<TexturePackSprite>();
    Dictionary<string, TexturePackAnimate> animateDic = new Dictionary<string, TexturePackAnimate>();

    //精灵资源
    TexturePackSpriteAsset texturePackAsset;

    //打包类型
    private PackingType AtlasPacking = PackingType.TexturePack;

    //Unity打包图集配置
    private UnityPackSetting Setting;

    [MenuItem("Assets/TexturePack Asset Creator")]
    public static void ShowArtFontAssetGenWindow()
    {
        TexturePackWindow window = EditorWindow.GetWindow<TexturePackWindow>();
        window.titleContent = new GUIContent("TexturePackAsset  Creator");
        window.Focus();
    }

    private void OnEnable()
    {
        genConsole = string.Empty;
        texSource = null;
        texSheet = null;
        texturePackAsset = null;
        InitGUIStyle();
    }


    void InitGUIStyle()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.fontSize = 16;
        labelStyle.richText = true;
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




    private List<Texture> SelectedTextures = null;
    Vector2 ScrollOffset = Vector2.zero;
    private string UnityPackAssetPath = string.Empty;

    void UnityPackGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        //新建图集
        Setting = EditorGUILayout.ObjectField("图集信息（UnityPack Asset）", Setting, typeof(UnityPackSetting), false, new GUILayoutOption[0]) as UnityPackSetting;
        EditorGUI.BeginDisabledGroup(Setting != null);
        if (GUILayout.Button("New", GUILayout.Width(40f)))
        {
            string path = EditorUtility.SaveFilePanel("新建图集", Application.dataPath, "NewUnityPackAsset", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = path.Substring(path.IndexOf("Assets")).Replace('\\', '/');
                Setting = ScriptableObject.CreateInstance<UnityPackSetting>();
                Setting.Name = Path.GetFileNameWithoutExtension(relativePath);
                Setting.OutputPath = Path.GetDirectoryName(relativePath);
                Setting.OutputAbsolutelyPath = Path.GetDirectoryName(path);

                AssetDatabase.CreateAsset(Setting, path);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }


        GUILayout.EndHorizontal();

        EditorGUI.EndDisabledGroup();

        GUILayout.EndVertical();


        if (Setting == null)
            return;

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Label("图集导出路径");
        Setting.OutputPath = GUILayout.TextField(Setting.OutputPath); 
        
        if(GUILayout.Button("..."))
        {
            string outputPath = EditorUtility.SaveFolderPanel("选择导出目录", Setting.OutputAbsolutelyPath,"");
            Setting.OutputAbsolutelyPath = outputPath;
            Setting.OutputPath = outputPath.Substring(outputPath.IndexOf("Assets")).Replace('\\', '/');
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("图集间隙");
        Setting.Padding = EditorGUILayout.IntField(Setting.Padding);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("剔除透明");
        Setting.AtlasTrimming = EditorGUILayout.Toggle(Setting.AtlasTrimming);
        GUILayout.EndHorizontal();


        if (GUILayout.Button("导入选中文件夹"))
        {
            Setting.PrePackTextures = TexturePackTool.GetSelectedTextures();
            Setting.PrePackTextures.Sort((a, b) => string.Compare(a.name, b.name));
        }

        ScrollOffset = EditorGUILayout.BeginScrollView(ScrollOffset);

        if (Setting.PrePackTextures != null && Setting.PrePackTextures.Count > 0)
        {
            for (int i = 0; i < Setting.PrePackTextures.Count; i++)
            {
                Texture texture = Setting.PrePackTextures[i];
                EditorGUILayout.ObjectField(texture.name, texture, typeof(UnityEngine.Object), false, new GUILayoutOption[0]);
            }
        }


        EditorGUILayout.EndScrollView();

        //导出图集
        if (GUILayout.Button("导出") && Setting.PrePackTextures != null)
        {
            Setting.PackTextures.Clear(); ;
            for (int i = 0; i < Setting.PrePackTextures.Count; i++)
            {
                Texture2D tex = Setting.PrePackTextures[i] as Texture2D;
                string texPath = AssetDatabase.GetAssetPath(tex.GetInstanceID());
                TexturePackTool.MakeTextureReadable(texPath, true);
                Setting.PackTextures.Add(tex);
            }
            Setting.Atlas = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            PackTextures(Setting.Atlas, Setting.PackTextures);

            byte[] bytes = Setting.Atlas.EncodeToPNG();
            System.IO.File.WriteAllBytes(Setting.OutputPath + "/" + Setting.Name + ".png", bytes);
            bytes = null;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        GUILayout.EndVertical();

    }
   

    bool PackTextures(Texture2D tex, List<Texture2D> textures)
    {
        Rect[] rects;

        int maxSize = SystemInfo.maxTextureSize;
        rects = tex.PackTextures(textures.ToArray(), Setting.Padding, maxSize);
        
       
        for (int i = 0; i < rects.Length; ++i)
        {
            TexturePackSprite se = new TexturePackSprite();
            se.x = Mathf.RoundToInt(rects[i].x);
            se.y = Mathf.RoundToInt(rects[i].y);
            se.width = Mathf.RoundToInt(rects[i].width);
            se.height = Mathf.RoundToInt(rects[i].height);
        }
        return true;
    }


    void TexturePackGUI()
    {
        EditorGUI.BeginChangeCheck();
        this.texSheet = (EditorGUILayout.ObjectField("Texture Data(Json)", this.texSheet, typeof(TextAsset), false, new GUILayoutOption[0]) as TextAsset);
        this.texSource = (EditorGUILayout.ObjectField("Texture Atlas", this.texSource, typeof(Texture2D), false, new GUILayoutOption[0]) as Texture2D);
        if (EditorGUI.EndChangeCheck())
        {
            this.genConsole = string.Empty;
        }

        if (texSheet == null || texSource == null)
        {
            return;
        }

        GUILayout.Space(10f);
        if (GUILayout.Button("Create Sprite Asset", new GUILayoutOption[0]))
        {
            this.genConsole = string.Empty;
            TP.TexturePackJsonData spriteDataObject = JsonUtility.FromJson<TP.TexturePackJsonData>(this.texSheet.text);
            if (spriteDataObject != null && spriteDataObject.frames != null && spriteDataObject.frames.Count > 0)
            {
                int count = spriteDataObject.frames.Count;
                this.genConsole = "<b>Import Results</b>\n-----------------------------\n";
                string str = this.genConsole;
                this.genConsole = string.Concat(new object[]
                {
                            str,
                            "<color=#C0ffff><b>",
                            count,
                            "</b></color> Sprites were imported from file."
                });
            }
            animateDic = new Dictionary<string, TexturePackAnimate>();
            fontSpriteInfoList = CreateSpriteInfoList(spriteDataObject);
        }

        GUILayout.Space(5f);
        GUI.enabled = (this.fontSpriteInfoList != null);
        if (GUILayout.Button("Save Sprite Asset", new GUILayoutOption[0]))
        {
            string text = string.Empty;
            text = EditorUtility.SaveFilePanel("Save Sprite Asset File", new FileInfo(AssetDatabase.GetAssetPath(this.texSheet)).DirectoryName, this.texSheet.name, "asset");
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

        EditorGUILayout.LabelField(this.genConsole, labelStyle, new GUILayoutOption[0]);
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
            if (!animateDic.ContainsKey(key))
            {
                TexturePackAnimate texturePackAnimate = new TexturePackAnimate();
                animateDic.Add(key, texturePackAnimate);
            }

            //fontData.key = key;
            spriteData.animatGroup = key;
            spriteData.x = frames[i].frame.x;
            spriteData.y = (float)this.texSource.height - (frames[i].frame.y + frames[i].frame.h);
            spriteData.width = frames[i].frame.w;
            spriteData.height = frames[i].frame.h;
            spriteData.pivot = frames[i].pivot;
            spriteData.rotated = frames[i].rotated;
            spriteData.xAdvance = spriteData.width;
            spriteData.scale = 1f;
            spriteData.xOffset = 0f - spriteData.width * spriteData.pivot.x;
            spriteData.yOffset = spriteData.height - spriteData.height * spriteData.pivot.y;
            Rect texCoords;

     
            texCoords = new Rect(frames[i].frame.x / (float)texSource.width, frames[i].frame.y / (float)texSource.height, frames[i].frame.w / (float)texSource.width, frames[i].frame.h / (float)texSource.height);
      
            Sprite sprite = Sprite.Create((Texture2D)texSource, texCoords, frames[i].pivot);
            spriteData.sprite = sprite;

            list.Add(spriteData);

            animateDic[key].animateName = key;
            animateDic[key].frameCount++;
            animateDic[key].spriteList.Add(spriteData);
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
            this.texturePackAsset = ScriptableObject.CreateInstance<TexturePackSpriteAsset>();
            AssetDatabase.CreateAsset(this.texturePackAsset, str + ".asset");
        }
        else
        {
            this.texturePackAsset = AssetDatabase.LoadAssetAtPath<TexturePackSpriteAsset>(str + ".asset");
            if (this.texturePackAsset == null)
            {
                this.texturePackAsset = ScriptableObject.CreateInstance<TexturePackSpriteAsset>();
                AssetDatabase.CreateAsset(this.texturePackAsset, str + ".asset");
            }
        }

        this.texturePackAsset.hashCode = this.texturePackAsset.name.GetHashCode();
        this.texturePackAsset.spriteSheet = this.texSource;
        this.texturePackAsset.spriteInfoList = this.fontSpriteInfoList;
        if (this.animateDic != null && this.animateDic.Count > 0)
        {
            foreach(var keyValue in animateDic)
            {
                this.texturePackAsset.animateList.Add(keyValue.Value);
            }
        }

        AddDefaultMaterial(this.texturePackAsset);

        EditorUtility.SetDirty(this.texturePackAsset);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        if (Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        this.animateDic = null;
        this.fontSpriteInfoList = null;
    }

    private static void AddDefaultMaterial(TexturePackSpriteAsset spriteAsset)
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
