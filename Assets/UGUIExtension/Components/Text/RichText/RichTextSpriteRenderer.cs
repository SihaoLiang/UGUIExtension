using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RendererInfo
{
    public TextMeshInfo uvInfo;
    public QuadPlaceholder placeholder;
    public int curFrame = 0;
    public float timeline = 0;
}

[RequireComponent(typeof(CanvasRenderer))]
public class RichTextSpriteRenderer : MaskableGraphic
{
    /// <summary>
    /// 精灵网格
    /// </summary>
    Mesh m_SpriteMesh;

    [SerializeField]
    private RichTextSpriteAsset m_TextSpriteAssett;
    public RichTextSpriteAsset textSpriteAsset
    {
        get { return m_TextSpriteAssett; }
        set { m_TextSpriteAssett = value; }
    }

    /// <summary>
    /// 纹理
    /// </summary>
    public override Texture mainTexture
    {
        get
        {
            if (textSpriteAsset == null)
                return s_WhiteTexture;
            else
                return textSpriteAsset.spriteSheet;
        }

    }

    protected override void OnEnable()
    {
        UpdateMaterial();
        UpdateMesh();
    }

    /// <summary>
    /// 材质球
    /// </summary>
    public override Material material
    {
        get
        {
            if (textSpriteAsset == null)
                return base.material;
            else
                return textSpriteAsset.material;
        }
    }

    /// <summary>
    /// 当前渲染中的纹理信息
    /// </summary>
    List<RendererInfo> m_RendererInfo = null;

    /// <summary>
    /// 初始化
    /// </summary>
    public void InitCanvasRenderer()
    {
        m_SpriteMesh = new Mesh();
        m_SpriteMesh.hideFlags = HideFlags.HideAndDontSave;
        m_RendererInfo = new List<RendererInfo>();
    }

    /// <summary>
    /// 设置图集
    /// </summary>
    /// <param name="spriteAsset"></param>
    public void SetSpriteAsset(RichTextSpriteAsset spriteAsset)
    {
        m_TextSpriteAssett = spriteAsset;
    }

    /// <summary>
    /// 更新网格
    /// </summary>
    public void UpdateMesh()
    {
        if (!isActiveAndEnabled)
            return;

        if (m_SpriteMesh != null)
        {
            canvasRenderer.SetMesh(m_SpriteMesh);
        }
    }

    /// <summary>
    /// 覆盖基类重绘方法，防止图集自动重绘出现整张图集，具体看源码
    /// </summary>
    protected override void UpdateGeometry()
    {

    }

    /// <summary>
    /// 清理网格
    /// </summary>
    public void Clear()
    {
        if (m_RendererInfo != null)
            m_RendererInfo.Clear();

        m_TempVertices.Clear();
        m_TempUv.Clear();
        m_TempTriangles.Clear();

        m_SpriteMesh.Clear();
        canvasRenderer.SetMesh(m_SpriteMesh);
    }

