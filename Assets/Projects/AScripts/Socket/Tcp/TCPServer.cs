using MTFrame.MTEvent;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TCPServer : MonoBehaviour
{
    public static TCPServer Instance;

    private TcpListener listener;
    private Thread listenerThread;
    private bool isRunning = false;

    private const string HEARTBEAT= "HEARTBEAT";


    private readonly static object o = new object();

    private Queue<string> MsgQueue = new Queue<string>();
    // 客户端列表
    private ConcurrentDictionary<TcpClient, ClientState> clients = new ConcurrentDictionary<TcpClient, ClientState>();

    // 心跳相关
    private const int HEARTBEAT_INTERVAL = 5000; // 5秒
    private Timer heartbeatTimer;
    private string ip;
    private int Port = 9512;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 启动心跳定时器
        ip = FMNetworkManager.instance.LocalIPAddress();
        heartbeatTimer = new Timer(Heartbeat, null, HEARTBEAT_INTERVAL, HEARTBEAT_INTERVAL);
        StartServer();
    }

    void OnDestroy()
    {
        StopServer();
    }

    private void OnApplicationQuit()
    {
        StopServer();
    }

    void StartServer()
    {
        listener = new TcpListener(IPAddress.Parse(ip), Port);
        listener.Start();
        isRunning = true;
        listenerThread = new Thread(ListenForClients);
        listenerThread.Start();

        Debug.Log("服务器已启动，监听端口 6000");
    }

    void StopServer()
    {
        isRunning = false;
        if (listener != null)
        {
            listener.Stop();
        }
        foreach (var client in clients.Keys)
        {
            client.Close();
        }
        clients.Clear();
        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Join();
        }
        Debug.Log("服务器已停止");
    }

    void ListenForClients()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                Debug.Log("客户端已连接: " + client.Client.RemoteEndPoint.ToString());
                ClientState state = new ClientState(client);
                if (clients.TryAdd(client, state))
                {
                    Thread clientThread = new Thread(HandleClientComm);
                    clientThread.Start(state);
                }
                else
                {
                    Debug.Log("无法添加客户端到列表");
                    client.Close();
                }
            }
            catch (SocketException ex)
            {
                Debug.LogError("监听客户端时出错: " + ex.Message);
            }
        }
    }

    void HandleClientComm(object obj)
    {
        ClientState state = obj as ClientState;
        TcpClient client = state.client;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[2048];
        int bytesRead;

        while (isRunning && client.Connected)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // 客户端断开连接
                    Debug.Log("客户端断开连接: " + client.Client.RemoteEndPoint.ToString());
                    break;
                }
                byte[] vs = new byte[bytesRead];
                Array.Copy(buffer,0,vs,0,bytesRead);
                // 处理接收到的数据
                state.buffer.AddRange(vs);
                ProcessData(state);
            }
            catch (Exception ex)
            {
                Debug.LogError("处理客户端通信时出错: " + ex.Message);
                break;
            }
        }

        // 清理
        clients.TryRemove(client, out _);
        //Debug.Log("客户端连接已关闭: " + client.Client.RemoteEndPoint.ToString());
        client.Close();   
    }

    void ProcessData(ClientState state)
    {
        // 分包组包逻辑：先读取消息长度，再读取消息内容
        while (state.buffer.Count >= 4)
        {
            // 读取消息长度（假设前4个字节为消息长度）
            byte[] lengthBytes = state.buffer.GetRange(0, 4).ToArray();
            int messageLength = BitConverter.ToInt32(lengthBytes, 0);
            if (state.buffer.Count < 4 + messageLength)
            {
                // 数据不足，等待更多数据
                break;
            }

            // 读取消息内容
            byte[] messageBytes = state.buffer.GetRange(4, messageLength).ToArray();
            string message = Encoding.UTF8.GetString(messageBytes);
            Debug.Log("收到客户端消息: " + message);

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
            state.buffer.RemoveRange(0, 4 + messageLength);
        }
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

    void Heartbeat(object state)
    {
        byte[] data = SerializeMessage(HEARTBEAT);
        foreach (var client in clients.Keys)
        {
            try
            {
                client.GetStream().Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError("发送心跳包给客户端时出错: " + ex.Message);
            }
        }
    }

    public void SendToAll(string msg)
    {
        try
        {
            byte[] data = SerializeMessage(msg);
            foreach (var client in clients.Keys)
            {
                try
                {
                    client.GetStream().Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError("发送心跳包给客户端时出错: " + ex.Message);
                }
            }
        }
        catch(Exception ex)
        {
            Debug.Log(ex.Message);
        }
        
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

    class ClientState
    {
        public TcpClient client;
        public List<byte> buffer = new List<byte>();

        public ClientState(TcpClient client)
        {
            this.client = client;
        }
    }
}
