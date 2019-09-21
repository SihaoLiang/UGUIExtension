using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[ExecuteInEditMode]
public class TextTypewriter : BaseMeshEffect
{
    const int PRE_WORD_VERTEX_COUNT = 6;

    public enum TypewriterStatus
    {
        Playing = 0,
        Pause,
        Stop,
    }


    public enum TypewriterTimeCrlType
    {
        Speed = 0,
        Duration,
    }

    /// <summary>
    /// 播放状态
    /// </summary>
    private TypewriterStatus Status = TypewriterStatus.Stop;

    /// <summary>
    /// 时间控制状态
    /// </summary>
    public TypewriterTimeCrlType TimeCrlType = TypewriterTimeCrlType.Speed;

    /// <summary>
    /// 启动打字
    /// </summary>
    public bool PlayOnAwake = false;

    /// <summary>
    /// 速度一秒10个字
    /// </summary>
    public float Speed = 10;

    /// <summary>
    /// 总时长
    /// </summary>
    public float Duration = 5;

    /// <summary>
    /// 当前的顶点
    /// </summary>
    int CurIndex = -1;

    /// <summary>
    /// 播放时间
    /// </summary>
    float PlayingTime = 0.0f;

    /// <summary>
    /// 过滤后的顶点
    /// </summary>
    List<UIVertex> FitterVertices = new List<UIVertex>();

    /// <summary>
    /// 显示中的顶点
    /// </summary>
    List<UIVertex> OperatingVertices = new List<UIVertex>();

    /// <summary>
    /// 原始顶点
    /// </summary>
    List<UIVertex> OriginalVertices = new List<UIVertex>();

    /// <summary>
    /// 完成回调
    /// </summary>
    public Action CompletedHandle;

    protected override void OnEnable()
    {
        if (PlayOnAwake)
            Play();
    }

    /// <summary>
    /// 播放
    /// </summary>
    [ContextMenu("Play")]
    public void Play()
    {
        ResetStatus();
        Status = TypewriterStatus.Playing;
    }


    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
        ResetStatus();
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        Status = TypewriterStatus.Pause;
    }

    /// <summary>
    /// 恢复
    /// </summary>
    public void Resume()
    {
        Status = TypewriterStatus.Playing;
    }

    /// <summary>
    /// 重置状态
    /// </summary>
    void ResetStatus()
    {
        PlayingTime = 0;
        OperatingVertices.Clear();
        Status = TypewriterStatus.Stop;
        CurIndex = -1;
    }


    private void Update()
    {
        if (!IsActive())
            return;

        ModifyTextVertices();
        SetVerticesDirty();
    }


    public void SetPlayTime(float time)
    {
        PlayingTime = time;
    }
    /// <summary>
    /// 修改顶点
    /// </summary>
    void ModifyTextVertices()
    {
        if (Status == TypewriterStatus.Playing)
        {
            if (CurIndex >= FitterVertices.Count)
            {
                Stop();

                if (CompletedHandle != null)
                    CompletedHandle.Invoke();

                return;
            }

            if (TimeCrlType == TypewriterTimeCrlType.Duration && Duration > 0)
                Speed = FitterVertices.Count / Duration / PRE_WORD_VERTEX_COUNT;

            Speed = Mathf.Max(0, Speed);

            for (int index = OperatingVertices.Count; index < CurIndex; index++)
            {
                OperatingVertices.Add(FitterVertices[index]);
            }

            PlayingTime += Time.deltaTime;
            CurIndex = PRE_WORD_VERTEX_COUNT * (int)(PlayingTime * Speed);
        }
    }


    /// <summary>
    /// 更新节点
    /// </summary>
    public void SetVerticesDirty()
    {
        graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        if (!Application.isPlaying)
            Status = TypewriterStatus.Stop;

        //获取顶点
        vh.GetUIVertexStream(OriginalVertices);
        vh.Clear();

        //剔除
        FitterRepeatVertices();

        if (Status == TypewriterStatus.Stop)
            vh.AddUIVertexTriangleStream(FitterVertices);
        else
            vh.AddUIVertexTriangleStream(OperatingVertices);
    }

    /// <summary>
    /// 剔除位置相同的三角形面
    /// </summary>
    void FitterRepeatVertices()
    {
        if (OriginalVertices == null || OriginalVertices.Count <= 0)
            return;

        FitterVertices.Clear();
        for (int i = 0; i <= OriginalVertices.Count - 3; i += 3)
        {
            //剔除富文本多余顶点
            if (OriginalVertices[i].position == OriginalVertices[i + 1].position && OriginalVertices[i].position == OriginalVertices[i + 2].position)
                continue;

            FitterVertices.Add(OriginalVertices[i]);
            FitterVertices.Add(OriginalVertices[i + 1]);
            FitterVertices.Add(OriginalVertices[i + 2]);
        }
    }
}