    /// <summary>
    /// 生成uv信息
    /// </summary>
    /// <param name="info"></param>
    /// <param name="startVertex"></param>
    /// <param name="unitsPerPixel"></param>
    /// <returns></returns>
    public int GenerateVertices(QuadPlaceholder info, Vector3 postion, int startVertex, float unitsPerPixel)
    {
        TextMeshInfo tempUv = new TextMeshInfo();
        tempUv.startPos = postion * unitsPerPixel;

        //设置图片的位置
        tempUv.vertices = new Vector3[4];
        tempUv.vertices[0] = new Vector3(0, 0, 0) + tempUv.startPos;
        tempUv.vertices[1] = new Vector3(info.size.x, info.size.y, 0) + tempUv.startPos;
        tempUv.vertices[2] = new Vector3(info.size.x, 0, 0) + tempUv.startPos;
        tempUv.vertices[3] = new Vector3(0, info.size.y, 0) + tempUv.startPos;


        TexturePackSprite sprite = null;
        if (info.isAnimate)
        {
            RichTextAnimate textAnimate = textSpriteAsset.GetAnimateListByName(info.animateName);
            if (textAnimate != null)
                sprite = textAnimate.spriteList[0];
        }
        else if (textSpriteAsset.spriteInfoList != null)
        {
            int index = int.Parse(info.sprite);
            if (textSpriteAsset.spriteInfoList.Count > index)
                sprite = textSpriteAsset.spriteInfoList[index];
        }

        Vector2 texSize = Vector2.zero;
        if (sprite == null)
        {
            startVertex += 4 * info.length - 1;
            return startVertex;
        }

        //计算其uv
        texSize = new Vector2(textSpriteAsset.spriteSheet.width, textSpriteAsset.spriteSheet.height);
        tempUv.uv = new Vector2[4];

        if (!sprite.rotated)
        {
            tempUv.uv[0] = new Vector2(sprite.x / texSize.x, sprite.y / texSize.y);
            tempUv.uv[1] = new Vector2((sprite.x + sprite.width) / texSize.x, (sprite.y + sprite.width) / texSize.y);
            tempUv.uv[2] = new Vector2((sprite.x + sprite.width) / texSize.x, sprite.y / texSize.y);
            tempUv.uv[3] = new Vector2(sprite.x / texSize.x, (sprite.y + sprite.width) / texSize.y);
        }
        else
        {
            tempUv.uv[0] = new Vector2(sprite.x / texSize.x, sprite.y / texSize.y);
            tempUv.uv[1] = new Vector2((sprite.x + sprite.width) / texSize.x, (sprite.y + sprite.width) / texSize.y);
            tempUv.uv[2] = new Vector2((sprite.x + sprite.width) / texSize.x, sprite.y / texSize.y);
            tempUv.uv[3] = new Vector2(sprite.x / texSize.x, (sprite.y + sprite.width) / texSize.y);
        }
        //声明三角顶点所需要的数组
        tempUv.triangles = new int[6];
        startVertex += 4 * info.length - 1;

        RendererInfo rendererInfo = new RendererInfo();
        rendererInfo.uvInfo = tempUv;
        rendererInfo.placeholder = info;
        m_RendererInfo.Add(rendererInfo);

        return startVertex;
    }


    /// <summary>
    /// 生成新的UV，用帧动画
    /// </summary>
    /// <param name="sprite"></param>
    /// <param name="tempUv"></param>
    /// <returns></returns>
    protected TextMeshInfo GenerateNewUv(TexturePackSprite sprite, ref TextMeshInfo tempUv)
    {
        //TexUvInfo tempUv = new TexUvInfo();
        //计算其uv
        Vector2 texSize = Vector2.zero;
        texSize = new Vector2(textSpriteAsset.spriteSheet.width, textSpriteAsset.spriteSheet.height);

        tempUv.uv[0] = new Vector2(sprite.x / texSize.x, sprite.y / texSize.y);
        tempUv.uv[1] = new Vector2((sprite.x + sprite.width) / texSize.x, (sprite.y + sprite.height) / texSize.y);
        tempUv.uv[2] = new Vector2((sprite.x + sprite.width) / texSize.x, sprite.y / texSize.y);
        tempUv.uv[3] = new Vector2(sprite.x / texSize.x, (sprite.y + sprite.height) / texSize.y);

        return tempUv;
    }


