using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiRandomMove : MonoBehaviour {
    private RectTransform rectTransform
    {
        get
        {
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();

            if (m_RectTransform == null)
                m_RectTransform = gameObject.AddComponent<RectTransform>();

            return m_RectTransform;
        }
    }

    private RectTransform m_RectTransform;

    private float time = 2;
    private Vector2 To;
    void Update()
    {
        time += Time.deltaTime;//定时
        if (time > 1) { 
            To = new Vector2(Random.Range(0, 1920), Random.Range(0, 1080));
            time = 0;
        }

        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, To, time/1);

    }
}
