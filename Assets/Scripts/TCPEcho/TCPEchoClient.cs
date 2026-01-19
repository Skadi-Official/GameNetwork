using System;
using Network.Core;
using Network.TCP;
using System.IO;
using System.Text;
using UnityEngine;
using Random = System.Random;

public class TCPEchoClient : MonoBehaviour
{
    private TCPClient m_ClientSession;

    private byte[] EchoGeneraterMessage()
    {
        const string msg = "Hello, Server!";
        ColoredLogger.Log(msg, ColoredLogger.LogColor.Green);
        
        return Encoding.ASCII.GetBytes(msg);
    }

    private byte[] EchoGenerateLengthHeadMessage()
    {
        string msg = string.Empty;
        var random = new Random();
        msg += random.Next(10000);
        msg += " Hello Server! This is a test message from client.";
        byte[] msgBody = Encoding.UTF8.GetBytes(msg);
        int bodyLen = msgBody.Length;
        // GetByte(int) 总是返回四个字节
        byte[] msgHead = BitConverter.GetBytes(bodyLen);
        byte[] packet = new byte[msgHead.Length + msgBody.Length];
        msgHead.CopyTo(packet, 0);
        msgBody.CopyTo(packet, msgHead.Length);
        
        StringBuilder sb = new StringBuilder();
        foreach (byte b in packet)
        {
            sb.Append(b.ToString("X2")).Append(" ");
        }
        ColoredLogger.Log(msg);
        ColoredLogger.Log(sb.ToString(), ColoredLogger.LogColor.Green);

        return packet;
    }

    // private void Start()
    // {
    //     byte[] data = EchoGenerateLengthHeadMessage();
    //
    //     int bodyLen = BitConverter.ToInt32(data, 0);   // 读长度头
    //     string body = Encoding.UTF8.GetString(data, 4, bodyLen);
    //
    //     Debug.Log("Body = " + body);
    // }

    private void OnGUI()
    {
        int margin = (int)(Mathf.Min(Screen.width, Screen.height) * 0.25f);
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Connect"))
        {
            if(m_ClientSession == null)
            {
                //m_ClientSession = new TCPClient(EchoGeneraterMessage);
                m_ClientSession = new TCPClient(EchoGenerateLengthHeadMessage);
                if (m_ClientSession.Init("127.0.0.1", 30000))
                {
                    ColoredLogger.Log("Init success");
                    m_ClientSession.Start();
                }
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
