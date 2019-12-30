using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BMFontText : MaskableGraphic, ILayoutElement
{

    /// <summary>
    /// 文本框
    /// </summary>
    [TextArea(3, 10)]
    [SerializeField]
    string m_Text = string.Empty;

    /// <summary>
    /// 文字资源
    /// </summary>
    [SerializeField]
    BMFontAsset m_FontAsset;

    /// <summary>
    /// 间距
    /// </summary>
    [SerializeField]
    float m_Gap = 0.1f;

    /// <summary>
    /// 包围盒
    /// </summary>
    Bounds m_ContentBounds;

    /// <summary>
    /// 当前使用的字体组
    /// </summary>
    [SerializeField]
    public string m_CurrentGroup;

    /// <summary>
    /// 分组信息
    /// </summary>
    [SerializeField]
    public List<string> m_FontGroups = new List<string>();
    Dictionary<string, Dictionary<string, BMFontSprite>> m_AllGroup = new Dictionary<string, Dictionary<string, BMFontSprite>>();

    /// <summary>
    /// 存储需要绘制的文字信息
    /// </summary>
    private readonly List<BMFontSprite> m_FontList = new List<BMFontSprite>();

    /// <summary>
    /// 存储需要绘制的UV信息
    /// </summary>
    private readonly List<TextureInfo> m_FontInfoList = new List<TextureInfo>();

    /// <summary>
    /// 绘制过程中不允许重建
    /// </summary>
    private bool m_DisableFontTextureRebuiltCallback = false;

    /// <summary>
    /// 字体资源
    /// </summary>
    public BMFontAsset font
    {
        get
        {
            return m_FontAsset;
        }
        set
        {
            if (m_FontAsset == value)
                return;


            m_FontAsset = value;

            SetAllDirty();
        }
    }



    /// <summary>
    /// 字体间距
    /// </summary>
    public float gap
    {
        get
        {
            return m_Gap;
        }
        set
        {
            if (m_Gap == gap)
                return;

            gap = value;
            SetAllDirty();
        }
    }


    public override void SetAllDirty()
    {
        SetVerticesDirty();
        SetLayoutDirty();
        SetMaterialDirty();

#if UNITY_EDITOR && (UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_IOS)
        if (m_FontAsset != null)
            UnityUtility.ReBindShader(material);
#endif
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        GroupingFontAsset();
        ParserText();
    }

    /// <summary>
    /// 主纹理从Shader获得
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (font == null || font.spriteSheet == null)
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return font.spriteSheet;
        }
    }

    /// <summary>
    /// 显示的Text
    /// </summary>
    public virtual string text
    {
        get
        {
            return m_Text;
        }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";

            }
            else if (m_Text != value)
            {
                m_Text = value;
            }
            SetAllDirty();
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetAllDirty();
    }
