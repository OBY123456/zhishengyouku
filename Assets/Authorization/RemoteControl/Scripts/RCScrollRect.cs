using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RC
{
    public class RCScrollRect : BaseRC
    {
        private RCPanel rcpanel;
        public RCManager rcmanager;
        private Button button;

        protected override void Awake()
        {
            base.Awake();
            button = GetComponent<Button>();
            if(button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            button.transition = Selectable.Transition.None;

            rcpanel = GetComponent<ScrollRectPanel>();
            if(rcpanel == null)
            {
                rcpanel = gameObject.AddComponent<ScrollRectPanel>();
            }

            rcmanager = FindObjectOfType<RCManager>();
        }

        public override void OnClick()
        {
            base.OnClick();
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }

        private void click()
        {
            rcpanel.SetFirstRC();
            if(rcpanel.FirstRC != null)
            rcmanager?.SetPanel(rcpanel);
        }

        public override void Open()
        {
            base.Open();
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            button.onClick.AddListener(click);
        }

        public override void Hide()
        {
            base.Hide();
            button.onClick.RemoveListener(click);
        }
    }
}

