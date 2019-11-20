using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public struct SpriteFrame
{
    public float h;
    public float w;
    public float x;
    public float y;

    public override string ToString() { return string.Format("h:{0},w:{1},x:{2},y:{3}", h, w, x, y); }
}

[System.Serializable]
public class UnityPackSprite
{
    public string filename;
    public SpriteFrame frame;
    public Vector2 pivot;
    public bool rotated;
    public float width;
    public float height;
    public float x;
    public float y;
    public SpriteFrame spriteSourceSize;
    public bool trimmed;
    public int index;
    public Vector4 border;
}