#endif

    protected override void OnEnable()
    {
        base.OnEnable();
        SetAllDirty();
    }

    /// <summary>
    /// 分组
    /// </summary>
    private void GroupingFontAsset()
    {

        m_FontGroups.Clear();
        m_AllGroup.Clear();

        if (font == null)
            return;

        if (font.spriteInfoList.Count <= 0)
            return;

        int len = font.spriteInfoList.Count;

        for (int index = 0; index < len; index++)
        {
            BMFontSprite sprite = font.spriteInfoList[index];
            string group = sprite.group;
            if (!m_AllGroup.ContainsKey(group))
            {
                m_FontGroups.Add(group);
                m_AllGroup.Add(group, new Dictionary<string, BMFontSprite>());
            }

            if(!m_AllGroup[group].ContainsKey(sprite.key))
                m_AllGroup[group].Add(sprite.key, sprite);
            else
            {
                Debug.LogError(string.Format("组{0},键值{1}冲突",group,sprite.key));
            }
        }

        if (!m_FontGroups.Contains(m_CurrentGroup) || m_CurrentGroup == string.Empty)
            m_CurrentGroup = m_FontGroups[0];

    }

    /// <summary>
    /// 解释字符串
    /// </summary>
    private void ParserText()
    {
        if(m_FontList != null)
            m_FontList.Clear();

        if (font == null)
            return;

        if (m_AllGroup.Count <= 0)
            GroupingFontAsset();

        if (m_CurrentGroup == string.Empty || !m_AllGroup.ContainsKey(m_CurrentGroup))
            m_CurrentGroup = font.spriteInfoList[0].group;


        if (string.IsNullOrEmpty(text))
            return;

        char[] charArr = text.ToCharArray();

        if (charArr.Length <= 0)
            return;

        Vector2 maxSize = Vector2.zero;

        for (int i = 0; i < charArr.Length; i++)
        {
            string s = charArr[i].ToString();
            if (m_AllGroup[m_CurrentGroup].ContainsKey(s))
            {
                BMFontSprite sprite = m_AllGroup[m_CurrentGroup][s];
                m_FontList.Add(sprite);
                maxSize = new Vector2(maxSize.x + sprite.width + gap, maxSize.y > sprite.height ? maxSize.y : sprite.height);
            }
        }

        m_ContentBounds = new Bounds(Vector3.zero, new Vector3(maxSize.x, maxSize.y, 0));
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        m_DisableFontTextureRebuiltCallback = true;

        GenTextureVerts();
        var color32 = color;

        vh.Clear();

        for (int index = 0; index < m_FontInfoList.Count; index++)
        {
            TextureInfo info = m_FontInfoList[index];
            vh.AddVert(info.vertices[0], color32, info.uv[0]);
            vh.AddVert(info.vertices[1], color32, info.uv[1]);
            vh.AddVert(info.vertices[2], color32, info.uv[2]);
            vh.AddVert(info.vertices[3], color32, info.uv[3]);

            int num = index * 4;

            vh.AddTriangle(0 + num, num + 1, num + 2);
            vh.AddTriangle(num + 1, num + 3, num);
        }

        m_DisableFontTextureRebuiltCallback = false;
    }


    public float minWidth
    {
        get
        {
            return 0;
        }
    }

    public float preferredWidth
    {
        get
        {
            return m_ContentBounds.size.x;
        }
    }

    public virtual float flexibleWidth { get { return -1; } }

    public float minHeight
    {
        get
        {
            return 0;
        }
    }

    public float preferredHeight
    {
        get
        {
            return m_ContentBounds.size.y;
        }
    }

    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return 0; } }


    public void CalculateLayoutInputHorizontal()
    {

    }

    public void CalculateLayoutInputVertical()
    {

    }

    /// <summary>
    /// 计算纹理信息
    /// </summary>
    public void GenTextureVerts()
    {
        m_FontInfoList.Clear();
        Vector3 position = Vector3.zero;
        for (int i = 0; i < m_FontList.Count; i++)
        {
            TextureInfo tempSprite = new TextureInfo();
            int index = i * 4 + 3;

            tempSprite.startPos = position;
            Vector2 pivot = rectTransform.pivot;
            tempSprite.startPos += new Vector3(-m_ContentBounds.size.x * pivot.x, -m_ContentBounds.size.y * pivot.y, 0.0f);

            //设置图片的位置
            tempSprite.vertices = new Vector3[4];
            tempSprite.vertices[0] = new Vector3(0, 0, 0) + tempSprite.startPos;
            tempSprite.vertices[1] = new Vector3(m_FontList[i].width, m_FontList[i].height, 0) + tempSprite.startPos;
            tempSprite.vertices[2] = new Vector3(m_FontList[i].width, 0, 0) + tempSprite.startPos;
            tempSprite.vertices[3] = new Vector3(0, m_FontList[i].height, 0) + tempSprite.startPos;

            //设置图片的uv位置
            Rect fontRect = new Rect(m_FontList[i].x, m_FontList[i].y, m_FontList[i].width, m_FontList[i].height);
            Vector2 texSize = font.Size;
            tempSprite.uv = new Vector2[4];
            tempSprite.uv[0] = new Vector2(fontRect.x / texSize.x, fontRect.y / texSize.y);
            tempSprite.uv[1] = new Vector2((fontRect.x + fontRect.width) / texSize.x, (fontRect.y + fontRect.height) / texSize.y);
            tempSprite.uv[2] = new Vector2((fontRect.x + fontRect.width) / texSize.x, fontRect.y / texSize.y);
            tempSprite.uv[3] = new Vector2(fontRect.x / texSize.x, (fontRect.y + fontRect.height) / texSize.y);

            position += new Vector3(m_FontList[i].width + m_Gap, 0, 0);
            tempSprite.triangles = new int[6];
            m_FontInfoList.Add(tempSprite);
        }
    }

    [System.Serializable]
    public class TextureInfo
    {
        //起点
        public Vector3 startPos;
        // 4 顶点 
        public Vector3[] vertices;
        //4 uv
        public Vector2[] uv;
        //6 三角顶点顺序
        public int[] triangles;
    }

}
