using MTFrame;
using System;

using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 软件前置工具
/// </summary>
public class SoftwareSettingsTool : MonoBehaviour
{
    private bool isOpenPrepose = true;

    public string productName;

    private void Start()
    {
        
    }


#if UNITY_STANDALONE_WIN

    [DllImport("User32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("User32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("User32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

    void FixedUpdate()
    {
        if (!isOpenPrepose) return;

        // apptitle自己到查看进程得到，一般就是程序名不带.exe
        // 或者用spy++查看
        IntPtr hwnd = FindWindow(null, productName);

        // 如果没有找到，则不做任何操作（找不到一般就是apptitle错了）
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        IntPtr activeWndHwnd = GetForegroundWindow();

        // 当前程序不是活动窗口，则设置为活动窗口
        if (hwnd != activeWndHwnd)
        {
            ShowWindowAsync(hwnd,3);
            SetForegroundWindow(hwnd);
        }
    }

#endif
}