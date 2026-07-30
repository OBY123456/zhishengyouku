using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

/// <summary>
/// 文件读取
/// </summary>
public static class FileHandle
{
    /// <summary>
    /// 保存文件，默认System.IO.FileMode.Create, System.IO.FileAccess.Write
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="FilePath"></param>
    public static void WriteAllText(string FilePath,string msg)
    {
        var fss = new System.IO.FileStream(FilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
#if UNITY_EDITOR
        var sws = new System.IO.StreamWriter(fss);
#else
        var sws = new System.IO.StreamWriter(fss,Encoding.UTF8);
#endif
        sws.Write(msg);
        sws.Flush();
        fss.Flush(true);
        sws.Close();
        fss.Close();
    }

    /// <summary>
    /// 判断文件夹是否存在
    /// </summary>
    /// <param name="Folderpath"></param>
    public static void IsExisFolder(string Folderpath)
    {
        if (!Directory.Exists(Folderpath))
            Directory.CreateDirectory(Folderpath);
    }

    /// <summary>
    ///  读取文件内容
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public static string ReadAllText(string filepath)
    {
        FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        StreamReader reader = new StreamReader(fs, Encoding.UTF8);
        try
        {
            return reader.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
        finally
        {
            fs.Close();
            reader.Close();
        }
    }


    /// <summary>
    /// //获取unity根目录下的图片文件夹下的所有文件的路径 路径+ 名称全部存储在字符串数组中
    /// </summary>
    /// <returns></returns>
    public static List<string> GetImagePath(string Path)
    {
        List<string> filePaths = new List<string>();
        string imgtype = "*.JPG|*.PNG";
        string[] ImageType = imgtype.Split('|');
        for (int i = 0; i < ImageType.Length; i++)
        {
            
            string[] dirs = Directory.GetFiles(Path, ImageType[i]);
            for (int j = 0; j < dirs.Length; j++)
            {
                filePaths.Add(dirs[j]);
            }
            Debug.Log(ImageType[i] + ":一共读取到" + dirs.Length + "张图片");
        }
        return filePaths;
    }

    /// <summary>
    /// 返回Sprite图片
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="width">图片宽</param>
    /// <param name="height">图片高</param>
    /// <returns></returns>
    public static Sprite GetSprite(string path,int width,int height)
    {
        //创建文件读取流
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        //创建Texture
        Texture2D texture2D = new Texture2D(width, height);
        texture2D.LoadImage(bytes);
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);
    }

    /// <summary>
    /// 返回Texture2D图片
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static Texture2D GetTexture2D(string path)
    {
        //创建文件读取流
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取流
        fileStream.Close();
        fileStream.Dispose();
        //创建Texture
        Texture2D texture2D = new Texture2D(0, 0);
        texture2D.LoadImage(bytes);
        texture2D.Apply();
        return texture2D;
    }

    /// <summary>
    /// 获取全部视频路径
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static List<string> GetVideoPath(string Path)
    {
        List<string> filePaths = new List<string>();
        string[] dirs = Directory.GetFiles(Path, "*.mp4");
        for (int j = 0; j < dirs.Length; j++)
        {
            filePaths.Add(dirs[j]);
        }
        //Debug.Log(".mp4" + ":一共读取到" + dirs.Length + "个视频");
        return filePaths;
    }

    /// <summary>
    /// 返回全部文件夹路径和名称的字典
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetFolderPath(string Path)
    {
        Dictionary<string, string> filePaths = new Dictionary<string, string>();
        string[] dirs = Directory.GetDirectories(Path, "*");
        for (int j = 0; j < dirs.Length; j++)
        {
            string st1 = dirs[j];
            int temp = st1.IndexOf(@"\");
            string st2 = st1.Substring(temp + 1);
            filePaths.Add(st2, st1);
        }
        //Debug.Log("Folder" + ":一共读取到" + dirs.Length + "个子文件夹");
        return filePaths;
    }

    /// <summary>
    /// 选择文件,返回路径
    /// </summary>
    /// <param name="_FileType">文件类型，格式如："mp4文件(*.mp4)\0*.mp4",多类型则格式如："图片文件(*.jpg;*.png)\0*.jpg;*.png"</param>
    /// <returns></returns>
    public static string GetFile(string _FileType)
    {
        OpenFileName openFileName = new OpenFileName();
        openFileName.structSize = Marshal.SizeOf(openFileName);
        openFileName.filter = _FileType + "\0\0";
        openFileName.file = new string(new char[256]);
        openFileName.maxFile = openFileName.file.Length;
        openFileName.fileTitle = new string(new char[64]);
        openFileName.maxFileTitle = openFileName.fileTitle.Length;
        openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\'); //默认路径
        openFileName.title = "窗口标题";
        openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetSaveFileName(openFileName))
        {
            if(!string.IsNullOrEmpty(openFileName.file))
            {
                return openFileName.file;
            }
        }
        
        return string.Empty;
    }

    /// <summary>
    /// 获取指定文件夹下全部的图片
    /// </summary>
    /// <returns></returns>
    public static List<string> GetPictureList()
    {
        string path = WindowsExplorer.GetPathFromWindowsExplorer();  
        if (!string.IsNullOrEmpty(path))
        {
            List<string> vs = GetImagePath(path);
            if(vs != null && vs.Count > 0)
            {
                return vs;
            }
        }

        return null;
    }

    /// <summary>
    /// 时间换算
    /// </summary>
    /// <returns>The time.</returns>
    /// <param name="inputTime">输入的时间(秒)</param>
    public static string UpdateTime(float inputTime)
    {
        int day = (int)(inputTime / (60 * 60 * 24));  
        int hour = (int)(inputTime / (60 * 60) % 24);  
        int minute = (int)(inputTime / 60 % 60);  
        int second = (int)(inputTime % 60);  
        string dayTemp = (day < 10) ? "0" + day.ToString() : day.ToString();  
        string hourTemp = (hour < 10) ? "0" + hour.ToString() : hour.ToString();  
        string minuteTemp = (minute < 10) ? "0" + minute.ToString() : minute.ToString();  
        string secondTemp = (second < 10) ? "0" + second.ToString() : second.ToString();  
        string result = dayTemp + ":" + hourTemp + ":" + minuteTemp + ":" + secondTemp;  
        return result;  
    }

    public static string SerializeObject(object o, bool IsNewtonsoft = false)
    {
        if (IsNewtonsoft)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(o);
        }
        else
        {
            return JsonUtility.ToJson(o);
        }
    }

    public static T DeserializeObject<T>(string data, bool IsNewtonsoft = false)
    {
        if (IsNewtonsoft)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data);
        }
        else
        {
            return JsonUtility.FromJson<T>(data);
        }
    }
}