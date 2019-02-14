using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LayoutTest : MonoBehaviour {

    public GameObject TextPrefab = null;
    public Button BtnGo = null;
    public GameObject Layout = null;
	void Start () {
        BtnGo.onClick.AddListener(OnClickGo);
    }


    void OnClickGo()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = GameObject.Instantiate(TextPrefab);
            obj.SetActive(true);
            Text text = obj.GetComponent<Text>();
            text.text = "一个都不能少";
            obj.transform.SetParent(Layout.transform, false);

        }
    }
}
