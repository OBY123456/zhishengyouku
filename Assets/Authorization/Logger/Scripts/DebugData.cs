using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OBYDebug
{
    public class DebugData : MonoBehaviour
    {
        public Text TitleText;

        public Text DataText;

        public void Init(string _Title,string _Data)
        {
            TitleText.text = _Title;
            DataText.text = _Data;
        }

        public void SetData(string _Data)
        {
            DataText.text = _Data;
        }
    }
}

