using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
[DisallowMultipleComponent]
public class DynamicTableNormal : UIBehaviour
{
    //用于修正某些界限
    const float CALCULATE_OFFSET = 0.001f;
    //开始下表
    const int START_INDEX = 1;
    /// <summary>
    /// 滑动组件
    /// </summary>
    public ScrollRect ScrollRectInstance = null;
    public ScrollRect ScrRect
    {
        get
        {
            if (ScrollRectInstance == null)
                ScrollRectInstance = GetComponent<ScrollRect>();
            return ScrollRectInstance;
        }
    }

    /// <summary>
    /// 总数
    /// </summary>
    public int TotalCount = 0;

    /// <summary>
    /// 动态节点
    /// </summary>
    public DynamicGrid Grid;

    /// <summary>
    /// 节点大小
    /// </summary>
    public Vector2 GridSize = Vector2.zero;

    /// <summary>
    /// 是否为Gird添加点击事件
    /// </summary>
    public bool IsGridTouchEventEnable = true;

    /// <summary>
    /// 事件
    /// </summary>
    public Action<int, int, DynamicGrid> DynamicTableGridDelegate;

    /// <summary>
    /// 可显示的数量
    /// </summary>
    public int AvailableViewCount = 0;

    /// <summary>
    /// 使用ViewPort
    /// </summary>
    public bool UseViewportSize = true;

    /// 可视区域大小
    /// </summary>
    public Vector2 ViewSize = Vector2.zero;

    /// <summary>
    /// 开始索引
    /// </summary>
    private int StartIndex = START_INDEX;

    /// <summary>
    /// 检查数据源是否更新
    /// </summary>
    private bool IsDataDirty = true;

    /// <summary>
    /// 方向
    /// </summary>
    public LayoutRule.Direction Direction = LayoutRule.Direction.Vertical;

    /// <summary>
    /// 使用中的节点
    /// </summary>
    public HashSet<DynamicGrid> UsingGridSet = new HashSet<DynamicGrid>();

    /// <summary>
    /// 准备回收
    /// </summary>
    public Stack<DynamicGrid> PreRecycleGridStack = new Stack<DynamicGrid>();

    /// <summary>
    /// 缓存列表
    /// </summary>
    private Stack<DynamicGrid> GridPoolStack = new Stack<DynamicGrid>();

    /// <summary>
    /// 是否第一次初始化完成
    /// </summary>
    public bool IsInitCompeleted { get; private set; }

    /// <summary>
    /// 是否异步加载中
    /// </summary>
    bool IsAsyncLoading = false;

    /// <summary>
    /// 原始gridSize
    /// </summary>
    public Vector2 OriginGridSize = Vector2.one;

    /// <summary>
    /// 自动拉伸Grid
    /// </summary>
    public bool GridStretching = false;

    /// <summary>
    /// 等比拉伸
    /// </summary>
    public bool GridStretchingEqualRatio = true;

    /// <summary>
    /// 是否已经Start
    /// </summary>
    bool IsStart = false;

    /// <summary>
    /// 是否需要重载
    /// </summary>
    bool IsNeedReload = false;

    /// <summary>
    /// 是否是异步重载
    /// </summary>
    bool IsAsyncReload = false;

    /// <summary>
    /// grid异步加载间隔 默认0 一帧
    /// </summary>
    public float GridLoadInteral = 0;

    /// <summary>
    /// 异步加载规则
    /// </summary>
    public LayoutRule.GridLoadRule GridLoadRule = LayoutRule.GridLoadRule.PER_GRID;

#if XLUA
    /// <summary>
    /// 持有LuaTable
    /// </summary>
    public LuaTable LuaTableDelegate;
#endif

    /// <summary>
    /// RectTransform
    /// </summary>
    protected RectTransform RectTrans;
    public RectTransform rectTransform
    {
        get
        {
            if (RectTrans == null)
                RectTrans = GetComponent<RectTransform>();
            return RectTrans;
        }
    }

    #region 布局相关
    /// <summary>
    /// 布局起始角落
    /// </summary>
    public LayoutRule.Corner StartCorner = LayoutRule.Corner.UpperLeft;

    /// <summary>
    /// 起始轴
    /// </summary>
    public RectTransform.Axis StartAxis = RectTransform.Axis.Horizontal;

    /// <summary>
    /// 分割布局
    /// </summary>
    public LayoutRule.Constraint Constraint = LayoutRule.Constraint.Flexible;

    /// <summary>
    /// 分割数量
    /// </summary>
    public int ConstraintCount = 2;

    /// <summary>
    /// 容器真实的行列数
    /// </summary>
    private int ActualColumn, ActualRow;

    /// <summary>
    /// 相对容器的偏移
    /// </summary>
    public RectOffset Padding = new RectOffset();

