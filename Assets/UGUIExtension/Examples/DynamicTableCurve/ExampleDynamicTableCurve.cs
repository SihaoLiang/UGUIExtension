using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleDynamicTableCurve : MonoBehaviour
{
    public DynamicTableCurve DynamicTableCurveExample1;
    public DynamicTableCurve DynamicTableCurve1Example2;

    public Button BtnExample1;
    public Button BtnExample2;


    void Start()
    {
        BtnExample1.onClick.AddListener(OnBtnExample1Click);
        BtnExample2.onClick.AddListener(OnBtnExample2Click);

        DynamicTableCurveExample1.DynamicTableGridDelegate = DynamicTableGridDelegateEx1;
        DynamicTableCurve1Example2.DynamicTableGridDelegate = DynamicTableGridDelegateEx2;
    }

    private void DynamicTableGridDelegateEx1(int evt,int index, DynamicGrid grid)
    {
        if (evt == (int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_ATINDEX)
        {
            TestNormalGrid testGrid = grid as TestNormalGrid;
            testGrid.SetContent(index);
        }
        else if (evt == (int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_TOUCHED)
        {
            DynamicTableCurveExample1.TweenToIndex(index);
        }
    }

    private void DynamicTableGridDelegateEx2(int evt, int index, DynamicGrid grid)
    {
        if (evt == (int)LayoutRule.DYNAMIC_DELEGATE_EVENT.DYNAMIC_GRID_ATINDEX)
        {
            TestNormalGrid testGrid = grid as TestNormalGrid;
            testGrid.SetContent(index);
        }      
    }


    void OnBtnExample1Click()
    {
        DynamicTableCurve1Example2.gameObject.SetActive(false);
        DynamicTableCurveExample1.gameObject.SetActive(true);
        DynamicTableCurveExample1.SetTotalCount(100);
        DynamicTableCurveExample1.ReloadData(-1);
    }


    void OnBtnExample2Click()
    {
        DynamicTableCurveExample1.gameObject.SetActive(false);
        DynamicTableCurve1Example2.gameObject.SetActive(true);

        DynamicTableCurve1Example2.SetTotalCount(100);
        DynamicTableCurve1Example2.ReloadData(-1);
    }
}
