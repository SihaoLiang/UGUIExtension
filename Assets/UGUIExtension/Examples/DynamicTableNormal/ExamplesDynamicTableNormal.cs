using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExamplesDynamicTableNormal : MonoBehaviour
{
    public DynamicTableNormal HorizontalDynamicTableNormal;
    public DynamicTableNormal VerticalDynamicTableNormal;

    public Button BtnHorizontal;
    public Button BtnVertical;

    void Start()
    {
        BtnHorizontal.onClick.AddListener(OnBtnHorizontalClick);
        BtnVertical.onClick.AddListener(OnBtnVerticalClick);

        HorizontalDynamicTableNormal.DynamicTableGridDelegate = OnHorizontalDynamicTableViewCallBack;
        VerticalDynamicTableNormal.DynamicTableGridDelegate = OnHorizontalDynamicTableViewCallBack;
    }

    private void OnHorizontalDynamicTableViewCallBack(int evt, int index, DynamicGrid grid)
    {
        if (evt == (int) LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_ATINDEX)
        {
            TestNormalGrid testGrid = grid as TestNormalGrid;
            testGrid.SetContent(index);
        }
    }

    void OnBtnHorizontalClick()
    {
        HorizontalDynamicTableNormal.SetTotalCount(100);
        HorizontalDynamicTableNormal.ReloadDataAsync();
    }

    void OnBtnVerticalClick()
    {
        VerticalDynamicTableNormal.SetTotalCount(100);
        VerticalDynamicTableNormal.ReloadDataAsync();
    }
}
