using System;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
public class AutoLayoutGroup : AutoLayout
{
    /// <summary>
    /// 方向
    /// </summary>
    [SerializeField]
    protected LayoutRule.Direction m_Direction = 0;
    public LayoutRule.Direction direction { get { return m_Direction; } set { SetProperty(ref m_Direction, value); } }

    /// <summary>
    /// 顺序
    /// </summary>
    [SerializeField]
    protected LayoutRule.Order m_Order = 0;
    public LayoutRule.Order order { get { return m_Order; } set { SetProperty(ref m_Order, value); } }

    /// <summary>
    /// 拉伸
    /// </summary>
    [SerializeField]
    public bool m_Stretch = false;
    public bool stretch { get { return m_Stretch; } set { SetProperty(ref m_Stretch, value); } }

    /// <summary>
    /// 总宽高
    /// </summary>
    [NonSerialized]
    protected float totalWidth, totalHeight;

    /// <summary>
    /// 最小
    /// </summary>
    [SerializeField]
    protected Vector2 m_MinSize = Vector2.zero;
    public Vector2 minSize
    {
        set { SetProperty(ref m_MinSize, value); }
        get { return m_MinSize; }
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
            ILayoutController layoutCtrl = item.GetComponent<ILayoutController>();
            if (layoutCtrl != null)
            {
                layoutCtrl.SetLayoutHorizontal();
                layoutCtrl.SetLayoutVertical();
            }

            Vector2 size = item.rect.size;
            LayoutElement element = item.GetComponent<LayoutElement>();
            if (element != null)
            {
                size = new Vector2(element.preferredWidth, element.preferredHeight);
            }

            totalWidth += size.x;
            totalHeight += size.y;
            maxWidth = Mathf.Max(maxWidth, size.x);
            maxHeight = Mathf.Max(maxHeight, size.y);
        }


        if (m_Direction == LayoutRule.Direction.Horizontal)
        {
            vec2.x = totalWidth + padding.horizontal + spacing.x * (usingChildren.Count - 1);
            vec2.y = maxHeight + padding.vertical;

            if (this.m_Stretch)
                vec2.y = rectTransform.rect.size.y;
        }

        else if (m_Direction == LayoutRule.Direction.Vertical)
        {
            vec2.x = maxWidth + padding.horizontal;
            vec2.y = totalHeight + padding.vertical + spacing.y * (usingChildren.Count - 1);

            if (this.m_Stretch)
                vec2.x = rectTransform.rect.size.x;
        }

        vec2 = Vector2.Max(minSize, vec2);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vec2.x);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vec2.y);
    }

    /// <summary>
    /// 获取Item大小
    /// </summary>
    /// 
    /// <param name="size"></param>
    /// <returns></returns>
    protected override Vector2 GetPerferSize(RectTransform trans)
    {
        Vector2 size = base.GetPerferSize(trans);

        if (usingChildren.Count <= 0 || m_Stretch == false)
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

    #region------------增删改查-------

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
        rect.SetParent(rectTransform, false);
        item.SetActive(true);
        //  m_AllChildren.Add(rect);
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
        rect.SetParent(rectTransform, false);
        rect.gameObject.SetActive(true);

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

        rect.SetParent(rectTransform, false);
        item.SetActive(true);

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

    #endregion

    #region----------布局接口----------
    public override float minWidth
    {
        get
        {
            return m_MinSize.x;
        }
    }

    public override float minHeight
    {
        get
        {
            return m_MinSize.y;
        }
    }

    public override void SetLayoutHorizontal()
    {
        if (direction != LayoutRule.Direction.Horizontal)
            return;

        CaculateLayoutContainerSize();
        SetCellsAlongAxis();
    }

    public override void SetLayoutVertical()
    {
        if (direction != LayoutRule.Direction.Vertical)
            return;

        CaculateLayoutContainerSize();
        SetCellsAlongAxis();
    }
    #endregion
}

