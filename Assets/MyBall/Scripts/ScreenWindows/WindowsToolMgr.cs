using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*xbb
* 
* */
public class WindowsToolMgr : MonoBehaviour
{

    //系统方法类实体
    public WindowsTools winTool = new WindowsTools();

    //当前Unity程序进程
    private static IntPtr currentWindow;

    void Start()
    {
        currentWindow = WindowsTools.GetForegroundWindow();
    }

    //最小化窗口
    public void MinWindows()
    {
        winTool.SetMinWindows();

    }

    //设置无边框窗口
    public void SetNoFrame()
    {
        //窗口大小  以此为例
        float windowWidth = 1024;
        float windowHeight = 768;
        //计算框体显示位置
        float posX = (Screen.currentResolution.width - windowWidth) / 2;
        float posY = (Screen.currentResolution.height - windowHeight) / 2;
        Rect _rect = new Rect(posX, posY, windowWidth, windowHeight);

        winTool.SetNoFrameWindow(_rect);
    }

    public void SetFrame()
    {
        //窗口大小  以此为例
        float windowWidth = 1024;
        float windowHeight = 768;
        //计算框体显示位置
        float posX = (Screen.currentResolution.width - windowWidth) / 2;
        float posY = (Screen.currentResolution.height - windowHeight) / 2;
        Rect _rect = new Rect(posX, posY, windowWidth, windowHeight);

        winTool.SetFrameWindow(_rect);
    }

    public void SetWindow500()
    {

        //窗口大小  以此为例
        float windowWidth = 500;
        float windowHeight = 500;
        //计算框体显示位置
        float posX = (Screen.currentResolution.width - windowWidth) / 2;
        float posY = (Screen.currentResolution.height - windowHeight) / 2;
        Rect _rect = new Rect(posX, posY, windowWidth, windowHeight);

        winTool.SetNoFrameWindow(_rect);
    }

    public void SetWindow1024()
    {
        //窗口大小  以此为例
        float windowWidth = 1920f;
        float windowHeight = 1080f;
        //计算框体显示位置
        float posX = (Screen.currentResolution.width - windowWidth) / 2;
        float posY = (Screen.currentResolution.height - windowHeight) / 2;
        Rect _rect = new Rect(50f, 0, windowWidth, windowHeight);

        winTool.SetWindowSize(_rect);

    }

    /// <summary>
    /// 全屏
    /// </summary>
    public void FullScreen()
    {
        if (Screen.fullScreen)
        {
            Screen.SetResolution(1024, 768, false);
        }
        else
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        }
        //等待当前帧完成 再去除边框
        StartCoroutine(IE_NoFrame());
    }

    private IEnumerator IE_NoFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        if (!Screen.fullScreen)
        {
            SetNoFrame();
        }
    }

    //窗口拖动
    public void Drag()
    {
        winTool.DragWindow(currentWindow);
    }
}

