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
    /// RectTransform
    /// </summary>
    [System.NonSerialized]
    public RectTransform rectTransform;

    public string LuaTable = "CSProxy";
    public string CS = "CSProxy";

    private void Awake()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
    }

    public Vector2 GetSize()
    {
        return rectTransform.rect.size;
    }
}
