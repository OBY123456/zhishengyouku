using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Config;

namespace Setting
{
    /// <summary>
    /// 入口类
    /// </summary>
    public class Main : MonoBehaviour
    {
        [Header("公司名称")]
        /// <summary>
        /// 公司名称
        /// </summary>
        public string companyName = "ZSKJ";

        [Header("产品名称")]
        /// <summary>
        /// 产品名称
        /// </summary>
        public string productName = "OBYFrame";

#if UNITY_STANDALONE_WIN
        [Header("是否后台运行")]
        /// <summary>
        /// 是否后台运行
        /// </summary>
        public bool runInBackground = true;
#endif

        void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Start()
        {

        }

        [Button("设置")]
        public void SetPlayerSettings()
        {
#if UNITY_EDITOR
            Debug.Log("设置");
            if (productName != null && productName != "" && productName != string.Empty)
                PlayerSettings.productName = productName;
            PlayerSettings.companyName = companyName;
#if UNITY_STANDALONE_WIN
            //如果是2019以上，需要Player Setting->Resolution and Presentation->Standalone Player Options->Use DXGI Filp Model Swapchain for D3D1为false
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
            PlayerSettings.runInBackground = runInBackground;
            QualitySettings.vSyncCount = 0;
#if UNITY_2019_1_OR_NEWER
            PlayerSettings.useFlipModelSwapchain = false;
#endif
#elif UNITY_ANDROID
#if UNITY_STANDALONE_WIN
        PlayerSettings.runInBackground = runInBackground;
#endif
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android,ApiCompatibilityLevel.NET_4_6);
#endif
#endif
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        private void OnApplicationQuit()
        {
            TimeTool.Instance?.Dispose();
        }
    }
}

