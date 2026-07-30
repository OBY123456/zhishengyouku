using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Config;

namespace Setting
{
    public class ChangeDisplay : MonoBehaviour
    {
        [HideInInspector]
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
        const uint SWP_SHOWWINDOW = 0x0040;
        const int GWL_STYLE = -16;
        const int WS_BORDER = 1;
        void Start()
        {
#if UNITY_STANDALONE_WIN
            StartCoroutine(nameof(Init));
#endif
        }

        private IEnumerator Init()
        {
#if UNITY_STANDALONE_WIN
            Screen.SetResolution(ProgramConfig.Instance.programData.分辨率X, ProgramConfig.Instance.programData.分辨率Y, false);
#endif
            yield return new WaitForSecondsRealtime(0.5f);
#if !UNITY_EDITOR && UNITY_STANDALONE_WIN
        SetWindowLong(GetActiveWindow(), GWL_STYLE, WS_BORDER);
        SetWindowPos(GetActiveWindow(), -1, 0, 0, ProgramConfig.Instance.programData.分辨率X, ProgramConfig.Instance.programData.分辨率Y, SWP_SHOWWINDOW);
#endif
        }
    }
}

