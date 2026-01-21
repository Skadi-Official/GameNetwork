using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MyNetwork.Core;
using UnityEngine;

namespace MyNetwork.UDP
{
    public class MyUDPClient : MyNetworkSession
    {
        private UdpClient m_Socket; // UDP 网络通信类
        private Thread m_SendThread; // 发送消息线程
        private Thread m_ReceiveThread; // 接收消息线程

        private AutoResetEvent m_SendDataSignal; // 是否可以发送消息
        private Queue<byte[]> m_PendingSendData; // 存放需要被发送的消息
        private Queue<byte[]> m_ReceivedData; // 存放已经接收到的消息
        private CancellationTokenSource m_cts; // 控制是否取消线程的执行
        private string m_ClientKey; // 

        #region 重写的生命周期方法

        protected override bool OnInit()
        {
            try
            {
                m_cts = new CancellationTokenSource();
                // 绑定目标地址和目标端口
                m_Socket = new UdpClient();
                m_Socket.Connect(m_Addr);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"MyUDPSession初始化失败：{e}");
            }

            return false;
        }

        protected override void OnStart()
        {
            base.OnStart();
            // 记录socket绑定的地址和端口
            m_ClientKey = m_Socket.Client.LocalEndPoint.ToString();
            // 初始化数据
            m_SendDataSignal = new AutoResetEvent(false);
            m_PendingSendData = new Queue<byte[]>();
            m_ReceivedData = new Queue<byte[]>();
            m_SendThread = CreateThread(SendThreadFunc);
            m_ReceiveThread = CreateThread(ReceiveThreadFunc);
        }

        protected override void OnClose()
        {
            base.OnClose();
            m_cts.Cancel();
            if (m_ReceiveThread != null)
            {
                m_ReceiveThread.Join(500);
                m_ReceiveThread = null;
            }

            m_SendDataSignal.Set();
            if (m_SendThread != null)
            {
                m_SendThread.Join(500);
                m_SendThread = null;
            }

            if (m_Socket != null)
            {
                m_Socket.Close();
                m_Socket = null;
            }
        }

        #endregion

        #region 发送与接受数据方法

        public void Send(byte[] msg)
        {
            lock (m_PendingSendData)
            {
                m_PendingSendData.Enqueue(msg);
                m_SendDataSignal.Set();
            }
        }

        public bool GetReceivedData(Queue<byte[]> output)
        {
            lock (m_ReceivedData)
            {
                while (m_ReceivedData.Count != 0)
                {
                    var data = m_ReceivedData.Dequeue();
                    output.Enqueue(data);
                }
            }
            return output.Count > 0;
        }

        #endregion

        #region 接收和发送线程执行的方法

        // 提供给接收消息线程执行的方法
        private void ReceiveThreadFunc()
        {
            IPEndPoint remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (m_cts.Token.IsCancellationRequested == false)
            {
                if (IsClosed()) return;
                try
                {
                    var data = m_Socket.Receive(ref remoteIPEndPoint);
                    if (data.Length == 0) return;
                    lock (m_ReceivedData)
                    {
                        m_ReceivedData.Enqueue(data);
                    }
                }
                catch (Exception e)
                {
                    // 其他未知异常才需要记录
                    if (!m_cts.Token.IsCancellationRequested)
                    {
                        Debug.LogError("MyUDPSession ReceiveThreadFunc error:" + e);
                    }
                }
            }
        }

        // 提供给发送消息线程执行的方法
        private void SendThreadFunc()
        {
            var dataToSend = new Queue<byte[]>();
            while (m_cts.Token.IsCancellationRequested == false)
            {
                if (IsClosed()) return;
                m_SendDataSignal.WaitOne();
                // 在这里执行实际逻辑
                try
                {
                    lock (m_PendingSendData)
                    {
                        while (m_PendingSendData.Count != 0)
                        {
                            var packet = m_PendingSendData.Dequeue();
                            dataToSend.Enqueue(packet);
                        }
                    }

                    while (dataToSend.Count != 0)
                    {
                        var finalData = dataToSend.Dequeue();
                        if (finalData is { Length: > 0 })
                        {
                            m_Socket.Send(finalData, finalData.Length);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"MyUDPSession SendThreadFunc error:{e}");
                }
            }
        }

        #endregion
    }
}