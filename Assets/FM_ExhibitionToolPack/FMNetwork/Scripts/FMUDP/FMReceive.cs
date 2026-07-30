using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System;
using MTFrame.MTEvent;

public class FMReceive : MonoBehaviour
{
    private void Start()
    {
        
    }

    public void Action_ProcessByteData(byte[] _string)
    {
        if (_string!= null && _string.Length > 0)
        {
            string receive = Encoding.UTF8.GetString(_string, 0, _string.Length);
            if (!string.IsNullOrEmpty(receive) && !string.IsNullOrWhiteSpace(receive))
            {
                BroadcastMsg(receive);
                //Debug.Log("receive ==" + receiveMsg);
            }
        }
    }

    /// <summary>
    /// 接收到消息按数据类型分发出去
    /// </summary>
    /// <param name="msg"></param>
    private void BroadcastMsg(string msg)
    {
        try
        {
            DataClass dataClass = Newtonsoft.Json.JsonConvert.DeserializeObject<DataClass>(msg);

            EventParamete eventParamete = new EventParamete();
            eventParamete.AddParameter(dataClass.classJson);
            EventManager.TriggerEvent(GenericEventEnumType.Generic, dataClass.className, eventParamete);
        }
        catch
        {
            Debug.LogError("不是自己定义的封装类");
        }
    }
}
