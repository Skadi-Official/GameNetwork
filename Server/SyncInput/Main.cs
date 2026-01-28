using System.Collections.Generic;
using Common;

namespace SyncInput
{
    internal class SyncInputApp : AppBase
    {
        // private MyUDPSession m_UDPServer;
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();
        
        private readonly InputMsg m_ClientInput = new InputMsg();
        // 保存所有的输入消息
        private readonly FrameClientInputsMsg m_FrameClientInputs = new FrameClientInputsMsg();
        // 处理输入消息的字典，用来给m_FrameClientInputs提供数据
        private readonly Dictionary<string, FrameClientInputsMsg.ClientInputData> m_CachedClientInputs = new Dictionary<string, FrameClientInputsMsg.ClientInputData>();
        
        private int m_FrameCount = 0;
        
        protected override void OnInit()
        {
            SetTargetFPS(15);
            
            m_UDPServer = new UDPSession();
            m_UDPServer.Init("127.0.0.1", 30000);
            m_UDPServer.Start();
        }
        
        protected override bool OnRun(float curTimestamp)
        {
            if (m_UDPServer == null || m_UDPServer.IsClosed())
            {
                return false;
            }
            m_RecvData.Clear();
            m_UDPServer.GetRecvedData(m_RecvData);
            
            //handle client input
            while (m_RecvData.Count != 0)
            {
                // 1.取出所有的网络包
                UDPSession.UDPPacket packet = m_RecvData.Dequeue();
                m_ClientInput.Unserialize(packet.Data);
                // 2.根据key查找字典中对应的用户
                if(!m_CachedClientInputs.TryGetValue(packet.ClientKey, out var clientInput))
                {
                    clientInput = new FrameClientInputsMsg.ClientInputData()
                    {
                        ClientKey = packet.ClientKey
                    };
                    m_CachedClientInputs[packet.ClientKey] = clientInput;
                }
                // 3.将对应用户的数据更新
                clientInput.X = m_ClientInput.X;
                clientInput.Y = m_ClientInput.Y;
            }
            
            //construct frame message and push it to client
            // 4.推进逻辑帧并在要发送的数据中记录当前逻辑帧
            m_FrameCount++;
            m_FrameClientInputs.FrameCount = m_FrameCount;
            foreach (var pair in m_CachedClientInputs)
            {
                // 5.将处理好的用户输入数据加入要发送的数据中
                m_FrameClientInputs.ClientInputs.Add(pair.Value);
            }
            m_UDPServer.BroadcastToClients(m_FrameClientInputs.Serialize());
            
            //clear last inputs
            // 6.发送完成后全部清空
            m_CachedClientInputs.Clear();
            m_FrameClientInputs.ClientInputs.Clear();
            return true;
        }

        protected override void OnCleanup()
        {
            if (m_UDPServer != null)
            {
                m_UDPServer.Close();
                m_UDPServer = null;
            }
        }
    }

    internal static class SyncInput
    {
        public static void Main(string[] args)
        {
            var app = new SyncInputApp();
            app.Run();
        }
    }
}