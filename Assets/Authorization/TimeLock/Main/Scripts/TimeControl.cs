using Encryption;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OBY.时间锁
{
    /// <summary>
    /// 安卓无联网时间锁
    /// </summary>
    public class TimeControl : MonoBehaviour
    {
        public static TimeControl Instance; 

        private int TotalTime = -1;

        private Panelname panelname;

        private float IntervalTime = 300;

        private string FileName = "Times";

        private void Awake()
        {
            Instance = this; 
        }

        // Start is called before the first frame update
        void Start()
        {
            if(Exists(FileName,AESEncryption.FolderName))
            {
                try
                {
                    TotalTime = int.Parse(AESEncryption.ReadAllText(FileName,AESEncryption.FolderName));
                    if (TotalTime <= 0)
                    {
                        panelname = Panelname.TipsPanel;
                        UIControl.Instance?.Open(panelname);
                    }
                    else
                    {
                        StartCoroutine(nameof(CountDown));
                        InvokeRepeating(nameof(Save), 10, IntervalTime);
                    }
                }
                catch(Exception e)
                {
                    DebugManager.LogError(e.Message);
                }
            }
        }

        WaitForSeconds forSeconds = new WaitForSeconds(1);
        private IEnumerator CountDown()
        {
            while (TotalTime > 0)
            {
                yield return forSeconds;
                TotalTime--;
                if (DebugManager.Instance.GetIsShow())
                {
                    DebugManager.Instance.AddData("倒计时", FileHandle.UpdateTime(TotalTime));
                }
                if (TotalTime <= 0)
                {
                    panelname = Panelname.TipsPanel;
                    CancelInvoke(nameof(Save));
                    Save();
                    UIControl.Instance?.Open(panelname);
                }
            }
        }

        private void OnDestroy()
        {
            Instance = null;
            CancelInvoke(nameof(Save));
            StopCoroutine(nameof(CountDown));
        }

        public void SetTime(int date)
        {
            StopCoroutine(nameof(CountDown));
            CancelInvoke(nameof(Save));
            if(date == 6868)
            {
                if(Exists(FileName,AESEncryption.FolderName))
                {
                    AESEncryption.Delete(FileName,AESEncryption.FolderName);
                }
            }
            else
            {
                TotalTime = date * 24 * 60 * 60;
                Save();
            }

            Invoke(nameof(ReStart),1.0f);
        }

        private void ReStart()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            
#else
            Application.Quit();
#endif
        }

        public void Save()
        {
            DebugManager.Instance?.AddData("打印信息","保存：" + FileHandle.UpdateTime(TotalTime));
            AESEncryption.WriteAllText(FileName,TotalTime.ToString(),AESEncryption.FolderName);
        }

        public bool IsTimeOut()
        {
            if (Exists(FileName,AESEncryption.FolderName))
            {
                if (TotalTime == 0)
                    return true;
            }

            return false;
        }

        public bool IsUse()
        {
            if(Exists(FileName,AESEncryption.FolderName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Exists(string FileName,string FolderName = "")
        {
            return AESEncryption.Exists(FileName,FolderName);
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.F))
            {
                Open();
            }
        }

        public Panelname GetPanel()
        {
            return panelname;
        }

        public void Open()
        {
            panelname = Panelname.PasswordPanel;
            UIControl.Instance?.Open(panelname);
        }
    }
}

