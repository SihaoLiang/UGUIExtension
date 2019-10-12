using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Serialize;
using System.IO;
using System;

public class TexturePackWindow : EditorWindow
{

    string genConsole;
    GUIStyle labelStyle;

    //scrollview
    private Vector2 PrePackScrollOffset = Vector2.zero;
    private Vector2 ScrollOffset = Vector2.zero;

    //Unity打包图集配置
    private UnityPackSetting Setting;

    [MenuItem("TexturePackTool/TexturePack Asset Creator")]
    public static void ShowArtFontAssetGenWindow()
    {
        TexturePackWindow window = EditorWindow.GetWindow<TexturePackWindow>();
        window.titleContent = new GUIContent("TexturePackAsset  Creator");
        window.Focus();
    }

    private void OnEnable()
    {
        genConsole = string.Empty;
        InitGUIStyle();
        EditorUtility.ClearProgressBar();
    }


    void InitGUIStyle()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 16;
        labelStyle.richText = true;
    }

    void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Space(5);

        UnityPackGUI();

        GUILayout.EndVertical();
    }



    void UnityPackGUI()
    {
        if (TexturePackTool.DrawHeader("图集信息", true))
        {

            GUILayout.BeginVertical("box");

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

                    AssetDatabase.CreateAsset(Setting, path);
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
            }


            GUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();



            if (Setting == null)
            {
                GUILayout.EndVertical();
                return;
            }

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
            GUILayout.EndVertical();
        }

        if (TexturePackTool.DrawHeader("预打包纹理", true))
        {
            if (Setting.PrePackTextures != null && Setting.PrePackTextures.Count > 0)
            {
                PrePackScrollOffset = EditorGUILayout.BeginScrollView(PrePackScrollOffset);

                for (int i = 0; i < Setting.PrePackTextures.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    Texture2D texture = Setting.PrePackTextures[i] as Texture2D;

                    EditorGUILayout.ObjectField(texture.name, texture, typeof(UnityEngine.Object), false,
                        new GUILayoutOption[0]);

                    if (Setting.PrePackTexturesMD5.Count > i)
                        GUILayout.Label(Setting.PrePackTexturesMD5[i]);
                    EditorGUILayout.EndHorizontal();

                }
                EditorGUILayout.EndScrollView();
            }
        }

        if (TexturePackTool.DrawHeader("控制台", true))
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("导入纹理", GUILayout.Height(30)))
            {
                Setting.PrePackTextures.Clear();
                Setting.PrePackTexturesMD5.Clear();
                string selectPath = EditorUtility.SaveFolderPanel("选择导入纹理目录", Application.dataPath, "");
                if (!string.IsNullOrEmpty(selectPath))
                {
                    Setting.PrePackTextures = TexturePackTool.GetTexturesFromPath(selectPath);
                    Setting.PrePackTextures.Sort((a, b) => string.Compare(a.name, b.name));
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    try
                    {
                        for (int i = 0; i < Setting.PrePackTextures.Count; i++)
                        {

                            Texture2D texture = Setting.PrePackTextures[i] as Texture2D;
                            bool isCancle = EditorUtility.DisplayCancelableProgressBar("导入纹理.", texture.name, (float)i / Setting.PrePackTextures.Count);
                            if (isCancle)
                            {
                                EditorUtility.ClearProgressBar();
                                break;
                            }

                            string filePath = AssetDatabase.GetAssetPath(texture);
                            TexturePackTool.MakeTextureReadable(filePath, false);

                            byte[] bytes = texture.EncodeToPNG();
                            string texMd5 = System.BitConverter.ToString(md5.ComputeHash(bytes));
                            Setting.PrePackTexturesMD5.Add(texMd5);
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

            if (GUILayout.Button("导出图集", GUILayout.Height(30)) && Setting.PrePackTextures != null)
            {
                if (Setting.PrePackTextures.Count <= 0)
                {
                    ShowNotification(new GUIContent("打包图集不能为空"));
                    return;
                }

                Setting.PackTextures.Clear();
                Setting.PackTexturesMD5Dic.Clear();
                Setting.TexturePackSprite.Clear();
                int index = 0;

                for (int i = 0; i < Setting.PrePackTextures.Count; i++)
                {
                    Texture2D tex = Setting.PrePackTextures[i] as Texture2D;
                    UnityPackSprite sprite = new UnityPackSprite();
                    sprite.filename = tex.name;
                    sprite.index = index;
                    Setting.TexturePackSprite.Add(sprite);
                    //如果是相同的图片建立关联
                    if (Setting.TrimSimilar)
                    {
                        string md5 = Setting.PrePackTexturesMD5[i];
                        if (Setting.PackTexturesMD5Dic.ContainsKey(md5))
                        {
                            sprite.index = Setting.PackTexturesMD5Dic[md5].index;
                            continue;
                        }
                        else
                        {
                       
                            Setting.PackTexturesMD5Dic.Add(md5, sprite);
                        }
                    }
             
                    Setting.PackTextures.Add(tex);
                    index++;
                }

                bool newAtlas = false;

                //打包图集
                Texture2D texture = Setting.Atlas as Texture2D;
                if (Setting.Atlas == null)
                {
                    newAtlas = true;
                    texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                }

                Rect[] rects = TexturePackTool.PackTextures(texture, Setting.PackTextures, Setting.Padding);

                if (rects != null)
                {
                    for (int i = 0; i < Setting.TexturePackSprite.Count; i++)
                    {
                        UnityPackSprite sprite = Setting.TexturePackSprite[i];
                        if (sprite.index >= rects.Length)
                        {
                            Debug.LogError("图集信息出错");
                            return;
                        }

                        Rect rect = rects[sprite.index];
                        sprite.x = Mathf.RoundToInt(rect.x * texture.width);
                        sprite.y = Mathf.RoundToInt(rect.y * texture.width);
                        sprite.width = Mathf.RoundToInt(rect.width * texture.width);
                        sprite.height = Mathf.RoundToInt(rect.height * texture.width);
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

                if (newAtlas)
                {
                    byte[] bytes = texture.EncodeToPNG();
                    System.IO.File.WriteAllBytes(filePath, bytes);
                    bytes = null;
                    TexturePackTool.MakeTextureReadable(filePath, true);
                    Setting.Atlas = AssetDatabase.LoadAssetAtPath<Texture>(filePath);
                }

                Setting.Width = (int)texture.width;
                Setting.Height = (int)texture.height;


                EditorUtility.SetDirty(Setting);
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            this.genConsole = string.Concat(new object[]
            {
                string.Format( "打包纹理数:{0}", Setting.PackTextures.Count),
                "\n",
                string.Format( "预打包纹理数:{0}", Setting.PrePackTextures.Count),
            });

            EditorGUILayout.LabelField(this.genConsole, labelStyle, GUILayout.Height(80));

            EditorGUILayout.ObjectField(Setting.Name, Setting.Atlas, typeof(Texture2D), false, GUILayout.Height(80));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

        }
    }
}
