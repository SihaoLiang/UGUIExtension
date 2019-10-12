using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnityPackSetting : ScriptableObject
{
    //图集名字
    public string Name;

    //输出绝对路径
    public string OutputAbsolutelyPath;

    //输出路径
    public string OutputPath;

    //大图集
    [SerializeField]
    public Texture Atlas;

    //打包的图片
    [NonSerialized]
    public List<Texture2D> PackTextures = new List<Texture2D>();

    //预打包的图片
    [NonSerialized]
    public List<Texture> PrePackTextures = new List<Texture>();

    //预打包的图片
    [NonSerialized]
    public List<string> PrePackTexturesMD5 = new List<string>();
    //图集信息
    public List<UnityPackSprite> TexturePackSprite = new List<UnityPackSprite>();

    //间隙
    public int Padding = 0;

    //剔除透明
    public bool TrimAlpha;

    //剔除相似
    public bool TrimSimilar = true;

    //宽
    public int Width;

    //高
    public int Height;

    //最大尺寸
    public int MaxSize;

    //用于剔除相似
    [NonSerialized]
    public Dictionary<string,UnityPackSprite> PackTexturesMD5Dic = new Dictionary<string, UnityPackSprite>();
}
