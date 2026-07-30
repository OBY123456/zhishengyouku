using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LanguageSetting;

public class SetEN : MonoBehaviour
{
    public Text text;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(text != null)
        {
            text.text = LanguageManger.Instance.GetValue(text.text);
        }
    }
}
