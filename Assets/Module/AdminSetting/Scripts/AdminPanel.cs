using UnityEngine;
using UnityEngine.UI;
using Config;
using Setting;
using System;
using Setting.Code;
using PayCode;

namespace AdminSetting
{
    public class AdminPanel : MonoBehaviour
    {
        public Dropdown pay,language;

        public Toggle MouseTog;

        public Button BackBtn, ReStartBtn;

        public CanvasGroup Tips;

        private ProgramData programData;

        public InputField inputField_收费设置;

        public PasswordPanel passwordPanel;

        public GameObject[] Hideobj;

        private void Awake()
        {
#if UNITY_STANDALONE_WIN
            Cursor.visible = true;
#endif
            Init();
            EventInit();
        }

        // Start is called before the first frame update
        void Start()
        {
            for (int i = 0; i < Hideobj.Length; i++)
            {
                Hideobj[i].SetActive(false);
            }
        }


        public void Init()
        {
            Tips.alpha = 0;
            programData = ProgramConfig.Instance.programData;
            ValueInit();
        }

        private void ValueInit()
        {
            language.value = (int)programData.语言类型;
            pay.value = (int)programData.收费模式;
            switch (programData.收费模式)
            {
                case PaymentModel.免费模式:
                    inputField_收费设置.transform.parent.gameObject.SetActive(false);
                    break;
                case PaymentModel.付费模式:
                    inputField_收费设置.transform.parent.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
            MouseTog.isOn = programData.是否显示鼠标;
            inputField_收费设置.text = programData.费用.ToString("#0.00");
        }

        private void EventInit()
        {
            inputField_收费设置.onEndEdit.AddListener(onEndEdit);
            BackBtn.onClick.AddListener(() =>
            {
                PayManager.Instance?.Open();
                UnityEngine.SceneManagement.SceneManager.LoadScene("PaycodeDemo");
            });

            ReStartBtn.onClick.AddListener(() =>
            {
                Save();
            });

            pay.onValueChanged.AddListener(PayValueChange);
        }

        private void onEndEdit(string arg0)
        {
            try
            {
                float temp = float.Parse(arg0);
                if(temp == 0)
                {
                    inputField_收费设置.text = programData.费用.ToString("#0.00");
                }
            }
            catch(Exception e)
            {
                inputField_收费设置.text = programData.费用.ToString("#0.00");
            }
            
        }

        private void PayValueChange(int arg0)
        {
            switch (arg0)
            {
                case 0:
                    inputField_收费设置.transform.parent.gameObject.SetActive(false);
                    break;
                case 1:
                    inputField_收费设置.transform.parent.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private void Save()
        {
            programData.是否显示鼠标 = MouseTog.isOn;
            programData.收费模式 = (PaymentModel)pay.value;
            programData.语言类型 = (LanguageSetting.LanguageType)language.value;
            programData.费用 = float.Parse(inputField_收费设置.text);
            ProgramConfig.Instance?.SaveData();
            Tips.alpha = 1;
            Invoke(nameof(ResetApp), 2.0f);
        }

        public void OpenOrHide()
        {
            if(Hideobj.Length > 0 && passwordPanel.canvasGroup.alpha == 0)
            {
                if(Hideobj[0].activeInHierarchy)
                {
                    for (int i = 0; i < Hideobj.Length; i++)
                    {
                        Hideobj[i].SetActive(false);
                    }
                }
                else
                {
                    for (int i = 0; i < Hideobj.Length; i++)
                    {
                        Hideobj[i].SetActive(true);
                    }
                }
            }
        }

        private void ResetApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            OpenZS.Ins?.WriteShareMemory("kill");
            ReStartApp.ReStart();
#endif
        }
    }
}

