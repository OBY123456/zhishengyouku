using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using DG.Tweening;

namespace PayCode
{
    public class PayData
    {
        public string description = "无";
        public string amount = "0.01";

        public PayData() { }
        public PayData(string _description, string _amount)
        {
            description = _description;
            amount = _amount;
        }
    }

    public class PayBackData
    {
        public PayOrderCoderData Data;
        public int Total;
        public int Tag;
        public string Message;
        public string Description;
    }

    public class PayOrderCoderData
    {
        public string PayOrderCode;
        public string QrCodeUrl;
    }

    public class OrdersData
    {
        public string payOrderCode;

        public OrdersData() { }

        public OrdersData(string _payOrderCode)
        {
            payOrderCode = _payOrderCode;
        }
    }

    public static class PayEvent
    {
        public static event Action payevent;

        public static void OnPayEvent()
        {
            payevent?.Invoke();
        }
    }

    public class PayManager : MonoBehaviour
    {
        private static PayManager instance;
        public static PayManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject Prefab = Resources.Load<GameObject>("PayManager");
                    GameObject temp = Instantiate(Prefab);
                    temp.name = nameof(PayManager);
                    instance = temp.GetComponent<PayManager>();
                }
                return instance;
            }
        }

        /// <summary>
        /// 支付接口，返回支付二维码
        /// </summary>
        private string Url_pay = "https://api.itaocow.com.cn/factory/api/Order/CreateOrder";

        /// <summary>
        /// 订单接口，返回订单信息
        /// </summary>
        private string Url_order = "https://api.itaocow.com.cn/factory/api/Order/CheckOrderPayReuslt";

        [SerializeField]
        private RawImage rawImage;

        [SerializeField]
        private Text text;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private bool IsNetwork = false;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="Money"></param>
        public void Init(float Money)
        {
            text.text = Money.ToString() + "元/次";
            StartCoroutine(nameof(GetNetWorking));
            StartCoroutine(Pay(Money));
        }

        /// <summary>
        /// 打开页面
        /// </summary>
        /// <param name="OpenTimes"></param>
        public void Open(float OpenTimes = 0)
        {
            if(canvasGroup.alpha == 0)
            canvasGroup.DOFade(1,OpenTimes);
        }

        /// <summary>
        /// 关闭页面
        /// </summary>
        /// <param name="HideTimes"></param>
        public void Hide(float HideTimes = 0)
        {
            if(canvasGroup.alpha == 1)
            canvasGroup.DOFade(0, HideTimes);
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        public void Stop()
        {
            StopAllCoroutines();
        }

        private IEnumerator GetNetWorking()
        {
            while(!IsNetwork)
            {
                UnityWebRequest request = UnityWebRequest.Get("https://www.baidu.com/");
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    if (IsNetwork)
                    {
                        IsNetwork = false;

                    }
                }
                else
                {
                    if (!IsNetwork)
                    {
                        IsNetwork = true;
                    }
                }

                yield return secondsRealtime;
            }
            
        }


        private IEnumerator Pay(float Money)
        {
            while(!IsNetwork)
            {
                yield return null;
            }

            UnityWebRequest request = new UnityWebRequest(Url_pay, "POST");
            DownloadHandler downloadHandler = new DownloadHandlerBuffer();
            request.downloadHandler = downloadHandler;
            request.SetRequestHeader("Content-Type", "application/json-patch+json");
            byte[] vs = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new PayData(System.DateTime.Now.ToString("yyyy:MM:dd"), Money.ToString())));
            request.uploadHandler = new UploadHandlerRaw(vs);

            yield return request.SendWebRequest();
            if (request.isDone)
            {
                string msg = request.downloadHandler.text;
                Debug.Log("提交订单==" + msg);
                if (!string.IsNullOrEmpty(msg))
                {
                    PayBackData payBackData = JsonConvert.DeserializeObject<PayBackData>(msg);
                    if (payBackData.Tag == 0)
                    {
                        rawImage.texture = null;
                        Debug.LogError(msg);
                    }
                    else if (payBackData.Tag == 1)
                    {
                        //Debug.Log("QrCodeUrl==" + payBackData.Data.QrCodeUrl);
                        StartCoroutine(DownPicTexture(payBackData.Data.QrCodeUrl, rawImage));
                        //Debug.Log("PayOrderCode==" + payBackData.Data.PayOrderCode);
                        StartCoroutine(检查订单是否支付(payBackData.Data.PayOrderCode));
                    }
                }
                else
                {
                    rawImage.texture = null;
                    Debug.Log(request.error);
                }
            }
        }

        private IEnumerator DownPicTexture(string url, RawImage rawImage)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerTexture();
            request.SetRequestHeader("accept", "*/*");
            yield return request.SendWebRequest();
            if (request.isDone)
            {
                if (string.IsNullOrEmpty(request.error))
                {
                    rawImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                }
                else
                {
                    rawImage.texture = null;
                    Debug.Log(request.error);
                }
            }
        }

        WaitForSecondsRealtime secondsRealtime = new WaitForSecondsRealtime(1);
        private IEnumerator 检查订单是否支付(string OrderCoder)
        {
            string Temp = OrderCoder;
            while (true)
            {
                UnityWebRequest request = new UnityWebRequest(Url_order, "POST");
                DownloadHandler downloadHandler = new DownloadHandlerBuffer();
                request.downloadHandler = downloadHandler;
                request.SetRequestHeader("Content-Type", "application/json-patch+json");
                byte[] vs = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new OrdersData(Temp)));
                request.uploadHandler = new UploadHandlerRaw(vs);

                yield return request.SendWebRequest();
                if (request.isDone)
                {
                    string msg = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(msg))
                    {
                        JObject pairs = JObject.Parse(msg);
                        //Debug.Log("检查订单是否支付==" + msg);
                        if (pairs.ContainsKey("Data"))
                        {
                            bool data = (bool)pairs.GetValue("Data");
                            if(data)
                            {
                                PayEvent.OnPayEvent();
                            }
                        }

                        if (!rawImage.enabled)
                        {
                            rawImage.enabled = true;
                        }
                    }
                    else
                    {
                        if(rawImage.enabled)
                        {
                            rawImage.enabled = false;
                        }
                        Debug.LogError("网络错误");
                    }
                }

                yield return secondsRealtime;
            }
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}


