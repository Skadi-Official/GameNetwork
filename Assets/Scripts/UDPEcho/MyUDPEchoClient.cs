using System;
using System.Collections.Generic;
using Network.UDP;
using System.Text;
using MyNetwork.UDP;
using Network.Core;
using UnityEngine;
using Utils;

public class MyUDPEchoClient : MonoBehaviour
{
    private MyUDPClient m_ClientSession = new MyUDPClient();

    private void Start()
    {
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
        }
    }

    private void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Say Hello"))
        {
            // const string msg = "Hello, Server!This is a test message from client";
            // m_ClientSession.Send(XOREncrypt.XOR(Encoding.ASCII.GetBytes(msg)));
            // ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
            // 1. 获取当前时间戳的 Binary 形式，转为字符串
            string timestampStr = DateTime.Now.ToBinary().ToString();
            var LongResult = long.Parse(timestampStr);
            // 2. 转为字节流
            byte[] rawData = Encoding.UTF8.GetBytes(timestampStr);
        
            // 3. 加密并发送
            m_ClientSession.Send(XOREncrypt.XOR(rawData));
        
            Debug.Log($"客户端发出:{DateTime.FromBinary(LongResult):HH:mm:ss.fff}");
        }
    }

    private Queue<byte[]> m_receivedData = new Queue<byte[]>();
    private void Update()
    {
        if (m_ClientSession.GetReceivedData(m_receivedData))
        {
            while (m_receivedData.Count != 0)
            {
                var data = m_receivedData.Dequeue();
                string result = Encoding.UTF8.GetString(data);
                var LongResult = long.Parse(result);
                // LongResult不能直接使用，要再转换一次
                //Debug.Log(LongResult);
                DateTime receiveTime = DateTime.Now;
                // 计算差值
                TimeSpan delay = receiveTime - DateTime.FromBinary(LongResult);
                Debug.Log($"客户端收到回传: {receiveTime:HH:mm:ss.fff}");
                Debug.Log($"延迟时间: {delay.TotalMilliseconds} 毫秒");
                
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
    }
}