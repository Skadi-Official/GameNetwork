using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Network.Core;
using UnityEngine;

namespace MyNetwork.TCP
{
    public class MyTCPClient : NetworkSession
    {
        // EchoHandler 代表的是一个没有参数、返回值为 byte[]的方法
        public delegate byte[] EchoHandler();
        private readonly EchoHandler m_EchoHandler;
        
        private CancellationTokenSource m_cts;          // 提供给线程方法的结束标记
        private TcpClient m_Client;                     // tcp客户端，读写数据，每个连接对应一个实例
        private TcpListener m_Listener;                 // tcp服务器，只负责接待连接
        private Thread m_SendThread;                    // 发送消息的线程
        private Thread m_AcceptThread;                  // 处理连接的线程
        private Thread m_ReceiveThread;                 // 接收消息的线程
        public MyTCPClient(EchoHandler handler)
        {
            m_EchoHandler = handler;
        }

        #region 重写的生命周期

        protected override bool OnInit()
        {
            try
            {
                m_Client = new TcpClient();
                m_Client.Connect(m_Addr);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"MyTCPSession OnInit Error : {e}");
            }
            return false;
        }

        protected override void OnStart()
        {
            base.OnStart();
            m_cts = new CancellationTokenSource();
            m_SendThread = CreateThread(SendThreadFunc);
            m_AcceptThread = CreateThread(AcceptThreadFunc);
        }

        protected override void OnClose()
        {

            if (m_Client != null)
            {
                m_Client.Close();
                m_Client = null;
            }
            base.OnClose();
        }

        #endregion

        #region 线程所使用的方法

        private void SendThreadFunc()
        {
            NetworkStream networkStream = m_Client.GetStream();
            while (m_cts.IsCancellationRequested == false)
            {
                if (IsClosed() || m_Client.Connected == false) return;
                try
                {
                    byte[] data = null;
                    if (m_EchoHandler != null)
                    {
                        data = m_EchoHandler.Invoke();
                    }

                    if (data is { Length: > 0 })
                    {
                        networkStream.Write(data, 0 , data.Length);
                    }
                    Thread.Sleep(10);
                }
                catch (Exception e)
                {
                    Debug.LogError($"MyTCPSession SendThread Error : {e}");
                }
            }
        }

        private void AcceptThreadFunc()
        {
            
        }

        private void ReceiveThreadFunc()
        {
            
        }

        #endregion
    }
}
