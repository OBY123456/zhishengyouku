using MTFrame.MTEvent;
using System;
using System.Collections;
using System.Collections.Generic;
using LanguageSetting;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Lean.Pool;
using OBYDebug;
using Setting;
using Config;

/// <summary>
/// OBY日志管理模块
/// </summary>
public class DebugManager : MonoBehaviour
{
    private static DebugManager instance;
    public static DebugManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject Prefab = Resources.Load<GameObject>("Prefabs/本机信息");
                GameObject temp = Instantiate(Prefab);
                temp.transform.position = Vector3.zero;
                temp.name = nameof(DebugManager);
                temp.transform.localScale = Vector3.one;
                instance = temp.GetComponent<DebugManager>();
            }
            return instance;
        }
    }

    public GameObject Prefabs;

    public CanvasGroup canvasGroup;

    private Dictionary<string, DebugData> DebugDic = new Dictionary<string, DebugData>();

    public Transform 父物体;

    private WaitForEndOfFrame waitForEnd = new WaitForEndOfFrame();

    [Header("位置大小")]
    private Rect rect = new Rect(10, 10, 500f, 300f);

    [Header("位置大小")]
    private Rect rect2 = new Rect(10, 70, 500f, 300f);

    [Header("颜色")]
    private Color textColor = Color.red;
    [Header("字体模式")]
    private FontStyle fontStyle = FontStyle.Normal;
    [Header("字体大小")]
    private int guiFontSize = 50;
    [Header("更新频率")]
    private float updateInterval = 0.5F;
    private GUIStyle style = new GUIStyle();

    private double lastInterval;
    private int frames = 0;
    private float fps;

    //private DisplayPanel displayPanel;

    private bool isShow = false;

    private void Awake()
    {      
        instance = this;
        DontDestroyOnLoad(gameObject);
        if(canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }
        canvasGroup.blocksRaycasts = false;
        //Hide();
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }

    private void Start()
    {
        
    }

    public void Open()
    {
        canvasGroup.alpha = 1;
        isShow = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        isShow = false;
    }

    public void ShowLogMsg()
    {
        if(isShow)
        {
            Hide();
        }
        else
        {
            Open();
        }
    }

    public bool GetIsShow()
    {
        return isShow;
    }

    private void OnGUI()
    {
        if (!isShow) 
            return;
        style.fontSize = guiFontSize;
        style.fontStyle = fontStyle;
        style.normal.textColor = textColor;
        GUI.Label(rect2, "fps:" + fps.ToString("f2"), this.style);
        GUI.Label(rect, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, this.style);
    }

    /// <summary>
    /// 设置/添加数据
    /// </summary>
    /// <param name="_Tiltle"></param>
    /// <param name="_data"></param>
    public void AddData(string _Tiltle, string _data)
    {
        _Tiltle = LanguageManger.Instance.GetValue(_Tiltle);
        StartCoroutine(_SetData(_Tiltle, _data));
    }

    private IEnumerator _SetData(string _Tiltle, string _data)
    {
        if (DebugDic.ContainsKey(_Tiltle))
        {
            DebugDic[_Tiltle].SetData(_data);
        }
        else
        {
            GameObject Obj = LeanPool.Spawn(Prefabs, 父物体);
            Obj.transform.localScale = Vector3.one;
            DebugData debugData = Obj.GetComponent<DebugData>();
            debugData.Init(_Tiltle, _data);
            DebugDic.Add(_Tiltle, debugData);
            yield return waitForEnd;
            LayoutRebuilder.ForceRebuildLayoutImmediate(父物体.GetComponent<RectTransform>());
        }
    }

    public void RemoveData(string _Tiltle)
    {
        if (DebugDic.ContainsKey(_Tiltle))
        {
            StartCoroutine(nameof(_RemoveData));
        }
    }

    private IEnumerator _RemoveData(string _Tiltle)
    {
        LeanPool.Despawn(DebugDic[_Tiltle].gameObject);
        DebugDic.Remove(_Tiltle);
        yield return waitForEnd;
        LayoutRebuilder.ForceRebuildLayoutImmediate(父物体.GetComponent<RectTransform>());
    }

    public static void Log(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
    }

    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogError(msg);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (canvasGroup.alpha == 1)
            {
                Hide();
            }
            else
            {
                Open();
            }
        }

        //if(Input.GetKeyDown(KeyCode.Y))
        //{
        //    if(Debuger.Instance)
        //    {
        //        if(Debuger.Instance.IsVisible)
        //        {
        //            Debuger.Instance.IsVisible = false;
        //        }
        //        else
        //        {
        //            Debuger.Instance.IsVisible = true;
        //        }
        //    }
        //    else
        //    {
        //        Debuger.Instance.Init();
        //    }
        //}

        if(!isShow)
            return;

        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow > lastInterval + updateInterval)
        {
            fps = (float)(frames / (timeNow - lastInterval));
            frames = 0;
            lastInterval = timeNow;

            AddData("屏幕分辨率", Display.main.systemWidth + "x" + Display.main.systemHeight);

            AddData("版本号", Application.version);
        }
    }
}

