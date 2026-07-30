using MTFrame.MTEvent;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TCPClient : MonoBehaviour
{
    public static TCPClient Instance;

    private System.Net.Sockets.TcpClient client;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning = false;

    private const string HEARTBEAT = "HEARTBEAT";
    // 重连相关
    private const int RECONNECT_INTERVAL = 5000; // 5秒
    private Timer reconnectTimer;

    // 心跳相关
    private const int HEARTBEAT_INTERVAL = 5000; // 5秒
    private Timer heartbeatTimer;
    private bool isConnected = false;


    private readonly static object o = new object();

    private Queue<string> MsgQueue = new Queue<string>();

    private string ip;

    private int Port = 9512;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(nameof(Init));
    }

    private IEnumerator Init()
    {
        while (!FMClient.instance)
        {
            yield return null;
        }

        while (!FMClient.instance.FoundServer)
        {
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1);
        ip = FMClient.instance.ServerIP;
        ConnectToServer(ip);
    }

    void ConnectToServer(string _ip)
    {
        client = new System.Net.Sockets.TcpClient();
        ip = _ip;
        try
        {
            client.BeginConnect(ip, Port, ConnectCallback, null);
        }
        catch (Exception ex)
        {
            Debug.LogError("连接服务器时出错: " + ex.Message);
            StartReconnect();
        }
    }

    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            client.EndConnect(ar);
            if (client.Connected)
            {
                stream = client.GetStream();
                isConnected = true;
                Debug.Log("已连接到服务器");
                StartReceive();
                // 启动心跳定时器
                heartbeatTimer = new Timer(SendHeartbeat, null, HEARTBEAT_INTERVAL, HEARTBEAT_INTERVAL);
            }
            else
            {
                Debug.Log("连接失败");
                StartReconnect();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("连接回调时出错: " + ex.Message);
            StartReconnect();
        }
    }

    void StartReceive()
    {
        receiveThread = new Thread(ReceiveData);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    List<byte> Allbuffer = new List<byte>();
    async void ReceiveData()
    {
        byte[] buffer = new byte[2048];
        int bytesRead;
        while (isConnected && client.Connected)
        {
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // 连接断开
                    Debug.Log("服务器断开连接");
                    isConnected = false;
                    break;
                }
                List<byte> receivedData = new List<byte>(buffer).GetRange(0, bytesRead);
                Allbuffer.AddRange(receivedData);
                // 处理接收到的数据
                ProcessReceivedData(Allbuffer);
            }
            catch (Exception ex)
            {
                Debug.LogError("接收数据时出错: " + ex.Message);
                isConnected = false;
                break;
            }
        }

        // 处理断开连接
        Disconnect();
        StartReconnect();
    }

    void ProcessReceivedData(List<byte> buffer)
    {
        // 分包组包逻辑：先读取消息长度，再读取消息内容

        while (buffer.Count >= 4)
        {
            // 读取消息长度（假设前4个字节为消息长度）
            byte[] lengthBytes = buffer.GetRange(0, 4).ToArray();
            int messageLength = BitConverter.ToInt32(lengthBytes, 0);
            if (buffer.Count < 4 + messageLength)
            {
                // 数据不足，等待更多数据
                break;
            }

            // 读取消息内容
            byte[] messageBytes = buffer.GetRange(4, messageLength).ToArray();
            string message = Encoding.UTF8.GetString(messageBytes);
            Debug.Log("收到服务器消息: " + message);

            // 处理消息（例如，识别心跳包）
            if (message == "HEARTBEAT")
            {
                // 心跳包处理
                Debug.Log("收到心跳包");
            }
            else
            {
                lock (o)
                {
                    MsgQueue.Enqueue(message);
                }
            }

            // 移除已处理的数据
            buffer.RemoveRange(0, 4 + messageLength);
        }
    }

    void SendHeartbeat(object state)
    {

        Send(HEARTBEAT);
    }

    public void Send(string message)
    {
        if (client.Connected && isConnected)
        {
            byte[] data = SerializeMessage(message);
            try
            {
                stream.BeginWrite(data, 0, data.Length, SendCallback, null);
            }
            catch (Exception ex)
            {
                Debug.LogError("发送消息时出错: " + ex.Message);
            }
        }
    }

    void SendCallback(IAsyncResult ar)
    {
        try
        {
            stream.EndWrite(ar);
        }
        catch (Exception ex)
        {
            Debug.LogError("发送回调时出错: " + ex.Message);
        }
    }

    void Disconnect()
    {
        isConnected = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Join();
        }
        if (client != null && client.Connected)
        {
            client.Close();
        }
        if (heartbeatTimer != null)
        {
            heartbeatTimer.Dispose();
        }
        if (reconnectTimer != null)
        {
            reconnectTimer.Dispose();
        }
        Debug.Log("已断开与服务器的连接");
    }

    private void Update()
    {
        lock (o)
        {
            if (MsgQueue.Count > 0)
            {
                BroadcastMsg(MsgQueue.Dequeue());
            }
        }
    }

    private void BroadcastMsg(string Msg)
    {
        DataClass temp = JsonConvert.DeserializeObject<DataClass>(Msg);
        //Debug.Log("收到消息:" + Msg);
        EventParamete eventParamete = new EventParamete();
        eventParamete.AddParameter(temp.classJson);
        EventManager.TriggerEvent(GenericEventEnumType.Generic, temp.className, eventParamete);
    }

    void StartReconnect()
    {
        Debug.Log("尝试重新连接服务器...");
        reconnectTimer = new Timer(Reconnect, null, RECONNECT_INTERVAL, Timeout.Infinite);
    }

    void Reconnect(object state)
    {
        ConnectToServer(ip);
    }

    byte[] SerializeMessage(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
        return Combine(lengthBytes, messageBytes);
    }

    byte[] Combine(byte[] first, byte[] second)
    {
        byte[] ret = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, ret, 0, first.Length);
        Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
        return ret;
    }

    void OnDestroy()
    {
        Disconnect();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}