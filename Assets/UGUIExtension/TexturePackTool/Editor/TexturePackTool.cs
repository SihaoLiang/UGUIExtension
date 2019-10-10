using System.Collections;
using System.Collections.Generic;
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

        if (force || !settings.readable || settings.npotScale != TextureImporterNPOTScale.None)
        {
            settings.readable = true;

            var platform = ti.GetDefaultPlatformTextureSettings();
            platform.format = TextureImporterFormat.RGBA32;

            settings.npotScale = TextureImporterNPOTScale.None;
            ti.SetTextureSettings(settings);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }
        return true;
    }


    public static bool PackTextures(Texture2D tex, List<Texture2D> textures,int padding,int maxSize = 1024)
    {
        Rect[] rects = tex.PackTextures(textures.ToArray(), padding, maxSize);

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
}