    /// <summary>
    /// 对齐方式
    /// </summary>
    public TextAnchor ChildAlignment = TextAnchor.UpperLeft;

    /// <summary>
    /// 偏移
    /// </summary>
    protected Vector2 ContentOffset = Vector2.zero;

    /// <summary>
    /// 间距
    /// </summary>
    public Vector2 Spacing = Vector2.zero;

    /// <summary>
    /// 基于对齐，间隙的偏移
    /// </summary>
    protected Vector2 StartOffset;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        InitScrollRect();
    }

    protected override void Start()
    {
        base.Start();

        IsStart = true;

        if (IsNeedReload)
        {
            IsNeedReload = false;
            if (IsAsyncReload)
                ReloadDataAsync(StartIndex);
            else
                ReloadDataSync(StartIndex);
        }
    }

    void InitScrollRect()
    {
        if (OriginGridSize == Vector2.one)
            OriginGridSize = GridSize;

        ScrRect.onValueChanged.AddListener(this.OnScrollRectValueChanged);
        ScrRect.content.anchorMax = new Vector2(0, 1);
        ScrRect.content.anchorMin = new Vector2(0, 1);

        SetDirection((int)Direction);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Grid = null;
        Clear();
    }


    #region 控制相关

    /// <summary>
    /// 设置可视区域
    /// </summary>
    /// <param name="size"></param>
    public virtual void SetViewSize(Vector2 size)
    {
        ViewSize = size;

        if (ScrRect.viewport == null)
            return;

        ScrRect.viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        ScrRect.viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }

    /// <summary>
    /// 总数
    /// </summary>
    /// <param name="count"></param>
    public virtual void SetTotalCount(int count)
    {
        if (count < 0)
        {
            Debug.Log("Table Request TotalCellCount at least 0.");
            return;
        }

        if (TotalCount != count)
            IsDataDirty = true;

        TotalCount = count;
    }

    /// <summary>
    /// 设置方向
    /// </summary>
    /// <param name="direction">考虑到在Hotfix使用int</param>
    public virtual void SetDirection(int direction)
    {
        LayoutRule.Direction direct = (LayoutRule.Direction)direction;
        switch (direct)
        {
            case LayoutRule.Direction.Horizontal:
                ScrRect.vertical = false;
                ScrRect.horizontal = true;
                StartAxis = RectTransform.Axis.Vertical;
                break;
            case LayoutRule.Direction.Vertical:
                ScrRect.vertical = true;
                ScrRect.horizontal = false;
                StartAxis = RectTransform.Axis.Horizontal;
                break;
        }

        Direction = direct;
    }

    public virtual bool IsActive()
    {
        if (ScrRect.viewport.rect.size == Vector2.zero)
            return false;

        return IsStart && isActiveAndEnabled;
    }


    /// <summary>
    /// 重载
    /// </summary>
    /// <param name="startIndex">开始索引为</param>
    public virtual void ReloadDataSync(int startIndex = -1, bool forceReload = false)
    {
        IsAsyncReload = false;

        if (!IsActive())
        {
            IsNeedReload = true;
            StartIndex = startIndex;
            return;
        }

        if (forceReload && IsAsyncLoading)
            StopAllCoroutines();

        if (IsAsyncLoading && !forceReload)
            return;

        if (UseViewportSize)
            SetViewSize(ScrRect.viewport.rect.size);

        if (GridStretching)
            ReCalculateGridSize();
        else
            GridSize = OriginGridSize;

        //计算可视区域和显示的节点数量
        CalculateAvailableViewGridCount();
        //计算容器大小
        CalculateContainerContentSize();
        //重载时计算所有节点公用的偏移
        CalculateStartOffest();
        //检测索引是否越界,跳转位置，否则保持当前位置，只更新当前显示的Grid
        ResetStartIndex(startIndex);
        //设置容器偏移
        SetContentOffest();
        // 重载Grid
        ReloadGrids();
    }



    /// <summary>
    /// 重载
    /// </summary>
    /// <param name="startIndex">开始索引为</param>
    public virtual void ReloadDataAsync(int startIndex = -1, bool forceReload = false)
    {
        IsAsyncReload = true;

        if (!IsActive())
        {
            IsNeedReload = true;
            StartIndex = startIndex;
            return;
        }

        if (forceReload && IsAsyncLoading)
            StopAllCoroutines();

        if (IsAsyncLoading && !forceReload)
            return;

        if (UseViewportSize)
            SetViewSize(ScrRect.viewport.rect.size);

        if (GridStretching)
            ReCalculateGridSize();
        else
            GridSize = OriginGridSize;


        //计算可视区域和显示的节点数量
        CalculateAvailableViewGridCount();
        //计算容器大小
        CalculateContainerContentSize();
        //重载时计算所有节点公用的偏移
        CalculateStartOffest();
        //检测索引是否越界跳转位置，否则保持当前位置，只更新当前显示的Grid
        ResetStartIndex(startIndex);
        //设置容器偏移
        SetContentOffest();
        // 重载Grid
        StartCoroutine(AsyncLoadGrid());
    }

    IEnumerator AsyncLoadGrid()
    {
        IsAsyncLoading = true;

        Freeze();

        foreach (var grid in UsingGridSet)
        {
            if (grid == null)
                continue;

            RecycleTableGrid(grid);
        }

        UsingGridSet.Clear();
        //一列或者一行
        int preFrameLoadCount = Direction == LayoutRule.Direction.Horizontal ? ActualRow : ActualColumn;

        /*防止超出*/
        int count = (TotalCount > AvailableViewCount + StartIndex - START_INDEX) ? AvailableViewCount : TotalCount - StartIndex + START_INDEX;
        for (int i = 0; i < count; i++)
        {
            DynamicGridAtIndex(i + StartIndex);
            //按照行列进行分帧加载
            if (GridLoadRule == LayoutRule.GridLoadRule.PER_GRID)
            {
                yield return new WaitForSeconds(GridLoadInteral);
            }
            else
            {
                if (preFrameLoadCount != 0 && i % preFrameLoadCount == 0)
                    yield return new WaitForSeconds(GridLoadInteral);
            }

        }

        UnFreeze();

        IsAsyncLoading = false;
        IsInitCompeleted = true;

        OnTableGridReloadCompleted();
    }


    /// <summary>
    /// 重载Grid
    /// </summary>
    /// <param name="prefab"></param>
    private void ReloadGrids()
    {
        foreach (var grid in UsingGridSet)
        {
            if (grid == null)
                continue;

            RecycleTableGrid(grid);
        }

        UsingGridSet.Clear();

        /*防止超出*/
        int count = (TotalCount > AvailableViewCount + StartIndex - START_INDEX) ? AvailableViewCount : TotalCount - StartIndex + START_INDEX;

        for (int i = 0; i < count; i++)
        {
            DynamicGridAtIndex(i + StartIndex);
        }

        IsInitCompeleted = true;

        OnTableGridReloadCompleted();
    }


    /// <summary>
    /// 锁住不让滑动
    /// </summary>
    public void Freeze()
    {
        ScrRect.horizontal = false;
        ScrRect.vertical = false;
    }

    /// <summary>
    /// 解锁
    /// </summary>
    public void UnFreeze()
    {
        ScrRect.horizontal = Direction == LayoutRule.Direction.Horizontal;
        ScrRect.vertical = Direction == LayoutRule.Direction.Vertical;
    }

    /// <summary>
    /// 根据Index更新Cell
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual DynamicGrid DynamicGridAtIndex(int index)
    {
        if (index <= 0 || index - START_INDEX >= TotalCount)
        {
            Debug.LogErrorFormat("Index {0} Overflow TotalCount {1}", index, TotalCount);
            return null;
        }

        DynamicGrid grid = LoadGridFormPool();

        if (grid == null)
            Debug.LogErrorFormat("DynamicTable Load Grid at Index {0} Fail!", index);

        grid.Index = index;
        UsingGridSet.Add(grid);

        //设置位置
        OnTableGridAtIndex(grid);

        if (!grid.gameObject.activeSelf)
            grid.gameObject.SetActive(true);

        SetGridsAlongAxis(grid, index);

        return grid;
    }


    /// <summary>
    /// 从本地加载一个Grid
    /// </summary>
    /// <returns></returns>
    protected virtual DynamicGrid LoadGridFormPool()
    {
        if (Grid == null)
        {
            Debug.LogError("Error:TableView m_Grid is null");
            return null;
        }

        DynamicGrid grid = DequeueCell();

        if (grid != null)
            return grid;


        GameObject obj = Instantiate(Grid.gameObject);
        grid = obj.GetComponent<DynamicGrid>();
        grid.transform.SetParent(ScrRect.content, false);

        //内嵌套点击事件
        if (IsGridTouchEventEnable)
        {
            DynamicGridClickHelper trigger = grid.GetComponent<DynamicGridClickHelper>();
            if (trigger == null)
                trigger = grid.gameObject.AddComponent<DynamicGridClickHelper>();

            trigger.SetupClickEnable(true, delegate (PointerEventData eventData)
            {
                OnTableGridTouched(grid, eventData);
            });
        }
        else
        {
            DynamicGridClickHelper trigger = grid.GetComponent<DynamicGridClickHelper>();
            if (trigger != null)
                trigger.SetupClickEnable(false);
        }


        return grid;
    }


    /// <summary>
    /// 从池中获取Grid
    /// </summary>
    /// <returns></returns>
    protected DynamicGrid DequeueCell()
    {
        if (GridPoolStack.Count <= 0)
            return null;

        return GridPoolStack.Pop();
    }

    /// <summary>
    /// 重置索引
    /// </summary>
    /// <param name="startIndex"></param>
    void ResetStartIndex(int startIndex)
    {

        StartIndex = StartIndex < START_INDEX ? START_INDEX : StartIndex;

        //保持当前位置，只更新当前显示的Grid
        if (startIndex < START_INDEX && IsInitCompeleted && !IsDataDirty)
            return;

        IsDataDirty = false;
        StartIndex = startIndex < START_INDEX ? StartIndex : startIndex;


        //填不满内容框
        if ((ScrRect.content.rect.size.y < ViewSize.y && Direction == LayoutRule.Direction.Vertical)
        || ScrRect.content.rect.size.x < ViewSize.x && Direction == LayoutRule.Direction.Horizontal)
            StartIndex = START_INDEX;

        //跳转到索引
        JumpToIndex(StartIndex);
    }


    /// <summary>
    /// 定位到某个Grid
    /// </summary>
    /// <param name="index">下标</param>
    /// <returns></returns>
    protected virtual void JumpToIndex(int index)
    {
        if (TotalCount == 0)
            return;

        Vector3 vec3 = CalculateStartPosByIndex(Mathf.Clamp(index, START_INDEX, TotalCount));
        ContentOffset = new Vector2(vec3.x, vec3.y);
        StartIndex = (int)vec3.z;
    }

    /// <summary>
    /// 定位到某个位置
    /// </summary>
    protected virtual void JumpToPosition()
    {
        if (TotalCount == 0)
        {
            ContentOffset = Vector2.zero;
            return;
        }

        CalculateStartPosByContentPos();
    }



    /// <summary>
    /// 设置偏移
    /// </summary>
    protected virtual void SetContentOffest()
    {
        ScrRect.horizontalNormalizedPosition = Mathf.Clamp01(ContentOffset.x);
        ScrRect.verticalNormalizedPosition = Mathf.Clamp01(ContentOffset.y);
    }

    /// <summary>
    /// 回收
    /// </summary>
    void RecycleGrids()
    {
        if (PreRecycleGridStack == null || PreRecycleGridStack.Count <= 0)
            return;

        while (PreRecycleGridStack.Count > 0)
        {
            var grid = PreRecycleGridStack.Pop();
            UsingGridSet.Remove(grid);

            OnTableGridRecycle(grid);
            RecycleTableGrid(grid);
        }
    }

    /// <summary>
    /// 活动回调
    /// </summary>
    /// <param name="offset"></param>
    protected void OnScrollRectValueChanged(Vector2 offset)
    {
        if (!IsInitCompeleted || IsAsyncLoading)
            return;

        ContentOffset = offset;

        int startIndex = GetStartIndexByOffest(offset);
        int endIndex = (startIndex + AvailableViewCount - 1);
        endIndex = endIndex > TotalCount ? TotalCount : endIndex;

        if (StartIndex == startIndex)
            return;

        StartIndex = startIndex;
        //回收
        foreach (var grid in UsingGridSet)
        {
            if (grid.Index > endIndex || grid.Index < startIndex)
                PreRecycleGridStack.Push(grid);
        }

        RecycleGrids();

        //出现
        for (int i = startIndex; i <= endIndex; ++i)
        {
            DynamicGrid grid = GetGridByIndex(i);
            if (grid != null)
                continue;

            DynamicGridAtIndex(i);
        }
    }

    /// <summary>
    /// 获取Grid
    /// </summary>
    /// <param name="index">下标</param>
    /// <returns></returns>
    public DynamicGrid GetGridByIndex(int index)
    {
        foreach (var grid in UsingGridSet)
        {
            if (grid.Index != index)
                continue;

            return grid;
        }
        return null;
    }


    /// <summary>
    /// 回收所有节点到池
    /// </summary>
    public void RecycleAllTableGrid()
    {
        if (UsingGridSet == null || UsingGridSet.Count == 0)
            return;

        foreach (var grid in UsingGridSet)
        {
            if (grid != null)
                RecycleTableGrid(grid);
        }
    }

    /// <summary>
    /// 回收节点
    /// </summary>
    /// <param name="grid"></param>
    private void RecycleTableGrid(DynamicGrid grid)
    {
        if (grid == null)
            return;

        OnTableGridRecycle(grid);
        grid.Index = -1;

        if (grid.gameObject.activeSelf)
            grid.gameObject.SetActive(false);

        GridPoolStack.Push(grid);
    }

    public void Clear()
    {
        foreach (var grid in UsingGridSet)
        {
            if (grid != null)
            {
                RecycleTableGrid(grid);

                if (!Application.isPlaying)
                    DestroyImmediate(grid.gameObject);
                else
                    Destroy(grid.gameObject);
            }
        }

        while (GridPoolStack.Count > 0)
        {
            var grid = GridPoolStack.Pop();
            if (grid != null)
            {
                if (!Application.isPlaying)
                    DestroyImmediate(grid.gameObject);
                else
                    Destroy(grid.gameObject);
            }
        }

        UsingGridSet.Clear();
        GridPoolStack.Clear();

        IsAsyncLoading = false;
        IsInitCompeleted = false;
    }

    #endregion

    #region 事件

    /// <summary>
    /// 加载完成
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridReloadCompleted()
    {
        if (DynamicTableGridDelegate == null)
            return;
        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_RELOAD_COMPLETED, -1, null);
    }

    /// <summary>
    /// 回收节点
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridRecycle(DynamicGrid grid)
    {
        if (DynamicTableGridDelegate == null)
            return;
        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_RECYCLE, grid.Index, grid);
    }

    /// <summary>
    /// Grid被点击
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridTouched(DynamicGrid grid, PointerEventData eventData = null)
    {
        if (DynamicTableGridDelegate == null)
            return;
        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_TOUCHED, grid.Index, grid);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridAtIndex(DynamicGrid grid)
    {
        if (DynamicTableGridDelegate == null)
            return;
        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_ATINDEX, grid.Index, grid);
    }
    #endregion

    #region 布局方法
    /// <summary>
    /// 计算可视区最小显示的数量
    /// </summary>
    protected virtual void CalculateAvailableViewGridCount()
    {
        float width = ViewSize.x;
        float height = ViewSize.y;

        // 可视区域行列数
        int column = 1, row = 1;

        /*如果指定列数*/
        if (Constraint == LayoutRule.Constraint.FixedColumnCount)
        {
            column = ConstraintCount;
            if (Direction == LayoutRule.Direction.Vertical)
                row = Mathf.Max(1, Mathf.CeilToInt((height + Spacing.y + CALCULATE_OFFSET) / (GridSize.y + Spacing.y))) + 1;
            else //一般不考虑这种情况，尽量垂直限定列数,水平限定行数
                row = Mathf.CeilToInt(TotalCount / (float)column);

        }
        /*如果指定行数*/
        else if (Constraint == LayoutRule.Constraint.FixedRowCount)
        {
            row = ConstraintCount;
            if (Direction == LayoutRule.Direction.Horizontal)
                column = Mathf.Max(1, Mathf.CeilToInt((width + Spacing.x + CALCULATE_OFFSET) / (GridSize.x + Spacing.x))) + 1;
            else //一般不考虑这种情况，尽量垂直限定列数,水平限定行数
                column = Mathf.CeilToInt(TotalCount / (float)row);
        }
        /*自动适配*/
        else
        {
            if (Direction == LayoutRule.Direction.Vertical)
            {
                row = Mathf.Max(1, Mathf.CeilToInt((height + Spacing.y + CALCULATE_OFFSET) / (GridSize.y + Spacing.y))) + 1;
                column = Mathf.Max(1, Mathf.FloorToInt((width + Spacing.x - CALCULATE_OFFSET) / (GridSize.x + Spacing.x)));
            }
            else if (Direction == LayoutRule.Direction.Horizontal)
            {
                row = Mathf.Max(1, Mathf.FloorToInt((height  + Spacing.y - CALCULATE_OFFSET) / (GridSize.y + Spacing.y)));
                column = Mathf.Max(1, Mathf.CeilToInt((width  + Spacing.x + CALCULATE_OFFSET) / (GridSize.x + Spacing.x))) + 1;
            }
        }

        int total = column * row;
        //假如不足以填满
        AvailableViewCount = (total > TotalCount) ? TotalCount : total;

        //计算真实的长宽个数
        int girdsPerMainAxis;
        if (StartAxis == RectTransform.Axis.Horizontal)
        {
            girdsPerMainAxis = column;
            ActualColumn = Mathf.Clamp(column, 1, TotalCount);
            ActualRow = Mathf.CeilToInt((float)TotalCount / girdsPerMainAxis);
        }
        else
        {
            girdsPerMainAxis = row;
            ActualRow = Mathf.Clamp(row, 1, TotalCount);
            ActualColumn = Mathf.CeilToInt((float)TotalCount / girdsPerMainAxis);
        }
    }


    /// <summary>
    /// 设置容器的大小
    /// </summary>
    public virtual void CalculateContainerContentSize()
    {
        Vector2 actualGridCount = new Vector2(ActualColumn, ActualRow);
        /*计算Align上的偏移*/
        Vector2 requiredSpace = new Vector2(
                actualGridCount.x * GridSize.x + (actualGridCount.x - 1) * Spacing.x + Padding.horizontal,
                actualGridCount.y * GridSize.y + (actualGridCount.y - 1) * Spacing.y + Padding.vertical
                );

        if (Direction == LayoutRule.Direction.Vertical && ViewSize.x > requiredSpace.x)
            requiredSpace.x = ViewSize.x;
        else if (Direction == LayoutRule.Direction.Horizontal && ViewSize.y > requiredSpace.y)
            requiredSpace.y = ViewSize.y;

        ScrRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, requiredSpace.x);
        ScrRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredSpace.y);
    }

    /// <summary>
    /// 计算来自于停靠，对齐的坐标偏移
    /// </summary>
    protected virtual Vector2 CalculateStartOffest()
    {
        /*计算Align上的偏移*/
        Vector2 requiredSpace = new Vector2(
                ActualColumn * GridSize.x + (ActualColumn - 1) * Spacing.x,
                ActualRow * GridSize.y + (ActualRow - 1) * Spacing.y
                );
        Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
                );

        StartOffset = startOffset;

        return startOffset;
    }


    /// <summary>
    /// 计算开始位置的预留空间偏移
    /// </summary>
    /// <param name="axis">方向</param>
    /// <param name="requiredSpaceWithoutPadding">内容所占的实际大小</param>
    /// <returns></returns>
    protected virtual float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
    {
        float requiredSpace = requiredSpaceWithoutPadding + (axis == 0 ? Padding.horizontal : Padding.vertical);
        float availableSpace = ScrRect.content.rect.size[axis];
        float surplusSpace = availableSpace - requiredSpace;
        float alignmentOnAxis = 0;
        if (axis == 0)
            alignmentOnAxis = ((int)ChildAlignment % 3) * 0.5f;
        else
            alignmentOnAxis = ((int)ChildAlignment / 3) * 0.5f;

        return (axis == 0 ? Padding.left : Padding.top) + surplusSpace * alignmentOnAxis;
    }


    /// <summary>
    /// 根据位置获取偏移
    /// </summary>
    /// <returns></returns>
    public virtual void CalculateStartPosByContentPos()
    {
        /*开始的位置*/
        int cornerX = (int)StartCorner % 2;
        int cornerY = (int)StartCorner / 2;

        Vector2 pos = ScrRect.content.anchoredPosition - GetPositionOffsetOfPivotByTopLeft(ScrRect.content);
        Vector2 realSize = ScrRect.content.rect.size - ViewSize;

        ContentOffset = Vector2.zero;

        if (Direction == LayoutRule.Direction.Horizontal)
        {
            if (realSize.x <= 0 && cornerX == 1)
            {
                ContentOffset = Vector2.right;
            }
            else
            {
                if (cornerX == 1)
                    ContentOffset = new Vector2(Mathf.Abs(1 - pos.x / realSize.x), 0.0f);
                else
                    ContentOffset = new Vector2(Mathf.Abs(pos.x / realSize.x), 0.0f);
            }
        }
        else if (Direction == LayoutRule.Direction.Vertical)
        {
            if (realSize.y <= 0 && cornerY != 1)
            {
                ContentOffset = Vector2.up;
            }
            else
            {
                if (cornerY == 1)
                    ContentOffset = new Vector2(0.0f, Mathf.Abs(pos.y / realSize.y));
                else
                    ContentOffset = new Vector2(0.0f, Mathf.Abs(1 - pos.y / realSize.y));
            }
        }
    }



    /// <summary>
    /// 计算当前索引容器的开始位置
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual Vector3 CalculateStartPosByIndex(int index)
    {
        float startIndex = 1;

        /*开始的位置*/
        int cornerX = (int)StartCorner % 2;
        int cornerY = (int)StartCorner / 2;

        int i = index - 1;
        int positionX;
        int positionY;

        //每条轴的Grid个数
        int GridCountPerMainAxis;
        if (StartAxis == RectTransform.Axis.Horizontal)
        {
            GridCountPerMainAxis = ActualColumn == 0 ? 1 : ActualColumn;
            positionX = i % GridCountPerMainAxis;
            positionY = i / GridCountPerMainAxis;
        }
        else
        {
            GridCountPerMainAxis = ActualRow == 0 ? 1 : ActualRow;
            positionX = i / GridCountPerMainAxis;
            positionY = i % GridCountPerMainAxis;
        }

        //容器偏移的百分比
        Vector2 posOffest = Vector2.zero;

        int indexOff = 0;
        //水平
        if (Direction == LayoutRule.Direction.Horizontal)
        {
            posOffest.y = 0;
            //如果是倒序，需要添加视觉大小的偏移，前半部分算出是超出屏幕的部分 cellSize.x + spacing.x) + startOffset.x 
          
            float leftSpace = positionX * (GridSize.x + Spacing.x) / (ScrRect.content.rect.size.x - ViewSize.x);

            if (cornerX == 1)
                posOffest.x = 1.0f - leftSpace;
            else
                posOffest.x = leftSpace;

            //如果没有超出
            startIndex = positionX * ActualRow + 1;
            /*对齐起始点 相对差值*/
            indexOff = TotalCount - (int)startIndex;
            /*可视范围可以存放的Cell的最大值*/
            int viewCount = Mathf.CeilToInt(ViewSize.x / (GridSize.x + Spacing.x)) * (ActualRow) - 1;

            if (indexOff < viewCount && TotalCount > viewCount)
                startIndex = TotalCount - viewCount;

        }
        else if (Direction == LayoutRule.Direction.Vertical)
        {
            posOffest.x = 0;

            float leftSpace = positionY * (GridSize.y + Spacing.y) / (ScrRect.content.rect.size.y - ViewSize.y);

            //如果是倒序，需要添加视觉大小的偏移，前半部分算出是超出屏幕的部分 cellSize.x + spacing.x) + startOffset.x 
            if (cornerY == 1)
                posOffest.y = leftSpace;
            else
                posOffest.y = 1.0f - leftSpace;

            //如果没有超出
            startIndex = positionY * ActualColumn + 1;
            /*对齐起始点*/
            indexOff = TotalCount - (int)startIndex;
            /*可视范围可以存放的Cell的最大值*/
            int viewCount = Mathf.CeilToInt(ViewSize.y / (GridSize.y + Spacing.y)) * (ActualColumn) - 1;

            if (indexOff < viewCount && TotalCount > viewCount)
                startIndex = TotalCount - viewCount;
        }
        return new Vector3(posOffest.x, posOffest.y, startIndex);
    }


    /// <summary>
    /// 计算Cells位置
    /// </summary>
    /// <param name="axis">轴</param>
    public virtual void SetGridsAlongAxis(DynamicGrid grid, int index)
    {
        if (grid == null)
            return;

        //计算真实的长宽个数
        int cellsPerMainAxis;

        /*开始的位置*/
        int cornerX = (int)StartCorner % 2;
        int cornerY = (int)StartCorner / 2;

        /*设置Cell的位置*/
        int i = index - 1;

        int positionX;
        int positionY;
        if (StartAxis == RectTransform.Axis.Horizontal)
        {
            cellsPerMainAxis = ActualColumn;
            positionX = i % cellsPerMainAxis;
            positionY = i / cellsPerMainAxis;
        }
        else
        {
            cellsPerMainAxis = ActualRow;
            positionX = i / cellsPerMainAxis;
            positionY = i % cellsPerMainAxis;
        }

        if (cornerX == 1)
            positionX = ActualColumn - 1 - positionX;
        if (cornerY == 1)
            positionY = ActualRow - 1 - positionY;

        SetChildAlongAxis(grid.rectTransform, 0, StartOffset.x + (GridSize[0] + Spacing[0]) * positionX, GridSize[0]);
        SetChildAlongAxis(grid.rectTransform, 1, StartOffset.y + (GridSize[1] + Spacing[1]) * positionY, GridSize[1]);
    }

    /// <summary>
    /// 设置Grid的位置
    /// </summary>
    /// <param name="rect">RectTransform</param>
    /// <param name="axis">轴</param>
    /// <param name="pos">相对的距离</param>
    /// <param name="size">Grid 的大小</param>
    protected void SetChildAlongAxis(RectTransform rect, int axis, float pos, float size)
    {
        if (rect == null)
            return;

        /*相对于父节点的停靠位置*/
        rect.SetInsetAndSizeFromParentEdge(axis == 0 ? RectTransform.Edge.Left : RectTransform.Edge.Top, pos, size);
    }

    /// <summary>
    /// 获取开始索引
    /// </summary>
    /// <param name="offest">容器的偏移</param>
    /// <param name="viewSize">可视区域的大小</param>
    public virtual int GetStartIndexByOffest(Vector2 offest)
    {
        Vector2 col_row = GetCurScrollPerLineIndex();
        //不需要计算缓存
        if (AvailableViewCount >= TotalCount)
            return START_INDEX;

        int startIndex = 0;
        switch (Direction)
        {
            case LayoutRule.Direction.Horizontal: //水平方向  
                startIndex = (int)(ActualRow * col_row.x + col_row.y) + 1;
                break;
            case LayoutRule.Direction.Vertical://垂直方向  
                startIndex = (int)(ActualColumn * col_row.y + col_row.x) + 1;
                break;
        }

        startIndex = Mathf.Clamp(startIndex, 1, TotalCount);
        return startIndex;
    }

    /// <summary>
    /// 获取基于锚点左上的位置偏移
    /// </summary>
    /// <param name="rectTrans"></param>
    /// <returns></returns>
    public Vector2 GetPositionOffsetOfPivotByTopLeft(RectTransform rectTrans)
    {
        if (rectTrans == null)
            return Vector2.zero;

        Vector2 offset = new Vector2(rectTrans.pivot.x * rectTrans.rect.size.x, (rectTrans.pivot.y - 1.0f) * rectTrans.rect.size.y);
        return offset;
    }


    /// <summary>  
    /// 根据Content偏移,计算当前开始显示所在数据列表中的行或列  
    /// </summary>  
    /// <returns></returns>  
    private Vector2 GetCurScrollPerLineIndex()
    {
        Vector2 vec2 = Vector2.zero;

        /*开始的位置*/
        int cornerX = (int)StartCorner % 2;
        int cornerY = (int)StartCorner / 2;
        Vector2 operatePosition = ScrRect.content.anchoredPosition - GetPositionOffsetOfPivotByTopLeft(ScrRect.content);

        float hideOffestX = Mathf.Max(0, ScrRect.content.rect.width - ViewSize.x + operatePosition.x - StartOffset.x);
        int x = cornerX == 1 ? Mathf.FloorToInt(hideOffestX / (GridSize.x + Spacing.x)) : Mathf.FloorToInt(Mathf.Abs((Mathf.Min(0, operatePosition.x) + StartOffset.x)) / (GridSize.x + Spacing.x));

        float hideOffestY = Mathf.Max(0, ScrRect.content.rect.height - ViewSize.y - operatePosition.y - StartOffset.y);
        int y = cornerY == 1 ? Mathf.FloorToInt(hideOffestY / (GridSize.y + Spacing.y)) : Mathf.FloorToInt((Mathf.Max(0, operatePosition.y) - StartOffset.y) / (GridSize.y + Spacing.y));

        switch (Direction)
        {
            case LayoutRule.Direction.Horizontal: //水平方向  
                vec2.x = x;
                break;
            case LayoutRule.Direction.Vertical://垂着方向  
                vec2.y = y;
                break;
        }

        return vec2;
    }

    #endregion


