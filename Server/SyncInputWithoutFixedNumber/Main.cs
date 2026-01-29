using Common;
namespace SyncInputWithoutFixedNumber
{
    public class SyncInputWithoutFixedNumberApp : AppBase
    {
        private MyUDPSession m_UDPServer;
        private readonly Queue<MyUDPSession.UDPPacket> m_ReceivedPackets = new();
        // 专门用于反序列化的临时缓存
        private ProtoMsg.InputMsg m_InputMsgFromClient = new();
        // 按 ClientKey 存储本帧内收到的最新操作
        private Dictionary<string, ProtoMsg.FrameClientInputMsg.ClientInputData> m_CachedClientInputs = new();
        // 构造最终发送给所有人的广播包
        private readonly ProtoMsg.FrameClientInputMsg m_FrameClientInputs = new();
        // 帧数计数器
        private int m_FrameCount = 0;
        protected override void OnInit()
        {
            m_UDPServer = new MyUDPSession();
            m_UDPServer.Init("127.0.0.1", 30000);
            m_UDPServer.Start();
            base.OnInit();
        }

        protected override bool OnRun(float curTimestamp)
        {
            if(m_UDPServer == null || m_UDPServer.IsClosed()) return false;
            m_ReceivedPackets.Clear();
            m_UDPServer.GetReceivedData(m_ReceivedPackets);
            while (m_ReceivedPackets.Count > 0)
            {
                //1. 取出所有收到的消息，发过来的消息只有XY的浮点数
                var packet = m_ReceivedPackets.Dequeue();
                string clientKey = packet.ClientKey;
                m_InputMsgFromClient.Deserialize(packet.Data);
                if (!m_CachedClientInputs.TryGetValue(clientKey, out var clientInput))
                {
                    // 2.根据key查找字典中对应的用户 没有的话就在字典中新建
                    clientInput = new ProtoMsg.FrameClientInputMsg.ClientInputData
                    {
                        ClientKey = clientKey,
                    };
                    m_CachedClientInputs.Add(clientKey, clientInput);
                }
                // 3.将对应用户的数据更新
                clientInput.X = m_InputMsgFromClient.X;
                clientInput.Y = m_InputMsgFromClient.Y;
            }
            
            // 4.推进逻辑帧 并在要发送的数据中记录当前逻辑帧
            m_FrameCount++;
            m_FrameClientInputs.FrameCount = m_FrameCount;
            foreach (var pair in m_CachedClientInputs)
            {
                // 5.将处理好的用户输入数据加入要发送的数据中，这里面包含了目标地址，以及移动方向的XY分量
                m_FrameClientInputs.ClientInputs.Add(pair.Value);
            }
            m_UDPServer.BoardToAllClients(m_FrameClientInputs.Serialize());
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

    internal static class SyncInputWithoutFixedNumber
    {
        public static void Main(string[] args)
        {
            var app = new SyncInputWithoutFixedNumberApp();
            app.Run();
        }
    }
}

