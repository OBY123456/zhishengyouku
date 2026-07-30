using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 只适用于PC端-Tcp的话只能判断客户端的
/// </summary>
public class NetworkPing : MonoBehaviour
{
    public static NetworkPing Instance;

    private bool IsNetwork = false;
    private bool IsUdp = false;
    private bool IsTcp = false;

    public CanvasGroup NPCanvas,UDPCanvas,TcpCanvas;

    public Text LogText;
    public CanvasGroup LogCanvas;

    private void Awake()
    {
        Instance = this;
        SetNetSprite(IsNetwork);
        SetUdpSprite(IsUdp);
        LogCanvas.alpha = 0;
        TcpCanvas.alpha = 1;
        StartCoroutine(nameof(Get));
    }

    private void Start()
    {
        
    }

    IEnumerator Get()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get("https://www.baidu.com/");
            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                if(IsNetwork)
                {
                    IsNetwork = false;
                    SetNetSprite(IsNetwork);
                }
            }
            else
            {
                if(!IsNetwork)
                {
                    IsNetwork = true;
                    SetNetSprite(IsNetwork);
                }
            }

            if(FMNetworkManager.instance)
            {
                switch (FMNetworkManager.instance.NetworkType)
                {
                    case FMNetworkType.Server:
                        if (FMServer.instance)
                        {
                            if (FMServer.instance.IsConnected)
                            {
                                if (!IsUdp)
                                {
                                    IsUdp = true;
                                    SetUdpSprite(IsUdp);
                                }
                            }
                            else
                            {
                                if (IsUdp)
                                {
                                    IsUdp = false;
                                    SetUdpSprite(IsUdp);
                                }
                            }
                        }
                        break;
                    case FMNetworkType.Client:
                        if (FMClient.instance)
                        {
                            if (FMClient.instance.IsConnected)
                            {
                                if (!IsUdp)
                                {
                                    IsUdp = true;
                                    SetUdpSprite(IsUdp);
                                }
                            }
                            else
                            {
                                if (IsUdp)
                                {
                                    IsUdp = false;
                                    SetUdpSprite(IsUdp);
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    public void ShowMsg(string msg)
    {
        LogText.text = msg;
        transform.DOKill();
        LogCanvas.alpha = 1;
        LogCanvas.DOFade(0, 2f).SetEase(Ease.InQuint).OnComplete(() =>
        {
            LogCanvas.alpha = 0;
        });
    }

    public bool GetNetworkState()
    {
        return IsNetwork;
    }

    public bool GetTcpState()
    {
        return IsTcp;
    }

    public bool GetUdpState()
    {
        return IsUdp;
    }

    private void SetNetSprite(bool IsNet)
    {
        if(IsNet)
        {
            NPCanvas.alpha = 0;
        }
        else
        {
            NPCanvas.alpha = 1;
        }
    }

    private void SetUdpSprite(bool IsUdp)
    {
        if (IsUdp)
        {
            UDPCanvas.alpha = 0;
        }
        else
        {
            UDPCanvas.alpha = 1;
        }
    }

    public void SetTcpSprite(bool Istcp)
    {
        IsTcp = Istcp;
        if(IsTcp)
        {
            TcpCanvas.alpha = 0;
        }
        else
        {
            TcpCanvas.alpha = 1;
        }
    }

    bool IsPrivateNetwork4(IPAddress ipv4Address)
    {
        byte[] ipBytes = ipv4Address.GetAddressBytes();
        if (ipBytes[0] == 10) return true;
        if (ipBytes[0] == 172 && ipBytes[1] >= 16 && ipBytes[1] <= 31) return true;
        if (ipBytes[0] == 192 && ipBytes[1] == 168) return true;

        return false;
    }
}
