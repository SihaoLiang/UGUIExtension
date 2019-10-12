using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicTableCurve), true)]
[CanEditMultipleObjects]
public class DynamicTableCurveEditor : Editor
{
    protected SerializedProperty PositionCurve;
    protected SerializedProperty ScaleCurve;
    protected SerializedProperty DepthCurve;

    protected SerializedProperty Direction;
    protected SerializedProperty Order;
    protected SerializedProperty MoveType;
    protected SerializedProperty Elasticity;
    protected SerializedProperty ElasticRate;

    protected SerializedProperty ViewSize;
    protected SerializedProperty TotalCount;
    protected SerializedProperty Grid;
    protected SerializedProperty IsGridTouchEventEnable;
    protected SerializedProperty Content;
    protected SerializedProperty Viewport;

    protected SerializedProperty GridSize;
    protected SerializedProperty OriginGridSize;
    protected SerializedProperty GridStretching;

    protected SerializedProperty DragFactor;
    protected SerializedProperty StartIndex;
    protected SerializedProperty CentralIndex;
    protected SerializedProperty AxisOffset;

    protected SerializedProperty IsNeedTweenToFix;
    protected SerializedProperty LerpDuration;

    protected SerializedProperty Inertia;
    protected SerializedProperty FixMoveType;
    protected SerializedProperty ForceTweenVelocity;

    protected SerializedProperty DecelerationRate;
    protected SerializedProperty SpaceRate;
    protected SerializedProperty UseViewportSize;


    protected GUIContent PositionCurveContent;
    protected GUIContent ScaleCurveContent;
    protected GUIContent DepthCurveContent;
    protected GUIContent MoveTypeContent;
    protected GUIContent ElasticityContent;
    protected GUIContent ElasticRateContent;

    protected GUIContent ContentContent;
    protected GUIContent ViewportContent;
    protected GUIContent GridContent;
    protected GUIContent IsGridTouchEventEnableContent;
    protected GUIContent TotalCountContent;
    protected GUIContent ViewSizeContent;
    protected GUIContent DirectionContent;

    protected GUIContent GridSizeContent;
    protected GUIContent OriginGridSizeContent;
    protected GUIContent GridStretchingContent;

    protected GUIContent OrderContent;
    protected GUIContent DragFactorContent;
    protected GUIContent StartIndexContent;
    protected GUIContent CentralIndexContent;
    protected GUIContent AxisOffsetContent;

    protected GUIContent IsNeedTweenToFixContent;
    protected GUIContent LerpDurationContent;


    protected GUIContent FixMoveTypeContent;
    protected GUIContent ForceTweenVelocityContent;
    protected GUIContent InertiaContent;
    protected GUIContent DecelerationRateContent;
    protected GUIContent SpaceRateContent;
    protected GUIContent UseViewportSizeContent;

    protected GUIContent ClearButtonContent;
    protected GUIContent CorrectButtonContent;

