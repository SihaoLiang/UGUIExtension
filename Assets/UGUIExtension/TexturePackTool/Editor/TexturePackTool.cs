using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Serialize;
using UnityEditor;
using UnityEngine;

public class TexturePackTool
{

    /// <summary>
    /// 把文件设置为可读
    /// </summary>

    public static bool MakeTextureReadable(string path, bool force)
    {
        if (string.IsNullOrEmpty(path)) return false;
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return false;

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None ||
            ti.textureCompression != TextureImporterCompression.Uncompressed)
        {
            settings.readable = true;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            var platform = ti.GetDefaultPlatformTextureSettings();
            platform.format = TextureImporterFormat.RGBA32;

            settings.npotScale = TextureImporterNPOTScale.None;
            settings.mipmapEnabled = false;
            ti.SetTextureSettings(settings);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        return true;
    }


    public static Rect[] PackTextures(Texture2D tex, List<Texture2D> textures, int padding, int maxSize = 4096, bool foreSquare = true)
    {
        //Rect[] rects = tex.PackTextures(textures.ToArray(), padding, maxSize);
        Rect[] rects = NewUITexturePacker.PackTextures(tex, textures.ToArray(), 4, 4, 1, maxSize, foreSquare);
        //Rect[] rects = UITexturePacker.PackTextures(tex, textures.ToArray(), 4, 4, padding, maxSize);
        return rects;
    }

    /// <summary>
    /// 剔除像素
    /// </summary>

    static public List<Texture2D> TexturesTrimAlpha(List<Texture2D> textures)
    {
        List<Texture2D> list = new List<Texture2D>();

        foreach (Texture2D tex in textures)
        {
            Texture2D oldTex = tex;

            // If we want to trim transparent pixels, there is more work to be done
            Color32[] pixels = oldTex.GetPixels32();

            int xmin = oldTex.width;
            int xmax = 0;
            int ymin = oldTex.height;
            int ymax = 0;
            int oldWidth = oldTex.width;
            int oldHeight = oldTex.height;

            // Find solid pixels

            for (int y = 0, yw = oldHeight; y < yw; ++y)
            {
                for (int x = 0, xw = oldWidth; x < xw; ++x)
                {
                    Color32 c = pixels[y * xw + x];

                    if (c.a != 0)
                    {
                        if (y < ymin) ymin = y;
                        if (y > ymax) ymax = y;
                        if (x < xmin) xmin = x;
                        if (x > xmax) xmax = x;
                    }
                }
            }

            int newWidth = (xmax - xmin) + 1;
            int newHeight = (ymax - ymin) + 1;

            if (newWidth > 0 && newHeight > 0)
            {
                Texture2D sprite;

                // If the dimensions match, then nothing was actually trimmed
                if (newWidth == oldWidth && newHeight == oldHeight)
                {
                    sprite = oldTex;
                }
                else
                {
                    // Copy the non-trimmed texture data into a temporary buffer
                    Color32[] newPixels = new Color32[newWidth * newHeight];

                    for (int y = 0; y < newHeight; ++y)
                    {
                        for (int x = 0; x < newWidth; ++x)
                        {
                            int newIndex = y * newWidth + x;
                            int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                            //if (NGUISettings.atlasPMA)
                            //    newPixels[newIndex] = NGUITools.ApplyPMA(pixels[oldIndex]);
                            //else
                            newPixels[newIndex] = pixels[oldIndex];
                        }
                    }

                    // Create a new texture
                    sprite = new Texture2D(newWidth, newHeight);
                    sprite.name = oldTex.name;

                    sprite.SetPixels32(newPixels);
                    sprite.Apply();

                    // Remember the padding offset
                    //sprite.SetPadding(xmin, ymin, oldWidth - newWidth - xmin, oldHeight - newHeight - ymin);
                }

                list.Add(sprite);
            }
        }

        return list;
    }


    /// <summary>
    /// 获取选中文件夹下的所有图片
    /// </summary>
    /// <returns></returns>
    public static List<Texture> GetTexturesFromPath(string path)
    {
        var textures = new List<Texture>();
        var names = new List<string>();

        if (Directory.Exists(path))
        {
            DirectoryInfo direction = new DirectoryInfo(path);

            FileInfo[] files = direction.GetFiles("*.png", SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                string filePath = files[i].FullName;
                filePath = filePath.Substring(filePath.IndexOf("Assets")).Replace('\\', '/');
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(filePath);
                if (tex == null) continue;
                if (names.Contains(tex.name)) continue;

                names.Add(tex.name);
                textures.Add(tex);
            }
        }

        return textures;
    }

    /// <summary>
    /// 获取选中文件夹下的所有图片
    /// </summary>
    /// <returns></returns>
    public static List<Texture> GetSelectedTextures()
    {
        var textures = new List<Texture>();
        var names = new List<string>();

        if (Selection.objects != null && Selection.objects.Length > 0)
        {
            var objects = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);

            foreach (UnityEngine.Object o in objects)
            {
                var tex = o as Texture;
                if (tex == null) continue;
                if (names.Contains(tex.name)) continue;

                names.Add(tex.name);
                textures.Add(tex);
                continue;
            }
        }

