using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
public class DynamicGrid : MonoBehaviour
{
    /// <summary>
    /// 索引
    /// </summary>
    public int Index = -1;

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

    public string LuaTable = "CSProxy";
    public string CS = "CSProxy";

    public Vector2 GetSize()
    {
        return rectTransform.rect.size;
    }
}
