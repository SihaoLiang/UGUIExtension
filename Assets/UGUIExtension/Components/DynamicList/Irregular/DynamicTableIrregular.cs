using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary> 
/// 1.不规则动态列表，目前把滑动逻辑和动态列表的逻辑都放在一起
/// 后续可以拆分成两个组件，因为目前的两者逻辑部分基本没有强耦合的，拆分容易。
/// 2.关于是否添加进度条，其实进度条的方案目前有，但是还要在考虑，所以先保留。
/// 3.另一个关于Scrollrect的阉割版本，拆出来后只做纯粹的滑动逻辑
/// 对于后续添加其他功能也可以较好支持，例如下拉加载更多之类的
/// </summary>

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class DynamicTableIrregular : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ILayoutGroup
{
    #region 滑动相关，阉割版的ScrollRect
    /// <summary>
    /// 滑动类型去掉不限制类型
    /// </summary>
    public enum MovementType
    {
        Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
        Clamped, // Restricted movement where it's not possible to go past the edges
    }

    //考虑把滑动分出去 
    //public class ScrollRectExEvent : UnityEvent<Bounds, Bounds> { }
    //private ScrollRectExEvent OnValueChanged = new ScrollRectExEvent();

    /// <summary>
    /// 方向
    /// </summary>
    public LayoutRule.Direction Direction = LayoutRule.Direction.Vertical;

    /// <summary>
    /// 内容节点
    /// </summary>
    public RectTransform Content;

    /// <summary>
    /// 视野节点
    /// </summary>
    public RectTransform Viewport;

    /// <summary>
    /// 滑动类型
    /// </summary>
    public MovementType MoveType = MovementType.Elastic;

    /// <summary>
    /// 弹力系数 Only used for MovementType.Elastic
    /// </summary>
    public float Elasticity = 0.1f;

    /// <summary>
    /// 使用惯性
    /// </summary>
    public bool Inertia = true;

    /// <summary>
    /// 惯性系数Only used when inertia is enabled
    /// </summary>
    public float DecelerationRate = 0.135f; 

    /// <summary>
    /// 速度
    /// </summary>
    private Vector2 Velocity;

    /// <summary>
    /// 拖拽中
    /// </summary>
    private bool IsDragging;

    /// <summary>
    /// 开始拖动的位置，用于反弹和惯性速度计算
    /// </summary>
    protected Vector2 ContentStartPosition = Vector2.zero;

    /// <summary>
    /// 上一帧光标滑动的位置
    /// </summary>
    protected Vector2 LastCursorStartPosition = Vector2.zero;

    /// <summary>
    /// Content上次变化前的位置
    /// </summary>
    protected Vector2 ContentPrevPosition = Vector2.zero;

    /// <summary>
    /// 内容的边界范围
    /// </summary>
    protected Bounds ContentBounds;

    /// <summary>
    /// 视图的边界范围
    /// </summary>
    private Bounds ViewBounds;

    /// <summary>
    /// 上次的边界
    /// </summary>
    private Bounds PrevContentBounds;
    private Bounds PrevViewBounds;

    [System.NonSerialized] private RectTransform m_Rect;
    private RectTransform rectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }
    #endregion

    #region 动态列表基础属性

    //开始索引
    const int START_INDEX = 1;
    //最小兼容距离
    const int MIN_COMPATIBLE_DISTANCE = 20;

    /// <summary>
    /// 总数
    /// </summary>
    public int TotalCount = 0;

    /// <summary>
    /// 对象池
    /// </summary>
    public ObjectPools ObjectPool = null;

    /// <summary>
    /// 开始索引
    /// </summary>
    [SerializeField]
    protected int StartIndex = 0;

    /// <summary>
    /// 结束索引
    /// </summary>
    [SerializeField]
    protected int EndIndex = 0;

    /// <summary>
    /// 正反向
    /// </summary>
    public bool IsReverse = false;

    /// <summary>
    /// 兼容距离
    /// </summary>
    public int CompatibleDistance = MIN_COMPATIBLE_DISTANCE;

    /// <summary>
    /// 可视区域大小
    /// </summary>
    public Vector2 ViewSize = Vector2.zero;

    /// <summary>
    /// 预加载的节点
    /// </summary>
    protected Stack<DynamicGrid> PreLoadStack = null;

    /// <summary>
    /// 最大主轴大小。主轴跟方向相对应
    /// </summary>
    float MaxGridMainAxisSize = 0;

    /// <summary>
    /// 所有子节点的大小
    /// </summary>
    Vector2 AllGridsSizeWithSpace = Vector2.zero;

    /// <summary>
    /// 使用中的节点
    /// </summary>
    protected List<DynamicGrid> UsingGridSet = new List<DynamicGrid>();

    /// <summary>
    /// 是否为Gird添加点击事件
    /// </summary>
    public bool IsGridTouchEventEnable = true;

    /// <summary>
    /// 事件
    /// </summary>
    public Action<int, int> DynamicTableGridDelegate;

    /// <summary>
    /// 是否第一次初始化完成
    /// </summary>
    bool IsInitCompeleted = false;

    /// <summary>
    /// 是否异步加载中
    /// </summary>
    bool IsAsyncLoading = false;

    #endregion

    #region 布局相关
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

    #endregion

    #region 滑动实现

    /// <summary>
    /// 是否激活
    /// </summary>
    /// <returns></returns>
    public override bool IsActive()
    {
        return base.IsActive() && Content != null && Viewport != null;
    }

    /// <summary>
    /// 开始滑动
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!IsActive())
            return;

        IsDragging = true;
        ContentStartPosition = Content.anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport, eventData.position, eventData.pressEventCamera, out LastCursorStartPosition);
    }

    /// <summary>
    /// 拖拽中
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!IsActive())
            return;

        if (IsAsyncLoading)
            return;

        //获取当前滑动光标的位置
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        //算出delta，更新光标
        Vector2 delta = localCursor - LastCursorStartPosition;
        Vector2 position = ContentStartPosition + delta;

        //更新位置
        Vector2 offset = CalculateOffset(position - Content.anchoredPosition);
        position += offset;

        float rubb = 0;
        if (MoveType == MovementType.Elastic)
        {
            if (offset.x != 0)
                position.x = position.x - RubberDelta(offset.x, ViewBounds.size.x);
            if (offset.y != 0)
            {
                rubb = RubberDelta(offset.y, ViewBounds.size.y);
                position.y = position.y - RubberDelta(offset.y, ViewBounds.size.y);
            }


        }

        SetContentAnchoredPosition(position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        IsDragging = false;
    }

    /// <summary>
    /// 再次滑动，重置上一次速度
    /// </summary>
    /// <param name="eventData"></param>
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Velocity = Vector2.zero;
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!IsActive())
            return;

    }

    /// <summary>
    /// 设置内容框大小
    /// </summary>
    /// <param name="pos"></param>
    void SetContentAnchoredPosition(Vector2 position)
    {
        if (Direction == LayoutRule.Direction.Vertical)
            position.x = Content.anchoredPosition.x;
        if (Direction == LayoutRule.Direction.Horizontal)
            position.y = Content.anchoredPosition.y;

        if (position != Content.anchoredPosition)
            Content.anchoredPosition = position;
    }


    /// <summary>
    /// 此处处理速度变化，惯性，反弹逻辑
    /// </summary>
    private void LateUpdate()
    {
        if (!IsActive())
            return;

        UpdateBounds();

        float deltaTime = Time.unscaledDeltaTime;
        Vector2 offset = CalculateOffset(Vector2.zero);

        //已经脱手，需要考虑回弹和惯性的问题
        if (!IsDragging && (offset != Vector2.zero || Velocity != Vector2.zero))
        {
            Vector2 position = Content.anchoredPosition;
            for (int axis = 0; axis < 2; axis++)
            {
                // 获得物理弹性如果弹性运动并且内容超出了视图
                if (MoveType == MovementType.Elastic && offset[axis] != 0)
                {
                    float speed = Velocity[axis];
                    //平滑阻尼，类似弹簧
                    position[axis] = Mathf.SmoothDamp(Content.anchoredPosition[axis], Content.anchoredPosition[axis] + offset[axis], ref speed, Elasticity, Mathf.Infinity, deltaTime);
                    if (Mathf.Abs(speed) < 1)
                        speed = 0;
                    Velocity[axis] = speed;
                }
                // 惯性运动
                else if (Inertia)
                {
                    //速度根据摩擦系数递减
                    Velocity[axis] *= Mathf.Pow(DecelerationRate, deltaTime);
                    if (Mathf.Abs(Velocity[axis]) < 1)
                        Velocity[axis] = 0;
                    position[axis] += Velocity[axis] * deltaTime;
                }
                // 如果不使用惯性.
                else
                {
                    Velocity[axis] = 0;
                }

                //保证Content边界不会掉进视图内部
                if (MoveType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }
        }
        //如果是用惯性和滑动中，计算脱手前的速度
        if (IsDragging)
        {
            Vector3 newVelocity = (Content.anchoredPosition - ContentPrevPosition) / deltaTime;
            Velocity = Vector3.Lerp(Velocity, newVelocity, deltaTime * 10);
        }

        //记录上个位置和Content和ViewPort偏移
        if (ViewBounds != PrevViewBounds || ContentBounds != PrevContentBounds || Content.anchoredPosition != ContentPrevPosition)
        {
            UISystemProfilerApi.AddMarker("DynamicTableIrregular.value", this);
            OnValueChange();
            UpdatePrevData();
        }

    }

    /// <summary>
    /// 更新这一帧数据
    /// </summary>
    protected void UpdatePrevData()
    {
        if (Content == null)
            ContentPrevPosition = Vector2.zero;
        else
            ContentPrevPosition = Content.anchoredPosition;
        PrevViewBounds = ViewBounds;
        PrevContentBounds = ContentBounds;
    }


    public void StopMovement()
    {
        Velocity = Vector2.zero;
    }
    /// <summary>
    /// 更新界限
    /// </summary>
    protected void UpdateBounds()
    {
        ViewBounds = new Bounds(Viewport.rect.center, Viewport.rect.size);
        ContentBounds = GetBounds();

        if (Content == null)
            return;

        Vector3 contentSize = ContentBounds.size;
        Vector3 contentPos = ContentBounds.center;
        var contentPivot = Content.pivot;

        //当ContentBounds 小于 ViewBounds的时候要保证 ContentBounds =  ViewBounds
        AdjustBounds(ref ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
        ContentBounds.size = contentSize;
        ContentBounds.center = contentPos;

        //保证内容边界在视图内
        if (MoveType == MovementType.Clamped)
        {
            //调整内容，使内容边界底部（右侧）永远不会高于视图边界底部（右侧）。顶部（左侧）永远不会低于视图边界顶部（左侧）。如果内容缩小，所有这些都会发生。所以保证内容边界范围大于等于视图范围内
            Vector2 delta = Vector2.zero;
            if (ViewBounds.max.x > ContentBounds.max.x)
            {
                delta.x = Math.Min(ViewBounds.min.x - ContentBounds.min.x, ViewBounds.max.x - ContentBounds.max.x);
            }
            else if (ViewBounds.min.x < ContentBounds.min.x)
            {
                delta.x = Math.Max(ViewBounds.min.x - ContentBounds.min.x, ViewBounds.max.x - ContentBounds.max.x);
            }

            if (ViewBounds.min.y < ContentBounds.min.y)
            {
                delta.y = Math.Max(ViewBounds.min.y - ContentBounds.min.y, ViewBounds.max.y - ContentBounds.max.y);
            }
            else if (ViewBounds.max.y > ContentBounds.max.y)
            {
                delta.y = Math.Min(ViewBounds.min.y - ContentBounds.min.y, ViewBounds.max.y - ContentBounds.max.y);
            }
            if (delta.sqrMagnitude > float.Epsilon)
            {
                contentPos = Content.anchoredPosition + delta;
                if (Direction != LayoutRule.Direction.Horizontal)
                    contentPos.x = Content.anchoredPosition.x;
                if (Direction != LayoutRule.Direction.Vertical)
                    contentPos.y = Content.anchoredPosition.y;
                AdjustBounds(ref ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
            }
        }
    }

    // 调整包围盒
    // 确保Content跟View同样大小，如果不添加填充,可能比View小，但是滑动,还是允许的,这是一种特殊情况,只有在内容比视图大的时候才需要滑动，否则就没意义了,我们通过参照点去决定内容的缩放方向
    // 如果参照点在顶部，那么就会在往下拓展, 这也使的ContentSizeFitter可以很好地工作
    internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
    {
        Vector3 excess = viewBounds.size - contentSize;
        if (excess.x > 0)//可视区域的宽大于内容区域
        {
            contentPos.x -= excess.x * (contentPivot.x - 0.5f);
            contentSize.x = viewBounds.size.x;
        }
        if (excess.y > 0)
        {
            contentPos.y -= excess.y * (contentPivot.y - 0.5f);
            contentSize.y = viewBounds.size.y;
        }
    }


    /// <summary>
    /// 获取视图边界和Content边界的偏移，用于做反弹，橡皮筋
    /// </summary>
    /// <param name="delta">预处理偏移（下一帧的偏移）</param>
    /// <returns></returns>
    private Vector2 CalculateOffset(Vector2 delta)
    {
        return InternalCalculateOffset(ref ViewBounds, ref ContentBounds, Direction, ref delta);
    }


    /// <summary>
    /// 获取视图边界和Content边界的偏移
    /// 不规则列表的核心就是比较这两个边界的偏移做回收和实例化
    /// </summary>
    /// <param name="viewBounds">视图边界</param>
    /// <param name="contentBounds">内容边界</param>
    /// <param name="direction">方向</param>
    /// <param name="delta">预处理偏移</param>
    /// <returns></returns>
    internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, LayoutRule.Direction direction, ref Vector2 delta)
    {
        Vector2 offset = Vector2.zero;

        Vector2 min = contentBounds.min;
        Vector2 max = contentBounds.max;

        if (direction == LayoutRule.Direction.Horizontal)
        {
            min.x += delta.x;
            max.x += delta.x;
            if (min.x > viewBounds.min.x)
                offset.x = viewBounds.min.x - min.x;
            else if (max.x < viewBounds.max.x)
                offset.x = viewBounds.max.x - max.x;
        }

        if (direction == LayoutRule.Direction.Vertical)
        {
            min.y += delta.y;
            max.y += delta.y;
            if (max.y < viewBounds.max.y)
                offset.y = viewBounds.max.y - max.y;
            else if (min.y > viewBounds.min.y)
                offset.y = viewBounds.min.y - min.y;
        }

        return offset;
    }



    /// <summary>
    /// 重绘的时候更新
    /// </summary>
    public void SetLayoutHorizontal()
    {
        ViewBounds = new Bounds(Viewport.rect.center, Viewport.rect.size);
        ContentBounds = new Bounds();
    }

    /// <summary>
    /// 重绘的时候更新
    /// </summary>
    public void SetLayoutVertical()
    {
        ViewBounds = new Bounds(Viewport.rect.center, Viewport.rect.size);
        ContentBounds = new Bounds();
    }

    /// <summary>
    /// 用于获取Content四个顶点位置
    /// </summary>
    private readonly Vector3[] m_Corners = new Vector3[4];
    private Bounds GetBounds()
    {
        if (Content == null)
            return new Bounds();
        Content.GetWorldCorners(m_Corners);
        var viewWorldToLocalMatrix = Viewport.worldToLocalMatrix;
        return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
    }

    /// <summary>
    /// 将Content的四个顶点世界坐标转换到Viewport的本地坐标
    /// </summary>
    /// <param name="corners">四个顶点</param>
    /// <param name="viewWorldToLocalMatrix">变幻矩阵</param>
    /// <returns></returns>
    internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
    {
        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int j = 0; j < 4; j++)
        {
            Vector3 v = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);//吞掉那个点，吞掉四个点就是一个四边形区域
        return bounds;
    }

    /// <summary>
    /// 橡皮筋，用于拖拽反弹
    /// </summary>
    /// <param name="overStretching"></param>
    /// <param name="viewSize"></param>
    /// <returns></returns>
    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    #endregion

    #region 动态列表操作相关

    protected override void Awake()
    {
        PreLoadStack = new Stack<DynamicGrid>();

        Init();
    }

    void Init()
    {
        if (!IsActive())
            return;

        Content.anchorMax = new Vector2(0, 1);
        Content.anchorMin = new Vector2(0, 1);
        ObjectPool = Content.GetComponent<ObjectPools>();
        SetViewSize(rectTransform.rect.size);
    }

    /// <summary>
    /// 设置可视范围
    /// </summary>
    /// <param name="size"></param>
    public virtual void SetViewSize(Vector2 size)
    {
        if (Viewport != null)
        {
            Viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            Viewport.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }
        ViewSize = size;
    }

    /// <summary>
    /// 同步重載
    /// </summary>
    /// <param name="startIndex"></param>
    public virtual void ReloadDataSync(int startIndex = -1)
    {
        if (!IsActive())
            return;

        if (IsAsyncLoading)
        {
            StopAllCoroutines();
        }

        StopMovement();

        //重置内容框大小
        ResetContentSize();
        //回收所有节点
        RecycleAllGrids();
        //重置索引
        ResetStartIndex(startIndex);
        //重载节点
        ReloadGrids();
    }

    /// <summary>
    /// 异步重載
    /// </summary>
    /// <param name="startIndex"></param>
    public virtual void ReloadDataAsync(int startIndex = -1)
    {
        if (!IsActive())
            return;

        if (IsAsyncLoading)
        {
            StopAllCoroutines();
        }

        StopMovement();

        //重置内容框大小
        ResetContentSize();
        //回收所有节点
        RecycleAllGrids();
        //重置索引
        ResetStartIndex(startIndex);
        // 重载Grid
        if (!IsInitCompeleted)
            StartCoroutine(AsyncReloadGrids());
        else
            ReloadGrids();
    }

    /// <summary>
    /// 重置索引
    /// </summary>
    /// <param name="startIndex"></param>
    void ResetStartIndex(int startIndex)
    {
        //保持当前位置，只更新当前显示的Grid
        if (startIndex < START_INDEX && IsInitCompeleted)
            return;

        StartIndex = Math.Max(startIndex, START_INDEX);
        StartIndex = Math.Min(StartIndex, TotalCount);
        EndIndex = StartIndex;
    }


    /// <summary>
    /// 重载Cell
    /// </summary>
    /// <param name="prefab"></param>
    IEnumerator AsyncReloadGrids()
    {
        IsAsyncLoading = true;

        float containerSize = 0;
        bool IsGridFillToFull = true;
        int index = StartIndex;
        bool isBottom = false;

        //第一次加载先填满可视区域
        while (containerSize < GetAxis(ViewSize))
        {
            //没有铺满可视区域
            if (TotalCount < index)
            {
                IsGridFillToFull = false;
                break;
            }

            EndIndex = index;

            DynamicGrid grid = DynamicGridAtIndex(EndIndex);
            if (grid == null)
                break;

            if (!IsReverse)
                PushGridTail(grid);
            else
                PushGridHead(grid);

            yield return new WaitForEndOfFrame();

            index++;
            containerSize += GetAxis(grid.GetSize());
        }

        //假如已经填到末尾还没填满，就往回填
        if (!IsGridFillToFull && StartIndex > START_INDEX)
        {
            IsGridFillToFull = true;
            isBottom = true;
            while (containerSize < GetAxis(ViewSize))
            {

                StartIndex = StartIndex - 1;

                DynamicGrid grid = DynamicGridAtIndex(StartIndex);
                if (grid == null)
                    break;

                if (IsReverse)
                    PushGridTail(grid);
                else
                    PushGridHead(grid);

                containerSize += GetAxis(grid.GetSize());

                //没有铺满可视区域
                if (StartIndex == START_INDEX)
                {
                    IsGridFillToFull = false;
                    break;
                }

                yield return new WaitForEndOfFrame();

            }
        }

        if (!IsReverse)
        {
            if (IsGridFillToFull && !isBottom)
            {
                SetContentAnchoredPosition(Vector2.zero);
            }
            else
            {
                SetContentAnchoredPosition(Content.rect.size - ViewSize);
            }
        }
        else
        {
            if (IsGridFillToFull && !isBottom)
            {
                SetContentAnchoredPosition(Content.rect.size - ViewSize);
            }
            else
            {
                SetContentAnchoredPosition(Vector2.zero);
            }
        }

        IsInitCompeleted = true;
        IsAsyncLoading = false;
        OnTableGridReloadCompleted();

    }


    /// <summary>
    /// 重载Cell
    /// </summary>
    /// <param name="prefab"></param>
    private void ReloadGrids()
    {
        float containerSize = 0;
        bool IsGridFillToFull = true;
        int gridCount = 0;
        int index = StartIndex;
        bool isBottom = false;
        //第一次加载先填满可视区域
        while (containerSize < GetAxis(ViewSize))
        {
            //没有铺满可视区域
            if (TotalCount < index)
            {
                IsGridFillToFull = false;
                break;
            }

            EndIndex = index;

            DynamicGrid grid = DynamicGridAtIndex(EndIndex);
            if (grid == null)
                break;

            if (!IsReverse)
                PushGridTail(grid);
            else
                PushGridHead(grid);

            index++;
            gridCount++;
            containerSize += GetAxis(grid.GetSize());
        }
        //假如已经填到末尾还没填满，就往回填
        if (!IsGridFillToFull && StartIndex > START_INDEX)
        {
            IsGridFillToFull = true;
            isBottom = true;
            while (containerSize < GetAxis(ViewSize))
            {
      
                StartIndex = StartIndex - 1;

                DynamicGrid grid = DynamicGridAtIndex(StartIndex);
                if (grid == null)
                    break;

                if (IsReverse)
                    PushGridTail(grid);
                else
                    PushGridHead(grid);

                containerSize += GetAxis(grid.GetSize());

                //没有铺满可视区域
                if (StartIndex == START_INDEX)
                {
                    IsGridFillToFull = false;
                    break;
                }
            }
        }

        if (!IsReverse)
        {
            if (IsGridFillToFull && !isBottom)
            {
                SetContentAnchoredPosition(Vector2.zero);
            }
            else
            {
                SetContentAnchoredPosition(Content.rect.size - ViewSize);
            }


        }
        else
        {
            if (IsGridFillToFull && !isBottom)
            {
                SetContentAnchoredPosition(Content.rect.size - ViewSize);
            }
            else
            {
                SetContentAnchoredPosition(Vector2.zero);
            }
        }

        IsInitCompeleted = true;
        OnTableGridReloadCompleted();
    }


    /// <summary>
    /// 根据Index更新Grid
    /// </summary>
    /// <param name="index">索引</param>
    /// <returns></returns>
    public virtual DynamicGrid DynamicGridAtIndex(int index)
    {
        //此处派发事件到Hotfix，Hotfix预加载特定类型的Grid，因为只有逻辑才知道这个索引的Grid的类型
        OnTableGridAtIndex(index);

        //从预加载缓存中获取Grid
        DynamicGrid grid = GetGridFromPreLoadStack(index);
        if (grid == null)
        {
            Debug.LogErrorFormat("PreLoadStack load grid at index {0} fail!", index);
            return null;
        }

        grid.Index = index;

        //更新兼容距离，一方面为了做回收距离做判断，另一方面做进度条需要用到
        MaxGridMainAxisSize = Mathf.Max(MaxGridMainAxisSize, (int)GetAxis(grid.GetSize()));
        CompatibleDistance = (int)MaxGridMainAxisSize + MIN_COMPATIBLE_DISTANCE;

        //设置可视
        if (!grid.gameObject.activeSelf)
            grid.gameObject.SetActive(true);

        return grid;
    }


    /// <summary>
    /// 缓存中获取动态Gird
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    DynamicGrid GetGridFromPreLoadStack(int index)
    {
        //此处出现报错，说明加载前没预载Grid
        if (PreLoadStack == null || PreLoadStack.Count <= 0)
        {
            Debug.LogError("PreLoadStack is null or empty,please preload the grid of current index type first.");
            return null;
        }

        DynamicGrid grid = PreLoadStack.Pop();

        //预载顺序出错，索引不对应
        if (grid == null || grid.Index != index)
            Debug.LogError("DynamicGrid is null or PreLoadStack peloaded order is wrong.");

        return grid;
    }

    /// <summary>
    /// 从池中预加载,此方法在Hotfix中调用，必须对应 预加载->加载的索引
    /// </summary>
    /// <param name="poolName"></param>
    /// <param name="obj"></param>
    public virtual RectTransform PreDequeueGrid(string poolName, int index)
    {
        GameObject obj = null;
        PoolObject poolObject = ObjectPool.Spawn(poolName);

        if (poolObject != null)
            obj = poolObject.gameObject;

        if (obj == null)
        {
            Debug.LogErrorFormat("ObjectPool of Key:{0} init fail.", poolName);
            return null;
        }

        RectTransform trans = (RectTransform)obj.transform;
        DynamicGrid grid = obj.GetComponent<DynamicGrid>();
        if (grid == null)
            grid = obj.AddComponent<DynamicGrid>();

        grid.Index = index;

        trans.SetParent(Content, false);
        trans.localPosition = Vector3.zero;
        obj.SetActive(true);

        //内嵌套点击事件 Ps:不规则列表建议点击事件在对应节点上做，当然只是建议
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

        PreLoadStack.Push(grid);

        return trans;
    }

    /// <summary>
    /// 回收所有节点
    /// </summary>
    public void RecycleAllGrids()
    {
        if (UsingGridSet == null || UsingGridSet.Count <= 0)
            return;

        for (int i = 0; i < UsingGridSet.Count; i++)
        {
            TableGridRecycle(UsingGridSet[i]);
        }
        UsingGridSet.Clear();
    }

    /// <summary>
    /// 清理
    /// </summary>
    public void Clear()
    {
        StopMovement();

        RecycleAllGrids();
    }


    /// <summary>
    /// 回收节点
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public void TableGridRecycle(DynamicGrid grid)
    {
        if (grid == null)
            return;

        OnTableGridRecycleIndex(grid.Index);
        ObjectPool.DespawnByObject(grid.gameObject);
    }


    /// <summary>
    /// 从头部添加节点，默认是左上正方向（水平就是左，垂直就是上）
    /// </summary>
    /// <param name="grid"></param>
    void PushGridHead(DynamicGrid grid)
    {
        if (grid == null)
            return;

        //获取本次压入容器的偏移
        Vector2 contentOffset = GetVector(grid.GetSize() + Spacing);
        //由于水平和垂直的上下界限的差异，这里需要取反
        Vector2 contentOffsetWithDirection = GetVectorWithDirection(contentOffset);
        //设置容器大小
        SetContentSize(Content.rect.size + contentOffset);

        //遍历所有使用中的节点偏移
        for (int index = 0; index < UsingGridSet.Count; index++)
        {
            DynamicGrid usingGird = UsingGridSet[index];
            usingGird.rectTransform.anchoredPosition -= contentOffsetWithDirection;
        }
        //放入头部
        UsingGridSet.Insert(0, grid);
        //因为内容框相对锚点扩大了，内容需要做位置上的偏移，否则会跳
        SetContentAnchoredPosition(Content.anchoredPosition + contentOffsetWithDirection);
        //设置节点位置
        SetGridAlong(grid, true);
        //更新所有节点占得大小
        AllGridsSizeWithSpace += contentOffset;
    }

    /// <summary>
    /// 尾部推出
    /// </summary>
    /// <returns></returns>
    DynamicGrid PopGridTail()
    {
        if (UsingGridSet == null || UsingGridSet.Count <= 0)
            return null;

        DynamicGrid grid = GetTailGrid();
        if (grid == null)
            return null;

        //获取本次压入容器的偏移
        Vector2 contentOffset = GetVector(grid.GetSize() + Spacing);
        //设置容器大小
        SetContentSize(Content.rect.size - contentOffset);
        //移除
        UsingGridSet.Remove(grid);
        //更新所有节点占得大小
        AllGridsSizeWithSpace -= contentOffset;

        return grid;
    }

    /// <summary>
    /// 上边界推出
    /// </summary>
    /// <returns></returns>
    DynamicGrid PopGridHead()
    {
        if (UsingGridSet == null || UsingGridSet.Count <= 0)
            return null;

        DynamicGrid grid = GetHeadGrid();
        if (grid == null)
            return null;

        //获取本次压入容器的偏移
        Vector2 contentOffset = GetVector(grid.GetSize() + Spacing);
        //由于水平和垂直的上下界限的差异，这里需要取反
        Vector2 contentOffsetWithDirection = GetVectorWithDirection(contentOffset);
        //设置容器大小
        SetContentSize(Content.rect.size - contentOffset);

        UsingGridSet.RemoveAt(0);

        //修正位置
        for (int index = 0; index < UsingGridSet.Count; index++)
        {
            UsingGridSet[index].rectTransform.anchoredPosition += contentOffsetWithDirection;
        }
        //因为内容框相对锚点变化了，内容需要做位置上的偏移，否则会跳
        SetContentAnchoredPosition(Content.anchoredPosition - contentOffsetWithDirection);

        AllGridsSizeWithSpace -= contentOffset;
        return grid;
    }

    /// <summary>
    /// 尾部加入节点(右下)(默认是左上正方向（水平就是左，垂直就是上）
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    void PushGridTail(DynamicGrid grid)
    {
        if (grid == null)
            return;

        //获取本次压入容器的偏移
        Vector2 contentOffset = GetVector(grid.GetSize() + Spacing);
        //设置容器大小
        SetContentSize(Content.rect.size + contentOffset);
        //设置位置
        SetGridAlong(grid, false);
        //放入尾部
        UsingGridSet.Add(grid);

        AllGridsSizeWithSpace += contentOffset;
    }

    /// <summary>
    /// 设置内容的大小
    /// </summary>
    /// <param name="vec2"></param>
    protected void SetContentSize(Vector2 vec2)
    {
        if (Content.rect.size != vec2)
        {
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vec2.x);
            Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vec2.y);
        }
    }

    /// <summary>
    /// 设置Gird的位置
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="IsHead">是否是上边界</param>
    protected virtual void SetGridAlong(DynamicGrid grid, bool IsHead)
    {
        if (grid == null)
            return;

        Vector2 gridSize = grid.GetSize();
        Vector2 startOffset = CalulateStartOffest(gridSize);

        //Grid的位置需要加上最后一个节点的位置
        if (!IsHead)
            startOffset += AllGridsSizeWithSpace;

        /*相对于父节点的停靠位置*/
        grid.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, startOffset[0], gridSize[0]);
        grid.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, startOffset[1], gridSize[1]);
    }


    /// <summary>
    /// 回收节点
    /// </summary>
    /// <param name="trans"></param>
    /// <returns></returns>
    public DynamicGrid TableCellRecycle(DynamicGrid grid)
    {
        if (grid == null)
            return null;

        OnTableGridRecycleIndex(grid.Index);
        ObjectPool.DespawnByObject(grid.gameObject);

        return grid;
    }


    /// <summary>
    /// 获取尾部节点
    /// </summary>
    /// <returns></returns>
    public DynamicGrid GetTailGrid()
    {
        if (UsingGridSet == null || UsingGridSet.Count <= 0)
            return null;

        return UsingGridSet[UsingGridSet.Count - 1];
    }

    /// <summary>
    /// 获取尾部节点
    /// </summary>
    /// <returns></returns>
    public DynamicGrid GetHeadGrid()
    {
        if (UsingGridSet == null || UsingGridSet.Count <= 0)
            return null;

        return UsingGridSet[0];
    }

    /// <summary>
    /// 获取头部节点大小
    /// </summary>
    /// <returns></returns>
    protected Vector2 GetHeadSize()
    {
        DynamicGrid grid = GetHeadGrid();
        if (grid == null)
            return Vector2.zero;
        return grid.GetSize();
    }


    /// <summary>
    /// 获取尾部节点大小
    /// </summary>
    /// <returns></returns>
    protected Vector2 GetTailSize()
    {
        DynamicGrid grid = GetTailGrid();
        if (grid == null)
            return Vector2.zero;
        return grid.GetSize();
    }

    /// <summary>
    /// 上边界压入
    /// </summary>
    protected void PushHead()
    {
        //当前要更新的索引
        int index = IsReverse ? EndIndex + 1 : StartIndex - 1;

        //检查越界
        if (CheckIndexOutLine(index))
            return;

        //从池中获取grid
        DynamicGrid grid = DynamicGridAtIndex(index);
        //压入
        PushGridHead(grid);

        if (grid == null)
            return;
        //如果是拖拽中需要改变内容的锚点位置，这个滑动开始位置要加上偏移，不然会影响速度
        ContentStartPosition += GetVectorWithDirection(grid.GetSize());

        //更新索引
        if (!IsReverse)
            StartIndex = index;
        else
            EndIndex = index;
    }

    /// <summary>
    /// 下边界推出
    /// </summary>
    public void PushTail()
    {
        //当前要更新的索引
        int index = IsReverse ? StartIndex - 1 : EndIndex + 1;
        // 检查越界
        if (CheckIndexOutLine(index))
            return;

        //从池中获取grid，放进子布局里
        DynamicGrid grid = DynamicGridAtIndex(index);
        PushGridTail(grid);

        // 更新索引
        if (!IsReverse)
            EndIndex = index;
        else
            StartIndex = index;
    }

    /// <summary>
    /// 上边界推出
    /// </summary>
    public void PopHead()
    {
        //当前要更新的索引
        int index = IsReverse ? EndIndex : StartIndex;

        //检查越界
        if (CheckIndexOutLine(index))
            return;

        if (!IsReverse && EndIndex == TotalCount)
            return;

        if (IsReverse && StartIndex == START_INDEX)
            return;

        //回收
        DynamicGrid grid = PopGridHead();

        if (grid == null)
            return;

        //如果是拖拽中需要改变内容的锚点位置，这个滑动开始位置要加上偏移，不然会影响速度
        ContentStartPosition -= GetVectorWithDirection(grid.GetSize());

        //回收节点，派发回收事件
        TableCellRecycle(grid);

        //更新上下边界索引
        if (!IsReverse)
            StartIndex = index + 1;
        else
            EndIndex = index - 1;
    }

    /// <summary>
    /// 从下边界推出
    /// </summary>
    public void PopTail()
    {
        /* 当前要更新的索引*/
        int index = IsReverse ? StartIndex : EndIndex;
        /* 检查越界*/
        if (CheckIndexOutLine(index))
            return;

        if (IsReverse && EndIndex == TotalCount)
            return;

        if (!IsReverse && StartIndex == START_INDEX)
            return;

        //派发回收事件
        TableCellRecycle(PopGridTail());

        //更新索引
        if (!IsReverse)
            EndIndex = index - 1;
        else
            StartIndex = index + 1;
    }

    /// <summary>
    /// 检查越界
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected bool CheckIndexOutLine(int index)
    {
        //越界
        if (index <= 0 || index > TotalCount)
            return true;

        return false;
    }


    /// <summary>
    /// 滑动中
    /// </summary>
    void OnValueChange()
    {
        //如果未初始化或者速度为 0
        if (!IsInitCompeleted || Velocity == Vector2.zero)
            return;

        //需要比较的轴（上边界）
        float viewPortAxisMax = GetAxis(ViewBounds.max);
        float contentAxisMax = GetAxis(ContentBounds.max);

        //需要比较的轴（下边界）
        float viewPortAxisMin = GetAxis(ViewBounds.min);
        float contentAxisMin = GetAxis(ContentBounds.min);

        if (GetAxis(Velocity) < 0)
        {
            //内容的上边界低于视野的上边界（兼容距离是一个偏移，实质上内容上边界必须高于视野上边界，不然会露馅）
            if (viewPortAxisMax > contentAxisMax - CompatibleDistance)
            {
                if (Direction == LayoutRule.Direction.Vertical)
                    PushHead();
                else
                    PushTail();
            }
            //内容的下边界低于视野的下边界推出节点
            if (viewPortAxisMin > contentAxisMin + CompatibleDistance)
            {
                if (Direction == LayoutRule.Direction.Vertical)
                    PopTail();
                else
                    PopHead();
            }
        }

        if (GetAxis(Velocity) > 0)
        {
            //内容上边界高于视野上边界
            if (viewPortAxisMax < contentAxisMax - CompatibleDistance)
            {
                if (Direction == LayoutRule.Direction.Vertical)
                    PopHead();
                else
                    PopTail();
            }
            //内容的下边界高于视野的下边界
            if (viewPortAxisMin < contentAxisMin + CompatibleDistance)
            {
                if (Direction == LayoutRule.Direction.Vertical)
                    PushTail();
                else
                    PushHead();
            }

        }
    }

    #endregion

    #region 事件
    /// <summary>
    /// 回收事件
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridRecycleIndex(int index)
    {
        if (DynamicTableGridDelegate == null)
            return;

        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_RECYCLE, index);
    }

    /// <summary>
    /// 更新事件
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridAtIndex(int index)
    {
        if (DynamicTableGridDelegate == null)
            return;

        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_ATINDEX, index);
    }

    /// <summary>
    /// Grid被点击
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridTouched(DynamicGrid grid, PointerEventData eventData = null)
    {
        if (DynamicTableGridDelegate == null)
            return;

        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_TOUCHED, grid.Index);
    }

    /// <summary>
    /// 加载完成
    /// </summary>
    /// <param name="index"></param>
    public void OnTableGridReloadCompleted()
    {
        if (DynamicTableGridDelegate == null)
            return;

        DynamicTableGridDelegate((int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_RELOAD_COMPLETED, -1);
    }

    #endregion

    #region 布局相关
    /// <summary>
    /// 计算来自于停靠，对齐的坐标偏移
    /// </summary>
    protected virtual Vector2 CalulateStartOffest(Vector2 size)
    {
        /*计算Align上的偏移*/
        Vector2 requiredSpace = Vector2.zero;
        if (Direction == LayoutRule.Direction.Horizontal)
            requiredSpace = new Vector2(requiredSpace.x + (UsingGridSet.Count - 1) * Spacing.x, size.y);
        else if (Direction == LayoutRule.Direction.Vertical)
            requiredSpace = new Vector2(size.x, requiredSpace.y + (UsingGridSet.Count - 1) * Spacing.y);

        Vector2 startOffset = new Vector2(
                GetStartOffset(0, requiredSpace.x),
                GetStartOffset(1, requiredSpace.y)
                );

        return startOffset;
    }

    /// <summary>
    /// 重置布置大小
    /// </summary>
    public void ResetContentSize()
    {
        AllGridsSizeWithSpace = Vector2.zero;
        Vector2 contentSize = Vector2.zero;
        if (Direction == LayoutRule.Direction.Vertical)
            contentSize = new Vector2(ViewSize.x, Padding.vertical - Spacing.y);
        else
            contentSize = new Vector2(Padding.horizontal - Spacing.x, ViewSize.y);

        SetContentSize(contentSize);
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
        float availableSpace = Content.rect.size[axis];
        float surplusSpace = availableSpace - requiredSpace;
        float alignmentOnAxis = 0;
        if (axis == 0 && Direction == LayoutRule.Direction.Vertical)
            alignmentOnAxis = ((int)ChildAlignment % 3) * 0.5f;
        else if (axis == 1 && Direction == LayoutRule.Direction.Horizontal)
            alignmentOnAxis = ((int)ChildAlignment / 3) * 0.5f;

        return (axis == 0 ? Padding.left : Padding.top) + surplusSpace * alignmentOnAxis;
    }

    #endregion

    #region 方向转换相关 
    /// <summary>
    /// 根据方向返回对应轴的值
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    float GetAxis(Vector2 vector)
    {
        if (Direction == LayoutRule.Direction.Vertical)
            return vector.y;
        else
            return vector.x;
    }

    Vector2 GetVectorInverse(Vector2 vec2)
    {
        if (Direction == LayoutRule.Direction.Vertical)
            return new Vector2(vec2.x, 0);
        else
            return new Vector2(0, vec2.y);
    }

    Vector2 GetVector(Vector2 vec2)
    {
        if (Direction == LayoutRule.Direction.Vertical)
            return new Vector2(0, vec2.y);
        else
            return new Vector2(vec2.x, 0);
    }

    protected Vector2 GetVectorWithDirection(Vector2 vec2)
    {
        if (Direction == LayoutRule.Direction.Vertical)
            return new Vector2(0, vec2.y);
        else
            return new Vector2(-vec2.x, 0);
    }
    #endregion
}
