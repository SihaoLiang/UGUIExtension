﻿using System.Collections;
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

        if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None || ti.textureCompression != TextureImporterCompression.Uncompressed)
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


    public static Rect[] PackTextures(Texture2D tex, List<Texture2D> textures,int padding,int maxSize = 4096)
    {
        Rect[] rects = tex.PackTextures(textures.ToArray(), padding, maxSize);
        return rects;
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
}