#if XLUA
    #region 其他
    public void SetDelegate(LuaTable table)
    {
        LuaTableDelegate = table;
    }
    #endregion
#endif

    /// <summary>
    /// 重新计算GridSize
    /// </summary>
    public virtual void ReCalculateGridSize()
    {
        if (Direction == LayoutRule.Direction.Vertical)
        {
            Vector2 unusedSpace = new Vector2(ViewSize.x - Padding.horizontal, ViewSize.y);
            int count = Mathf.FloorToInt((unusedSpace.x + Spacing.x) / (OriginGridSize.x + Spacing.x));
            count = count <= 0 ? 1 : count;

            if (Constraint == LayoutRule.Constraint.FixedColumnCount)
                count = ConstraintCount;
            else if (Constraint == LayoutRule.Constraint.FixedRowCount)
                count = Mathf.CeilToInt((float)TotalCount / (float)ConstraintCount);

            float surpluX = (unusedSpace.x + Spacing.x) - (OriginGridSize.x + Spacing.x) * count;

            float targetX = surpluX / count + OriginGridSize.x - CALCULATE_OFFSET;
            float targetY = OriginGridSize.y;
            if (GridStretchingEqualRatio)
                targetY = targetX / OriginGridSize.x * OriginGridSize.y;

            GridSize = new Vector2(targetX, targetY);
        }
        else
        {
            Vector2 unusedSpace = new Vector2(ViewSize.x, ViewSize.y - Padding.vertical);
            int count = Mathf.FloorToInt((unusedSpace.y + Spacing.y) / (OriginGridSize.y + Spacing.y));
            count = count <= 0 ? 1 : count;

            if (Constraint == LayoutRule.Constraint.FixedRowCount)
                count = ConstraintCount;
            else if (Constraint == LayoutRule.Constraint.FixedColumnCount)
                count = Mathf.CeilToInt((float)TotalCount / (float)ConstraintCount);

            float surpluY = (unusedSpace.y + Spacing.y) - (OriginGridSize.y + Spacing.y) * count;
            float targetY = surpluY / count + OriginGridSize.y - CALCULATE_OFFSET ;
            ;
            float targetX = OriginGridSize.x;
            if (GridStretchingEqualRatio)
                targetX = targetY / OriginGridSize.y * OriginGridSize.x;

            GridSize = new Vector2(targetX, targetY);
        }
    }


    /// <summary>
    /// 下一帧刷新
    /// </summary>
    private void Update()
    {
        if (!IsNeedReload)
            return;

        if (!IsActive())
            return;

        IsNeedReload = false;

        if (!IsAsyncReload)
            ReloadDataSync(StartIndex);
        else
            ReloadDataAsync(StartIndex);
    }
}


