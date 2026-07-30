using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RC
{
    public class BaseRC : MonoBehaviour
    {
        protected CanvasGroup Choose;

        /// <summary>
        /// 屏幕坐标
        /// </summary>
        public Vector2 ScreenPos;

        /// <summary>
        /// 必须使用这个组件来控制UI显示或者隐藏
        /// </summary>
        protected CanvasGroup MySelfCanvas;

        public RCPanel RCpanel;

        [HideInInspector]
        public RectTransform rectTransform;

        protected virtual void Awake()
        {
            if(transform.Find("选中框") == null)
            {
                GameObject obj = Instantiate(Resources.Load<GameObject>("选中框"),transform);
                obj.transform.SetAsFirstSibling();
                Choose = obj.GetComponent<CanvasGroup>();
            }
            else
            {
                Choose = transform.Find("选中框").GetComponent<CanvasGroup>();
            }
            
            Choose.blocksRaycasts = false;
            Choose.alpha = 0;

            MySelfCanvas = GetComponent<CanvasGroup>();
            rectTransform = GetComponent<RectTransform>();
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(Add_BaseRC());
        }

        private IEnumerator Add_BaseRC()
        {
            yield return new WaitForEndOfFrame();
            AddBaseRC();
        }

        protected virtual void OnDisable()
        {
            RemoveBaseRC();
        }

        /// <summary>
        /// 确定
        /// </summary>
        public virtual void OnClick()
        {
            //Debug.Log("222" + name);
        }

        /// <summary>
        /// 选中
        /// </summary>
        public virtual void Open()
        {
            Choose.alpha = 1;
        }

        /// <summary>
        /// 未选中
        /// </summary>
        public virtual void Hide()
        {
            Choose.alpha = 0;
        }

        /// <summary>
        /// 通过判断CanvasGroup看UI是否处于显示状态
        /// </summary>
        /// <returns></returns>
        public bool IsHide()
        {
            if(MySelfCanvas == null)
                return true;

            if(MySelfCanvas.alpha == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// 在RCPanel里面把自己添加进去
        /// </summary>
        /// <param name="parent"></param>
        private void AddBaseRC()
        {
            if(RCpanel == null)
                RCpanel = FindRCPanel(transform.parent);

            RCpanel?.AddBaseRC(this);
            ScreenPos = transform.position;
        }

        private void RemoveBaseRC()
        {
            if (RCpanel == null)
                RCpanel = FindRCPanel(transform.parent);

            RCpanel?.RemoveBaseRC(this);
            //RCpanel = null;
        }

        private RCPanel FindRCPanel(Transform parent)
        {
            if(parent.GetComponent<RCPanel>() == null)
            {
                if(parent.parent != null)
                {
                    return FindRCPanel(parent.parent);
                }
                else
                {
                    Debug.Log("没有找到RCPanel所在父物体");
                    return null;
                }
            }
            else
            {
                return parent.GetComponent<RCPanel>();
            }
        }
    }
}


