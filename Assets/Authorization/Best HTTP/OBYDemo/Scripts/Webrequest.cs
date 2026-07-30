using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using UnityEngine.Networking;
using System;
using Newtonsoft.Json.Linq;

namespace OBY.Networking
{
    public class HeadData
    {
        public string head;

        public string data;

        public HeadData(){ }

        public HeadData(string _head,string _data)
        {
            head = _head;
            data = _data;
        }
    }

    public static class Webrequest
    {
        private static List<HeadData> headDatas = new List<HeadData>
        {
            new HeadData("accept","*/*"),
            new HeadData("Content-Type","application/json-patch+json"),
        };

        /// <summary>
        /// 基于Besthttp的请求，不堵塞主线程
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="action">回调：请求异常则返回string.Empty</param>
        /// <param name="HTTPMethods">请求类型,目前只考虑Post和Get，其他还没用过</param>
        /// <param name="RawData">需要携带的字符串数据</param>
        /// <param name="_headDatas">设置头部参数的链表</param>
        public static void Request(string url, HTTPMethods hTTPMethods,Action<string> action, string RawData = "", List<HeadData> _headDatas = null)
        {
            HTTPRequest request = new HTTPRequest(new Uri(url), methodType: hTTPMethods, (req, resp) =>
            {
                switch (req.State)
                {
                    case HTTPRequestStates.Finished:
                        if (!string.IsNullOrEmpty(resp.DataAsText))
                        {
                            action?.Invoke(resp.DataAsText);
                        }
                        else
                        {
                            action?.Invoke(string.Empty);
                        }
                        break;
                    case HTTPRequestStates.Error:
                    case HTTPRequestStates.Aborted:
                    case HTTPRequestStates.ConnectionTimedOut:
                    case HTTPRequestStates.TimedOut:
                        action?.Invoke(string.Empty);
                        break;
                }
            });

            if(_headDatas == null)
            {
                _headDatas = headDatas;
            }

            if (_headDatas.Count > 0)
            {
                for (int i = 0; i < _headDatas.Count; i++)
                {
                    request.SetHeader(_headDatas[i].head, _headDatas[i].data);
                }
            }

            if (!string.IsNullOrEmpty(RawData))
                request.RawData = System.Text.Encoding.UTF8.GetBytes(RawData);

            request.Send();
        }

        /// <summary>
        /// 基于Besthttp的下载图片，不堵塞主线程
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="action">回调：如下载错误则回调返回Null</param>
        public static void GetTexture(string url, Action<Texture2D> action)
        {
            HTTPRequest request = new HTTPRequest(new Uri(url), methodType: HTTPMethods.Get, (req, resp) =>
            {
                switch (req.State)
                {
                    case HTTPRequestStates.Finished:
                        action?.Invoke(resp.DataAsTexture2D);
                        break;
                    case HTTPRequestStates.Error:
                    case HTTPRequestStates.Aborted:
                    case HTTPRequestStates.ConnectionTimedOut:
                    case HTTPRequestStates.TimedOut:
                        action?.Invoke(null);
                        break;
                }
            });
            request.SetHeader("accept", "*/*");
            request.Send();
        }

        /// <summary>
        /// 基于Unity协程的图片下载
        /// </summary>
        /// <param name="_Url">请求地址</param>
        /// <param name="action">回调：如下载错误则回调返回Null</param>
        /// <param name="IsLoop">是否循环下载，直到下载成功为止</param>
        /// <param name="mono">开启这个协程的继承mono的脚本</param>
        /// <param name="WaitTimes">开启循环下载之后循环的间隔时间</param>
        /// <returns></returns>
        public static IEnumerator DownPicture(string _Url, Action<Texture2D> action,bool IsLoop = false,MonoBehaviour mono = null,float WaitTimes = 1)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(_Url);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError || !string.IsNullOrEmpty(request.error))
            {
                if(IsLoop && mono != null)
                {
                    yield return new WaitForSecondsRealtime(WaitTimes);
                    yield return mono.StartCoroutine(DownPicture(_Url,action,IsLoop,mono,WaitTimes));
                }
                else
                {
                    action?.Invoke(null);
                }
            }
            else
            {
                action?.Invoke(((DownloadHandlerTexture)request.downloadHandler).texture);
            }
        }

        /// <summary>
        /// UnityWebRequest Post请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="action">回调：请求异常则返回string.Empty</param>
        /// <param name="IsLoop">是否循环请求，直到请求成功为止</param>
        /// <param name="mono">开启这个协程的继承mono的脚本</param>
        /// <param name="WaitTimes">开启循环下载之后循环的间隔时间</param>
        /// <param name="RawData">携带的字符串数据</param>
        /// <param name="_headDatas">设置头部参数的链表</param>
        /// <returns></returns>
        public static IEnumerator Post(string url, Action<string> action,bool IsLoop = false,MonoBehaviour mono = null,float WaitTimes = 1,string RawData = "", List<HeadData> _headDatas = null)
        {
            UnityWebRequest request = UnityWebRequest.Post(url, "POST");

            if(!string.IsNullOrEmpty(RawData))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(RawData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            if(_headDatas == null)
            {
                _headDatas = headDatas;
            }

            if (_headDatas.Count > 0)
            {
                for (int i = 0; i < _headDatas.Count; i++)
                {
                    request.SetRequestHeader(_headDatas[i].head, _headDatas[i].data);
                }
            }

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError || !string.IsNullOrEmpty(request.error))
            {
                if(IsLoop && mono != null)
                {
                    yield return new WaitForSecondsRealtime(WaitTimes);
                    mono.StartCoroutine(Post(url,action,IsLoop,mono,WaitTimes,RawData,_headDatas));
                }
                else
                {
                    action?.Invoke(null);
                }
            }
            else
            {
                action?.Invoke(request.downloadHandler.text);
            }
        }

        /// <summary>
        /// UnityWebRequest Get请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="action">回调：请求异常则返回string.Empty</param>
        /// <param name="IsLoop">是否循环请求，直到请求成功为止</param>
        /// <param name="mono">开启这个协程的继承mono的脚本</param>
        /// <param name="WaitTimes">开启循环下载之后循环的间隔时间</param>
        /// <param name="RawData">携带的字符串数据</param>
        /// <param name="_headDatas">设置头部参数的链表</param>
        /// <returns></returns>
        public static IEnumerator Get(string url, Action<string> action, bool IsLoop = false, MonoBehaviour mono = null, float WaitTimes = 1, string RawData = "", List<HeadData> _headDatas = null)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            if (!string.IsNullOrEmpty(RawData))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(RawData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            if (_headDatas == null)
            {
                _headDatas = headDatas;
            }

            if (_headDatas.Count > 0)
            {
                for (int i = 0; i < _headDatas.Count; i++)
                {
                    request.SetRequestHeader(_headDatas[i].head, _headDatas[i].data);
                }
            }

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError || !string.IsNullOrEmpty(request.error))
            {
                if (IsLoop && mono != null)
                {
                    yield return new WaitForSecondsRealtime(WaitTimes);
                    mono.StartCoroutine(Get(url, action, IsLoop, mono, WaitTimes, RawData, _headDatas));
                }
                else
                {
                    action?.Invoke(null);
                }
            }
            else
            {
                action?.Invoke(request.downloadHandler.text);
            }
        }
    }
}

