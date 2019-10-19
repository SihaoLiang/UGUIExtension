using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UguiFrameAnimation : MaskableGraphic, ILayoutElement
{
    [Tooltip("所有精灵")]
    [SerializeField]
    Sprite[] m_Sprites;

    [Tooltip("动画时间")]
    [SerializeField]
    [Range(0.1f, 10f)]
    float m_Duration = 1f;

    [Tooltip("循环")]
    [SerializeField]
    public bool m_Loop = true;

    [Tooltip("反向")]
    [SerializeField]
    public bool m_Reverce = false;

    [Tooltip("自动播放")]
    [SerializeField]
    public bool m_AutoPlay = true;

    bool m_IsPlaying = false;

    int m_Index = 0;
    int m_PreIndex = -1;
    float m_ElapsedTime = 0;
    int m_TotalFrame = 0;

    Mesh m_SpriteMesh;

    //当前帧的顶点和UV
    Vector3[] m_Vertices = new Vector3[4];
    Vector2[] m_Uv = new Vector2[4];
    int[] m_Triangles = new int[] { 0, 1, 2, 2, 3, 0 };

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_Sprites != null && m_Sprites.Length > 0)
        {
            m_TotalFrame = m_Sprites.Length;

            if (m_SpriteMesh == null)
                m_SpriteMesh = new Mesh();

            m_SpriteMesh.Clear();
            m_Index = 0;
        }

        if (m_AutoPlay)
            Play();
    }

    public bool IsPlaying()
    {
        return m_IsPlaying;
    }

    /// <summary>
    /// 绘制
    /// </summary>
    /// <param name="vh"></param>
    protected override void OnPopulateMesh(VertexHelper vh)
    {

        if (mainTexture == null)
        {
            base.OnPopulateMesh(vh);
            return;
        }

        m_SpriteMesh.vertices = GetVertices();
        m_SpriteMesh.triangles = m_Triangles;
        m_SpriteMesh.uv = GetSpriteUV();
        Color32 color32 = color;

        vh.Clear();
        vh.AddVert(m_Vertices[0], color32, m_Uv[0]);
        vh.AddVert(m_Vertices[1], color32, m_Uv[1]);
        vh.AddVert(m_Vertices[2], color32, m_Uv[2]);
        vh.AddVert(m_Vertices[3], color32, m_Uv[3]);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }


    /// <summary>
    /// 获取顶点
    /// </summary>
    /// <returns></returns>
    protected Vector3[] GetVertices()
    {
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

        m_Vertices[0] = new Vector3(v.x, v.y);
        m_Vertices[1] = new Vector3(v.x, v.w);
        m_Vertices[2] = new Vector3(v.z, v.w);
        m_Vertices[3] = new Vector3(v.z, v.y);

        return m_Vertices;
    }

    /// <summary>
    /// 获取uv
    /// </summary>
    /// <returns></returns>
    protected Vector2[] GetSpriteUV(Sprite sp = null)
    {
        if (sp == null && m_Sprites != null && m_Sprites.Length > m_Index && m_Sprites[m_Index] != null)
            sp = m_Sprites[m_Index];

        var uv = (sp != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(sp) : Vector4.zero;

        m_Uv[0] = new Vector2(uv.x, uv.y);
        m_Uv[1] = new Vector2(uv.x, uv.w);
        m_Uv[2] = new Vector2(uv.z, uv.w);
        m_Uv[3] = new Vector2(uv.z, uv.y);

        return m_Uv;
    }

    /// <summary>
    /// 切换帧
    /// </summary>
    private void LateUpdate()
    {

        if (!isActiveAndEnabled)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        if (m_Sprites == null || m_Sprites.Length == 0)
            return;

        Play(Time.deltaTime);
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Play(float deltaTime)
    {

        if (!IsPlaying())
            return;

        m_ElapsedTime += deltaTime;

        float percentage = m_ElapsedTime / m_Duration;

        if (m_Reverce)
            m_Index = (int)(m_TotalFrame * (1 - percentage));
        else
            m_Index = (int)(m_TotalFrame *  percentage);


        float leftTime = m_ElapsedTime - m_Duration;

        if (m_PreIndex != m_Index)
        {
            m_PreIndex = m_Index;

            if (leftTime >= 0)
            {
                if (m_Loop)
                {
                    m_ElapsedTime = 0;
                    Play(leftTime);
                }
                else
                {
                    Stop();
                }
            }
            else if (m_Sprites.Length > m_Index && m_Sprites[m_Index] != null)
            {
                GetSpriteUV(m_Sprites[m_Index]);
                DrawMesh();
            }
        }

       
    }

    /// <summary>
    /// 重绘网格
    /// </summary>
    void DrawMesh()
    {
        if (m_SpriteMesh != null && m_SpriteMesh.vertexCount == 4)
        {
            m_SpriteMesh.uv = m_Uv;
            canvasRenderer.SetMesh(m_SpriteMesh);
        }
    }

    [ContextMenu("播放")]
    public void Play()
    {
        m_IsPlaying = true;
    }


    [ContextMenu("暂停")]
    public void Pause()
    {
        m_IsPlaying = false;
    }

    [ContextMenu("恢复")]
    public void Resume()
    {
        m_IsPlaying = true;
    }

    [ContextMenu("重置")]
    public void Rewind()
    {
        m_Index = 0;
        m_PreIndex = -1;
    }

    [ContextMenu("停止")]
    public void Stop()
    {
        m_Index = 0;
        m_ElapsedTime = 0;
        m_PreIndex = -1;
        m_IsPlaying = false;
    }

    [ContextMenu("自适应大小")]
    public override void SetNativeSize()
    {
        if (m_Sprites == null || m_Sprites.Length <= m_Index)
            return;

        Sprite activeSprite = m_Sprites[m_Index];

        if (activeSprite != null)
        {
            float w = activeSprite.rect.width / pixelsPerUnit;
            float h = activeSprite.rect.height / pixelsPerUnit;
            rectTransform.anchorMax = rectTransform.anchorMin;
            rectTransform.sizeDelta = new Vector2(w, h);
            SetAllDirty();
        }
    }

    /// <summary>
    /// Image's texture comes from the UnityEngine.Image.
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (m_Sprites == null || m_Sprites.Length <= 0 || m_Sprites[0] == null || m_Sprites[0].texture == null) 
            {
                if (material != null && material.mainTexture != null)
                {
                    return material.mainTexture;
                }
                return s_WhiteTexture;
            }

            return m_Sprites[0].texture;
        }
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
            return rectTransform.rect.size.y;

        }
    }

    public virtual float flexibleWidth { get { return -1; } }


    public float pixelsPerUnit
    {
        get
        {
            float spritePixelsPerUnit = 100;
            if (m_Sprites != null && m_Sprites.Length > m_Index)
                spritePixelsPerUnit = m_Sprites[m_Index].pixelsPerUnit;

            float referencePixelsPerUnit = 100;
            if (canvas)
                referencePixelsPerUnit = canvas.referencePixelsPerUnit;

            return spritePixelsPerUnit / referencePixelsPerUnit;
        }
    }

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
            return rectTransform.rect.size.x;
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_SpriteMesh != null)
        {
            m_SpriteMesh.Clear();
            Object.DestroyImmediate(m_SpriteMesh);
        }
        m_SpriteMesh = null;
    }
}
