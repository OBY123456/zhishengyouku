using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Encryption
//{
    public class MonoSingle<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = FindObjectOfType<T>();
                    if (go != null)
                    {
                        instance = go.GetComponent<T>();
                    }
                    else
                    {
                        instance = new GameObject(typeof(T).Name).AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance != null)
            {
                if (instance != this)
                {
                    gameObject.SetActive(false);
                    Destroy(this.gameObject);
                    return;
                }
            }
            else
            {
                var go = FindObjectOfType<T>();
                if (go != null)
                {
                    instance = go.GetComponent<T>();
                }
                else
                {
                    instance = new GameObject(typeof(T).Name).AddComponent<T>();
                }
                DontDestroyOnLoad(go);
            }
        }
    }
//}


