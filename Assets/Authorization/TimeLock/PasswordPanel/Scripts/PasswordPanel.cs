using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OBY.时间锁
{
    public class PasswordPanel : OBYPanel
    {
        public InputField inputField;
        public Button OkBtn, BackBtn;
        private string PasswordStr = "68zs";

        private void Awake()
        {
            canvasGroup.Hide();

            inputField.onValueChanged.AddListener(Contrast);

            OkBtn = transform.Find("bg/确定").GetComponent<Button>();
            OkBtn.interactable = false;
            OkBtn.onClick.AddListener(() =>
            {
                UIControl.Instance?.Open(Panelname.SetDatePanel);
            });

            BackBtn = transform.Find("bg/退出").GetComponent<Button>();
            BackBtn.onClick.AddListener(() =>
            {
                if (TimeControl.Instance.IsTimeOut())
                {
                    UIControl.Instance.Open(Panelname.TipsPanel);
                }
                else
                {
                    UIControl.Instance.Hide(Panelname.PasswordPanel);
                }
            });
        }

        private void Contrast(string arg0)
        {
            if (arg0 == PasswordStr)
            {
                OkBtn.interactable = true;
            }
            else
            {
                OkBtn.interactable = false;
            }
        }

        public override void Open()
        {
            base.Open();
            inputField.text = string.Empty;
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {

        }
    }
}