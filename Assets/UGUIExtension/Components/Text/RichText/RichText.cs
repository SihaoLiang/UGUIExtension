using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 图文混拼
/// </summary>
 [RequireComponent(typeof(RichTextSpriteRenderer))]
public class RichText : Text, ILayoutGroup, IPointerClickHandler
{
    private DrivenRectTransformTracker m_Tracker;

    /// <summary>
    /// 图集数据
    /// </summary>
    [SerializeField]
    public RichTextSpriteAsset m_SpriteAsset;
    public RichTextSpriteAsset spriteAsset
    {
        get { return m_SpriteAsset; }
        set
        {
            if (m_SpriteAsset == null)
            {
                m_SpriteAsset = value;
                if (mRichTextSpriteRender != null)
                    mRichTextSpriteRender.SetSpriteAsset(m_SpriteAsset);
            }
        }
    }


    /// <summary>
    /// 表情渲染
    /// </summary>
    [SerializeField]
    public RichTextSpriteRenderer mRichTextSpriteRender;
    protected RichTextSpriteRenderer richTextSpriteRender
    {
        get { return mRichTextSpriteRender; }
        set { mRichTextSpriteRender = value; }
    }


    /// <summary>
    /// 存储占位符信息,用于绘制图片
    /// </summary>
    private Dictionary<int, QuadPlaceholder> m_QuadPlaceHolderInfos = new Dictionary<int, QuadPlaceholder>();

    /// <summary>
    /// 存储图片纹理信息,用于绘制图片
    /// </summary>
    private List<TextMeshInfo> m_TextureUvInfos;

    /// <summary>
    /// 处理字符串
    /// </summary>
    StringBuilder m_Builder = new StringBuilder();
    string m_Content = string.Empty;

    /// <summary>
    /// 下划线
    /// </summary>
    private readonly List<UnderLineInfo> m_UnderLineInfos = new List<UnderLineInfo>();


    /// <summary>
    /// 存储超文本信息,用于点击判断
    /// </summary>
    private readonly List<LinkInfo> m_LinkInfos = new List<LinkInfo>();


    /// <summary>
    /// 超文本点击监听
    /// </summary>
    private Action<string> m_LinkerListener;
    public Action<string> linkerListener
    {
        get { return m_LinkerListener; }
        set { m_LinkerListener = value; }
    }


    protected override void OnEnable()
    {
        base.OnEnable();

        alignByGeometry = true;
        m_TextureUvInfos = new List<TextMeshInfo>();
        SetupTextRender();
    }


    protected override void OnDisable()
    {
        base.OnDisable();
        if (richTextSpriteRender != null)
            richTextSpriteRender.Clear();

        m_Tracker.Clear();
    }