    protected virtual void OnEnable()
    {
        PositionCurve = serializedObject.FindProperty("PositionCurve");
        ScaleCurve = serializedObject.FindProperty("ScaleCurve");
        DepthCurve = serializedObject.FindProperty("DepthCurve");

        Content = serializedObject.FindProperty("Content");
        Viewport = serializedObject.FindProperty("Viewport");
        Direction = serializedObject.FindProperty("Direction");
        Order = serializedObject.FindProperty("Order");
        MoveType = serializedObject.FindProperty("MoveType");
        Elasticity = serializedObject.FindProperty("Elasticity");
        ElasticRate = serializedObject.FindProperty("ElasticRate");

        TotalCount = serializedObject.FindProperty("TotalCount");
        ViewSize = serializedObject.FindProperty("ViewSize");

        Grid = serializedObject.FindProperty("Grid");
        GridSize = serializedObject.FindProperty("GridSize");
        OriginGridSize = serializedObject.FindProperty("OriginGridSize");
        GridStretching = serializedObject.FindProperty("GridStretching");
        IsGridTouchEventEnable = serializedObject.FindProperty("IsGridTouchEventEnable");



        DragFactor = serializedObject.FindProperty("DragFactor");
        StartIndex = serializedObject.FindProperty("StartIndex");
        CentralIndex = serializedObject.FindProperty("CentralIndex");
        AxisOffset = serializedObject.FindProperty("AxisOffset");

        IsNeedTweenToFix = serializedObject.FindProperty("IsNeedTweenToFix");
        LerpDuration = serializedObject.FindProperty("LerpDuration");

        Inertia = serializedObject.FindProperty("Inertia");

        FixMoveType = serializedObject.FindProperty("FixMoveType");
        ForceTweenVelocity = serializedObject.FindProperty("ForceTweenVelocity");

        DecelerationRate = serializedObject.FindProperty("DecelerationRate");
        SpaceRate = serializedObject.FindProperty("SpaceRate");
        UseViewportSize = serializedObject.FindProperty("UseViewportSize");

        PositionCurveContent = new GUIContent("位置曲线");
        ScaleCurveContent = new GUIContent("缩放曲线");
        DepthCurveContent = new GUIContent("深度曲线");

        DirectionContent = new GUIContent("布局方向");
        OrderContent = new GUIContent("正反向");

        ElasticityContent = new GUIContent("弹力系数");
        ElasticRateContent = new GUIContent("反弹边界系数");

        MoveTypeContent = new GUIContent("滑动类型");
        ContentContent = new GUIContent("内容");
        ViewportContent = new GUIContent("视窗");
        ViewSizeContent = new GUIContent("可视区域");
        TotalCountContent = new GUIContent("总节点数");
        GridContent = new GUIContent("动态节点");
        IsGridTouchEventEnableContent = new GUIContent("节点点击");

        GridSizeContent = new GUIContent("节点大小");
        OriginGridSizeContent = new GUIContent("节点原始大小");
        GridStretchingContent = new GUIContent("节点适配拉伸");

        DragFactorContent = new GUIContent("滑动系数（影响滑动速度）");
        StartIndexContent = new GUIContent("开始索引");
        CentralIndexContent = new GUIContent("聚焦位置（影响缓存个数）");
        AxisOffsetContent = new GUIContent("副轴偏移");

        IsNeedTweenToFixContent = new GUIContent("自动修正");
        LerpDurationContent = new GUIContent("修正时间");

        InertiaContent = new GUIContent("使用惯性");
        ForceTweenVelocityContent = new GUIContent("强制修正速度");
        FixMoveTypeContent = new GUIContent("修正类型");

        DecelerationRateContent = new GUIContent("摩擦系数");
        SpaceRateContent = new GUIContent("间隙因子（影响间隔）");
        UseViewportSizeContent = new GUIContent("使用视图大小");


        ClearButtonContent = new GUIContent("清空动态节点", "清空动态节点");
        CorrectButtonContent = new GUIContent("重载动态节点", "重载动态节点");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("曲线", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(PositionCurve, PositionCurveContent, true);
        EditorGUILayout.PropertyField(ScaleCurve, ScaleCurveContent, true);
        EditorGUILayout.PropertyField(DepthCurve, DepthCurveContent, true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("基础属性", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Content, ContentContent, true);
        EditorGUILayout.PropertyField(Viewport, ViewportContent, true);
        EditorGUILayout.PropertyField(TotalCount, TotalCountContent, true);
        EditorGUILayout.PropertyField(UseViewportSize, UseViewportSizeContent, true);
        if (!UseViewportSize.boolValue)
            EditorGUILayout.PropertyField(ViewSize, ViewSizeContent, true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("动态节点", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Grid, GridContent, true);
        if (GUI.changed)
        {
            DynamicTableCurve dynamicTableCurve = target as DynamicTableCurve;
            if (Grid.objectReferenceValue != null)
            {
                DynamicGrid dynamicGrid = Grid.objectReferenceValue as DynamicGrid;
                dynamicTableCurve.OriginGridSize = dynamicGrid.rectTransform.rect.size;
                OriginGridSize.vector2Value = dynamicGrid.rectTransform.rect.size;
            }
            else
            {
                OriginGridSize.vector2Value = Vector2.one;
            }

        }
        EditorGUILayout.PropertyField(GridStretching, GridStretchingContent, true);
        EditorGUILayout.PropertyField(OriginGridSize, OriginGridSizeContent, true);
        EditorGUILayout.PropertyField(GridSize, GridSizeContent, true);

        EditorGUILayout.PropertyField(IsGridTouchEventEnable, IsGridTouchEventEnableContent, true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("显示因数", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(ViewSize, ViewSizeContent, true);
        EditorGUILayout.PropertyField(Direction, DirectionContent, true);
        EditorGUILayout.PropertyField(Order, OrderContent, true);
        EditorGUILayout.PropertyField(MoveType, MoveTypeContent, true);
        if (MoveType.enumValueIndex == 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(Elasticity, ElasticityContent, true);
            EditorGUILayout.PropertyField(ElasticRate, ElasticRateContent, true);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(StartIndex, StartIndexContent, true);
        EditorGUILayout.PropertyField(CentralIndex, CentralIndexContent, true);
        EditorGUILayout.PropertyField(DragFactor, DragFactorContent, true);
        EditorGUILayout.PropertyField(AxisOffset, AxisOffsetContent, true);
        EditorGUILayout.PropertyField(SpaceRate, SpaceRateContent, true);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("修正", "ShurikenModuleTitle");


        //EditorGUILayout.BeginVertical("box");
        //GUILayout.Label("惯性", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(FixMoveType, FixMoveTypeContent, true);


       // EditorGUILayout.PropertyField(Inertia, InertiaContent, true);
        if (FixMoveType.enumValueIndex == 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(DecelerationRate, DecelerationRateContent, true);
            EditorGUI.indentLevel--;
        }
        else
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ForceTweenVelocity, ForceTweenVelocityContent, true);
            EditorGUI.indentLevel--;
        }
        //EditorGUILayout.EndVertical();

        EditorGUILayout.PropertyField(IsNeedTweenToFix, IsNeedTweenToFixContent, true);
        if (IsNeedTweenToFix.boolValue)
            EditorGUILayout.PropertyField(LerpDuration, LerpDurationContent, true);
        EditorGUILayout.EndVertical();



        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(ClearButtonContent))
        {
            foreach (DynamicTableCurve dynamicTable in targets.Select(obj => obj as DynamicTableCurve))
            {
                dynamicTable.Clear();
                EditorUtility.SetDirty(dynamicTable);
            }
        }
        if (GUILayout.Button(CorrectButtonContent))
        {
            foreach (DynamicTableCurve dynamicTable in targets.Select(obj => obj as DynamicTableCurve))
            {
                dynamicTable.ReloadData();
                EditorUtility.SetDirty(dynamicTable);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

    }
}
