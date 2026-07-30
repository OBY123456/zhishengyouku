using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RC;
using System;

public class RCManager : MonoBehaviour
{
    public RCPanel CurrentPanel;

    private Stack<RCPanel> BackPanel = new Stack<RCPanel>();

    /// <summary>
    /// 进入下一级页面，需要手动调
    /// </summary>
    /// <param name="panel"></param>
    public void SetPanel(RCPanel panel,bool IsOpen = true)
    {
        BackPanel.Push(CurrentPanel);
        CurrentPanel.Hide(false);
        CurrentPanel = panel;
        if(IsOpen)
        CurrentPanel.Open();
    }

    /// <summary>
    /// 返回上一级页面
    /// </summary>
    public void Return(bool IsNull = true)
    {
        if (BackPanel.Count > 0)
        {
            CurrentPanel.Hide(IsNull);
            CurrentPanel = BackPanel.Pop();
            CurrentPanel.Open();
        }
    }

    public void OnClick()
    {
        if (CurrentPanel != null)
        {
            CurrentPanel.Onclick();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            InputRotation(Rotation.上);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            InputRotation(Rotation.下);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InputRotation(Rotation.左);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            InputRotation(Rotation.右);
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            OnClick();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        { 
            Return();
        }
    }

    private void InputRotation(Rotation rotation)
    {
        CurrentPanel.Next(rotation);
    }

    private void OnEnable()
    {
        RCDataControl.DataEvent += Callback;
    }

    private void Callback(byte bytes)
    {
        try
        {
            RCKey key = (RCKey)bytes;
            switch (key)
            {
                case RCKey.remote_menu_value:
                    Return();
                    break;
                default:
                    break;
            }
        }
        catch
        {
            Debug.LogError("RCKey转化失败");
        }
    }

    private void OnDisable()
    {
        RCDataControl.DataEvent -= Callback;
    }
}
