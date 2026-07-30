using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// 组合键
/// </summary>
public class KeyCombination : MonoBehaviour
{
    private int visibleIndex = 0;
    private float visibleTimer = 0;

    /// <summary>
    /// 组合键
    /// </summary>
    public KeyCode[] keyCodes;

    /// <summary>
    /// 组合键事件
    /// </summary>
    private Action action;

    [Serializable]
    /// <summary>
    /// Function definition for a button click event.
    /// </summary>
    public class KeyCodeEvent:UnityEvent {}

    [FormerlySerializedAs("组合键事件")]
    [SerializeField]
    private KeyCodeEvent KeyCodeAction = new KeyCodeEvent();

    public void Init(KeyCode[] _keyCodes,Action _action)
    {
        keyCodes = _keyCodes;
        action = _action;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyCodes[visibleIndex]))
        {
            if (visibleTimer == 0 || Time.time - visibleTimer < 1f)
            {
                visibleIndex++;
                visibleTimer = Time.time;
            }

            if (visibleIndex >= keyCodes.Length)
            {
                action?.Invoke();
                KeyCodeAction?.Invoke();
                visibleIndex = 0;
                visibleTimer = 0;
            }
        }

        if (visibleIndex > 0 && (Time.time - visibleTimer >= 1f))
        {
            visibleIndex = 0;
            visibleTimer = 0;
        }
    }
}
