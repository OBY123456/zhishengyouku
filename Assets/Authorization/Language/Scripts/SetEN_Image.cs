using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LanguageSetting;

public class SetEN_Image : MonoBehaviour
{
    private Image image;

    [Header("英文版")]
    public Sprite sprite;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(LanguageManger.Instance.GetLanguageType() == LanguageType.英文 && image != null && sprite != null)
        {
            image.sprite = sprite;
        }
    }
}
