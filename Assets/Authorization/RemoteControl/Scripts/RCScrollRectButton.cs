using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RC
{
    public class RCScrollRectButton : BaseRC
    {
        protected Button MySelfBtn;

        protected override void Awake()
        {
            base.Awake();
            MySelfBtn = GetComponent<Button>();
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.None;
            MySelfBtn.navigation = navigation;
        }

        public override void OnClick()
        {
            base.OnClick();
            Hide();
            EventSystem.current.SetSelectedGameObject(MySelfBtn.gameObject);
        }

        public override void Open()
        {
            base.Open();
            EventSystem.current.SetSelectedGameObject(MySelfBtn.gameObject);
            MySelfBtn.onClick.AddListener(click);
        }

        public override void Hide()
        {
            base.Hide();
            MySelfBtn.onClick.RemoveListener(click);
        }

        private void click()
        {
            RCpanel?.Onclick();
        }
    }
}

