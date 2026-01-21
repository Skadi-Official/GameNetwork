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
            const string msg = "Hello, Server!This is a test message from client";
            m_ClientSession.Send(XOREncrypt.XOR(Encoding.ASCII.GetBytes(msg)));
            ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
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
                Debug.Log($"服务器发送时间: {DateTime.FromBinary(LongResult):HH:mm:ss.fff}");
                Debug.Log($"客户端接收时间: {receiveTime:HH:mm:ss.fff}");
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