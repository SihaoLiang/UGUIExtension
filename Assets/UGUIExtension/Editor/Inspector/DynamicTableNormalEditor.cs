using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(DynamicTableNormal), true)]
[CanEditMultipleObjects]
public class DynamicTableNormalEditor : Editor
{
    protected SerializedProperty ScrRect;
    protected SerializedProperty Grid;
    protected SerializedProperty GridSize;
    protected SerializedProperty GridTouchEventEnable;

    protected SerializedProperty AvailableViewCount;
    protected SerializedProperty TotalCount;
    protected SerializedProperty ViewSize;

    protected SerializedProperty Direction;
    protected SerializedProperty StartAxis;
    protected SerializedProperty StartCorner;
    protected SerializedProperty ChildAlignment;

    protected SerializedProperty Padding;
    protected SerializedProperty Space;
    protected SerializedProperty Constraint;
    protected SerializedProperty ConstraintCount;


    protected GUIContent ScrRectContent;
    protected GUIContent GridContent;
    protected GUIContent GridSizeContent;
    protected GUIContent GridTouchEventEnableContent;

    protected GUIContent AvailableViewCountContent;
    protected GUIContent TotalCountContent;
    protected GUIContent ViewSizeContent;

    protected GUIContent DirectionContent;
    protected GUIContent StartAxisContent;
    protected GUIContent StartCornerContent;
    protected GUIContent ChildAlignmentContent;

    protected GUIContent PaddingContent;
    protected GUIContent SpaceContent;
    protected GUIContent ConstraintContent;
    protected GUIContent ConstraintCountContent;

    protected GUIContent ClearButtonContent;
    protected GUIContent CorrectButtonContent;

    protected virtual void OnEnable()
    {
        ScrRect = serializedObject.FindProperty("ScrRect");
        AvailableViewCount = serializedObject.FindProperty("AvailableViewCount");
        TotalCount = serializedObject.FindProperty("TotalCount");
        ViewSize = serializedObject.FindProperty("ViewSize");

        Grid = serializedObject.FindProperty("Grid");
        GridSize = serializedObject.FindProperty("GridSize");
        GridTouchEventEnable = serializedObject.FindProperty("IsGridTouchEventEnable");

        Direction = serializedObject.FindProperty("Direction");
        StartAxis = serializedObject.FindProperty("StartAxis");
        StartCorner = serializedObject.FindProperty("StartCorner");
        ChildAlignment = serializedObject.FindProperty("ChildAlignment");

        Padding = serializedObject.FindProperty("Padding");
        Space = serializedObject.FindProperty("Spacing");
        Constraint = serializedObject.FindProperty("Constraint");
        ConstraintCount = serializedObject.FindProperty("ConstraintCount");


        ScrRectContent = new GUIContent("滑动组件");
        TotalCountContent = new GUIContent("总数");
        AvailableViewCountContent = new GUIContent("动态节点数量");
        ViewSizeContent = new GUIContent("可视区域");

        GridContent = new GUIContent("动态节点");
        GridSizeContent = new GUIContent("节点大小");
        GridTouchEventEnableContent = new GUIContent("节点点击");

        DirectionContent = new GUIContent("布局方向");
        ChildAlignmentContent = new GUIContent("布局停靠");
        StartAxisContent = new GUIContent("布局起始轴");
        StartCornerContent = new GUIContent("布局起始位置");
        SpaceContent = new GUIContent("布局间距");
        PaddingContent = new GUIContent("布局四周间距");

        ConstraintContent = new GUIContent("布局分割类型");
        ConstraintCountContent = new GUIContent("分割个数");

        ClearButtonContent = new GUIContent("清空动态节点", "清空动态节点");
        CorrectButtonContent = new GUIContent("重载动态节点", "重载动态节点");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //基础信息
        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("基础属性", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(ScrRect, ScrRectContent, true);
        EditorGUILayout.PropertyField(TotalCount, TotalCountContent, true);
        EditorGUILayout.PropertyField(AvailableViewCount, AvailableViewCountContent, true);
        EditorGUILayout.PropertyField(ViewSize, ViewSizeContent, true);
        EditorGUILayout.EndVertical();
        //节点
        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("动态节点", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Grid, GridContent, true);
        EditorGUILayout.PropertyField(GridSize, GridSizeContent, true);
        EditorGUILayout.PropertyField(GridTouchEventEnable, GridTouchEventEnableContent, true);
        EditorGUILayout.EndVertical();

        GUILayout.Space(5f);
        EditorGUILayout.BeginVertical("box");
        GUILayout.Label("动态布局", "ShurikenModuleTitle");
        EditorGUILayout.PropertyField(Direction, DirectionContent, true);
        EditorGUILayout.PropertyField(ChildAlignment, ChildAlignmentContent, true);
        EditorGUILayout.PropertyField(StartAxis, StartAxisContent, true);
        EditorGUILayout.PropertyField(StartCorner, StartCornerContent, true);
        EditorGUILayout.PropertyField(Space, SpaceContent, true);

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(Padding, PaddingContent, true);
        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;

        EditorGUILayout.PropertyField(Constraint, ConstraintContent, true);
        EditorGUILayout.EndVertical();

        if (Constraint.enumValueIndex > 0)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(ConstraintCount, ConstraintCountContent, true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(ClearButtonContent))
        {
            foreach (DynamicTableNormal dynamicTable in targets.Select(obj => obj as DynamicTableNormal))
            {
                dynamicTable.Clear();
                EditorUtility.SetDirty(dynamicTable);
            }
        }
        if (GUILayout.Button(CorrectButtonContent))
        {
            foreach (DynamicTableNormal dynamicTable in targets.Select(obj => obj as DynamicTableNormal))
            {
                dynamicTable.ReloadDataAsync(1);
                EditorUtility.SetDirty(dynamicTable);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

}
