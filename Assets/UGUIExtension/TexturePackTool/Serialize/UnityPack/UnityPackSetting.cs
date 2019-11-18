using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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

    //强制方形
    public bool ForeSquare = true;

    //宽
    public int Width;

    //高
    public int Height;

    //最大尺寸
    public int MaxSize = 4096;


    public SpriteMetaData[] GetSpriteMetaData()
    {
        if (Atlas == null || TexturePackSprite == null || TexturePackSprite.Count <= 0)
            return null;

        SpriteMetaData[] spriteMetaDatas = new SpriteMetaData[TexturePackSprite.Count];

        for (int index = 0; index < TexturePackSprite.Count; index++)
        {
            SpriteMetaData data = new SpriteMetaData();
            UnityPackSprite sprite = TexturePackSprite[index];
            data.rect = new Rect(sprite.x,sprite.y,sprite.width,sprite.height);
            data.alignment = (int)SpriteAlignment.Custom;
            data.border = Vector4.zero;
            data.pivot = sprite.pivot;
            data.name = sprite.filename;
            spriteMetaDatas[index] = data;
        }

        return spriteMetaDatas;

    }
}
