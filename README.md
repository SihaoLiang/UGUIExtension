# UGUIExtension
# UGUI拓展包，目前包含3个动态列表，富文本，艺术字，打字机，部分布局等常用的UI组件，大部分组件已经在上线商业项目稳定使用

## 动态列表
  ### 一.DynamicTableNormal
  
   DynamicTableNormal，原理是根据可视区域生成最大显示的Grid数，使用滑动位置作为生成Gird的内容
   支持C# 和 lua 使用，支持布局设置,支持自动适配可视区域，支持进度条。
  
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/dlist1.png)  
  
  栗子：
  
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/dlist.png)  
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/dlist2.png)  

  ### 二.DynamicTableIrregular
  DynamicTableIrregular 不规则动态列表，制作聊天窗口必备

  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/ilist2.png)  
  
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/ilist.png)  
  
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/ilist3.png)  

    
    1.不规则动态列表，目前把滑动逻辑和动态列表的逻辑都放在一起，后续可以拆分成两个组件，因为目前的两者逻辑部分基本没有强耦合的，拆分容易。
    2.关于是否添加进度条，其实进度条的方案目前有，但是还要在考虑，所以先保留。
    3.另一个关于Scrollrect的阉割版本，拆出来后只做纯粹的滑动逻辑，对于后续添加其他功能也可以较好支持，例如下拉加载更多之类的
    
  ### 三.DynamicTableCurve

