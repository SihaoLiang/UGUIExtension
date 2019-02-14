using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class LayoutNode : UnityEngine.EventSystems.UIBehaviour
{
    /// <summary>
    /// 顺序
    /// </summary>
    [SerializeField]
    protected LayoutRule.Order m_Order = 0;
    public LayoutRule.Order order { get { return m_Order; } set { m_Order = value; } }

    /// <summary>
    /// 拉伸
    /// </summary>
    [SerializeField]
    public bool bIsStretch = false;


    /// <summary>
    /// 方向
    /// </summary>
    [SerializeField]
    protected LayoutRule.Direction m_Direction = 0;
    public LayoutRule.Direction direction { get { return m_Direction; } set { m_Direction = value; } }

    /// <summary>
    /// 是否忽略非激活的
    /// </summary>
    [SerializeField]
    protected bool m_IgnoreUnActive = true;
    public bool ignoreUnActive { get { return m_IgnoreUnActive; } set { m_IgnoreUnActive = value; } }


    /// <summary>
    /// 子节点相对于容器边缘的偏移
    /// </summary>
    [SerializeField]
    protected RectOffset m_Padding = new RectOffset();
    public RectOffset padding { get { return m_Padding; } set { m_Padding = value; } }

    /// <summary>
    /// 子节点的对齐方式
    /// </summary>
    [SerializeField]
    protected TextAnchor m_ChildAlignment = TextAnchor.UpperLeft;
    public TextAnchor childAlignment { get { return m_ChildAlignment; } set { m_ChildAlignment = value; } }

    /// <summary>
    /// 间隙
    /// </summary>
    [SerializeField]
    protected Vector2 m_Spacing = Vector2.zero;
    public Vector2 spacing { get { return m_Spacing; } set { m_Spacing = value; } }


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
    [SerializeField]
    protected List<RectTransform> m_UsingChildren = new List<RectTransform>();
    public List<RectTransform> usingChildren { get { return m_UsingChildren; } set { m_UsingChildren = value; } }

    /// <summary>
    /// 用于存储所有子节点队列
    /// </summary>
    [SerializeField]
    protected List<RectTransform> m_AllChildren = new List<RectTransform>();
    public List<RectTransform> allChildren { get { return m_AllChildren; } set { m_AllChildren = value; } }


    /// <summary>
    /// 总宽高
    /// </summary>
    protected float totalWidth, totalHeight;

    /// <summary>
    /// 最小
    /// </summary>
    [SerializeField]
    protected Vector2 m_MinSize;
    public Vector2 minSize
    {
        set { m_MinSize = value; }
        get { return m_MinSize; }
    }

    /// <summary>
    /// 更新布局
    /// </summary>
    public virtual void SetDirty()
    {
        CalculateLayoutChildren();
        CalculateUsingLayoutChildren();
        CaculateLayoutContainerSize();
        SetCellsAlongAxis();
    }


    /// <summary>
    /// 计算来自于停靠，对齐的坐标偏移
    /// </summary>
    protected virtual Vector2 CalulateStartOffest(Vector2 size)
    {
        /*计算Align上的偏移*/
        Vector2 requiredSpace = Vector2.zero;
        if (direction == LayoutRule.Direction.Horizontal)
        {
            requiredSpace = new Vector2(
                totalWidth + (m_UsingChildren.Count - 1) * spacing.x,
                size.y
                );
        }
        else if (direction == LayoutRule.Direction.Vertical)
        {
            requiredSpace = new Vector2(
              size.x,
              totalHeight + (m_UsingChildren.Count - 1) * spacing.y
              );
        }


        Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
                );

        return startOffset;
    }
    /// <summary>
    /// 计算容器大小
    /// </summary>
    protected virtual void CaculateLayoutContainerSize()
    {
        Vector2 vec2 = Vector2.zero;

        totalWidth = 0.0f;
        totalHeight = 0.0f;
        float maxWidth = 0.0f;
        float maxHeight = 0.0f;

        foreach (var item in usingChildren)
        {
            //兼容LayoutElement
            //   LayoutElement element = item.GetComponent<LayoutElement>();
            Vector2 size = item.rect.size;

            //if (element != null)
            //    size = new Vector2(element.preferredWidth, element.preferredHeight);

            totalWidth += size.x;
            totalHeight += size.y;
            maxWidth = Mathf.Max(maxWidth, size.x);
            maxHeight = Mathf.Max(maxHeight, size.y);
        }


        if (m_Direction == LayoutRule.Direction.Horizontal)
        {
            vec2.x = totalWidth + padding.horizontal + spacing.x * (usingChildren.Count - 1);
            vec2.y = maxHeight + padding.vertical;

            if (this.bIsStretch)
                vec2.y = rectTransform.rect.size.y;
        }

        else if (m_Direction == LayoutRule.Direction.Vertical)
        {
            vec2.x = maxWidth + padding.horizontal;
            vec2.y = totalHeight + padding.vertical + spacing.y * (usingChildren.Count - 1);

            if (this.bIsStretch)
                vec2.x = rectTransform.rect.size.x;
        }

        vec2 = Vector2.Max(minSize, vec2);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vec2.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vec2.y);
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
        //LayoutElement element = trans.GetComponent<LayoutElement>();
        //if (element != null)
        //    size = new Vector2(element.preferredWidth, element.preferredHeight);

        if (usingChildren.Count <= 0 || bIsStretch == false)
            return size;

        Vector2 contentSize = rectTransform.rect.size;

        if (m_Direction == LayoutRule.Direction.Horizontal)
            return new Vector2(size.x, contentSize.y - padding.vertical);

        else if (m_Direction == LayoutRule.Direction.Vertical)
            return new Vector2(contentSize.x - padding.horizontal, size.y);

        return size;
    }

    /// <summary>
    /// 计算Cells位置
    /// </summary>
    /// <param name="axis">轴</param>
    protected virtual void SetCellsAlongAxis()
    {

        Vector2 cellSize = Vector2.zero;

        Vector2 lastOffest = Vector2.zero;

        /*设置Cell的位置*/
        for (int i = 0; i < usingChildren.Count; i++)
        {
            float positionX = 0;
            float positionY = 0;

            //获取子节点PerferSize
            cellSize = GetPerferSize(usingChildren[i]);

            Vector2 startOffset = CalulateStartOffest(cellSize);

            switch (m_Direction)
            {
                case LayoutRule.Direction.Horizontal:
                    positionX = (int)lastOffest.x + startOffset.x;
                    positionY += startOffset.y;
                    break;
                case LayoutRule.Direction.Vertical:
                    positionY = (int)lastOffest.y + startOffset.y;
                    positionX += startOffset.x;

                    break;
            }

            lastOffest.x += (cellSize[0] + spacing[0]);
            lastOffest.y += (cellSize[1] + spacing[1]);

            SetChildAlongAxis(usingChildren[i], 0, positionX, cellSize[0]);
            SetChildAlongAxis(usingChildren[i], 1, positionY, cellSize[1]);
        }
    }




    /// <summary>
    /// 设置item的位置
    /// </summary>
    /// <param name="rect">RectTransform</param>
    /// <param name="axis">轴</param>
    /// <param name="pos">相对的距离</param>
    /// <param name="size">item 的大小</param>
    protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
    {
        if (rect == null)
            return;

        RectTransform.Edge edge = axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top;
        if (m_Direction == LayoutRule.Direction.Horizontal)
        {
            if (axis == 0 && m_Order == LayoutRule.Order.Reverse)
                edge = RectTransform.Edge.Right;
            else if (axis == 0 && m_Order == LayoutRule.Order.Positive)
                edge = RectTransform.Edge.Left;
        }
        else if (m_Direction == LayoutRule.Direction.Vertical)
        {
            if (axis == 1 && m_Order == LayoutRule.Order.Reverse)
                edge = RectTransform.Edge.Bottom;
            else if (axis == 1 && m_Order == LayoutRule.Order.Positive)
                edge = RectTransform.Edge.Top;
        }

        /*相对于父节点的停靠位置*/
        rect.SetInsetAndSizeFromParentEdge(edge, pos, size);
    }

    /// <summary>
    /// 添加一个节点
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public GameObject Add(GameObject item)
    {
        if (item == null)
        {
            Debug.LogError("ScollView can not add a null item");
            return null;
        }

        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = item.AddComponent<RectTransform>();
            Debug.LogWarning("ScollView item do not have RectTransform component");
        }
        item.SetActive(true);
        rect.SetParent(rectTransform, false);
        m_AllChildren.Add(rect);
        return item;
    }


    /// <summary>
    /// 插入节点
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="index">位置</param>
    /// <returns></returns>
    public RectTransform Insert(RectTransform rect, int index)
    {
        if (rect == null)
        {
            Debug.LogError("ScollView can not add a null item");
            return null;
        }
        //保证index在长度范围之内
        Mathf.Clamp(index, 0, usingChildren.Count);
        rect.gameObject.SetActive(true);
        rect.SetParent(rectTransform, false);
        m_AllChildren.Insert(index, rect);

        return rect;
    }

    /// <summary>
    /// 插入节点
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="index">位置</param>
    /// <returns></returns>
    public GameObject Insert(GameObject item, int index)
    {
        if (item == null)
        {
            Debug.LogError("LayoutNode can not add a null item");
            return null;
        }

        RectTransform rect = item.GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = item.AddComponent<RectTransform>();
            Debug.LogWarning("LayoutNode item do not have RectTransform component");
        }
        //保证index在长度范围之内
        Mathf.Clamp(index, 0, usingChildren.Count);
        item.SetActive(true);
        rect.SetParent(rectTransform, false);
        m_AllChildren.Insert(index, rect);

        return item;
    }

    /// <summary>
    /// 删除一个子节点
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public void Remove(GameObject item, bool isClear = true)
    {
        RectTransform rect = item.GetComponent<RectTransform>();
        bool suss = m_UsingChildren.Remove(rect);
        if (!suss)
        {
            Debug.LogErrorFormat("Remove item {0} fail.", item.name);
            return;
        }

        if (isClear)
        {
            m_AllChildren.Remove(rect);
            GameObject.Destroy(item);
        }
    }

    /// <summary>
    /// 删除一个子节点
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public void Remove(RectTransform rect, bool isClear = true)
    {

        bool suss = m_UsingChildren.Remove(rect);
        if (!suss)
        {
            Debug.LogErrorFormat("Remove item {0} fail.", rect.name);
            return;
        }

        if (isClear)
        {
            m_AllChildren.Remove(rect);
            GameObject.Destroy(rect.gameObject);
        }
    }

    /// <summary>
    /// 通过索引删除子节点
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public void RemoveByIndex(int index, bool isClear = true)
    {
        RectTransform rect = usingChildren[index];
        if (usingChildren[index] == null)
        {
            Debug.LogErrorFormat("Can not find index {0} in LayoutNode.", index);
            return;
        }

        m_UsingChildren.Remove(rect);
        if (isClear)
        {
            m_AllChildren.Remove(rect);
            GameObject.Destroy(rect.gameObject);
        }
    }

    /// <summary>
    /// 获取子节点
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject GetChildByIndex(int index)
    {
        RectTransform rect = allChildren[index];
        if (allChildren[index] == null)
        {
            Debug.LogErrorFormat("Can not find index {0} in LayoutNode.", index);
            return null;
        }
        return rect.gameObject;
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

            //判断是否过滤布局
            var toIgnoreList = rect.GetComponents<ILayoutIgnorer>();

            if (toIgnoreList.Length == 0)
            {
                m_UsingChildren.Add(rect);
                continue;
            }
            //忽略多个，只要有一个不忽略就加入计算
            for (int j = 0; j < toIgnoreList.Length; j++)
            {
                var ignorer = toIgnoreList[j];
                if (!ignorer.ignoreLayout)
                {
                    m_UsingChildren.Add(rect);
                    break;
                }
            }
            toIgnoreList = null;
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
    }

    protected override void Awake()
    {
        m_UsingChildren.Clear();
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
}