    /// <summary>
    /// 帧动画更新
    /// </summary>
    public void LateUpdate()
    {
        for (int index = 0; index < m_RendererInfo.Count; index++)
        {
            RendererInfo rendererInfo = m_RendererInfo[index];
            if (rendererInfo.placeholder.isAnimate)
            {
                rendererInfo.timeline += Time.deltaTime;
                if (rendererInfo.timeline >= (1 / (float)rendererInfo.placeholder.rate))
                {
                    int frame = rendererInfo.curFrame + 1;
                    string animateName = rendererInfo.placeholder.animateName;
                    RichTextAnimate richTextAnimate = textSpriteAsset.GetAnimateListByName(animateName);
                    List<TexturePackSprite> sprites = richTextAnimate.spriteList;

                    if (frame >= sprites.Count)
                        frame = 0;

                    GenerateNewUv(sprites[frame], ref rendererInfo.uvInfo);
                    rendererInfo.timeline = 0.0f;
                    rendererInfo.curFrame = frame;
                    PlayAnimate();
                }
            }
        }
    }

    /// <summary>
    /// 绘制当前图集信息
    /// </summary>
    public void PlayAnimate()
    {
        int len = 0;
        for (int index = 0; index < m_RendererInfo.Count; index++)
        {
            if (m_RendererInfo[index] != null)
            {
                Vector2[] uv = m_RendererInfo[index].uvInfo.uv;
                for (int j = 0; j < uv.Length; j++)
                {
                    m_TempUvArray[len] = uv[j];
                    len++;
                }
            }
        }
        RefreshMesh();
    }


    protected override void OnDestroy()
    {
        Clear();
        m_SpriteMesh = null;
    }

    /// <summary>
    /// 绘制图片
    /// </summary>
    public void RefreshMesh()
    {
        if (m_SpriteMesh == null)
            return;

        if (m_SpriteMesh != null)
        {
            m_SpriteMesh.uv = m_TempUvArray;
            canvasRenderer.SetMesh(m_SpriteMesh);
        }
    }

    /// <summary>
    /// 绘制当前图集信息
    /// </summary>
    public void Draw()
    {
        List<TextMeshInfo> uvs = new List<TextMeshInfo>();
        for (int index = 0; index < m_RendererInfo.Count; index++)
        {
            if (m_RendererInfo[index] != null)
                uvs.Add(m_RendererInfo[index].uvInfo);
        }
        DrawMesh(uvs);
    }


    List<Vector3> m_TempVertices = new List<Vector3>();
    List<Vector2> m_TempUv = new List<Vector2>();
    List<int> m_TempTriangles = new List<int>();
    Vector2[] m_TempUvArray;


    /// <summary>
    /// 绘制图片
    /// </summary>
    public void DrawMesh(List<TextMeshInfo> uvList)
    {
        m_SpriteMesh.Clear();

        for (int i = 0; i < uvList.Count; i++)
        {
            for (int j = 0; j < uvList[i].vertices.Length; j++)
            {
                m_TempVertices.Add(uvList[i].vertices[j]);
            }
            for (int j = 0; j < uvList[i].uv.Length; j++)
            {
                m_TempUv.Add(uvList[i].uv[j]);
            }
            for (int j = 0; j < uvList[i].triangles.Length; j++)
            {
                m_TempTriangles.Add(uvList[i].triangles[j]);
            }
        }
        //计算顶点绘制顺序
        for (int i = 0; i < m_TempTriangles.Count; i++)
        {
            if (i % 6 == 0)
            {
                int num = i / 6;
                m_TempTriangles[i] = 0 + 4 * num;
                m_TempTriangles[i + 1] = 1 + 4 * num;
                m_TempTriangles[i + 2] = 2 + 4 * num;

                m_TempTriangles[i + 3] = 1 + 4 * num;
                m_TempTriangles[i + 4] = 0 + 4 * num;
                m_TempTriangles[i + 5] = 3 + 4 * num;
            }
        }

        if (m_SpriteMesh == null)
            return;

        m_TempUvArray = new Vector2[m_TempUv.Count];

        m_SpriteMesh.vertices = m_TempVertices.ToArray();
        m_SpriteMesh.uv = m_TempUv.ToArray();
        m_SpriteMesh.triangles = m_TempTriangles.ToArray();

        UpdateMesh();
    }
}
