using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Text;

namespace Setting
{
    [Serializable]
    public struct LanguageKV
    {
        public string Key, Value;
    }

    [CreateAssetMenu(fileName = "LanguageConfig", menuName = "Create AssetFile/Create LanguageConfig", order = 1)]
    [Serializable]
    public class LanguageConfig : ScriptableObject
    {
        [Header("英文配置")]
        public List<LanguageKV> LanguageEN;

        /// <summary>
        /// 中文不需要，默认就是中文
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string GetValue(string Key)
        {
            string temp = LanguageEN.Find((LanguageKV) => LanguageKV.Key == Key).Value;
            if (!string.IsNullOrEmpty(temp))
                return temp;
            else
                return Key;
        }
    }
}

