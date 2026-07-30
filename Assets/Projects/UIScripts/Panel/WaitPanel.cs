using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MTFrame;
using System;
using UnityEngine.UI;

public class WaitPanel : BasePanel
{
    public Text text;
    public int Count;
    protected override void Start()
    {
        base.Start();
        TimeTool.Instance.AddDelayed(TimeDownType.NoUnityTimeLineImpact, 1.0f, () =>
        {
            Count++;
            text.text = Count.ToString();
        },true);
    }
}
