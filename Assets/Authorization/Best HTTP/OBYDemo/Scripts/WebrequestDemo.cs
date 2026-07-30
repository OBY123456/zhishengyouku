using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System;

namespace OBY.Networking
{
    public class WebrequestDemo : MonoBehaviour
    {
        private const string PostUrl = @"https://jsonplaceholder.typicode.com/posts";

        private const string GetUrl = @"https://reqbin.com/echo/get/json";

        private const string TextureUrl = @"https://via.placeholder.com/50";

        public RawImage rawImage;

        public RawImage rawImage2;

        // Start is called before the first frame update
        IEnumerator Start()
        {
            Webrequest.Request(PostUrl, BestHTTP.HTTPMethods.Post,Post数据解析);
            yield return new WaitForEndOfFrame();
            Webrequest.Request(GetUrl, BestHTTP.HTTPMethods.Get,Get数据解析);
            yield return new WaitForEndOfFrame();
            //单次请求
            StartCoroutine(Webrequest.Post(PostUrl,Post数据解析2));
            yield return new WaitForEndOfFrame();
            //循环请求，直至成功 Get也是这么写，这里就不写了
            StartCoroutine(Webrequest.Post(PostUrl,Post数据解析3,true,this));
            yield return new WaitForEndOfFrame();
            //获取图片
            Webrequest.GetTexture(TextureUrl,Get图片数据);
            yield return new WaitForEndOfFrame();
            //循环请求，直至成功
            StartCoroutine(Webrequest.DownPicture(TextureUrl,Get图片数据2,true,this));
        }

        /// <summary>
        /// 在数据解析的时候根据情况是否要循环请求，直至请求成功
        /// </summary>
        /// <param name="msg"></param>
        private void Post数据解析(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("Post回调==" + msg);
                //如果取的部分是数组，则需要使用JArray
                JObject pairs = JObject.Parse(msg);
                if(pairs.ContainsKey("id"))
                {
                    Debug.Log("Post数据解析==" + pairs["id"]);
                }
            }
            //循环请求,运行前断网，运行之后联网即可测试,去掉else这部分就是单次请求
            else
            {
                 Webrequest.Request(PostUrl, BestHTTP.HTTPMethods.Post,Post数据解析);
            }
        }

        private void Post数据解析2(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("Post回调2==" + msg);
                //如果取的部分是数组，则需要使用JArray
                JObject pairs = JObject.Parse(msg);
                if(pairs.ContainsKey("id"))
                {
                    Debug.Log("Post数据解析2==" + pairs["id"]);
                } 
            }
        }

        private void Post数据解析3(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("Post回调3==" + msg);
                //如果取的部分是数组，则需要使用JArray
                JObject pairs = JObject.Parse(msg);
                if(pairs.ContainsKey("id"))
                {
                    Debug.Log("Post数据解析3==" + pairs["id"]);
                }
            }
        }

        private void Get数据解析(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                Debug.Log("Get回调==" + msg);
                //如果取的部分是数组，则需要使用JArray
                JObject pairs = JObject.Parse(msg);
                if(pairs.ContainsKey("success"))
                {
                    Debug.Log("Get数据解析==" + pairs["success"]);
                }
            }
            //循环请求,运行前断网，运行之后联网即可测试，去掉else这部分就是单次请求
            else
            {
                 Webrequest.Request(GetUrl, BestHTTP.HTTPMethods.Get,Get数据解析);
            }
        }

        public void Get图片数据(Texture2D texture)
        {
            if(texture != null)
            {
                rawImage.texture = texture;
                Debug.Log("Get图片数据==获取成功");
            }
            else//循环请求,运行前断网，运行之后联网即可测试，去掉else这部分就是单次请求
            {
                 Webrequest.GetTexture(TextureUrl,Get图片数据);
            }
        }

        public void Get图片数据2(Texture2D texture)
        {
            if (texture != null)
            {
                rawImage2.texture = texture;
                Debug.Log("Get图片数据2==获取成功");
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}

