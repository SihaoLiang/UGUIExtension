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
    public Texture Atlas;

    //图集信息
    public List<UnityPackSprite> TexturePackSprite = new List<UnityPackSprite>();

    //间隙
    public int Padding = 0;

    //剔除透明
    public bool TrimAlpha = false;

    //剔除相似
    public bool TrimSimilar = true;

    //宽
    public int Width;

    //高
    public int Height;

    //最大尺寸
    public int MaxSize = 4096;
}
