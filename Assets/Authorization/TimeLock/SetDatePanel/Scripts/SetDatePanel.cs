using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OBY.时间锁
{
    public class SetDatePanel : OBYPanel
    {
        public InputField inputField;
        public Button OkBtn, BackBtn;
        public Text Tips;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.Hide();

            inputField = transform.Find("bg/InputField").GetComponent<InputField>();
            inputField.onValueChanged.AddListener(onValueChanged);

            OkBtn = transform.Find("bg/确定").GetComponent<Button>();
            OkBtn.onClick.AddListener(() =>
            {
                if(string.IsNullOrEmpty(inputField.text) || inputField.text == "0")
                {
                    Tips.text = "输入不能为空或者为0";
                    inputField.text = "1";
                    return;
                }

                Tips.text = "数据保存成功!";
                int Times = int.Parse(inputField.text);
                TimeControl.Instance?.SetTime(Times);
                OkBtn.interactable = false;
                BackBtn.interactable = false;

                InvokeUtil.Instance.Run(() =>
                {
                    UIControl.Instance.Hide(Panelname.SetDatePanel);
                }, 1.0f);
            });

            BackBtn = transform.Find("bg/退出").GetComponent<Button>();
            BackBtn.onClick.AddListener(() =>
            {
                if (TimeControl.Instance.IsTimeOut())
                {
                    UIControl.Instance?.Open(Panelname.TipsPanel);
                }
                else
                {
                    UIControl.Instance.Hide(Panelname.SetDatePanel);
                }
            });
        }

        private void onValueChanged(string arg0)
        {
            try
            {
                int.Parse(arg0);
            }
            catch
            {
                Tips.text = "只能输入整数";
                inputField.text = string.Empty;
            }
        }

        public override void Open()
        {
            base.Open();
            OkBtn.interactable = true;
            BackBtn.interactable = true;
            inputField.text = string.Empty;
            Tips.text = "提示：1、每次设置完成都会重新开始计时。2、只计算程序运行时间。3、最多只能设置9999天。";
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {

        }
    }
}

