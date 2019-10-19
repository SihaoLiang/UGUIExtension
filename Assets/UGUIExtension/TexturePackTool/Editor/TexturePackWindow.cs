using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;

public class TexturePackWindow : EditorWindow
{

    string genConsole;
    GUIStyle LabelStyle;

    //剔除数量
    int TrimCount = 0;

    //scrollview
    private Vector2 PrePackScrollOffset = Vector2.zero;

    //Unity打包图集配置
    private UnityPackSetting Setting;

    //散图MD5
    public Dictionary<string, UnityPackSprite> PackTexturesMD5Dic = new Dictionary<string, UnityPackSprite>();

    //预打包的图片
    public List<string> PrePackTexturesMD5 = new List<string>();

    //打包的图片
    public List<Texture2D> PackTextures = new List<Texture2D>();

    //预打包的图片
    public List<Texture> PrePackTextures = new List<Texture>();

    [MenuItem("图集工具/图集打包")]
    public static void ShowArtFontAssetGenWindow()
    {
        TexturePackWindow window = EditorWindow.GetWindow<TexturePackWindow>();
        window.titleContent = new GUIContent("图集打包");
        window.Focus();
    }

    private void OnEnable()
    {
        TrimCount = 0;
        genConsole = string.Empty;
        InitGUIStyle();
        EditorUtility.ClearProgressBar();
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

        UnityPackGUI();

        GUILayout.EndVertical();
    }

