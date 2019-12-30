using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(BMFontAsset))]
[CanEditMultipleObjects]
public class FontAssetEditor : Editor
{
    private SerializedProperty m_SpriteAtlas;
    private SerializedProperty m_Material;
    private SerializedProperty m_spriteInfoList_prop;
    private List<string> m_GroupOption = new List<string>();
    private Dictionary<string, List<BMFontSprite>> m_FontSpriteListDic = new Dictionary<string, List<BMFontSprite>>();
    private List<BMFontSprite> m_FontNotGroupSpriteList = new List<BMFontSprite>();
    public Dictionary<string, Texture2D> SpriteDic = new Dictionary<string, Texture2D>();

    Vector2 texPos = Vector2.zero;
    int selectGroup = 0;
    Vector2 scrollPos = Vector2.zero;
    private BMFontAsset m_BMFontAsset;
    private string m_PreKey;
    public void OnEnable()
    {
        this.m_SpriteAtlas = base.serializedObject.FindProperty("spriteSheet");
        this.m_Material = base.serializedObject.FindProperty("material");
        this.m_spriteInfoList_prop = base.serializedObject.FindProperty("spriteInfoList");


        m_BMFontAsset = target as BMFontAsset;

        if (m_BMFontAsset == null)
            return;

        SetupGroup();
    }


    private void SetupGroup()
    {
        if (m_BMFontAsset == null)
            return;

        m_FontNotGroupSpriteList.Clear();
        m_FontSpriteListDic.Clear();

        int len = this.m_BMFontAsset.spriteInfoList.Count;
        for (int index = 0; index < len; index++)
        {
            BMFontSprite element = m_BMFontAsset.spriteInfoList[index];
            string group = element.group;
            if (string.IsNullOrEmpty(group))
                m_FontNotGroupSpriteList.Add(element);
            else
            {
                if (!m_FontSpriteListDic.ContainsKey(group))
                {
                    m_GroupOption.Add(group);
                    m_FontSpriteListDic.Add(group, new List<BMFontSprite>());
                }
                m_FontSpriteListDic[group].Add(element);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        Event current = Event.current;
        base.serializedObject.Update();

        if (TexturePackTool.DrawHeader("基础信息", true))
        {
            EditorGUILayout.PropertyField(this.m_SpriteAtlas, new GUIContent("合图"));
            EditorGUILayout.PropertyField(this.m_Material, new GUIContent("材质球"));
        }

        //if (DrawTextToggle("Sprite Info"))
        //{
        //    EditorGUI.indentLevel = 1;
        //    EditorGUI.BeginChangeCheck();
        //    EditorGUILayout.PropertyField(this.m_SpriteAtlas, new GUIContent("Sprite Atlas"));
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        Texture2D texture2D = this.m_SpriteAtlas.objectReferenceValue as Texture2D;
        //        if (texture2D != null)
        //        {
        //            Material material = this.m_Material.objectReferenceValue as Material;
        //            if (material != null)
        //            {
        //                material.mainTexture = texture2D;
        //            }
        //        }
        //    }
        //    GUI.enabled = true;
        //    EditorGUILayout.PropertyField(this.m_Material, new GUIContent("Default Material"), new GUILayoutOption[0]);
        //}

        if (TexturePackTool.DrawHeader("已分组", true))
        {
            selectGroup = EditorGUILayout.Popup("分组", selectGroup, m_GroupOption.ToArray());
            string groupKey = m_GroupOption[selectGroup];
            if (string.IsNullOrEmpty(groupKey) || !m_FontSpriteListDic.ContainsKey(groupKey))
                return;
            texPos = new Vector2(8, 30);
            List<BMFontSprite> list = m_FontSpriteListDic[groupKey];

            scrollPos = GUILayout.BeginScrollView(scrollPos, new GUIStyle("box"));
            for (int idx = 0; idx < list.Count; idx++)
            {
                OnFontSpriteElementGUI(list[idx]);
            }

            GUILayout.EndScrollView();
        }
    }


    private void OnFontSpriteElementGUI(BMFontSprite element)
    {
        Rect rect = EditorGUILayout.BeginVertical("box");
        float x = element.x;
        float y = element.y;
        float width = element.width;
        float height = element.height;
        Vector2 pivot = element.pivot;
        string key = element.key;

        Texture2D spriteSheet = this.m_SpriteAtlas.objectReferenceValue as Texture2D;
        if (spriteSheet == null)
        {
            Debug.LogWarning("Please assign a valid Sprite Atlas texture to the [" + serializedObject.targetObject.name + "] Sprite Asset.", serializedObject.targetObject);
            return;
        }

        Texture2D tex = null;

        if (SpriteDic.ContainsKey(element.name))
        {
            tex = SpriteDic[element.name];
        }
        else
        {
            tex = new Texture2D((int)element.width, (int)element.height);
            Color[] colors = spriteSheet.GetPixels((int)element.x, (int)element.y, (int)element.width, (int)element.height);
            tex.SetPixels(0, 0, (int)element.width, (int)element.height, colors);

            tex = TexturePackTool.ScaleTextureBilinear(tex, 2);
            tex.Apply();
            SpriteDic.Add(element.name, tex);
        }


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("ID: " + element.id);
        GUILayout.Space(10);
        GUILayout.Label(element.name);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(tex, GUILayout.Width(80), GUILayout.Height(80));
        EditorGUILayout.BeginVertical();
        GUILayout.Space(8);


        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" Group: ");
        GUILayout.TextField(element.group);
        EditorGUILayout.EndHorizontal();

   
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(" Key: ");
        element.key = EditorGUILayout.TextField(element.key);
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            if (m_FontSpriteListDic.ContainsKey(element.group))
            {
                List<BMFontSprite> tempGroup = m_FontSpriteListDic[element.group];
                for (int i = 0; i < tempGroup.Count; i++)
                {
                    if (tempGroup[i].key == element.key)
                    {
                        Debug.LogError("KEY 冲突");
                        break;
                    }
                }
            }
        }

        GUILayout.Label(string.Format(" Size  : W: {0}    H: {1}", element.width, element.height));
        GUILayout.Label(string.Format(" Pivot : X: {0}    Y: {1}", element.pivot.x, element.pivot.y));
        GUILayout.Label(string.Format(" UV   : X: {0}  Y: {1}  W: {2}  H: {3}", element.x, element.y, element.width, element.height));

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        EditorGUILayout.EndVertical();
    }



    /*文字选项开关*/
    public static bool DrawTextToggle(string title)
    {
        string key = title;
        bool state = EditorPrefs.GetBool(key, true);
        GUILayout.Space(3f);
        GUILayout.BeginHorizontal();
        /*小箭头*/
        title = "<b><size=11>" + title + "</size></b>";
        if (state) title = "\u25BC " + title;
        else title = "\u25BA " + title;

        if (!GUILayout.Toggle(true, title, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        GUILayout.Space(2f);

        if (GUI.changed) EditorPrefs.SetBool(key, state);
        GUILayout.EndHorizontal();
        return state;
    }

}
