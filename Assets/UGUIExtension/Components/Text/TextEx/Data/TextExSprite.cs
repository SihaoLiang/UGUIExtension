using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextExtend
{
    [System.Serializable]
    public class TextExSprite : TextExElement
    {
        public int hashCode;
        public string name;
        public Vector2 pivot;
        public Sprite sprite;
        public string key;
        public string animatGroup;
        public Vector2 offest;
    }
}