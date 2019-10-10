using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FontElement
{
    public float height;
    public int id;
    public float scale;
    public float width;
    public float x;
    public float xAdvance;
    public float xOffset;
    public float y;
    public float yOffset;
}

[System.Serializable]
public class BMFontSprite : FontElement
{
    public int hashCode;
    public string name;
    public Vector2 pivot;
    public Sprite sprite;
    public string key;
    public int asciiCode;
    public string group;
    public Vector2 offest;

}
