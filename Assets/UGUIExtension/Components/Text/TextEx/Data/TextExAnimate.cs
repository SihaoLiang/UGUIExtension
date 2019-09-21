using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextExtend
{
    [System.Serializable]
    public class TextExAnimate {
        public string m_AnimateName;
        public int frameRate { set; get; }
        public int frameCount { set; get; }

        public List<TextExSprite> m_SpriteList;

        public TextExAnimate()
        {
            frameRate = 26;
            frameCount = 0;
            m_SpriteList = new List<TextExSprite>();
        }

        public string animateName
        {
            get
            {
                return m_AnimateName;
            }

            set
            {
                m_AnimateName = value;
            }
        }

        public List<TextExSprite> spriteList
        {
            get
            {
                return m_SpriteList;
            }

            set
            {
                m_SpriteList = value;
            }
        }
    }
}