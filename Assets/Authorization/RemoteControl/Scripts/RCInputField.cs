using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace RC
{
    public class RCInputField : BaseRC
    {
        protected InputField inputField;
        private bool IsOpen;

        protected override void Awake()
        {
            base.Awake();
            inputField = GetComponent<InputField>();
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.None;
            inputField.navigation = navigation;
        }

        public override void Open()
        {
            base.Open();
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            IsOpen = true;
            RCDataControl.DataEvent += Callback;
            StartCoroutine(SetKeyBroadActive());
        }

        public override void Hide()
        {
            base.Hide();
            //inputField.DeactivateInputField();
            IsOpen = false;
            var type=inputField.GetType();
            var method=type.GetMethod("SendOnSubmit",System.Reflection.BindingFlags.Instance);
            method?.Invoke(inputField,new object[]{ });
            RCDataControl.DataEvent -= Callback;
        }

        private IEnumerator SetKeyBroadActive()
        {
            yield return new WaitForEndOfFrame();
            _SetKeyBroadActive(inputField,false);
        }

        void _SetKeyBroadActive(InputField inputField,bool IsActive)
        {
            var type = inputField.GetType();
            var Fields = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            for (int i = 0; i < Fields.Length; i++)
            {
                if (Fields[i].Name.Equals("m_Keyboard"))
                {
                    var m_Keyboard = Fields[i].GetValue(inputField);
                    if (m_Keyboard != null)
                    {
                        var keybroad = (TouchScreenKeyboard)m_Keyboard;
                        keybroad.active = IsActive;
                    }
                }
            }
        }

        private void Update()
        {

            if (IsOpen)
            {
#if UNITY_EDITOR
                var key0 = KeyCode.Alpha0;
                for (int i = 0; i < 10; i++)
                {
                    if (Input.GetKeyDown((key0 + i)))
                    {
                        inputField.text += i.ToString();
                    }
                }
#endif
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    StartCoroutine(SetKeyBroadActive());
                }
            }

        }

        private void Callback(byte bytes)
        {
            try
            {
                RCKey key = (RCKey)bytes;
                switch (key)
                {
                    case RCKey.remote_0_value:
                        inputField.text += 0;
                        break;
                    case RCKey.remote_1_value:
                        inputField.text += 1;
                        break;
                    case RCKey.remote_2_value:
                        inputField.text += 2;
                        break;
                    case RCKey.remote_3_value:
                        inputField.text += 3;
                        break;
                    case RCKey.remote_4_value:
                        inputField.text += 4;
                        break;
                    case RCKey.remote_5_value:
                        inputField.text += 5;
                        break;
                    case RCKey.remote_6_value:
                        inputField.text += 6;
                        break;
                    case RCKey.remote_7_value:
                        inputField.text += 7;
                        break;
                    case RCKey.remote_8_value:
                        inputField.text += 8;
                        break;
                    case RCKey.remote_9_value:
                        inputField.text += 9;
                        break;
                    case RCKey.remote_delete_value:
                        inputField.text = string.IsNullOrEmpty(inputField.text) ? string.Empty : inputField.text.Substring(0, inputField.text.Length - 1);
                        break;
                    case RCKey.remote_menu_value:
                        StartCoroutine(SetKeyBroadActive());
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
    }
}


