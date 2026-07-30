using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LanguageSetting
{
    public class SetDropDown : MonoBehaviour
    {
        public Dropdown dropdown;

        private void Awake()
        {
            dropdown = GetComponent<Dropdown>();
        }

        // Start is called before the first frame update
        void Start()
        {
            if(dropdown != null && dropdown.options.Count > 0)
            {
                for (int i = 0; i < dropdown.options.Count; i++)
                {
                    dropdown.options[i].text = LanguageManger.Instance.GetValue(dropdown.options[i].text);
                }
            }
        }
    }
}

