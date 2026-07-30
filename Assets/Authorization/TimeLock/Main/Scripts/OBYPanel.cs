using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace OBY.时间锁
{
    public abstract class OBYPanel : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        public float OpenTime = 0.5f;

        public float HideTime = 0.5f;

        public bool IsOpen = false;

        public virtual void Open()
        {
            canvasGroup.Open(OpenTime);
            IsOpen = true;
        }

        public virtual void Hide()
        {
            canvasGroup.Hide(HideTime);
            IsOpen = false;
        }
    }
}

