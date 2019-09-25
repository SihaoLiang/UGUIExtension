using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;
using System.Runtime.InteropServices;
using TextExtend;


public class FontAtlasGenWindow : EditorWindow
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

    List<TextExSprite> fontSpriteInfoList = new List<TextExSprite>();
    Dictionary<string, TextExAnimate> animateDic = new Dictionary<string, TextExAnimate>();

    TextExSpriteAsset fontAsset;

    private PackingType AtlasPacking = PackingType.TexturePack;

    [MenuItem("Assets/Art Font Asset Creator")]
    public static void ShowArtFontAssetGenWindow()
    {
        FontAtlasGenWindow window = EditorWindow.GetWindow<FontAtlasGenWindow>();
        window.titleContent = new GUIContent("ArtFontAsset Creator");
        window.Focus();
    }

    private void OnEnable()
    {
        genConsole = string.Empty;
        texSource = null;
        texSheet = null;
        fontAsset = null;
        // SetEditorWindowSize();
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


        AtlasPacking = (PackingType)EditorGUILayout.EnumPopup("Packing Type", AtlasPacking);

        if (AtlasPacking == PackingType.TexturePack)
        {
            TexturePackGUI();
        }
        else
        {
            
        }


        GUILayout.EndVertical();
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
            TP.JsonData spriteDataObject = JsonUtility.FromJson<TP.JsonData>(this.texSheet.text);
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
            animateDic = new Dictionary<string, TextExAnimate>();
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

    private List<TextExSprite> CreateSpriteInfoList(TP.JsonData spriteDataObject)
    {
        List<TP.SpriteData> frames = spriteDataObject.frames;
        List<TextExSprite> list = new List<TextExSprite>();
        for (int i = 0; i < frames.Count; i++)
        {
            TextExSprite spriteData = new TextExSprite();
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
                TextExAnimate textExAnimate = new TextExAnimate();
                animateDic.Add(key, textExAnimate);
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
            this.fontAsset = ScriptableObject.CreateInstance<TextExSpriteAsset>();
            AssetDatabase.CreateAsset(this.fontAsset, str + ".asset");
        }
        else
        {
            this.fontAsset = AssetDatabase.LoadAssetAtPath<TextExSpriteAsset>(str + ".asset");
            if (this.fontAsset == null)
            {
                this.fontAsset = ScriptableObject.CreateInstance<TextExSpriteAsset>();
                AssetDatabase.CreateAsset(this.fontAsset, str + ".asset");
            }
        }

        this.fontAsset.hashCode = this.fontAsset.name.GetHashCode();
        this.fontAsset.spriteSheet = this.texSource;
        this.fontAsset.spriteInfoList = this.fontSpriteInfoList;
        if (this.animateDic != null && this.animateDic.Count > 0)
        {
            foreach(var keyValue in animateDic)
            {
                this.fontAsset.animateList.Add(keyValue.Value);
            }
        }

        AddDefaultMaterial(this.fontAsset);

        EditorUtility.SetDirty(this.fontAsset);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        if (Application.isPlaying)
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();

        this.animateDic = null;
        this.fontSpriteInfoList = null;
    }

    private static void AddDefaultMaterial(TextExSpriteAsset spriteAsset)
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
