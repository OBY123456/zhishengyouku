using RC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RC
{
    public class RCPanel : MonoBehaviour
    {
        protected List<BaseRC> baseRCs = new List<BaseRC>();

        protected BaseRC CurrentRC;

        [Header("第一个UI")]
        public BaseRC FirstRC;

        protected virtual void Awake()
        {

        }

        // Start is called before the first frame update
        protected virtual void Start()
        {

        }

        public void AddBaseRC(BaseRC baseRC)
        {
            if (baseRCs != null)
            {
                if (!baseRCs.Contains(baseRC))
                {
                    baseRCs.Add(baseRC);
                }
            }
        }

        public void RemoveBaseRC(BaseRC baseRC)
        {
            if (baseRC != null)
            {
                if (baseRCs.Contains(baseRC))
                {
                    baseRCs.Remove(baseRC);
                }
            }
        }

        /// <summary>
        /// 自定义第一个UI
        /// </summary>
        /// <param name="baseRC"></param>
        public void SetFirstRC(BaseRC baseRC)
        {
            if (baseRC != null)
            {
                FirstRC = baseRC;
            }
        }

        /// <summary>
        /// 选取列表第一个UI作为FirstRC
        /// </summary>
        public virtual void SetFirstRC()
        {
            if(FirstRC == null && baseRCs.Count > 0)
            {
                FirstRC = baseRCs[0];
            }
        }

        public virtual void Open()
        {
            if(CurrentRC == null)
                CurrentRC = FirstRC;

            CurrentRC?.Open();
        }

        /// <summary>
        /// 关闭的时候选择是否清除索引，进入一个新的页面不需要，返回一个旧页面就需要
        /// </summary>
        /// <param name="IsNull"></param>
        public void Hide(bool IsNull)
        {
            CurrentRC?.Hide();
            if(IsNull)
            {
                CurrentRC = null;
                FirstRC = null;
            }
        }

        public virtual void Onclick()
        {
            if(CurrentRC != null)
            {
                CurrentRC.OnClick();
            }
        }

        public void Clear()
        {
            CurrentRC = null;
            FirstRC = null;
            baseRCs.Clear();
        }

        public virtual void Next(Rotation rotation)
        {
            if (CurrentRC == null)
            {
                CurrentRC = FirstRC;
                CurrentRC.Open();
            }
            else
            {
                BaseRC temp = null;
                for (int i = 0; i < baseRCs.Count; i++)
                {
                    if(baseRCs[i].IsHide())
                    {
                        switch (rotation)
                        {
                            case Rotation.上:
                                if(baseRCs[i].ScreenPos.y > CurrentRC.ScreenPos.y)
                                {
                                    if (temp == null)
                                    {
                                        temp = baseRCs[i];
                                    }
                                    else
                                    {
                                        if(Vector2.Distance(baseRCs[i].ScreenPos,CurrentRC.ScreenPos) < Vector2.Distance(temp.ScreenPos,CurrentRC.ScreenPos))
                                        {
                                            temp = baseRCs[i];
                                        }
                                    }
                                }
                                break;
                            case Rotation.下:
                                if(baseRCs[i].ScreenPos.y < CurrentRC.ScreenPos.y)
                                {
                                    if (temp == null)
                                    {
                                        temp = baseRCs[i];
                                    }
                                    else
                                    {
                                        if(Vector2.Distance(baseRCs[i].ScreenPos,CurrentRC.ScreenPos) < Vector2.Distance(temp.ScreenPos,CurrentRC.ScreenPos))
                                        {
                                            temp = baseRCs[i];
                                        }
                                    }
                                }
                                break;
                            case Rotation.左:
                                if(baseRCs[i].ScreenPos.x < CurrentRC.ScreenPos.x)
                                {
                                    if (temp == null)
                                    {
                                        temp = baseRCs[i];
                                    }
                                    else
                                    {
                                        if(Vector2.Distance(baseRCs[i].ScreenPos,CurrentRC.ScreenPos) < Vector2.Distance(temp.ScreenPos,CurrentRC.ScreenPos))
                                        {
                                            temp = baseRCs[i];
                                        }
                                    }
                                }
                                break;
                            case Rotation.右:
                                if(baseRCs[i].ScreenPos.x > CurrentRC.ScreenPos.x)
                                {
                                    if (temp == null)
                                    {
                                        temp = baseRCs[i];
                                    }
                                    else
                                    {
                                        if(Vector2.Distance(baseRCs[i].ScreenPos,CurrentRC.ScreenPos) < Vector2.Distance(temp.ScreenPos,CurrentRC.ScreenPos))
                                        {
                                            temp = baseRCs[i];
                                        }
                                    }
                                }
                                break;
                        }
                    }
                }

                if (temp != null)
                {
                    CurrentRC.Hide();
                    temp.Open();
                    CurrentRC = temp;
                }
            }
        }
    }
}


