using System.Collections;
using System.Collections.Generic;
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
    protected SerializedProperty ViewSize;
    protected SerializedProperty TotalCount;
    protected SerializedProperty Grid;
    protected SerializedProperty IsGridTouchEventEnable;
    protected SerializedProperty Content;
    protected SerializedProperty Viewport;

    protected SerializedProperty InteralFactor;
    protected SerializedProperty StartIndex;
    protected SerializedProperty CentralIndex;
    protected SerializedProperty AxisOffset;

    protected SerializedProperty IsNeedTweenToFix;
    protected SerializedProperty LerpDuration;

    protected SerializedProperty Inertia;
    protected SerializedProperty DecelerationRate;


    protected GUIContent PositionCurveContent;
    protected GUIContent ScaleCurveContent;
    protected GUIContent DepthCurveContent;

    protected GUIContent ContentContent;
    protected GUIContent ViewportContent;
    protected GUIContent GridContent;
    protected GUIContent IsGridTouchEventEnableContent;
    protected GUIContent TotalCountContent;
    protected GUIContent ViewSizeContent;
    protected GUIContent DirectionContent;

    protected GUIContent InteralFactorContent;
    protected GUIContent StartIndexContent;
    protected GUIContent CentralIndexContent;
    protected GUIContent AxisOffsetContent;

    protected GUIContent IsNeedTweenToFixContent;
    protected GUIContent LerpDurationContent;

    protected GUIContent InertiaContent;
    protected GUIContent DecelerationRateContent;

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
        TotalCount = serializedObject.FindProperty("TotalCount");
        ViewSize = serializedObject.FindProperty("ViewSize");

        Grid = serializedObject.FindProperty("Grid");
        IsGridTouchEventEnable = serializedObject.FindProperty("IsGridTouchEventEnable");

        InteralFactor = serializedObject.FindProperty("InteralFactor");
        StartIndex = serializedObject.FindProperty("StartIndex");
        CentralIndex = serializedObject.FindProperty("CentralIndex");
        AxisOffset = serializedObject.FindProperty("AxisOffset");

        IsNeedTweenToFix = serializedObject.FindProperty("IsNeedTweenToFix");
        LerpDuration = serializedObject.FindProperty("LerpDuration");

        Inertia = serializedObject.FindProperty("Inertia");
        DecelerationRate = serializedObject.FindProperty("DecelerationRate");


        PositionCurveContent = new GUIContent("位置曲线");
        ScaleCurveContent = new GUIContent("缩放曲线");
        DepthCurveContent = new GUIContent("深度曲线");

        DirectionContent = new GUIContent("布局方向");
        ContentContent = new GUIContent("内容");
        ViewportContent = new GUIContent("视窗");
        ViewSizeContent = new GUIContent("可视区域");
        TotalCountContent = new GUIContent("总节点数");
        GridContent = new GUIContent("动态节点");
        IsGridTouchEventEnableContent = new GUIContent("节点点击");

        InteralFactorContent = new GUIContent("间隔系数");
        StartIndexContent = new GUIContent("开始索引");
        CentralIndexContent = new GUIContent("聚焦位置");
        AxisOffsetContent = new GUIContent("副轴偏移");

        IsNeedTweenToFixContent = new GUIContent("自动修正");
        LerpDurationContent = new GUIContent("修正时间");

        InertiaContent = new GUIContent("使用惯性");
        DecelerationRateContent = new GUIContent("摩擦系数");


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
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");

        GUILayout.Label("动态节点", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Grid, GridContent, true);
        EditorGUILayout.PropertyField(IsGridTouchEventEnable, IsGridTouchEventEnableContent, true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("显示因数", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(ViewSize, ViewSizeContent, true);
        EditorGUILayout.PropertyField(Direction, DirectionContent, true);

        EditorGUILayout.PropertyField(StartIndex, StartIndexContent, true);
        EditorGUILayout.PropertyField(CentralIndex, CentralIndexContent, true);
        EditorGUILayout.PropertyField(InteralFactor, InteralFactorContent, true);
        EditorGUILayout.PropertyField(AxisOffset, AxisOffsetContent, true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("修正", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(IsNeedTweenToFix, IsNeedTweenToFixContent, true);
        if(IsNeedTweenToFix.boolValue)
            EditorGUILayout.PropertyField(LerpDuration, LerpDurationContent, true);
        EditorGUILayout.EndVertical();


        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("惯性", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Inertia, InertiaContent, true);
        if (Inertia.boolValue)
            EditorGUILayout.PropertyField(DecelerationRate, DecelerationRateContent, true);
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
