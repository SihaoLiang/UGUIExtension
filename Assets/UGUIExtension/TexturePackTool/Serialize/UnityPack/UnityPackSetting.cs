using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityPackSetting : ScriptableObject
{
    public string Name;

    public string OutputAbsolutelyPath;

    public string OutputPath;

    public Texture2D Atlas;

    public List<Texture2D> PackTextures = new List<Texture2D>();

    public List<Texture> PrePackTextures;

    public List<UnityPackSprite> TexturePackSprite;

    public int Padding = 0;

    public bool AtlasTrimming;

    public int Width;

    public int Height;
}
