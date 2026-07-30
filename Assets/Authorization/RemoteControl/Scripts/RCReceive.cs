using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using RC;

namespace RC
{
    public class RCReceive : MonoBehaviour
    {
        private IPEndPoint ipEndPoint;
        private Socket socket;
        private Thread thread;
        private int bytesLength;        //长度

        private readonly static object o = new object();
        private Queue<byte> RC = new Queue<byte>();

        private int Port = 3332;
        private string IP = "127.0.0.1";

        void Start()
        {
            Init();
        }

        public void Init()
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(ipEndPoint);
            thread = new Thread(new ThreadStart(Receive));
            thread.IsBackground = true;
            thread.Start();
        }

        //接收消息函数
        private void Receive()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remote = (EndPoint)sender;
            while (true)
            {
                byte[] bytes = new byte[1024];
                try
                {
                    bytesLength = socket.ReceiveFrom(bytes, ref remote);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

                if (bytesLength > 0)
                {
                    RC.Enqueue(bytes[0]);
                }
            }
        }

        //关闭socket，关闭thread
        private void OnDestroy()
        {
            Close();
        }

        private void Update()
        {
            lock (o)
            {
                if (RC.Count > 0)
                {
                    BroadcastMsg(RC.Dequeue());
                }
            }
        }

        public void Close()
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
            if (thread != null)
            {
                thread.Interrupt();
                thread.Abort();
            }
        }

        private void BroadcastMsg(byte bytes)
        {
            RCDataControl.RCDataEvent(bytes);
        }
    }
}

