using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace TextExtend
{
    [CustomEditor(typeof(TexturePackSpriteAsset))]
    [CanEditMultipleObjects]
    public class TexturePackSpriteAssetEditor : Editor
    {
        private SerializedProperty m_spriteAtlas_prop;
        private SerializedProperty m_material_prop;
        private SerializedProperty m_spriteInfoList_prop;
        private SerializedProperty m_animateDic_prop;

        private List<string> groupOption;
        private Dictionary<string, List<SerializedProperty>> fontSpriteListDic;
        Vector2 texPos = Vector2.zero;
        int selectGroup = 0;
        Vector2 scrollPos = Vector2.zero;


        GUIStyle labelStyle;
        GUIStyle labelStyle2;

        public void OnEnable()
        {
            this.m_spriteAtlas_prop = serializedObject.FindProperty("m_SpriteSheet");
            this.m_material_prop = serializedObject.FindProperty("m_Material");
            this.m_spriteInfoList_prop = serializedObject.FindProperty("m_SpriteInfoList");
            this.m_animateDic_prop = serializedObject.FindProperty("m_AnimateDic");

            if (fontSpriteListDic != null)
                fontSpriteListDic.Clear();

            //  groupOption = new List<string>();
            fontSpriteListDic = new Dictionary<string, List<SerializedProperty>>();
            //            int len = this.m_spriteInfoList_prop.arraySize;
            //             for (int index = 0; index < len; index++)
            //             {
            //                 SerializedProperty element = this.m_spriteInfoList_prop.GetArrayElementAtIndex(index);
            //               //  string group = element.FindPropertyRelative("group").stringValue;
            //                 if (!fontSpriteListDic.ContainsKey(group))
            //                 {
            //                     groupOption.Add(group);
            //                     fontSpriteListDic.Add(group, new List<SerializedProperty>());
            //                 }
            //                 fontSpriteListDic[group].Add(element);
            //             }


        }

        void initFontStyle()
        {
            labelStyle = new GUIStyle("BoldLabel");
            labelStyle.normal.textColor = new Color(1, 1, 1, 1);
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.contentOffset = new Vector2(0, -5);

            labelStyle2 = new GUIStyle("BoldLabel");
            labelStyle2.fontStyle = FontStyle.Bold;
            labelStyle2.contentOffset = new Vector2(0, -5);

        }
        Sprite sprite = null;
        Sprite sprite1 = null;

        public override void OnInspectorGUI()
        {
            initFontStyle();
            Event current = Event.current;
            base.serializedObject.Update();
            if (DrawTextToggle("Sprite Info"))
            {
                EditorGUI.indentLevel = 1;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(this.m_spriteAtlas_prop, new GUIContent("Sprite Atlas"));
                Texture2D texture2D = this.m_spriteAtlas_prop.objectReferenceValue as Texture2D;
                if (EditorGUI.EndChangeCheck())
                {

                    if (texture2D != null)
                    {
                        Material material = this.m_material_prop.objectReferenceValue as Material;
                        if (material != null)
                        {
                            material.mainTexture = texture2D;
                        }
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.PropertyField(this.m_material_prop, new GUIContent("Default Material"), new GUILayoutOption[0]);
                string filePath = UnityEditor.AssetDatabase.GetAssetPath(texture2D);
                Object[] objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(filePath);

                for (int i = 0; i < objects.Length; i++)
                {
                    if (objects[i] is Sprite)
                    {
                        sprite = objects[i] as Sprite;
                        EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false);
                    }
                }

                if (sprite1 == null)
                    sprite1 = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(512,512),100,0,SpriteMeshType.FullRect);
                
                EditorGUILayout.ObjectField("", sprite1.texture, typeof(Texture2D), false);
                EditorGUILayout.ObjectField("", texture2D, typeof(Texture2D), false);


            }
            // 
            //             if (DrawTextToggle("Group Info"))
            //             {
            //                 selectGroup = EditorGUILayout.Popup("Group", selectGroup, groupOption.ToArray());
            //                 string groupKey = groupOption[selectGroup];
            //                 if (string.IsNullOrEmpty(groupKey) || !fontSpriteListDic.ContainsKey(groupKey))
            //                     return;
            //                 texPos = new Vector2(8, 30);
            //                 List<SerializedProperty> list = fontSpriteListDic[groupKey];
            // 
            //                 scrollPos = GUILayout.BeginScrollView(scrollPos, new GUIStyle("AnimationKeyframeBackground"));
            //                 for (int idx = 0; idx < list.Count; idx++)
            //                 {
            //                     OnFontSpriteElementGUI(list[idx]);
            //                 }
            // 
            //                 GUILayout.EndScrollView();
            //             }
            //             texPos = new Vector2(8, 30);
            //             for (int idx = 0; idx < 10; idx++)
            //             {
            //              
            //                 OnFontSpriteElementGUI(this.m_spriteInfoList_prop.GetArrayElementAtIndex(idx));
            //                
            //             }
            // 
            // OnFontSpriteElementGUI(list[idx]);
            //    EditorGUILayout.PropertyField(this.m_spriteInfoList_prop, true);

            // Dictionary<string, List<FontSprite>> fontSpriteGroupDic = this.m_spriteInfoList_prop.objectReferenceValue as Dictionary<string, List<FontSprite>>;
        }


        private void OnFontSpriteElementGUI(SerializedProperty element)
        {
            Rect rect = EditorGUILayout.BeginVertical("GroupBox");
            string name = element.FindPropertyRelative("name").stringValue;
            int hashCode = element.FindPropertyRelative("hashCode").intValue;
            float x = element.FindPropertyRelative("x").floatValue;
            float y = element.FindPropertyRelative("y").floatValue;
            float width = element.FindPropertyRelative("width").floatValue;
            float height = element.FindPropertyRelative("height").floatValue;
            Vector2 pivot = element.FindPropertyRelative("pivot").vector2Value;
            Vector2 offest = element.FindPropertyRelative("offest").vector2Value;
            SerializedProperty sprite = element.FindPropertyRelative("sprite");


            // TexturePackSprite sprite = (TexturePackSprite)element.objectReferenceValue ;

            Texture2D spriteSheet = this.m_spriteAtlas_prop.objectReferenceValue as Texture2D;
            if (spriteSheet == null)
            {
                Debug.LogWarning("Please assign a valid Sprite Atlas texture to the [" + serializedObject.targetObject.name + "] Sprite Asset.", serializedObject.targetObject);
                return;
            }


            Vector2 vector2 = new Vector2(65f, 65f);
            if (width >= height)
            {
                vector2.y = height * vector2.x / width;
            }
            else
            {
                vector2.x = width * vector2.y / height;
            }

            //  Rect texCoords = new Rect(x / (float)spriteSheet.width, y / (float)spriteSheet.height, width / (float)spriteSheet.width, height / (float)spriteSheet.height);
            ///  GUI.DrawTextureWithTexCoords(new Rect(texPos.x + 20, texPos.y + 18, vector2.x, vector2.y), spriteSheet, texCoords);
            //  Sprite sprite = Sprite.Create((Texture2D)spriteSheet, texCoords, pivot);


            EditorGUILayout.BeginHorizontal();
            //   GUILayout.Label("ID: " + id, labelStyle);
            GUILayout.Space(10);
            GUILayout.Label(name, labelStyle2);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(sprite, typeof(Sprite));

            //             GUILayout.Box("", new GUILayoutOption[2] { GUILayout.Width(80), GUILayout.Height(80) });
            //            
            //             GUILayout.Space(8);
            //             EditorGUILayout.BeginHorizontal();
            // GUILayout.Label(" Key: ", labelStyle2); GUILayout.TextField(key);
            //EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical();
            GUILayout.Label(string.Format(" Size  : W: {0}    H: {1}", width, height), labelStyle2);
            GUILayout.Label(string.Format(" Pivot : X: {0}    Y: {1}", pivot.x, pivot.y), labelStyle2);
            GUILayout.Label(string.Format(" UV   : X: {0}  Y: {1}  W: {2}  H: {3}", x, y, width, height), labelStyle2);
            // GUILayout.Label(string.Format(" Offest: X: {0}    Y: {1}", offest.x, offest.y), labelStyle2);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();


            texPos += new Vector2(0, rect.height + 10);

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
}
