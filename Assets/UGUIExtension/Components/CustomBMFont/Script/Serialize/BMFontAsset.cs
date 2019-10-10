using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BMFontAsset : ScriptableObject
{
    public int hashCode;
    public Material material;
    public int materialHashCode;
    public List<BMFontSprite> spriteInfoList;
    public Texture spriteSheet;

    public BMFontAsset()
    {
        spriteInfoList = new List<BMFontSprite>();
    }

    public Vector2 Size
    {
        get
        {
            return new Vector2(spriteSheet.width, spriteSheet.height);
        }
    }
}
