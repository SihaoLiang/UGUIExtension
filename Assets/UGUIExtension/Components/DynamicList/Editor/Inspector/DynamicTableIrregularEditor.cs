using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DynamicTableIrregular), true)]
[CanEditMultipleObjects]
public class DynamicTableIrregularEditor : Editor
{
    protected SerializedProperty Content;
    protected SerializedProperty Viewport;
    protected SerializedProperty Direction;
    protected SerializedProperty MoveType;
    protected SerializedProperty Elasticity;
    protected SerializedProperty Inertia;
    protected SerializedProperty DecelerationRate;
    protected SerializedProperty HorizontalScrollbar;
    protected SerializedProperty VerticalScrollbar;

    protected SerializedProperty TotalCount;
    protected SerializedProperty ViewSize;
    protected SerializedProperty CompatibleDistance;
    protected SerializedProperty IsGridTouchEventEnable;

    protected SerializedProperty IsReverse;
    protected SerializedProperty ChildAlignment;
    protected SerializedProperty Padding;
    protected SerializedProperty Space;

    protected SerializedProperty ObjectPool;


    protected GUIContent ContentContent;
    protected GUIContent ViewportContent;
    protected GUIContent DirectionContent;
    protected GUIContent MoveTypeContent;
    protected GUIContent ElasticityContent;
    protected GUIContent InertiaContent;
    protected GUIContent DecelerationRateContent;

    protected GUIContent TotalCountContent;
    protected GUIContent ViewSizeContent;
    protected GUIContent CompatibleDistanceContent;
    protected GUIContent IsGridTouchEventEnableContent;

    protected GUIContent IsReverseContent;
    protected GUIContent ChildAlignmentContent;
    protected GUIContent PaddingContent;
    protected GUIContent SpaceContent;
    protected GUIContent ObjectPoolContent;

    protected GUIContent ClearButtonContent;
    protected GUIContent CorrectButtonContent;


    protected GUIContent HorizontalScrollbarContent;
    protected GUIContent VerticalScrollbarContent;
    protected virtual void OnEnable()
    {
        Content = serializedObject.FindProperty("Content");
        Viewport = serializedObject.FindProperty("Viewport");
        Direction = serializedObject.FindProperty("Direction");
        MoveType = serializedObject.FindProperty("MoveType");
        Inertia = serializedObject.FindProperty("Inertia");
        Elasticity = serializedObject.FindProperty("Elasticity");
        DecelerationRate = serializedObject.FindProperty("DecelerationRate");
        HorizontalScrollbar = serializedObject.FindProperty("m_HorizontalScrollbar");
        VerticalScrollbar = serializedObject.FindProperty("m_VerticalScrollbar");

        TotalCount = serializedObject.FindProperty("TotalCount");
        ViewSize = serializedObject.FindProperty("ViewSize");
        CompatibleDistance = serializedObject.FindProperty("CompatibleDistance");
        IsGridTouchEventEnable = serializedObject.FindProperty("IsGridTouchEventEnable");

        IsReverse = serializedObject.FindProperty("IsReverse");
        ChildAlignment = serializedObject.FindProperty("ChildAlignment");
        Padding = serializedObject.FindProperty("Padding");
        Space = serializedObject.FindProperty("Spacing");

        ObjectPool = serializedObject.FindProperty("ObjectPool");


        ContentContent = new GUIContent("内容");
        ViewportContent = new GUIContent("视窗");
        DirectionContent = new GUIContent("布局方向");
        MoveTypeContent = new GUIContent("活动类型");
        ElasticityContent = new GUIContent("弹力系数");
        InertiaContent = new GUIContent("使用惯性");
        DecelerationRateContent = new GUIContent("摩擦系数");

        TotalCountContent = new GUIContent("总节点数");
        ViewSizeContent = new GUIContent("可视区域");
        CompatibleDistanceContent = new GUIContent("兼容距离");
        IsGridTouchEventEnableContent = new GUIContent("节点点击");

        ChildAlignmentContent = new GUIContent("布局停靠");
        SpaceContent = new GUIContent("布局间距");
        PaddingContent = new GUIContent("布局四周间距");
        IsReverseContent = new GUIContent("布局反向");

        ObjectPoolContent = new GUIContent("对象池");

        ClearButtonContent = new GUIContent("清空动态节点", "清空动态节点");
        CorrectButtonContent = new GUIContent("重载动态节点", "重载动态节点");

        HorizontalScrollbarContent = new GUIContent("横向进度条");
        VerticalScrollbarContent = new GUIContent("纵向进度条");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //滑动
        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("滑动属性", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Content, ContentContent, true);
        EditorGUILayout.PropertyField(Viewport, ViewportContent, true);
        EditorGUILayout.PropertyField(MoveType, MoveTypeContent, true);
        if (MoveType.enumValueIndex == 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(Elasticity, ElasticityContent, true);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.PropertyField(Inertia, InertiaContent, true);
        if (Inertia.boolValue)
            EditorGUILayout.PropertyField(DecelerationRate, DecelerationRateContent, true);


        EditorGUILayout.PropertyField(VerticalScrollbar, VerticalScrollbarContent, true);
        EditorGUILayout.PropertyField(HorizontalScrollbar, HorizontalScrollbarContent, true);

        EditorGUILayout.EndVertical();

        //基础信息
        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("基础属性", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(TotalCount, TotalCountContent, true);
        EditorGUILayout.PropertyField(ViewSize, ViewSizeContent, true);
        EditorGUILayout.PropertyField(CompatibleDistance, CompatibleDistanceContent, true);
        EditorGUILayout.PropertyField(IsGridTouchEventEnable, IsGridTouchEventEnableContent, true);
        EditorGUILayout.EndVertical();
      

        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("动态布局", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(IsReverse, IsReverseContent, true);
        EditorGUILayout.PropertyField(Direction, DirectionContent, true);
        EditorGUILayout.PropertyField(ChildAlignment, ChildAlignmentContent, true);
        EditorGUILayout.PropertyField(Space, SpaceContent, true);

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(Padding, PaddingContent, true);
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("对象池", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(ObjectPool, ObjectPoolContent, true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(ClearButtonContent))
        {
            foreach (DynamicTableIrregular dynamicTable in targets.Select(obj => obj as DynamicTableIrregular))
            {
                dynamicTable.Clear();
                EditorUtility.SetDirty(dynamicTable);
            }
        }
        if (GUILayout.Button(CorrectButtonContent))
        {
            foreach (DynamicTableIrregular dynamicTable in targets.Select(obj => obj as DynamicTableIrregular))
            {
                dynamicTable.ReloadDataSync(1);
                EditorUtility.SetDirty(dynamicTable);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }
}
