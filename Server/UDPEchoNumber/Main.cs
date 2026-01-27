using System;
using System.Collections.Generic;
using System.Text;
using Common;

namespace UDPEchoNumber
{
    internal class UDPEchoNumberApp : AppBase
    {
        private UDPSession m_UDPServer;
        private readonly Queue<UDPSession.UDPPacket> m_RecvData = new Queue<UDPSession.UDPPacket>();
        protected override void OnInit()
        {
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
            if (!m_UDPServer.GetRecvedData(m_RecvData))
            {
                return true;
            }
            while (m_RecvData.Count != 0)
            {
                // var packet = m_RecvData.Dequeue();
                // var decryptedPacketData = XOREncrypt.XOR(packet.Data);
                // //var recvNumber = BitConverter.ToUInt32(packet.Data, 0);
                // var replyMsg = DateTime.Now.ToBinary().ToString();
                // var receivedMsg = Encoding.UTF8.GetString(decryptedPacketData);
                // //m_UDPServer.SendToClient(packet.ClientKey, BitConverter.GetBytes(recvNumber));
                // m_UDPServer.SendToClient(packet.ClientKey, Encoding.UTF8.GetBytes(receivedMsg));
                // Logger.LogInfo($"Msg From User({packet.ClientKey}): [{receivedMsg}]");
                // 假设这是服务器接收循环内部
                var packet = m_RecvData.Dequeue();

                // 1. 解密客户端发来的数据
                var decryptedData = XOREncrypt.XOR(packet.Data);

                // 2. 将内容转回字符串打印（可选）
                var receivedMsg = Encoding.UTF8.GetString(decryptedData);
                m_UDPServer.SendToClient(packet.ClientKey, Encoding.UTF8.GetBytes(receivedMsg));
                var LongResult = long.Parse(receivedMsg);
                Logger.LogInfo($"[Server] ({packet.ClientKey}): [{DateTime.FromBinary(LongResult):HH:mm:ss.fff}]");
            }
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
    
    internal static class UDPEchoNumber
    {
        public static void Main(string[] args)
        {
            var app = new UDPEchoNumberApp();
            app.Run();
        }
    }
    
    public static class XOREncrypt
    {
        private static readonly byte KEY = 0x59;
        public static byte[] XOR(byte[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                input[i] ^= KEY;
            }
            return input;
        }
    }
}