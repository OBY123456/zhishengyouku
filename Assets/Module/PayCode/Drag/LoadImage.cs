using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LoadImage : MonoBehaviour
{
    public string _path;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        string path = Path.Combine(Application.streamingAssetsPath,_path);
        if(File.Exists(path))
        {
            image.sprite = FileHandle.GetSprite(path,0,0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