        return textures;
    }


    public static bool DrawHeader(string text, bool detailed)
    {
        return DrawHeader(text, text, detailed, !detailed);
    }


    public static bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    /// <summary>
    /// 生成精灵
    /// </summary>
    /// <param name="sheetInfo"></param>
    public static void SetupSpriteMetaData(UnityPackSetting sheetInfo)
    {
        if (sheetInfo == null || sheetInfo.Atlas == null)
            return;

        TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sheetInfo.Atlas)) as TextureImporter;

        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }

        if (importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.SaveAndReimport();
        }

        SpriteMetaData[] metadata = sheetInfo.GetSpriteMetaData();
        CopyOldAttributes(importer.spritesheet, metadata, true, true);

        if (!AreEqual(importer.spritesheet, metadata))
        {
            importer.spritesheet = metadata;
            EditorUtility.SetDirty(importer);
            EditorUtility.SetDirty(sheetInfo.Atlas);
            AssetDatabase.WriteImportSettingsIfDirty(importer.assetPath);

            EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(delegate ()
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }));
        }
        else
        {
            Debug.Log("Meta data hasn't changed");
        }
    }

    /// <summary>
    /// 对比
    /// </summary>
    /// <param name="meta1"></param>
    /// <param name="meta2"></param>
    /// <returns></returns>
    public static bool AreEqual(SpriteMetaData[] meta1, SpriteMetaData[] meta2)
    {
        bool flag = meta1.Length == meta2.Length;
        int num = 0;
        while (flag && num < meta1.Length)
        {
            flag = (flag && meta1[num].name.Equals(meta2[num].name));
            flag = (flag && meta1[num].rect.Equals(meta2[num].rect));
            flag = (flag && meta1[num].border.Equals(meta2[num].border));
            flag = (flag && meta1[num].pivot.Equals(meta2[num].pivot));
            flag = (flag && meta1[num].alignment == meta2[num].alignment);
            num++;
        }
        return flag;
    }

    /// <summary>
    /// 复制在编辑器上的修改
    /// </summary>
    /// <param name="oldMeta"></param>
    /// <param name="newMeta"></param>
    /// <param name="copyPivotPoints"></param>
    /// <param name="copyBorders"></param>
    public static void CopyOldAttributes(SpriteMetaData[] oldMeta, SpriteMetaData[] newMeta, bool copyPivotPoints, bool copyBorders)
    {
        for (int i = 0; i < newMeta.Length; i++)
        {
            foreach (SpriteMetaData spriteMetaData in oldMeta)
            {
                if (spriteMetaData.name == newMeta[i].name)
                {
                    if (copyPivotPoints)
                    {
                        newMeta[i].pivot = spriteMetaData.pivot;
                        newMeta[i].alignment = spriteMetaData.alignment;
                    }
                    if (copyBorders)
                    {
                        newMeta[i].border = spriteMetaData.border;
                    }
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="totalNum"></param>
    /// <param name="size"></param>
    /// <param name="showCount"></param>
    /// <param name="space"></param>
    /// <param name="IndexDelegate"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static Vector2 BeginScrollViewEx(Vector2 offset, int totalNum,int size,int showCount,int space,Action<int> IndexDelegate, params GUILayoutOption[] options)
    {

        if (totalNum < showCount)
        {
            int temp = showCount;
            showCount = totalNum;
            offset = EditorGUILayout.BeginScrollView(offset, options);
            for (int i = 0; i < totalNum; i++)
            {
                if (IndexDelegate != null)
                {
                    IndexDelegate(i);
                }
            }

            EditorGUILayout.EndScrollView();
            return offset;
        }

        float totalSize = (totalNum) * (size + space);
        float viewSpace = (showCount * (space + size));
        float dir = offset.x - 0 < 0.0001f ? offset.y : offset.x;
        int startIndex = Mathf.FloorToInt(dir / (size + space)) - 1;// Mathf.FloorToInt((totalNum - showCount) * dir / totalSize);
        startIndex = Mathf.Clamp(startIndex, 0, totalNum - showCount);
        offset = EditorGUILayout.BeginScrollView(offset, options);

        float upSpace = (startIndex - 1) * (space + size);
        upSpace = Mathf.Clamp(upSpace, 0, totalSize);
        float downSpace = totalSize - upSpace - (showCount * (space + size)); //(totalNum - (startIndex + showCount)) * (space + size) - space;

        GUILayout.Space(upSpace);
        for (int i = 0; i < showCount; i++)
        {
            if (IndexDelegate != null)
            {
                IndexDelegate(i + startIndex);
            }
        }
    
        GUILayout.Space(downSpace);
        EditorGUILayout.EndScrollView();

        return offset;
    }

}


