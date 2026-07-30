using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OBY.时间锁
{
    public enum Panelname
    {
        PasswordPanel,
        SetDatePanel,
        TipsPanel,
    }

    public class UIControl : MonoBehaviour
    {
        public static UIControl Instance;

        private Dictionary<Panelname,OBYPanel> PanelDic = new Dictionary<Panelname, OBYPanel>();

        public RCManager manager;

        public GameObject UICanvas;

        private void Awake()
        {
            Instance = this;
            OBYPanel[] oBYPanels = GetComponentsInChildren<OBYPanel>(true);
            PanelDic.Clear();
            for (int i = 0; i < oBYPanels.Length; i++)
            {
                Panelname temp = (Panelname)Enum.Parse(typeof(Panelname),oBYPanels[i].name);
                PanelDic.Add(temp,oBYPanels[i]);
                if(temp == TimeControl.Instance?.GetPanel())
                {
                    oBYPanels[i].Open();
                    manager.SetPanel(oBYPanels[i].GetComponent<RC.RCPanel>());
                }
                else
                {
                    oBYPanels[i].Hide();
                }
            }
        }

        public void Open(Panelname panelname)
        {
            UICanvas.SetActive(true);
            if(PanelDic.ContainsKey(panelname))
            {
                foreach (var item in PanelDic)
                {
                    if(item.Key.Equals(panelname))
                    {
                        if (!PanelDic[panelname].IsOpen)
                        {
                            PanelDic[panelname].Open();
                            manager.SetPanel(PanelDic[panelname].GetComponent<RC.RCPanel>());
                        }
                    }
                    else
                    {
                        if (PanelDic[item.Key].IsOpen)
                        {
                            PanelDic[item.Key].Hide();
                        }
                    }
                }
                
            }
        }

        public void Hide(Panelname panelname)
        {
            if (PanelDic.ContainsKey(panelname))
            {
                if (PanelDic[panelname].IsOpen)
                {
                    PanelDic[panelname].Hide();
                }
            }
            UICanvas.SetActive(false);
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public bool GetIsOpen()
        {
            return UICanvas.activeInHierarchy;
        }
    }
}

//public static class ExtenTool
//{
//    public static void Open(this CanvasGroup canvasGroup)
//    {
//        canvasGroup.alpha = 1;
//        canvasGroup.blocksRaycasts = true;
//    }
//    public static void Hide(this CanvasGroup canvasGroup)
//    {
//        canvasGroup.alpha = 0;
//        canvasGroup.blocksRaycasts = false;
//    }

//    public static void Open(this CanvasGroup canvasGroup, float Time)
//    {
//        canvasGroup.DOFade(1, Time);
//        canvasGroup.blocksRaycasts = true;
//    }
//    public static void Hide(this CanvasGroup canvasGroup, float Time)
//    {
//        canvasGroup.DOFade(0, Time);
//        canvasGroup.blocksRaycasts = false;
//    }
//}


