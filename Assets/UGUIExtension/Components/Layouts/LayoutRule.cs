using System;
using System.Collections.Generic;

public class LayoutRule
{

    //角落
    public enum Corner { UpperLeft = 0, UpperRight = 1, LowerLeft = 2, LowerRight = 3 }
    //方向
    public enum Direction { Horizontal = 0, Vertical = 1, Locked = 2 }
   
    //顺序
    public enum Order { Positive = 0, Reverse = 1 }

    //异步加载规则
    public enum GridLoadRule { PER_GRID = 0, GROUP = 1 }

    //分割布局
    public enum Constraint { Flexible = 0, FixedColumnCount = 1, FixedRowCount = 2 }

    //动态列表事件
    public enum DYNAMIC_DELEGATE_EVENT
    {
        DYNAMIC_GRID_RELOAD_COMPLETED = 1,
        DYNAMIC_GRID_TOUCHED,
        DYNAMIC_GRID_ATINDEX,
        DYNAMIC_GRID_RECYCLE,
        DYNAMIC_TWEEN_OVER

    }

}

