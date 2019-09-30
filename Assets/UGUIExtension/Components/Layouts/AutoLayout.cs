using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class AutoLayout : UIBehaviour,ILayoutElement, ILayoutGroup
{
    /// <summary>
    /// 子节点的对齐方式
    /// </summary>
    [SerializeField]
    protected TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;
    public TextAnchor childAlignment { get { return m_ChildAlignment; } set { SetProperty(ref m_ChildAlignment, value); } }


    /// <summary>
    /// 子节点相对于容器边缘的偏移
    /// </summary>
    [SerializeField]
    protected RectOffset m_Padding = new RectOffset();
    public RectOffset padding { get { return m_Padding; } set { SetProperty(ref m_Padding, value); } }

    /// <summary>
    /// 间隙
    /// </summary>
    [SerializeField]
    protected Vector2 m_Spacing = Vector2.zero;
    public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

    /// <summary>
    /// 是否忽略非激活的
    /// </summary>
    [SerializeField]
    protected bool m_IgnoreUnActive = true;
    public bool ignoreUnActive { get { return m_IgnoreUnActive; } set { SetProperty(ref m_IgnoreUnActive, value); } }


    /// <summary>
    /// 容器RectTransform
    /// </summary>
    [System.NonSerialized]
    protected RectTransform m_Rect;
    public RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }

    /// <summary>
    /// 用于存储子节点队列
    /// </summary>
    [System.NonSerialized]
    protected List<RectTransform> m_UsingChildren = new List<RectTransform>();
    public List<RectTransform> usingChildren { get { return m_UsingChildren; } set { m_UsingChildren = value; } }

    /// <summary>
    /// 用于存储所有子节点队列
    /// </summary>
    [System.NonSerialized]
    protected List<RectTransform> m_AllChildren = new List<RectTransform>();
    public List<RectTransform> allChildren { get { return m_AllChildren; } set { m_AllChildren = value; } }

  
    #region----------布局接口----------
    public virtual float minWidth
    {
        get
        {
            return 0;
        }
    }

    public virtual float preferredWidth
    {
        get
        {
            return rectTransform.rect.size.x;
        }
    }

    public virtual float flexibleWidth
    {
        get
        {
            return rectTransform.rect.size.x;
        }
    }

    public virtual float minHeight
    {
        get
        {
            return 0;
        }
    }

    public virtual float preferredHeight
    {
        get
        {
            return rectTransform.rect.size.y;
        }
    }

    public virtual float flexibleHeight
    {
        get
        {
            return rectTransform.rect.size.y;
        }
    }

    public virtual int layoutPriority { get { return 0; } }

    #endregion

    protected override void OnEnable()
    {
        base.OnEnable();
        SetDirty();
    }

    protected override void OnDisable()
    {
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        base.OnDisable();
    }


    /// <summary>
    /// 更新布局
    /// </summary>
    public virtual void SetDirty()
    {
        if (!isActiveAndEnabled)
            return;

        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    }

    /// <summary>
    /// 计算子节点个数。无过滤
    /// </summary>
    public virtual void CalculateLayoutChildren()
    {
        allChildren.Clear();

        //子节点列表
        for (int i = 0; i < rectTransform.childCount; i++)
        {
            var rect = rectTransform.GetChild(i) as RectTransform;
            //过滤非激活的
            if (rect == null)
                continue;
            allChildren.Add(rect);
        }
    }

    /// <summary>
    /// 计算子节点个数
    /// </summary>
    public virtual void CalculateUsingLayoutChildren()
    {
        m_UsingChildren.Clear();
        //子节点列表
        for (int i = 0; i < allChildren.Count; i++)
        {
            var rect = allChildren[i];// RectTransform;
            //过滤非激活的
            if (rect == null || (m_IgnoreUnActive && !rect.gameObject.activeInHierarchy))
                continue;

            var toIgnoreList = new List<Component>();

            //判断是否过滤布局
            rect.GetComponents(typeof(ILayoutIgnorer), toIgnoreList);

            if (toIgnoreList.Count == 0)
            {
                m_UsingChildren.Add(rect);
                continue;
            }
            //忽略多个，只要有一个不忽略就加入计算
            for (int j = 0; j < toIgnoreList.Count; j++)
            {
                var ignorer = (ILayoutIgnorer)toIgnoreList[j];
                if (!ignorer.ignoreLayout)
                {
                    m_UsingChildren.Add(rect);
                    break;
                }
            }
            toIgnoreList.Clear();
        }
    }

    /// <summary>
    /// 赋值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="currentValue">当前值</param>
    /// <param name="newValue">目标值</param>
    protected void SetProperty<T>(ref T currentValue, T newValue)
    {
        if ((currentValue == null && newValue == null) || (currentValue != null && currentValue.Equals(newValue)))
            return;
        currentValue = newValue;
        SetDirty();
    }

    protected override void Awake()
    {
        CalculateLayoutChildren();
    }

    /// <summary>
    /// 计算开始位置的预留空间偏移
    /// </summary>
    /// <param name="axis">方向</param>
    /// <param name="requiredSpaceWithoutPadding">内容所占的实际大小</param>
    /// <returns></returns>
    protected virtual float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
    {
        float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? padding.horizontal : padding.vertical);
        float availableSpace = rectTransform.rect.size[axis];
        float surplusSpace = availableSpace - requiredSpace;
        float alignmentOnAxis = 0;
        if (axis == 0)
            alignmentOnAxis = ((int)childAlignment % 3) * 0.5f;
        else
            alignmentOnAxis = ((int)childAlignment / 3) * 0.5f;

        return (axis == 0 ? padding.left : padding.top) + surplusSpace * alignmentOnAxis;
    }

    /// <summary>
    /// 获取Item大小
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    protected virtual Vector2 GetPerferSize(RectTransform trans)
    {
        Vector2 size = trans.rect.size;
        //兼容LayoutElement
        LayoutElement element = trans.GetComponent<LayoutElement>();
        if (element != null)
            size = new Vector2(element.preferredWidth, element.preferredHeight);

        return size;
    }

    /// <summary>
    /// 清空子节点
    /// </summary>
    public virtual void ClearAllChildren()
    {
        if (allChildren.Count <= 0)
            return;

        int count = allChildren.Count;
        for (int i = 0; i < count; i++)
        {
            if (allChildren[i] == null)
                continue;

            if (!Application.isPlaying)
                GameObject.DestroyImmediate(allChildren[i].gameObject);
            else
                GameObject.Destroy(allChildren[i].gameObject);
        }

        m_UsingChildren.Clear();
        allChildren.Clear();
    }

    /// <summary>
    /// 是否空
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return allChildren.Count <= 0;
    }

    protected override void OnDidApplyAnimationProperties()
    {
        SetDirty();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetDirty();
    }

    protected virtual void OnTransformChildrenChanged()
    {
        CalculateLayoutChildren();
        SetDirty();
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        SetDirty();
    }
#endif

    public virtual void CalculateLayoutInputHorizontal()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            CalculateLayoutChildren();
#endif
        CalculateUsingLayoutChildren();
    }

    public virtual void CalculateLayoutInputVertical() { }

    public abstract void SetLayoutHorizontal();
    public abstract void SetLayoutVertical();
 
}
