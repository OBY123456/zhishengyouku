using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PayCode;
using System;

public class PayTest : MonoBehaviour
{
    private void Awake()
    {
        PayManager.Instance.Init(Config.ProgramConfig.Instance.programData.费用);
        PayEvent.payevent += Payevent;
    }

    private void Payevent()
    {
        Debug.Log("付款成功，开始游戏");
    }

    // Start is called before the first frame update
    void Start()
    {
        KeyCode[] keyCodes = new KeyCode[] {
            KeyCode.UpArrow,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.DownArrow,
            KeyCode.A,
            KeyCode.A,
            KeyCode.B,
            KeyCode.B
            };

        gameObject.AddComponent<KeyCombination>().Init(keyCodes, () =>
        {
            PayManager.Instance.Hide();
            UnityEngine.SceneManagement.SceneManager.LoadScene("后台设置");
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            PayManager.Instance?.Open();
        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            PayManager.Instance?.Hide();
        }
    }

    private void OnDestroy()
    {
        PayEvent.payevent -= Payevent;
    }
}
