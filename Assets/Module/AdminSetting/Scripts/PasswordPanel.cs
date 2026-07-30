using Config;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PasswordPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public InputField inputField;
    public Button OkBtn, BackBtn;
    private string PasswordStr = "zs123456";



    private void Awake()
    {
        canvasGroup.Open();
        inputField.onValueChanged.AddListener(Contrast);
        OkBtn.interactable = false;
        OkBtn.onClick.AddListener(() =>
        {
            canvasGroup.Hide();
        });

        BackBtn.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("PaycodeDemo");
        });

        EventSystem.current?.SetSelectedGameObject(inputField.gameObject);
        inputField.text = string.Empty;
        Cursor.visible = true;
    }

    private void Contrast(string arg0)
    {
        if (arg0 == PasswordStr)
        {
            OkBtn.interactable = true;
        }
        else
        {
            OkBtn.interactable = false;
        }
    }

    private void OnDestroy()
    {
        Cursor.visible = ProgramConfig.Instance.programData.是否显示鼠标;
    }
}