    /// <summary>
    /// 导入散图
    /// </summary>
    void ImportTextures()
    {
        if (GUILayout.Button("导入纹理", GUILayout.Height(30)))
        {
            PrePackTextures.Clear();
            PrePackTexturesMD5.Clear();
            string selectPath = EditorUtility.SaveFolderPanel("选择导入纹理目录", Application.dataPath, "");
            if (!string.IsNullOrEmpty(selectPath))
            {
                PrePackTextures = TexturePackTool.GetTexturesFromPath(selectPath);
                PrePackTextures.Sort((a, b) => string.Compare(a.name, b.name));
                System.Security.Cryptography.MD5 md5 =
                    new System.Security.Cryptography.MD5CryptoServiceProvider();
                try
                {
                    for (int i = 0; i < PrePackTextures.Count; i++)
                    {

                        Texture2D texture = PrePackTextures[i] as Texture2D;
                        bool isCancle = EditorUtility.DisplayCancelableProgressBar("导入纹理.", texture.name,
                            (float)i / PrePackTextures.Count);
                        if (isCancle)
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }

                        string filePath = AssetDatabase.GetAssetPath(texture);
                        TexturePackTool.MakeTextureReadable(filePath, false);

                        byte[] bytes = texture.EncodeToPNG();
                        string texMd5 = System.BitConverter.ToString(md5.ComputeHash(bytes));
                        PrePackTexturesMD5.Add(texMd5);
                    }

                    md5.Clear();
                }
                catch (Exception e)
                {
                    EditorUtility.ClearProgressBar();
                }

                EditorUtility.ClearProgressBar();
            }
        }
    }

    /// <summary>
    /// 打包图集
    /// </summary>
    void PackingTextures()
    {
        if (GUILayout.Button("导出图集", GUILayout.Height(30)) && PrePackTextures != null)
        {
            if (PrePackTextures.Count <= 0)
            {
                ShowNotification(new GUIContent("打包图集不能为空"));
                return;
            }

            TrimCount = 0;
            PackTextures.Clear();
            PackTexturesMD5Dic.Clear();
            Setting.TexturePackSprite.Clear();
            int index = 0;

            for (int i = 0; i < PrePackTextures.Count; i++)
            {
                Texture2D tex = PrePackTextures[i] as Texture2D;
                UnityPackSprite sprite = new UnityPackSprite();
                sprite.filename = tex.name;
                sprite.index = index;
                Setting.TexturePackSprite.Add(sprite);
                //如果是相同的图片建立关联
                if (Setting.TrimSimilar)
                {
                    string md5 = PrePackTexturesMD5[i];
                    if (PackTexturesMD5Dic.ContainsKey(md5))
                    {
                        TrimCount++;
                        sprite.index = PackTexturesMD5Dic[md5].index;
                        continue;
                    }
                    else
                    {

                        PackTexturesMD5Dic.Add(md5, sprite);
                    }
                }

                PackTextures.Add(tex);
                index++;
            }


            List<Texture2D> textures = PackTextures;
            //剔除透明
            if (Setting.TrimAlpha)
                textures = TexturePackTool.TexturesTrimAlpha(PackTextures);

            if (textures.Count != PackTextures.Count)
            {
                genConsole = "剔除透明像素出错";
                Debug.LogError(genConsole);
                return;
            }

            bool newAtlas = false;

            //打包图集
            Texture2D texture = Setting.Atlas as Texture2D;
            if (Setting.Atlas == null)
            {
                newAtlas = true;
                texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            }

            Rect[] rects = TexturePackTool.PackTextures(texture, textures, Setting.Padding, Setting.MaxSize);

            int totalSize = 0;

            for (int i = 0; i < textures.Count; i++)
            {
                totalSize += Mathf.RoundToInt(textures[i].width * textures[i].height);
            }

            if (rects != null)
            {
                for (int i = 0; i < Setting.TexturePackSprite.Count; i++)
                {
                    UnityPackSprite sprite = Setting.TexturePackSprite[i];
                    if (sprite.index >= rects.Length)
                    {
                        genConsole = "图集打包出错";
                        Debug.LogError(genConsole);
                        return;
                    }


                    Rect rect = rects[sprite.index];
                    sprite.x = Mathf.RoundToInt(rect.x * texture.width);
                    sprite.y = Mathf.RoundToInt(rect.y * texture.height);
                    sprite.width = Mathf.RoundToInt(rect.width * texture.width);
                    sprite.height = Mathf.RoundToInt(rect.height * texture.height);
                    sprite.pivot = Vector2.zero;
                    sprite.rotated = false;
                    sprite.spriteSourceSize.h = texture.height;
                    sprite.spriteSourceSize.w = texture.width;
                    sprite.spriteSourceSize.x = 0;
                    sprite.spriteSourceSize.y = 0;
                    Setting.TexturePackSprite[i] = sprite;
                }
            }

            string filePath = Setting.OutputPath + "/" + Setting.Name + ".png";


            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(filePath, bytes);
            bytes = null;
            TexturePackTool.MakeTextureReadable(filePath, true);
            Setting.Atlas = AssetDatabase.LoadAssetAtPath<Texture>(filePath);


            Setting.Width = (int)texture.width;
            Setting.Height = (int)texture.height;


            EditorUtility.SetDirty(Setting);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            if (totalSize > texture.width * texture.height)
                genConsole = "导出图集成功，图集最大尺寸不足,图集已压缩";
            else
                genConsole = "导出图集成功";
        }

    }

    void UnityPackInfoGUI()
    {
        GUILayout.BeginHorizontal();
        //新建图集
        GUILayout.Label("图集信息", GUILayout.Width(80));
        Setting =
            EditorGUILayout.ObjectField("", Setting, typeof(UnityPackSetting), false, new GUILayoutOption[0]) as
                UnityPackSetting;
        EditorGUI.BeginDisabledGroup(Setting != null);
        if (GUILayout.Button("新建", GUILayout.Width(60f)))
        {
            string path = EditorUtility.SaveFilePanel("新建图集", Application.dataPath, "NewUnityPackAsset", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                string relativePath = path.Substring(path.IndexOf("Assets")).Replace('\\', '/');
                Setting = ScriptableObject.CreateInstance<UnityPackSetting>();
                Setting.Name = Path.GetFileNameWithoutExtension(relativePath);
                Setting.OutputPath = Path.GetDirectoryName(relativePath);
                Setting.OutputAbsolutelyPath = Path.GetDirectoryName(path);

                AssetDatabase.CreateAsset(Setting, relativePath);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
        }


        GUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();


        if (Setting != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("图集导出路径", GUILayout.Width(80));
            Setting.OutputPath = GUILayout.TextField(Setting.OutputPath);

            if (GUILayout.Button("...", GUILayout.Width(60f)))
            {
                string outputPath = EditorUtility.SaveFolderPanel("选择导出目录", Setting.OutputAbsolutelyPath, "");
                if (!string.IsNullOrEmpty(outputPath))
                {
                    Setting.OutputAbsolutelyPath = outputPath;
                    Setting.OutputPath = outputPath.Substring(outputPath.IndexOf("Assets")).Replace('\\', '/');
                }
            }

            GUILayout.EndHorizontal();
            //图集设置
            GUILayout.BeginHorizontal();
            GUILayout.Label("图集间隙", GUILayout.Width(80));
            Setting.Padding = EditorGUILayout.IntField(Setting.Padding);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("剔除透明", GUILayout.Width(80));
            Setting.TrimAlpha = EditorGUILayout.Toggle(Setting.TrimAlpha);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("剔除相似", GUILayout.Width(80));
            Setting.TrimSimilar = EditorGUILayout.Toggle(Setting.TrimSimilar);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("最大尺寸", GUILayout.Width(80));
            Setting.MaxSize = EditorGUILayout.IntField(Setting.MaxSize);
            GUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// GUI
    /// </summary>
    void UnityPackGUI()
    {
        if (TexturePackTool.DrawHeader("图集信息", true))
        {
            GUILayout.BeginVertical("box");

            UnityPackInfoGUI();
            GUILayout.EndVertical();
        }

        if (Setting == null)
            return;

        if (PrePackTextures != null && PrePackTextures.Count > 0)
        {
            if (TexturePackTool.DrawHeader("预打包纹理", true))
            {

                PrePackScrollOffset = EditorGUILayout.BeginScrollView(PrePackScrollOffset);

                for (int i = 0; i < PrePackTextures.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Texture2D texture = PrePackTextures[i] as Texture2D;

                    EditorGUILayout.ObjectField(texture.name, texture, typeof(UnityEngine.Object), false,
                        new GUILayoutOption[0]);

                    if (PrePackTexturesMD5.Count > i)
                        GUILayout.Label("MD5:" + PrePackTexturesMD5[i]);
                    EditorGUILayout.EndHorizontal();

                }

                EditorGUILayout.EndScrollView();
            }
        }

        if (TexturePackTool.DrawHeader("控制台", true))
        {
            GUILayout.BeginVertical("box");
            //导入散图
            ImportTextures();
            //导出图集
            PackingTextures();

            GUILayout.BeginHorizontal("box");

            string console = string.Concat(new object[]
            {
                     string.Format("预打包纹理数:{0}", PrePackTextures.Count),
                    "\n",
                    string.Format("打包纹理数:{0}", PackTextures.Count),
                    "\n",
                    string.Format("剔除纹理数:{0}", TrimCount),
                    "\n",
                    genConsole,
            });

            EditorGUILayout.LabelField(console, LabelStyle, GUILayout.Height(80));

            EditorGUILayout.ObjectField(Setting.Name, Setting.Atlas, typeof(Texture2D), false,
                GUILayout.Height(80));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

        }
    }
}

