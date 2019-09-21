using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestNormalGrid : DynamicGrid
{
    public Text m_Text;

    public void SetContent(int index)
    {
        m_Text.text = index.ToString();
    }
}
