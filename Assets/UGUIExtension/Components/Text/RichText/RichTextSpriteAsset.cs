using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RichTextSpriteAsset : ScriptableObject
{
    public int hashCode;

    /// <summary>
    /// 材质
    /// </summary>
    public Material m_Material;
    public Material material
    {
        get { return m_Material; }
        set { m_Material = value; }
    }

    /// <summary>
    /// 所有uv信息
    /// </summary>
    [SerializeField]
    private List<TexturePackSprite> m_SpriteInfoList;
    public List<TexturePackSprite> spriteInfoList
    {
        get { return m_SpriteInfoList; }
        set { m_SpriteInfoList = value; }
    }

    /// <summary>
    /// 所有动画信息
    /// </summary>
    [SerializeField]
    private List<RichTextAnimate> m_AnimateList;
    public List<RichTextAnimate> animateList
    {
        get { return m_AnimateList; }
        set
        {
            m_AnimateList = value;
        }
    }

    /// <summary>
    /// 主纹理
    /// </summary>
    public Texture m_SpriteSheet;
    public Texture spriteSheet
    {
        get { return m_SpriteSheet; }
        set { m_SpriteSheet = value; }
    }

    public RichTextSpriteAsset()
    {
        m_AnimateList = new List<RichTextAnimate>();
        m_SpriteInfoList = new List<TexturePackSprite>();
    }

    /// <summary>
    /// 获取动画所有帧
    /// </summary>
    /// <param name="animateName"></param>
    /// <returns></returns>
    public RichTextAnimate GetAnimateListByName(string animateName)
    {
        if (m_AnimateList == null)
            return null;

        RichTextAnimate textAnimate = null;

        for (int i = 0; i < m_AnimateList.Count; i++)
        {
            if (m_AnimateList[i].animateName == animateName)
            {
                textAnimate = m_AnimateList[i];
                break;
            }
        }

        return textAnimate;
    }


    /// <summary>
    /// 图集大小
    /// </summary>
    public Vector2 Size
    {
        get
        {
            if (spriteSheet == null)
                return Vector2.zero;
            return new Vector2(spriteSheet.width, spriteSheet.height);
        }
    }
}
