using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MTFrame.MTAudio
{
    /// <summary>
    /// 音频类型
    /// </summary>
    public enum AudioEnunType
    {
        /// <summary>
        /// 背景音频-适合循环播放
        /// </summary>
        BGM,
        /// <summary>
        /// 效果音频-适合播放效果音
        /// </summary>
        Effset,
        /// <summary>
        /// 对话音频-适合播放对话
        /// </summary>
        Speech
    }
}