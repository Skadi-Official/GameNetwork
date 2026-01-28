using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Common
{
    public class MyUDPSession : NetworkSession
    {
        private UdpClient m_Socket;
        private Thread m_ListenerThread;
        private Queue<UDPPacket> m_ReceivedData;
        private CancellationTokenSource m_cts;
        public struct UDPPacket
        {
            public string ClientKey;
            public byte[] Data;
        }
        private class UDPClientInfo
        {
            public IPEndPoint ClientIPEndPoint;
            public Thread SendThread;
            public AutoResetEvent SendDataSignal;
            public Queue<byte[]> PendingSendData;

            /// <summary>
            /// 将data放入待发送队列中并触发发送信号
            /// </summary>
            /// <param name="data">需要发送的数据</param>
            public void Send(byte[] data)
            {
                // 实际发送逻辑由每个udp端创建时绑定的发送线程执行
                lock (PendingSendData)
                {
                    // 这里要保存一份待发送数据的快照，这样如果发送外部修改时数据就不会出错
                    var copiedData = (byte[])data.Clone();
                    PendingSendData.Enqueue(copiedData);
                    SendDataSignal.Set();
                }
            }
        }
        private Dictionary<string, UDPClientInfo> m_Clients;

        #region 生命周期

        protected override bool OnInit()
        {
            try
            {
                m_Socket = new UdpClient(m_Addr);
                return true;
            }
            catch (Exception e)
            {
                Logger.LogError($"MyUDPSession Init Failed : {e.Message}");
            }
            return false;
        }

        protected override void OnStart()
        {
            base.OnStart();
            m_Clients =  new Dictionary<string, UDPClientInfo>();
            m_ReceivedData = new Queue<UDPPacket>();
            m_ListenerThread = CreateThread(ListenThreadFun);
            m_cts = new CancellationTokenSource();
        }

        protected override void OnClose()
        {
            base.OnClose();
            m_cts.Cancel();
            if (m_ListenerThread != null)
            {
                if (!m_ListenerThread.Join(1000)) // 最多等 1000 毫秒
                {
                    Logger.LogWarning("ListenerThread check-out timeout!");
                }
                m_ListenerThread = null;
            }

            lock (m_Clients)
            {
                foreach (var client in m_Clients.Values)
                {
                    client.SendDataSignal.Set();
                    if(client.SendThread == null) continue;
                    if (!client.SendThread.Join(1000))
                    {
                        Logger.LogWarning("client.SendThread check-out timeout!");
                    }
                    client.SendThread = null;
                }
                m_Clients.Clear();
            }

            if (m_Socket == null) return;
            m_Socket.Close();
            m_Socket = null;
        }

        #endregion

        #region 广播与单播

        /// <summary>
        /// 向当前所有被记录的地址发送数据
        /// </summary>
        /// <param name="data">需要发送的数据</param>
        public void BoardToAllClients(byte[] data)
        {
            lock (m_Clients)
            {
                foreach (var pair in m_Clients)
                {
                    var clientInfo = pair.Value;
                    clientInfo.Send(data);
                }
            }
        }

        /// <summary>
        /// 向指定地址发送数据
        /// </summary>
        /// <param name="clientKey">目标地址</param>
        /// <param name="data">需要发送的数据</param>
        public void SendToClient(string clientKey, byte[] data)
        {
            lock (m_Clients)
            {
                if (!m_Clients.TryGetValue(clientKey, out var clientInfo))
                {
                    Logger.LogWarning($"send data to {clientKey} failed, no such client in Dictionary");
                    return;
                }
                clientInfo.Send(data);
            }
        }

        #endregion

        #region 取出和存储接收到的数据

        public bool GetReceivedData(Queue<UDPPacket> output)
        {
            lock (m_ReceivedData)
            {
                while (m_ReceivedData.Count > 0)
                {
                    var data = m_ReceivedData.Dequeue();
                    output.Enqueue(data);
                }
            }
            return output.Count > 0;
        }

        public void AddDataToReceivedDataQueue(UDPPacket packet)
        {
            lock (m_ReceivedData)
            {
                m_ReceivedData.Enqueue(packet);
            }
        }

        #endregion
        
        #region 监听线程执行的方法

        private void ListenThreadFun()
        {
            var remoteIPEndPoint = new IPEndPoint(IPAddress.Any, 9999);
            Logger.LogInfo("ListenThreadFun started");
            while (!m_cts.IsCancellationRequested)
            {
                if (IsClosed()) return;
                try
                {
                    while (m_Socket != null && m_Socket.Available > 0)
                    {
                        if (IsClosed()) return;
                        var data = m_Socket.Receive(ref remoteIPEndPoint);
                        if(data.Length == 0) continue;
                        var clientKey = remoteIPEndPoint.ToString();
                        lock (m_Clients)
                        {
                            if (!m_Clients.TryGetValue(clientKey, out var clientInfo))
                            {
                                // 字典里面没有说明是第一次收到，需要初始化
                                clientInfo = new UDPClientInfo
                                {
                                    ClientIPEndPoint = new IPEndPoint(remoteIPEndPoint.Address, remoteIPEndPoint.Port),
                                    SendDataSignal = new AutoResetEvent(false),
                                    PendingSendData = new Queue<byte[]>()
                                };
                                // 这里如果不是手动封装的线程创建方法，需要手动start
                                clientInfo.SendThread = CreateThread(() => SendThreadFunc(clientInfo));
                                
                                m_Clients.Add(clientKey, clientInfo);
                                Logger.LogInfo($"client {clientKey} connected");
                            }
                        }
                        AddDataToReceivedDataQueue(new UDPPacket
                        {
                            ClientKey = clientKey,
                            Data = data
                        });
                    }
                    do
                    {
                        if (IsClosed()) return;
                        Thread.Sleep(1);
                    }
                    while (m_Socket == null || m_Socket.Available <= 0);
                }
                catch (Exception e)
                {
                    Logger.LogError("error in listen thread:" + e);
                    return;
                }
            }
        }

        #endregion


        #region 发送线程执行的方法

        private void SendThreadFunc(UDPClientInfo clientInfo)
        {
            Logger.LogInfo("A new SendThreadFunc started on " + clientInfo.ClientIPEndPoint);
            var dataToSend = new Queue<byte[]>();
            while (!m_cts.Token.IsCancellationRequested)
            {
                if (IsClosed()) return;
                clientInfo.SendDataSignal.WaitOne();
                try
                {
                    lock (clientInfo.PendingSendData)
                    {
                        // 先把所有数据取出来，减小锁的范围
                        while (clientInfo.PendingSendData.Count > 0)
                        {
                            var data = clientInfo.PendingSendData.Dequeue();
                            dataToSend.Enqueue(data);
                        }
                    }
                    // 实际执行发送逻辑
                    while (dataToSend.Count > 0)
                    {
                        var data = dataToSend.Dequeue();
                        if (data != null && data.Length > 0)
                        {
                            m_Socket.Send(data, data.Length, clientInfo.ClientIPEndPoint);   
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"MyUDPSession SendThreadFunc Failed : {e.Message}");
                    return;
                }
            }
        }

        #endregion
    }
}