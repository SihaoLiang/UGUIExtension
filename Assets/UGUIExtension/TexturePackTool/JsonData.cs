using System.Collections.Generic;
using UnityEngine;
using System;
namespace Serialize
{
    public class TP
    {
        [Serializable]
        public struct SpriteData
        {
            public string filename;
            public SpriteFrame frame;
            public Vector2 pivot;
            public bool rotated;
            public SpriteSize sourceSize;
            public SpriteFrame spriteSourceSize;
            public bool trimmed;
        }

        [Serializable]
        public struct SpriteFrame
        {
            public float h;
            public float w;
            public float x;
            public float y;

            public override string ToString() { return string.Format("h:{0},w:{1},x:{2},y:{3}", h, w, x, y); }
        }

        [Serializable]
        public struct SpriteSize
        {
            public float h;
            public float w;

            public override string ToString()
            {
                return string.Format("h:{0},w:{1}", h, w);
            }
        }

        [Serializable]
        public class JsonData
        {
            public List<SpriteData> frames;
        }
    }
}
