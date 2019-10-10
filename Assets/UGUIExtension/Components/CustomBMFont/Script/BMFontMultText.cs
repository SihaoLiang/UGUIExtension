using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class FontData
{
    public FontData()
    {
        m_Text = string.Empty;
        m_CurrentGroup = string.Empty;
        valueDalegate = null;
    }

    /// <summary>
    /// 文本框
    /// </summary>
    public string m_Text = string.Empty;

    /// <summary>
    /// 当前使用的字体组
    /// </summary>
    public string m_CurrentGroup;
    public string group
    {
        get { return m_CurrentGroup; }
        set { m_CurrentGroup = value; }
    }

    /// <summary>
    /// 偏移
    /// </summary>
    Vector2 m_Offest = Vector2.zero;
    public Vector2 offest
    {
        get { return m_Offest; }
        set { m_Offest = value; }
    }

    /// <summary>
    /// 通知改变
    /// </summary>
    public delegate void ValueChageDelegate();
    public ValueChageDelegate valueDalegate = null;

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
                if (valueDalegate != null)
                    valueDalegate.Invoke();
            }
            else if (m_Text != value)
            {
                m_Text = value;
                if (valueDalegate != null)
                    valueDalegate.Invoke();
            }
        }
    }
}

public class BMFontMultText : MaskableGraphic, ILayoutElement
{

    /// <summary>
    /// 使用的字体组
    /// </summary>
    [SerializeField]
    List<FontData> m_FontDataList = new List<FontData>();
    public List<FontData> fontDataList
    {
        get { return m_FontDataList; }
        set { m_FontDataList = value; }
    }

    /// <summary>
    /// 文字资源
    /// </summary>
    [SerializeField]
    BMFontAsset m_FontAsset;

    /// <summary>
    /// 包围盒
    /// </summary>
    Bounds m_ContentBounds;

    /// <summary>
    /// 间距
    /// </summary>
    [SerializeField]
    float m_Gap = 0.1f;

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
    /// 存储文字偏移
    /// </summary>
    private readonly List<Vector2> m_OffestList = new List<Vector2>();


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

    public void OnFontAssetValueChanged()
    {
        this.SetVerticesDirty();
        this.SetLayoutDirty();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_FontDataList == null)
            m_FontDataList = new List<FontData>();

        SetAllDirty();

        if (m_FontDataList.Count == 0)
            AddFontGroup();
        else
            InitFontData();
    }

    protected void InitFontData() {
        for (int index = 0; index < m_FontDataList.Count; index++)
        {
            m_FontDataList[index].valueDalegate = OnFontAssetValueChanged;
        }
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
            m_AllGroup[group].Add(sprite.key, sprite);
        }

    }

    /// <summary>
    /// 解释字符串
    /// </summary>
    private void ParserText()
    {
        m_FontList.Clear();
        m_OffestList.Clear();

        Vector2 maxSize = Vector2.zero;

        if (m_FontDataList == null || m_FontDataList.Count == 0)
            return;

        if (font == null)
            return;

        for (int index = 0; index < m_FontDataList.Count; index++)
        {
            string text = m_FontDataList[index].text;

            if (string.IsNullOrEmpty(text))
                continue;

            if ((m_FontDataList[index].group == null || !m_AllGroup.ContainsKey(m_FontDataList[index].group)))
                m_FontDataList[index].group = null;

            if (m_FontGroups.Count > 0 && m_FontDataList[index].group == null)
                m_FontDataList[index].group = m_FontGroups[0];

            if (string.IsNullOrEmpty(m_FontDataList[index].group))
                continue;

            char[] charArr = text.ToCharArray();
            if (charArr.Length <= 0)
                return;

            for (int i = 0; i < charArr.Length; i++)
            {
                string s = charArr[i].ToString();
                if (m_AllGroup[m_FontDataList[index].group].ContainsKey(s))
                {
                    BMFontSprite sprite = m_AllGroup[m_FontDataList[index].group][s];
                    m_OffestList.Add(m_FontDataList[index].offest);
                    m_FontList.Add(sprite);
                    maxSize = new Vector2(maxSize.x + sprite.width + gap, maxSize.y > sprite.height ? maxSize.y : sprite.height);
                }
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

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetAllDirty();
    }
#endif

    protected override void UpdateGeometry()
    {
        base.UpdateGeometry();
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

            tempSprite.startPos = position + (Vector3)m_OffestList[i];
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



    /// <summary>
    /// 添加一个分组
    /// </summary>
    /// <param name="group"></param>
    public void AddFontGroup(string group = null)
    {
        if (group == null && m_FontGroups.Count > 0)
            group = m_FontGroups[0];

        FontData entry = new FontData();
        entry.group = group;
        entry.valueDalegate = OnFontAssetValueChanged;

        m_FontDataList.Add(entry);

        SetAllDirty();
    }

    /// <summary>
    /// 删除一个分组
    /// </summary>
    /// <param name="entry"></param>
    public void RemovedFontGroup(FontData entry)
    {
        if (!m_FontDataList.Contains(entry))
            return;

        m_FontDataList.Remove(entry);

        SetAllDirty();
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
