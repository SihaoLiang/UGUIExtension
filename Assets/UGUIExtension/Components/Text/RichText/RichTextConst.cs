using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


[System.Serializable]
public class TextMeshInfo
{
    //渲染起点
    public Vector3 startPos;
    //4 渲染的顶点 
    public Vector3[] vertices;
    //4 渲染的uv
    public Vector2[] uv;
    //6 三角顶点顺序
    public int[] triangles;
}

/// <summary>
/// 占位符添加图片信息
/// </summary>
public class QuadPlaceholder
{
    /// <summary>
    /// 动画名字
    /// </summary>
    public string animateName;

    /// <summary>
    /// 是否是动画
    /// </summary>
    public bool isAnimate;

    /// <summary>
    /// 是否是动画
    /// </summary>
    public int rate;

    /// <summary>
    /// Pic名称
    /// </summary>
    public string sprite;

    /// <summary>
    /// 对应的字符索引
    /// </summary>
    public int index;

    /// <summary>
    /// 大小
    /// </summary>
    public Vector2 size;

    /// <summary>
    /// 长度
    /// </summary>
    public int length;
}


/// <summary>
/// 超链接信息类
/// </summary>
public class UnderLineInfo
{
    /*开始位置*/
    public int startIndex;
    /*结束位置*/
    public int endIndex;
}

/// <summary>
/// 超链接信息类
/// </summary>
public class LinkInfo
{
    /*开始位置*/
    public int startIndex;
    /*结束位置*/
    public int endIndex;
    /*ID*/
    public string id;
    public string content;
    /*包围盒*/
    public readonly List<Rect> boxes = new List<Rect>();
}


public class RichTextConst
{
    /// <summary>
    /// 匹配单图
    /// </summary>
    public static readonly Regex SpriteRegex = new Regex(@"<quad sprite=(\d*\.?\d) />", RegexOptions.Singleline);

    /// <summary>
    /// 匹配动画
    /// </summary>
    public static readonly Regex AnimateRegex = new Regex(@"<quad animate=([a-z]\d*) rate=(\d*\.?\d) />", RegexOptions.Singleline);

    /// <summary>
    /// 下划线
    /// </summary>
    public static readonly Regex UnderLineRegex = new Regex(@"<quad u width=0>(.*?)<quad u width=0>", RegexOptions.Singleline);

    /// <summary>
    /// 用正则取超链接 文本 匹配类型 颜色
    /// </summary>
    public static readonly Regex LinkRegex = new Regex(@"<quad width=0 link=(\w*\.?\w)>(.*?)(<quad link width=0>)", RegexOptions.Singleline);

    /// <summary>
    /// 剔除嵌套
    /// </summary>
    public static readonly Regex FitterRegex = new Regex(@"<(.*?)>", RegexOptions.Singleline);

    //用来剔除嵌套
    public static string UnderLineShowStr = "<u>";
    public static string UnderLineStr = "<quad u width=0>";

    public static string LinkStartShowStr = "<link=";
    public static string LinkStartStr = "<quad width=0 link=";

    public static string LinkEndShowStr = "</link>";
    public static string LinkEndStr = "<quad link width=0>";

}
