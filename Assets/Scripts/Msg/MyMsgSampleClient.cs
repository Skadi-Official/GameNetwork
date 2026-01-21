using System;
using System.Collections;
using System.Collections.Generic;
using Network.Core;
using Network.UDP;
using UnityEngine;

public class MyMsgSampleClient : MonoBehaviour
{
    private UDPClient m_ClientSession = new UDPClient();

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
        if (GUI.Button(new Rect(margin, margin, Screen.width - 2 * margin, Screen.height - 2 * margin), "Send MoveToMsg"))
        {
            var msg = new MsgProto.AttackMsg();
            msg.PlayerID = 114;
            msg.TargetID = 514;
            msg.Power = 1919;
            var data = MsgProto.XOR(msg.Serialize());
            ColoredLogger.Log(BitConverter.ToString(data).Replace("-", " "), ColoredLogger.LogColor.Green);
            m_ClientSession.Send(data);
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
