
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(AspectRatioFitter))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(VideoPlayer))]
public class VideoPlayerUGUI : MonoBehaviour
{
    /// <summary>
    /// 图片
    /// </summary>
    public RawImage RawImageInst
    {
        get
        {
            if (m_RawImage == null)
            {
                m_RawImage = GetComponent<RawImage>();
            }
            return m_RawImage;
        }
    }
    private RawImage m_RawImage;


    /// <summary>
    /// 适配
    /// </summary>
    public AspectRatioFitter AspectRatioFitterInst
    {
        get
        {
            if (AspectRatio == null)
            {
                AspectRatio = GetComponent<AspectRatioFitter>();
            }
            return AspectRatio;
        }
    }

    private AspectRatioFitter AspectRatio;

    public VideoPlayer VideoPlayerInst
    {
        get
        {
            if (m_VideoPlayer == null)
            {
                m_VideoPlayer = GetComponent<VideoPlayer>();
            }
            return m_VideoPlayer;
        }
    }
    private VideoPlayer m_VideoPlayer;

    public AudioSource AudioSourceInst
    {
        get
        {
            if (m_AudioSource == null)
            {
                m_AudioSource = GetComponent<AudioSource>();
            }
            return m_AudioSource;
        }
    }
    private AudioSource m_AudioSource;

    /// <summary>
    /// RT
    /// </summary>
    private RenderTexture RtRenderTexture;

    /// <summary>
    /// 视频资源
    /// </summary>

    public VideoSource VideSourceMode = VideoSource.Url;
    public VideoClip VideoClipAsset;
    public string Url;

    public bool PlayOnAwake = true;
    public bool IsPlayAfterPrepare = true;
    public bool IsLooping = false;

    [Range(0, 10)]
    public float PlaybackSpeed = 1;

    public Action ActionStarted = null;
    public Action ActionEnded = null;
    public Action ActionPrepareCompleted = null;



    private void Awake()
    {
        VideoPlayerInst.loopPointReached += LoopPointReached;
        VideoPlayerInst.prepareCompleted += PrepareCompleted;
        VideoPlayerInst.started += Started;
        VideoPlayerInst.frameDropped += FrameDropped;
        VideoPlayerInst.errorReceived += ErrorReceived;
        VideoPlayerInst.seekCompleted += SeekCompleted;
        VideoPlayerInst.clockResyncOccurred += ClockResyncOccurred;
        VideoPlayerInst.playOnAwake = false;
        AudioSourceInst.playOnAwake = false;

        if (VideoClipAsset != null)
        {
            SetVideoClip(VideoClipAsset);
        }

        if (!string.IsNullOrEmpty(Url))
        {
            SetVideoUrl(Url);
        }

        VideoPlayerInst.playbackSpeed = PlaybackSpeed;
        VideoPlayerInst.isLooping = IsLooping;
    }

    private void Start()
    {
        Vector2 vector2 = GetRenderTextureSize();
        RtRenderTexture = RenderTexture.GetTemporary(Mathf.RoundToInt(vector2.x), Mathf.RoundToInt(vector2.y), 0, RenderTextureFormat.ARGB32);       
        VideoPlayerInst.targetTexture = RtRenderTexture;

        if (RawImageInst != null)
        {
            RawImageInst.texture = RtRenderTexture;
        }

        if (PlayOnAwake)
        {
            Play();
        }
    }

    private void OnDestroy()
    {
        if (RtRenderTexture != null)
        {
            FillRt(RtRenderTexture,Color.black);
            RenderTexture.ReleaseTemporary(RtRenderTexture);
        }
    }

    public void SetSpeed(float speed)
    {
        VideoPlayerInst.playbackSpeed = speed;
    }

    /// <summary>
    /// 获取RT大小
    /// </summary>
    /// <returns></returns>
    private Vector2 GetRenderTextureSize()
    {
        int screenWid = 1920;
        int screenHei = 1080;

        if (AspectRatioFitterInst == null)
        {
            return new Vector2(screenWid, screenHei);
        }

        float curAspectRatio = (float)screenWid / screenHei;
        float width = 0;
        float height = 0;
        if (AspectRatioFitterInst.aspectRatio > curAspectRatio)
        {
            width = screenWid;
            height = width / AspectRatioFitterInst.aspectRatio;
        }
        else
        {
            height = screenHei;
            width = height * AspectRatioFitterInst.aspectRatio;
        }

        Vector2 vector2 = new Vector2(width, height);

        return vector2;
    }

    public void FillRt(RenderTexture renderTexture, Color color)
    {
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, color);
        RenderTexture.active = rt;
    }


    /// <summary>
    /// 准备
    /// </summary>
    public void Prepare()
    {
        VideoPlayerInst.Prepare();
    }

    public void Play()
    {
        if (VideoPlayerInst.isPrepared)
        {
            VideoPlayerInst.Play();
        }
        else
        {
            Prepare();
        }
    }

    public void Pause()
    {
        VideoPlayerInst.Pause();
    }

    public void Stop()
    {
        VideoPlayerInst.Stop();
    }

    public void SetVideoClip(VideoClip clip)
    {
        VideoClipAsset = clip;
        VideoPlayerInst.clip = clip;
        VideoPlayerInst.audioOutputMode = VideoAudioOutputMode.AudioSource;
        VideoPlayerInst.SetTargetAudioSource(0, AudioSourceInst);
    }

#if CLIENT
    public void SetVideoFromRelateUrl(string url)
    {
        string resultStr = XFileManager.GetFileUrl(url);
        SetVideoUrl(resultStr);
    }
#endif

    public void SetVideoUrl(string url)
    {
        Url = url;
        VideoPlayerInst.url = url;
        VideoPlayerInst.audioOutputMode = VideoAudioOutputMode.AudioSource;
        VideoPlayerInst.controlledAudioTrackCount = 1;
        VideoPlayerInst.EnableAudioTrack(0, true);
        VideoPlayerInst.SetTargetAudioSource(0, AudioSourceInst);
    }
    public bool IsPlaying()
    {
        return VideoPlayerInst.isPlaying;
    }

    private void Started(VideoPlayer player)
    {
        if(ActionStarted!=null)
            ActionStarted.Invoke();
    }

    private void PrepareCompleted(VideoPlayer player)
    {
        if (ActionPrepareCompleted != null)
            ActionPrepareCompleted.Invoke();

        if (IsPlayAfterPrepare)
        {
            Play();
        }
    }

    private void LoopPointReached(VideoPlayer player)
    {
        if (ActionEnded != null)
            ActionEnded.Invoke();
    }

    void FrameDropped(VideoPlayer player)
    {
       // XLog.Error("VideoPlayer FrameDropped");
    }

    private void ClockResyncOccurred(VideoPlayer source, double seconds)
    {
       // XLog.Error("VideoPlayer ClockResyncOccurred");
    }

    private void SeekCompleted(VideoPlayer source)
    {
       // XLog.Error("VideoPlayer SeekCompleted");
    }

    private void ErrorReceived(VideoPlayer source, string message)
    {
        Debug.LogError("VideoPlayer ErrorReceived：" + message);
    }


}
