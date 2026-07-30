using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RC;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RC
{
    public class RCButton : BaseRC
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
            EventSystem.current.SetSelectedGameObject(MySelfBtn.gameObject);
        }

        public override void Open()
        {
            base.Open();
            EventSystem.current.SetSelectedGameObject(MySelfBtn.gameObject);
        }
    }
}

