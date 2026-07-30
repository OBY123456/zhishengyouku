using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Setting;
using UnityEditor;

public class ImageData
{
    public float ScaleX = 1;

    public float ScaleY = 1;

    public float PosX;

    public float PosY;

    public ImageData(){ }

    public ImageData(float _ScaleX,float _ScaleY,float _PosX,float _PosY)
    {
        ScaleX = _ScaleX;
        ScaleY = _ScaleY;
        PosX = _PosX;
        PosY = _PosY;
    }

    public Vector3 GetPostion()
    {
        return new Vector3(PosX,PosY);
    }

    public Vector3 GetScale()
    {
        return new Vector3(ScaleX,ScaleY);
    }
}

public class ImageListData
{
    public List<ImageData> imageDatas = new List<ImageData>();
}

public class ImageConfig : MonoBehaviour
{
    public static ImageConfig Instance;

    private readonly static string _path = "ImageConfig.txt";

    public ImageListData imageListData = new ImageListData();

    public List<ImageControl> imageControls = new List<ImageControl>();

    private void Awake()
    {
        Instance = this;
        string path = Path.Combine(Application.streamingAssetsPath, _path);
        if (File.Exists(path))
        {
            string Temp = File.ReadAllText(path);
            Debug.Log("UI位置信息:" + Temp);
            if (!string.IsNullOrEmpty(Temp))
            {
                imageListData = JsonConvert.DeserializeObject<ImageListData>(Temp);
                for (int i = 0; i < imageListData.imageDatas.Count; i++)
                {
                    imageControls[i].Init(imageListData.imageDatas[i]);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Save()
    {
        imageListData.imageDatas.Clear();
        for (int i = 0; i < imageControls.Count; i++)
        {
            imageListData.imageDatas.Add(imageControls[i].Save());
        }

        string path = Application.streamingAssetsPath + "/" + _path;
        string t = FileHandle.SerializeObject(imageListData, true);
        Debug.Log("ttt==" + t);
        File.WriteAllText(path, t);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            if(imageControls[0].IsShow())
            {
                for (int i = 0; i < imageControls.Count; i++)
                {
                    imageControls[i].HideDrag();
                }
            }
            else
            {
                for (int i = 0; i < imageControls.Count; i++)
                {
                    imageControls[i].ShowDrag();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.J))
        {
            Save();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            ReStartApp.ReStart();
#endif    
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
