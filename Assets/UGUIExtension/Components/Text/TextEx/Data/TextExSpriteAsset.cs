using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TextExtend
{
    [System.Serializable]
    public class TextExSpriteAsset : ScriptableObject
    {
        public int hashCode;

        /// <summary>
        /// 材质
        /// </summary>
        public Material m_Material;
        public Material material
        {
            get { return m_Material; }
            set { m_Material = value; }
        }

        /// <summary>
        /// 所有uv信息
        /// </summary>
        [SerializeField]
        private List<TextExSprite> m_SpriteInfoList;
        public List<TextExSprite> spriteInfoList
        {
            get { return m_SpriteInfoList; }
            set { m_SpriteInfoList = value; }
        }

        /// <summary>
        /// 所有动画信息
        /// </summary>
        [SerializeField]
        private List<TextExAnimate> m_AnimateList;
        public List<TextExAnimate> animateList
        {
            get { return m_AnimateList; }
            set
            {
                m_AnimateList = value;
            }
        }

        /// <summary>
        /// 主纹理
        /// </summary>
        public Texture m_SpriteSheet;
        public Texture spriteSheet
        {
            get { return m_SpriteSheet; }
            set { m_SpriteSheet = value; }
        }
       
        public TextExSpriteAsset()
        {
            m_AnimateList = new List<TextExAnimate>();
            m_SpriteInfoList = new List<TextExSprite>();
        }

        /// <summary>
        /// 获取动画所有帧
        /// </summary>
        /// <param name="animateName"></param>
        /// <returns></returns>
        public TextExAnimate GetAnimateListByName(string animateName)
        {
            if (m_AnimateList == null)
                return null;

            TextExAnimate textAnimate = null;

            for (int i = 0; i < m_AnimateList.Count; i++)
            {
                if (m_AnimateList[i].animateName == animateName)
                {
                    textAnimate = m_AnimateList[i];
                    break;
                }
            }

            return textAnimate;
        }


        /// <summary>
        /// 图集大小
        /// </summary>
        public Vector2 Size
        {
            get
            {
                if (spriteSheet == null)
                    return Vector2.zero;
                return new Vector2(spriteSheet.width, spriteSheet.height);
            }
        }
    }
}