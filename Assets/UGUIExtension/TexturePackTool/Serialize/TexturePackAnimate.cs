using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class TexturePackAnimate
{
    public string m_AnimateName;
    public int frameRate { set; get; }
    public int frameCount { set; get; }

    public List<TexturePackSprite> m_SpriteList;

    public TexturePackAnimate()
    {
        frameRate = 26;
        frameCount = 0;
        m_SpriteList = new List<TexturePackSprite>();
    }

    public string animateName
    {
        get
        {
            return m_AnimateName;
        }

        set
        {
            m_AnimateName = value;
        }
    }

    public List<TexturePackSprite> spriteList
    {
        get
        {
            return m_SpriteList;
        }

        set
        {
            m_SpriteList = value;
        }
    }
}
