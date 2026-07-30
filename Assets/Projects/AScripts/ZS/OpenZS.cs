using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace Setting.Code
{
    public class OpenZS : MonoBehaviour
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateMutex(IntPtr lpAttributes, int binitialOwner, string IpName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenMutex(IntPtr dwDesiredAccess, int bInheritHandle, string IpName);

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool ReleaseMutex(IntPtr mutex);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int WaitForSingleObject(IntPtr muetx, int dwMilliseconds);

        [DllImport("shell32.dll")]
        static extern IntPtr ShellExecute(
            IntPtr hwnd,
            StringBuilder lpOperation,
            StringBuilder lpFile,
            StringBuilder lpParameters,
            StringBuilder lpDirectory,
            ShowCommands nShowCmd);

        private readonly string zhishengInteractionExeName = "test.exe";
        public readonly string outputPath = @"C:\Program Files (x86)\ZhiSheng\zhishengjiaohu";

        private bool IsOpen;

        private int index = 1;

        public static event Action ZSHandle;


        private ShareMemoryHelper _helper; //创建共享
        private IntPtr _mutex;
        private bool isCreateStart = false;
        private string fileName = @"test.exe";

        string result;

        public static OpenZS Ins;

        // Start is called before the first frame update
        void Start()
        {
#if !UNITY_EDITOR
            CreatShare(); //创建共享
            WriteShareMemory("open");

            StartProcsee(outputPath); //打开交互exe  

            Ins = this;
#endif
        }

        // Update is called once per frame
        void Update()
        {
            if (IsOpen && index == 1)
            {
                //todo  触发打开交互事件
                OnZsHandle();
                index = 2;
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                WriteShareMemory("open");
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                WriteShareMemory("kill");
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                WriteShareMemory("kill");
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else

                Application.Quit();
#endif
            }
        }

        private Process foo;
        private bool isQuit = false;
        public async void StartProcsee(string ApplicationPath)
        {
            DirectoryInfo _directory = new DirectoryInfo(ApplicationPath);
            WriteShareMemory("");
            if (!_directory.Exists)  //交互安装路径不存在操作;
            {
                isQuit = true;
                Application.Quit();
            }
            else if (_directory.Exists)
            {
                byte[] _bytes = Encoding.Unicode.GetBytes(outputPath);
                string path = Encoding.Unicode.GetString(_bytes);
                //打开已经安装的交互软件
                IntPtr v = ShellExecute(IntPtr.Zero, new StringBuilder(@"open"), new StringBuilder(fileName), new StringBuilder(""), new StringBuilder(path),
                    ShowCommands.SW_NORMAL);

                if ((int)v > 31)   //判断返回值确认是否开启进程  返回值>32为成功开启 否则进程开启失败
                {
                    while (result != null && !result.Equals("ack"))
                    {
                        ReadShareMemory();
                    }
                    OnZsHandle();

                }
                else
                {
                    result = "找不到交互程序!" + v;
                    isQuit = true;
                    Application.Quit();
                }
            }
        }

        //创建共享内存以及锁
        void CreatShare()
        {
            _helper = new ShareMemoryHelper();
            _helper.CreateShareMemoryMap("ShareMemory", 1024);
            _mutex = CreateMutex(IntPtr.Zero, 0, "zs_interat");//此处不能修改
        }


        //延迟访问共享内存中的信息
        void ReadShareMemory()
        {

            var helper = new ShareMemoryHelper();
            helper.CreateShareMemoryMap("ShareMemory", 1024);
            WaitForSingleObject(_mutex, 2000);
            byte[] _bytes = new byte[1024];
            helper.Read(ref _bytes, 0, _bytes.Length);
            string str = Encoding.ASCII.GetString(_bytes);
            result = str.TrimEnd('\0');
            //Debug.Log("共享内容！" + str);
            ReleaseMutex(_mutex);
        }

        /*public async void OpenZhiShengInteraction()
        {
            var myprocess = Process.GetProcessesByName(zhishengInteractionExeName);
            if (myprocess.Length > 0)
            {
                return;
            }

            int returnCode = ShellExecute(
                                        IntPtr.Zero, new StringBuilder(@"open"),
                                        new StringBuilder(zhishengInteractionExeName),
                                        new StringBuilder(""),
                                        new StringBuilder(zhishengInteractionPath),
                                        1);
            byte[] bytes = new byte[3];

            if (returnCode <= 31)
            {
                return;
            }
            await Task.Delay(3000);
            string str = "";
            while (!str.Equals("ack"))
            {
                if (helper != null) helper.Read(ref bytes, 0, bytes.Length);
                str = System.Text.Encoding.UTF8.GetString(bytes);
            }
            OnZsHandle();
        }*/
        public void WriteMemory(string value)
        {
            value += '\0';
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            _helper.Write(bytes, 0, bytes.Length);
        }
        public void WriteShareMemory(string str)
        {
            WaitForSingleObject(_mutex, 200);
            ClearShareMemory();  //每次写入先清空共享内存
            WriteFile(str);
            ReleaseMutex(_mutex);
        }

        void WriteFile(string str)
        {
            string _str = str + '\0';
            byte[] bytes = Encoding.ASCII.GetBytes(_str);
            _helper.Write(bytes, 0, bytes.Length);
        }

        void ClearShareMemory()
        {
            var helper = new ShareMemoryHelper();
            helper.CreateShareMemoryMap("ShareMemory", 1024);
            byte[] _bytelen = new byte[1024];
            for (int i = 0; i < _bytelen.Length; i++)
            {
                _bytelen[i] = 0;
            }
            helper.Write(_bytelen, 0, _bytelen.Length);
        }

        private void OnApplicationQuit()
        {
#if !UNITY_EDITOR
            WriteShareMemory("kill");
#endif
        }

        IEnumerator wait(int time)
        {
            yield return new WaitForSeconds(time);
        }

        private static void OnZsHandle()
        {
            ZSHandle?.Invoke();
        }

    }
}
public enum ShowCommands : int
{
    SW_HIDE = 0,
    SW_SHOWNORMAL = 1,
    SW_NORMAL = 1,
    SW_SHOWMINIMIZED = 2,
    SW_SHOWMAXIMIZED = 3,
    SW_MAXIMIZE = 3,
    SW_SHOWNOACTIVATE = 4,
    SW_SHOW = 5,
    SW_MINIMIZE = 6,
    SW_SHOWMINNOACTIVE = 7,
    SW_SHOWNA = 8,
    SW_RESTORE = 9,
    SW_SHOWDEFAULT = 10,
    SW_FORCEMINIMIZE = 11,
    SW_MAX = 11
}