    /// <summary>
    /// 重写，否则定点数不对，内部用到时m_Text，我们用的是解释后的m_Content
    /// </summary>
    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(m_Content, settings) / pixelsPerUnit;
        }
    }

    /// <summary>
    /// 重写，否则顶点数不对
    /// </summary>
    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(GetPixelAdjustedRect().size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(m_Content, settings) / pixelsPerUnit;
        }
    }

    /// <summary>
    /// 更新顶点
    /// </summary>
    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();
        Parser();
        if (richTextSpriteRender != null)
            richTextSpriteRender.UpdateMesh();
    }

    /// <summary>
    /// 解析
    /// </summary>
    void Parser()
    {

        m_Content = text;

        m_Content = ParsingUnderLine(m_Content);
        m_Content = ParsingLinker(m_Content);

        ParsingSprite(m_Content);
    }

    public void SetLayoutHorizontal()
    {
        m_Tracker.Clear();
        if (mRichTextSpriteRender != null)
        {
            m_Tracker.Add(this, mRichTextSpriteRender.rectTransform,
                DrivenTransformProperties.All);

            // Make view full size to see if content fits.
            mRichTextSpriteRender.rectTransform.anchorMin = Vector2.zero;
            mRichTextSpriteRender.rectTransform.anchorMax = Vector2.one;
            mRichTextSpriteRender.rectTransform.sizeDelta = Vector2.zero;
            mRichTextSpriteRender.rectTransform.anchoredPosition = Vector2.zero;
            mRichTextSpriteRender.rectTransform.localScale = Vector2.one;
            mRichTextSpriteRender.rectTransform.pivot = rectTransform.pivot;


        }
    }

    /// <summary>
    /// 添加渲染器
    /// </summary>
    protected void SetupTextRender()
    {
        if (spriteAsset == null)
            return;

        mRichTextSpriteRender = GetComponentInChildren<RichTextSpriteRenderer>();

        if (mRichTextSpriteRender == null)
        {
            GameObject textExRender = new GameObject("TextExRender", typeof(RichTextSpriteRenderer));
            textExRender.transform.SetParent(transform);
            mRichTextSpriteRender = textExRender.GetComponent<RichTextSpriteRenderer>();
            mRichTextSpriteRender.SetSpriteAsset(spriteAsset);
        }

        mRichTextSpriteRender.InitCanvasRenderer();
    }


    /// <summary>
    /// 解析精灵和精灵动画占位符信息
    /// </summary>
    /// <param name="str">字符串</param>
    public void ParsingSprite(string str)
    {
        //解析标签属性
        m_QuadPlaceHolderInfos.Clear();
        foreach (Match match in RichTextConst.SpriteRegex.Matches(str))
        {
            QuadPlaceholder teamQuadInfo = new QuadPlaceholder();
            teamQuadInfo.isAnimate = false;
            teamQuadInfo.sprite = match.Groups[1].Value;
            teamQuadInfo.length = match.Length;
            teamQuadInfo.index = match.Index * 4;
            teamQuadInfo.size = new Vector2(fontSize, fontSize);
            m_QuadPlaceHolderInfos.Add(teamQuadInfo.index, teamQuadInfo);
        }

        foreach (Match match in RichTextConst.AnimateRegex.Matches(str))
        {
            QuadPlaceholder teamQuadInfo = new QuadPlaceholder();
            teamQuadInfo.isAnimate = true;
            teamQuadInfo.sprite = "1";
            teamQuadInfo.rate = int.Parse(match.Groups[2].Value);
            teamQuadInfo.animateName = match.Groups[1].Value;
            teamQuadInfo.length = match.Length;
            teamQuadInfo.index = match.Index * 4;
            teamQuadInfo.size = new Vector2(fontSize, fontSize);
            m_QuadPlaceHolderInfos.Add(teamQuadInfo.index, teamQuadInfo);
        }
    }


    /// <summary>
    /// 下划线解析器
    /// </summary>
    /// <param name="str">原字符串</param>
    /// <returns></returns>
    public string ParsingUnderLine(string content)
    {
        m_UnderLineInfos.Clear();

        int index = 0; //处理的位置索引
        string combineText = string.Empty;
        //取出下划线位置
        foreach (Match match in RichTextConst.UnderLineRegex.Matches(content))
        {
            //前半截
            combineText += content.Substring(index, match.Index - index);
            //下划线的内容
            string tempStr = match.Groups[1].Value;

            //信息
            var _underLineInfo = new UnderLineInfo
            {
                startIndex = combineText.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (tempStr.Length + combineText.Length) * 4, //终点索引
            };
            m_UnderLineInfos.Add(_underLineInfo);

            //拼接内容
            combineText += tempStr;
            index = match.Index + match.Length;
        }
        //拼接后半截
        combineText += content.Substring(index, content.Length - index);

        return combineText;
    }


    /// <summary>
    /// 超链接解析器
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public string ParsingLinker(string content)
    {
        var index = 0;
        string combineText = string.Empty;
        m_LinkInfos.Clear();
        foreach (Match match in RichTextConst.LinkRegex.Matches(content))
        {
            combineText += content.Substring(index, match.Index - index);

            //超链接的内容
            string tempStr = match.Groups[2].Value;
            var linkInfo = new LinkInfo
            {
                id = match.Groups[1].Value,
                startIndex = combineText.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (tempStr.Length + combineText.Length) * 4, //终点索引
                content = tempStr
            };

            m_LinkInfos.Add(linkInfo);

            //拼接内容
            combineText += tempStr;
            index = match.Index + match.Length;
        }
        combineText += content.Substring(index, content.Length - index);
        return combineText;
    }


    readonly UIVertex[] m_TempVerts = new UIVertex[4];

    /// <summary>
    /// 重写绘制
    /// </summary>
    /// <param name="toFill">模型</param>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;

        // We don't care if we the font Texture changes while we are doing our Update.
        // The end result of cachedTextGenerator will be valid for this instance.
        // Otherwise we can get issues like Case 619238.
        m_DisableFontTextureRebuiltCallback = true;

        Vector2 extents = rectTransform.rect.size;

        var settings = GetGenerationSettings(extents);
        bool error = cachedTextGenerator.PopulateWithErrors(m_Content, settings, gameObject);

        // Apply the offset to the vertices
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        //Last 4 verts are always a new line... (\n)
        int vertCount = verts.Count - 4;

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();

        //偏移情况，目前不考虑
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {   //绘制表情替换占位符的顶点
            m_TextureUvInfos.Clear();
            mRichTextSpriteRender.Clear();
            for (int idx = 0; idx < vertCount; idx++)
            {
                QuadPlaceholder info = null;
                if (mRichTextSpriteRender != null && supportRichText && m_QuadPlaceHolderInfos.TryGetValue(idx, out info))
                {
                    //占位符的其实定点在左上角，+3 第四个顶点在左下角
                    if (verts.Count <= info.index + 3)
                        continue;

                    idx = mRichTextSpriteRender.GenerateVertices(info, verts[info.index + 3].position, idx, unitsPerPixel);
                }
                else
                {   //继续绘制字体
                    int tempVertsIndex = idx & 3;
                    m_TempVerts[tempVertsIndex] = verts[idx];
                    m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                    if (tempVertsIndex == 3)
                        toFill.AddUIVertexQuad(m_TempVerts);
                }
            }
        }

        m_DisableFontTextureRebuiltCallback = false;

        if (!supportRichText)
            return;

        //绘制下划线
        if (m_UnderLineInfos != null && m_UnderLineInfos.Count > 0)
            OnDrawUnderLine(toFill, settings, verts);

        if (m_LinkInfos != null && m_LinkInfos.Count > 0)
        {
            OnDealLinkerBoundsBox(toFill, verts);
        }

        //网格重绘
        if (spriteAsset != null && m_TextureUvInfos != null)
        {
            mRichTextSpriteRender.Draw();
        }
    }

    /// <summary>
    ///  处理下划线
    /// </summary>
    protected void OnDrawUnderLine(VertexHelper toFill, TextGenerationSettings settings, IList<UIVertex> verts)
    {
        //处理超链接的下划线--拉伸实现
        TextGenerator genarator = new TextGenerator();
        genarator.Populate("——", settings);
        IList<UIVertex> underLineVertexs = genarator.verts; //8顶点   //Last 4 verts are always a new line... (\n)

        foreach (var underLine in m_UnderLineInfos)
        {
            if (underLine.startIndex >= verts.Count || underLine.endIndex >= verts.Count)
            {
                continue;
            }

            UIVertex startVert = verts[underLine.startIndex + 3]; //开始顶点
            UIVertex endVert = verts[underLine.endIndex];//结束顶点

            //在下方绘制下划线
            AddUnderlineQuad(toFill, underLineVertexs, startVert.position, endVert.position, startVert.color);
        }

    }


    /// <summary>
    ///  处理超链接的包围盒
    /// </summary>
    protected void OnDealLinkerBoundsBox(VertexHelper toFill, IList<UIVertex> verts)
    {
        // 处理超链接包围框
        UIVertex vert = new UIVertex();
        foreach (var hypefInfo in m_LinkInfos)
        {
            hypefInfo.boxes.Clear();
            if (hypefInfo.startIndex > verts.Count || hypefInfo.endIndex >= verts.Count)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            vert = verts[hypefInfo.startIndex]; //开始顶点

            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hypefInfo.startIndex, m = hypefInfo.endIndex; i < m; i++)
            {
                if (i >= verts.Count)
                    break;

                vert = verts[i];
                pos = vert.position;

                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {
                    hypefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }
            hypefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
        }
    }

    Vector3[] _underlinePos = new Vector3[4];

    /// <summary>
    /// 添加下划线
    /// </summary>
    /// <param name="toFill">模型</param>
    /// <param name="vertexs">顶点</param>
    /// <param name="starPos">开始坐标</param>
    /// <param name="endPos">结束坐标</param>
    void AddUnderlineQuad(VertexHelper toFill, IList<UIVertex> vertexs, Vector3 starPos, Vector3 endPos, Color32 color)
    {
        //厚度 0.1f * fontSize
        _underlinePos[0] = new Vector3(starPos.x, -fontSize * 0.5f, 0); ;
        _underlinePos[1] = new Vector3(endPos.x, -fontSize * 0.5f, 0); ;
        _underlinePos[2] = new Vector3(endPos.x, fontSize * -0.6f, 0);
        _underlinePos[3] = new Vector3(starPos.x, fontSize * -0.6f, 0);

        for (int i = 0; i < 4; ++i)
        {
            int tempVertsIndex = i & 3;
            m_TempVerts[tempVertsIndex] = vertexs[i % 4];
            m_TempVerts[tempVertsIndex].color = color;
            m_TempVerts[tempVertsIndex].position = _underlinePos[i];

            if (tempVertsIndex == 3)
                toFill.AddUIVertexQuad(m_TempVerts);
        }
    }

    public void SetLayoutVertical()
    {

    }

    protected override void OnDestroy()
    {
        mRichTextSpriteRender.Clear();
    }
    /// <summary>
    /// 点击到超链接
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localPos);

        foreach (var hrefInfo in m_LinkInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(localPos))
                {

                    Debug.Log(hrefInfo.id);
                    if (linkerListener != null)
                        linkerListener.Invoke(hrefInfo.id);

                    return;
                }
            }
        }
    }
}