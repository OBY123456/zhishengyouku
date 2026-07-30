using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using DG.Tweening;

namespace RC
{
    public class ScrollRectPanel : RCPanel
    {
        private ScrollRect scrollRect;
        protected int Index = 0;
        public Vector3[] corners = new Vector3[4];
        protected RectTransform ScrollRect;
        private RectTransform ContentRect;
        public RCManager rcmanager;
        protected GridLayoutGroup layoutGroup;
        protected int VerticalOffect;

        protected override void Awake()
        {
            base.Awake();
            scrollRect = GetComponent<ScrollRect>();
            Navigation navigation = new Navigation();
            navigation.mode = Navigation.Mode.None;

            layoutGroup = scrollRect.content.GetComponent<GridLayoutGroup>();

            if(scrollRect.horizontalScrollbar != null)
            scrollRect.horizontalScrollbar.navigation = navigation;

            if(scrollRect.verticalScrollbar != null)
            scrollRect.verticalScrollbar.navigation = navigation;

            ScrollRect = scrollRect.GetComponent<RectTransform>();
            ScrollRect.GetWorldCorners(corners);
            ContentRect = scrollRect.content.GetComponent<RectTransform>();
            rcmanager = FindObjectOfType<RCManager>();
        }

        public override void SetFirstRC()
        {
            GetbaseRCs();
            base.SetFirstRC();
        }

        private void GetbaseRCs()
        {
            BaseRC[] basestemp = scrollRect.content.GetComponentsInChildren<BaseRC>(false);
            if(basestemp.Length > 0)
            {
                baseRCs.Clear();
                for (int i = 0; i < basestemp.Length; i++)
                {
                    baseRCs.Add(basestemp[i]);
                    if(basestemp[i].RCpanel != this)
                    {
                        basestemp[i].RCpanel = this;
                    }
                }
            }
        }

        public override void Open()
        {
            Index = 0;
            FirstRC.Open();
            CurrentRC = FirstRC;
            if(scrollRect.horizontal)
            {
                scrollRect.horizontalNormalizedPosition = 0;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 1;
            }
            

            if (layoutGroup != null)
            {
                switch (layoutGroup.constraint)
                {
                    case GridLayoutGroup.Constraint.Flexible:
                        //还没遇到这种情况所以还没写
                        break;
                    case GridLayoutGroup.Constraint.FixedColumnCount:
                        VerticalOffect = layoutGroup.constraintCount;
                        break;
                    case GridLayoutGroup.Constraint.FixedRowCount:
                        if (layoutGroup.constraintCount != 0)
                        {

                            VerticalOffect = baseRCs.Count / layoutGroup.constraintCount;
                        }
                        else
                        {
                            VerticalOffect = 1;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Onclick()
        {
            if (CurrentRC != null)
            {
                CurrentRC.Hide();
                if (baseRCs.Count > 0)
                {
                    if (Index >= baseRCs.Count)
                        Index = baseRCs.Count - 1;


                    CurrentRC = baseRCs[Index];
                    CurrentRC.Open();
                }
                else
                {
                    rcmanager.Return();
                }
            }
            else
            {
                Debug.Log("CurrentRC == null");
            }
        }

        public override void Next(Rotation rotation)
        {
            if (CurrentRC == null)
            {
                CurrentRC = FirstRC;
                CurrentRC.Open();
            }
            else
            {
                int temp = 0;
                switch (rotation)
                {
                    case Rotation.上:
                        temp = Index - VerticalOffect;
                        break;
                    case Rotation.下:
                        temp = Index + VerticalOffect;
                        break;
                    case Rotation.左:
                        temp = Index - 1;
                        break;
                    case Rotation.右:
                        temp = Index + 1;
                        break;
                    default:
                        break;
                }

                if (temp >= 0 && temp < baseRCs.Count)
                {
                    CurrentRC.Hide();
                    CurrentRC = baseRCs[temp];
                    CurrentRC.Open();
                   
                    Index = temp;
                    Vector3[] baseRCcorners = new Vector3[4];
                    CurrentRC.rectTransform.GetWorldCorners(baseRCcorners);
                    if(scrollRect.horizontal)
                    {
                        if(baseRCcorners[0].x < corners[0].x || baseRCcorners[3].x > corners[3].x)
                        {
                            NextValue();
                        }
                    }
                    else
                    {
                        if (baseRCcorners[1].y > corners[1].y || baseRCcorners[0].y < corners[0].y)
                        {
                            NextValue();
                        }
                    }
                    
                }
            }
        }

        public void NextValue()
        {
            if(scrollRect.horizontal)
            {
                var OffsetW = ContentRect.rect.width - ScrollRect.rect.width;
                var ItemW = CurrentRC.rectTransform.anchoredPosition.x + ContentRect.anchoredPosition.x;
                //往右是减，往左是加
                var offset = ItemW - ScrollRect.rect.width / 2;
                var value = offset / OffsetW;

                float temp = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + value);
                DOTween.To(() => scrollRect.horizontalNormalizedPosition, x => scrollRect.horizontalNormalizedPosition = x, temp, 0.1f);
            }
            else
            {
                var OffsetH = ContentRect.rect.height - ScrollRect.rect.height;
                var ItemH = CurrentRC.rectTransform.anchoredPosition.y + ContentRect.anchoredPosition.y;
                //往上是减，往下是加
                var offset = ItemH + ScrollRect.rect.height / 2;
                var value = offset / OffsetH;
                
                float temp = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + value);
                DOTween.To(() => scrollRect.verticalNormalizedPosition, x => scrollRect.verticalNormalizedPosition = x, temp, 0.1f);
            }
        }
    }
}
