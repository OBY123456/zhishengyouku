using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OBYTouch
{
    public abstract class BaseTouch : MonoBehaviour
    {
        protected virtual void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Touch");
        }

        public virtual void OnTouch(Vector3 point)
        {

        }
    }
}

