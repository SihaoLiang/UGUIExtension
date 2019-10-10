using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteAnimation : MonoBehaviour {

    public enum AniState {
        IDLE = 0, //挂起
        PLAYING,  //播放中
        PAUSE,    //暂停中
    }

    /// <summary>
    /// 状态
    /// </summary>
    private AniState state = AniState.IDLE;

    /// <summary>
    /// 时间间隔
    /// </summary>
    public float duration = 0;

    /// <summary>
    /// 当前帧
    /// </summary>
    private int m_CurrentFrame = 0;

    public int currentFrame {
        set { m_CurrentFrame = value; }
        get { return m_CurrentFrame; }
    }

    [SerializeField]
    public int startFrame = 0;
    [SerializeField]
    public int endFrame = -1;

    /// <summary>
    /// 动画列表
    /// </summary>
    [SerializeField]
    private List<Sprite> animationList = new List<Sprite>();

    /// <summary>
    /// 是否在播放
    /// </summary>
    private bool m_IsPlaying = false;
    public bool isPlaying { get { return m_IsPlaying; } }

    /// <summary>
    /// 播放载体
    /// </summary>
    Image image = null;

    /// <summary>
    /// 循环
    /// </summary>
    public bool loop = false;

   
    void Awake()
    {
       // animationList = new List<Sprite>();
        image = GetComponent<Image>();
        Play();
    }
   
    //播放方法
    public bool Play()
    {
        if (m_IsPlaying)
        {
            Debug.LogWarning("Animation is playing!!!");
            return false;
        }

        if (animationList == null || animationList.Count <= 0)
        {
            Debug.LogWarning("The list of animation is null or empty!!!");
            return false;
        }


        state = AniState.PLAYING;
        m_IsPlaying = true;
        return true;
    }

    public void Stop()
    {
        if (!m_IsPlaying)
            return;

        state = AniState.IDLE;
        m_IsPlaying = false;
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        state = AniState.PAUSE;
    }

    /// <summary>
    /// 恢复
    /// </summary>
    public void Resume()
    {
        state = AniState.PLAYING;
    }

    /// <summary>
    /// 设置开始帧
    /// </summary>
    /// <param name="frame"></param>
    public void SetStartFrame(int frame)
    {
        currentFrame = frame;
    }

    float time = 0;

    void Update()
    {
        if (state == AniState.PLAYING)
        { 
            if (time > duration)
            {
                Sprite sprite = animationList[currentFrame];
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.SetNativeSize();
                }
                currentFrame = currentFrame == animationList.Count - 1 ? 0 : currentFrame + 1;

                if (!loop && currentFrame == animationList.Count - 1)
                    Stop();

                time = 0;
            }else
                time += Time.deltaTime;
        }
    }
}
