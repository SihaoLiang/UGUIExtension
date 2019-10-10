using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;

public class ArtFontAssetGenWindow : EditorWindow
{

    Texture2D texSource;
    TextAsset texSheet;
    string genConsole;
    GUIStyle labelStyle;

    List<BMFontSprite> fontSpriteInfoList = new List<BMFontSprite>();
    BMFontAsset fontAsset;

    [MenuItem("Assets/Art Font Asset Creator")]
    public static void ShowArtFontAssetGenWindow()
    {
        ArtFontAssetGenWindow window = EditorWindow.GetWindow<ArtFontAssetGenWindow>();
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
        GUILayout.BeginVertical(new GUILayoutOption[0]);
        //   GUILayout.Label("Art Text Generator", "LODRenderersText", new GUILayoutOption[0]);
        EditorGUI.BeginChangeCheck();
        this.texSheet = (EditorGUILayout.ObjectField("Texture Data(Json)", this.texSheet, typeof(TextAsset), false, new GUILayoutOption[0]) as TextAsset);
        this.texSource = (EditorGUILayout.ObjectField("Texture Atlas", this.texSource, typeof(Texture2D), false, new GUILayoutOption[0]) as Texture2D);
        if (EditorGUI.EndChangeCheck())
        {
            this.genConsole = string.Empty;
        }

        if (texSheet == null || texSource == null)
        {
            GUILayout.EndVertical();
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
        GUILayout.EndVertical();
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

            int pos = fontData.name.LastIndexOf("_");

            if (pos == -1)
                Debug.LogError("图集命名不合法，不存在 '_'");

            string key = fontData.name.Substring(pos + 1, fontData.name.Length - pos - 1);

            try{
                int.Parse(key);
            }
            catch(Exception ex) {
                Debug.LogError(string.Format(" 图片{0}, Key必须数字,{1}",fontData.name, ex));
            }

            fontData.key = key;
            fontData.group = fontData.name.Substring(0, pos);
            fontData.x = frames[i].frame.x;
            fontData.y = (float)this.texSource.height - (frames[i].frame.y + frames[i].frame.h);
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
            this.fontAsset = ScriptableObject.CreateInstance<BMFontAsset>();
            AssetDatabase.CreateAsset(this.fontAsset, str + ".asset");
        }
        else
        {
            this.fontAsset = AssetDatabase.LoadAssetAtPath<BMFontAsset>(str + ".asset");
            if (this.fontAsset == null)
            {
                this.fontAsset = ScriptableObject.CreateInstance<BMFontAsset>();
                AssetDatabase.CreateAsset(this.fontAsset, str + ".asset");
            }
        }

        this.fontAsset.hashCode = this.fontAsset.name.GetHashCode();
        this.fontAsset.spriteSheet = this.texSource;
        this.fontAsset.spriteInfoList = this.fontSpriteInfoList;
       // AssetDatabase.CreateAsset(this.fontAsset, str + ".asset");


        AddDefaultMaterial(this.fontAsset);

        EditorUtility.SetDirty(this.fontAsset);
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
