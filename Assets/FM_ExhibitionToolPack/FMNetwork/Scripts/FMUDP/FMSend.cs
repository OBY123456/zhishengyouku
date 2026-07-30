using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Setting;

public class FMSend : MonoBehaviour
{
    public static FMSend Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void SentToOther(string msg)
    {
        if(!string.IsNullOrEmpty(msg))
        FMNetworkManager.instance?.SendToOthers(Encoding.UTF8.GetBytes(msg));
    }

    public void SentToServer(string msg)
    {
        if(!string.IsNullOrEmpty(msg))
        FMNetworkManager.instance?.SendToServer(Encoding.UTF8.GetBytes(msg));

        Debug.Log(msg);
    }
}
