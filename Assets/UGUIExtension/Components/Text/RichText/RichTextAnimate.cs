using System.Collections.Generic;

[System.Serializable]
public class RichTextAnimate
{
    public string m_AnimateName;
    public int frameRate { set; get; }
    public int m_FrameCount { set; get; }

    public int frameCount {
        set
        {
            m_FrameCount = value;

        }
        get
        {
            return m_FrameCount;

        }
    }

    public List<TexturePackSprite> m_SpriteList;

    public RichTextAnimate()
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
