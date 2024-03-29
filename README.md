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
  DynamicTableCurve 曲线列表，根据曲线布局Gird。
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/clist.gif)  
  ![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/clist.png)  
 
  DynamicTableCurve 目前只支持位置，缩放，层次调整三个曲线，拓展性不怎么好，这里推荐一个比较好的曲线列表：
  [FancyScrollView](https://github.com/jorik041/UnityUIExtensions/tree/master/Examples/FancyScrollView)这个曲线列表做的很好，可以定制很多样式，甚至可以自己用公式插值
 
 ## 战斗飘字艺术字
 ### BMFontText BMFontMultText
 UGUI的Text 做动态变幻时性能不好，会出现频繁重绘和打断合批问题，所以做了一个艺术字组件，原理把艺术字的图片打进一张图集中，动态进行Uv拼接。（当然现在TMP的发展好像也能搞
，之前有研究，这两年没怎么用）

BMFontText 单一艺术字
BMFontMultText 可混合组艺术字

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/font.png)  

## 富文本
### RichText
内含生成图集工具
兼容原有的富文本，原理：利用quad作为占位渲染图片
支持图文混拼
支持序列帧表情
支持单图
支持超链接
支持下划线

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/richText.png) 

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/richText1.png)  

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/richText2.png)  

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/richText3.png)  

    没有支持inputField 输入和整体删除，建议在使用时，在输入框中配表易名,比如 [a01] 映射一张图片，之前还看到一种思路就是对文本节点化，之前在cocos做过一版，或许有空的话支持一下，或许....

## 简单打字机
TextTypewriter 原理：动态重组字符串的网格

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/Typew.gif)  

## 视频播放
对原生的VideoPlayer 简单封装了一下，当然我不建议使用原生的VideoPlayer，内存消耗高，码率超过3500 就会卡屏，推荐用Cri，或者AvPro

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/video.png)  

## 对齐组件 AutoLayoutGroup LayoutNode 
这里不细说，当初使用VLayoutGroup 和 HLayoutGroup 觉得不好用，所以自己写了两个

## 图集打包工具
### TexturePackTool 
类似TP工具，把散图打进一张图集，支持一键生成精灵，九宫格目前需要手动配置。使用二叉树原理合图，支持相同uv剔除。
富文本和艺术字可以使用该工具打出来的图集，一键生成可以用数据，当然也支持TP导入。

![image](https://github.com/SihaoLiang/UGUIExtension/blob/master/Icons/texpack.png)  

## 由于本工具是我这几年工作中需要用到的时候造的轮子（今天才想起来要整理一下），时间跨度比较大，代码写得不怎么好，大家多多包涵。

## 如果有什么可以问题或者BUG 可以联系 QQ 928441097
## 或者大家工作上有什么问题也可以跟我交流一下
