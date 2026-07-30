using UnityEngine;
using MTPanel;

/// <summary>
/// 封装类
/// </summary>
public class DataClass
{
    public string className;
    public string classJson;
    public DataClass() { }
    public DataClass(string str0, string str1)
    {
        className = str0;
        classJson = str1;
    }
}

/// <summary>
/// UDP传输的全部的数据类型枚举
/// </summary>
public enum ClassEnum
{
    UdpSceneName,
    GameData,
    DrawData,
    TimeData,
    OrderData,
    SceneState,
    MsgData,
}

/// <summary>
/// 场景名字
/// </summary>
public class UdpSceneName
{
    public string Scenename;

    public UdpSceneName() { }

    public UdpSceneName(string _Scenename)
    {
        Scenename = _Scenename;
    }
}

/// <summary>
/// 游戏选择
/// </summary>
public class GameData
{
    public PanelName panelName;

    public int Times;

    public float Moeny;

    public GameData(){ }

    public GameData(PanelName _panelName,int _Times = -1,float _Moeny = 0)
    {
        panelName = _panelName;
        Times = _Times;
        Moeny = _Moeny;
    }
}

public class MsgData
{
    public string Msg;

    [Newtonsoft.Json.JsonProperty]
    private string color;

    public int Index;

    public MsgData(){ }

    public MsgData(string _Msg,Color _color,int _Index)
    {
        Msg = _Msg;
        color = ColorUtility.ToHtmlStringRGBA(_color);
        Index = _Index;
    }

    public bool GetColor(out Color c)
    {
        return ColorUtility.TryParseHtmlString("#" + color,out c);
    }
}

public class DrawData
{
    public int Index;

    public string ImageBase64;

    public DrawData(){ }

    public DrawData(int _Index,string _ImageBase64)
    {
        Index = _Index;
        ImageBase64 = _ImageBase64;
    }
}

public class TimeData
{
    public int Times;

    public TimeData(){ }

    public TimeData(int _Times)
    {
        Times = _Times;
    }
}

public class SceneState
{
    public string SceneName;

    public PanelName panelName;

    public SceneState(){ }

    public SceneState(string _SceneName,PanelName _panelName)
    {
        SceneName = _SceneName;
        panelName = _panelName;
    }
}

public enum Order
{
    获取场景状态,
}

public class OrderData
{
    public Order order;

    public OrderData(){ }

    public OrderData(Order _order)
    {
        order = _order;
    }
}

/// <summary>
/// 将类转为封装类的json字符串
/// </summary>
public static class ClassToJson
{
    public static string GetJson(ClassEnum classEnum, object data)
    {
        if (data != null)
        {
            try
            {
                string temp = Newtonsoft.Json.JsonConvert.SerializeObject(new DataClass(classEnum.ToString(), Newtonsoft.Json.JsonConvert.SerializeObject(data)));
                return temp;
            }
            catch
            {
                Debug.LogError("GetJson 数据为空");
                return string.Empty;
            }

        }
        else
        {
            return string.Empty;
        }
    }
}
