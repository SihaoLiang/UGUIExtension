using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class AtlasNode
{
    public int padding = 1;
    public Rect rect;
    public AtlasNode[] children;
    public Texture2D texture;
    public int sortIndex;

    public AtlasNode Insert(Texture2D tex, int index)
    {
        //图片为空
        if (tex == null)
            return null;

        //判断是不是叶子节点，叶子代表已经被占有
        if (IsLeftNode())
            return null;


        if (children != null)
        {
            AtlasNode newNode = children[0].Insert(tex, index);
            if (newNode != null)
            {
                return newNode;
            }
            return children[1].Insert(tex, index);
        }

        //空间不够
        if (!IsTextureFit(tex))
        {
            return null;
        }

        //刚好放下纹理
        if (IsTexturePrefectFit(tex))
        {
            texture = tex;
            sortIndex = index;

            NewUITexturePacker.AllTNode[index] = this;
            return this;
        }

        children = new AtlasNode[2];
        children[0] = new AtlasNode();
        children[1] = new AtlasNode();

        float deltaWidth = rect.width - tex.width;
        float deltaHeight = rect.height - tex.height;

        if (deltaWidth >= deltaHeight)
        {
            children[0].rect = new Rect(rect.xMin, rect.yMin, tex.width, rect.height);
            children[1].rect = new Rect(rect.xMin + tex.width + NewUITexturePacker.Padding, rect.yMin, deltaWidth - NewUITexturePacker.Padding, rect.height);
        }
        else
        {
            children[0].rect = new Rect(rect.xMin, rect.yMin, rect.width, tex.height);
            children[1].rect = new Rect(rect.xMin, rect.yMin + tex.height + NewUITexturePacker.Padding, rect.width, deltaHeight - NewUITexturePacker.Padding);
        }


        return children[0].Insert(tex, index);
    }

    private bool IsTexturePrefectFit(Texture2D tex)
    {
        if (tex == null)
            return false;

        float widthDelta = rect.width - tex.width;
        float heightDelta = rect.height - tex.height;

        if (widthDelta >= 0 && widthDelta <= NewUITexturePacker.Padding && heightDelta >= 0 && heightDelta <= NewUITexturePacker.Padding)
            return true;

        return false;
    }


    public bool IsLeftNode()
    {
        return texture != null;
    }

    private bool IsTextureFit(Texture2D tex)
    {
        if (tex == null)
            return false;

        float widthDelta = rect.width - tex.width;
        float heightDelta = rect.height - tex.height;

        if (widthDelta < 0 || heightDelta < 0)
            return false;

        return true;
    }

}

public class NewUITexturePacker
{
    public static AtlasNode[] AllTNode;
    public static int Padding = 1;
    public static Rect[] PackTextures(Texture2D texture, Texture2D[] textures, int width, int height, int padding,int maxSize,bool foreSquare)
    {
        Padding = padding;

        //强制正方形
        if (foreSquare)
        {
            if (width > height)
                height = width;
            else if (height > width)
                width = height;
        }


        if (width > maxSize || height > maxSize)
            return null;

        //所有节点
        AllTNode = new AtlasNode[textures.Length];

        //根节点
        AtlasNode rooNode = new AtlasNode
        {
            rect = new Rect(0, 0, width, height)
        };

        int index = 0;

        texture.Resize(width, height);
        texture.SetPixels(new Color[width * height]);

        while (index < textures.Length)
        {
            AtlasNode node = rooNode.Insert(textures[index], index);
            if (node == null && index < textures.Length)
            {
                AllTNode = null;
                return PackTextures(texture, textures, width * (width <= height ? 2 : 1), height * (height < width ? 2 : 1), padding, maxSize,foreSquare);
            }

            index++;
        }

        Rect[] rects = new Rect[textures.Length];

        if (AllTNode == null || AllTNode.Length <= 0)
            return rects;


        for (int i = 0; i < textures.Length; i++)
        {
            AtlasNode node = AllTNode[i];

            Texture2D tex = node.texture;
            if (!tex) continue;

            Rect rect = node.rect;
            Color[] colors = tex.GetPixels();

            texture.SetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height, colors);
            rect.x /= width;
            rect.y /= height;
            rect.width = rect.width / width;
            rect.height = rect.height / height;
            rects[i] = rect;
        }

        AllTNode = null;

        return rects;
    }
}
