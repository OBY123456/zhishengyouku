using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Config
{
    [Serializable]
    public class ProgramData
    {
        /// <summary>
        /// 是否显示鼠标
        /// </summary>
        public bool 是否显示鼠标 = true;

        public int 分辨率X = 1920;

        public int 分辨率Y = 1080;

        public PaymentModel 收费模式;

        public float 费用 = 0.01f;

        public LanguageSetting.LanguageType 语言类型 = LanguageSetting.LanguageType.中文;
    }


    public enum PaymentModel
    {
        免费模式,
        付费模式
    }

    public class ProgramConfig : MonoSingle<ProgramConfig>
    {
        public ProgramData programData = new ProgramData();
        private string File_name = "ProgramConfig.txt";
        private string Path;

        protected override void Awake()
        {
            base.Awake();
#if UNITY_STANDALONE_WIN
            Path = Application.streamingAssetsPath + "/" + File_name;
#else
            Path = Application.persistentDataPath + "/" + File_name;
#endif
            if (File.Exists(Path))
            {
                string st = FileHandle.ReadAllText(Path);
                Debug.Log("配置路径：" + Path);
                Debug.Log("软件配置：" + st);
                try
                {
                    programData = FileHandle.DeserializeObject<ProgramData>(st,true);
                }
                catch
                {
                    Debuger.Instance.Init();
                    Debug.Log("数据格式不对");
                }
            }
            else
            {
                SaveData();
            }
        }

        private void Start()
        {
            Screen.SetResolution(programData.分辨率X, programData.分辨率Y, FullScreenMode.FullScreenWindow);
        }

        [Button("保存")]
        public void SaveData()
        {
#if UNITY_STANDALONE_WIN
            Path = Application.streamingAssetsPath + "/" + File_name;
#else
            Path = Application.persistentDataPath + "/" + File_name;
#endif
            string st = FileHandle.SerializeObject(programData,true);
            FileHandle.WriteAllText(Path,st);
            Debug.Log("配置路径：" + Path);
            Debug.Log("软件配置：" + st);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}

