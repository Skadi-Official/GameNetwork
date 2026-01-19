using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common;

namespace TCPEcho
{
    internal class TCPEchoApp : AppBase
    {
        private TCPSession m_TCPServer;
        private string m_PendingMsg = string.Empty;
        private void TCPDataHandlerBySpecialText(byte[] data, int dataLen)
        {
            var msg = Encoding.ASCII.GetString(data, 0, dataLen);
            m_PendingMsg += msg;
            while (true)
            {
                int endOfMsgPos = m_PendingMsg.IndexOf("!", StringComparison.Ordinal);
                if(endOfMsgPos >= 0)
                {
                    string helloMsg = m_PendingMsg.Substring(0, endOfMsgPos + 1);
                    Logger.LogInfo("有处理黏包：Msg From User: [" + helloMsg + "]");
                    m_PendingMsg = m_PendingMsg.Substring(endOfMsgPos + 1);
                }
                else
                {
                    break;
                }
            }
        }

        private List<byte> m_Buffer = new List<byte>();
        // 根据长度头来解析数据
        private void TCPDataHandlerByLengthHead(byte[] data, int dataLen)
        {
            for (int i = 0; i < dataLen; i++) m_Buffer.Add(data[i]);
            
            while (true)
            {
                // 第一次触发这个方法时，一定要保证收到四个字节才开始解析数据
                if (m_Buffer.Count < 4)
                {
                    Logger.LogInfo("已收到数据，长度头不完整，等待更多数据");
                    return;
                }
                int msgLen = BitConverter.ToInt32(m_Buffer.ToArray(), 0);
                // 如果此时收到的数据还不完整则返回
                if(m_Buffer.Count < msgLen + 4)
                {
                    Logger.LogInfo("长度头解析完成，消息体不完整，等待更多数据");
                    return;
                }
                byte[] body = m_Buffer.GetRange(4, msgLen).ToArray();
                var finalMsg = Encoding.ASCII.GetString(body);
                Logger.LogInfo("解析到的文本: [" + finalMsg + "], " + "文本长度:" + msgLen + ", " + "buffer长度:" + m_Buffer.Count);
                // 应当只移除被消费掉的数据而不是清空整个缓存，因为会出现粘包的情况
                m_Buffer.RemoveRange(0, msgLen + 4);
            }
        }

        protected override void OnInit()
        {
            m_TCPServer = new TCPSession(TCPDataHandlerByLengthHead);
            m_TCPServer.Init("127.0.0.1", 30000);
            m_TCPServer.Start();
        }

        protected override void OnCleanup()
        {
            if(m_TCPServer != null)
            {
                m_TCPServer.Close();
                m_TCPServer = null;
            }
        }
    }
    
    internal static class TCPEcho
    {
        public static void Main(string[] args)
        {
            var app = new TCPEchoApp();
            app.Run();
        }
    }
}