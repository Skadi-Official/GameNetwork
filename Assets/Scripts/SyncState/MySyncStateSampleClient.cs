using System;
using System.Collections;
using System.Collections.Generic;
using MyNetwork.UDP;
using SyncState;
using UnityEngine;

public class MySyncStateSampleClient : MonoBehaviour
{
    // 1.接受输入
    // 2. 将输入转换为消息发送给服务器
    // 3. 将服务器接收到消息后计算出的新位置数据应用
    // todo 4. 使用插值优化移动表现
    public Transform ClientObjectTF;
    public Transform ServerObjectTF;
    private class ClientObject
    {
        public Transform ObjectTF; 
        
        public Vector3 TargetPos = Vector3.zero; 
        public Quaternion TargetRot = Quaternion.identity;
        public Vector3 StartPos; 
        public Quaternion StartRot; 
        
        public float LastTimeStamp;                         // 该客户端物体最后一次接收到消息包的时间
        public float SimTime;                               // 当前插值过程持续的时间
        public float TotalTime;                             // 当前插值过程需持续的总时间
    }
    
    private MyUDPClient m_ClientSession = new MyUDPClient();
    private readonly Queue<byte[]> m_ClientReceivedData = new Queue<byte[]>();
    private readonly Dictionary<string, ClientObject> m_ClientObjects = new Dictionary<string, ClientObject>();
    void Start()
    {
        ClientObjectTF.gameObject.SetActive(false);
        ServerObjectTF.gameObject.SetActive(false);
        Application.targetFrameRate = 60;
        Debug.Log("开始初始化UDPClient");
        if (m_ClientSession.Init("127.0.0.1", 30000))
        {
            m_ClientSession.Start();
            Debug.Log("UDPClient初始化成功");
        }
        else
        {
            Debug.Log("UDPClient初始化失败");
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleInputAndSendInputMsg();
        ClientUpdate();
    }

    /// <summary>
    /// 客户端的更新逻辑
    /// </summary>
    private void ClientUpdate()
    {
        if (m_ClientSession.GetReceivedData(m_ClientReceivedData))
        {
            while (m_ClientReceivedData.Count != 0)
            {
                // 取出数据并且反序列化
                var data = m_ClientReceivedData.Dequeue();
                var msg = new StateMessage();
                msg.Deserialize(data);
                //Debug.Log($"收到消息clientKey: {msg.ClientKey},pos: {msg.TargetPos},time: {msg.TimeStamp}");
                if (!m_ClientObjects.TryGetValue(msg.ClientKey, out var obj))
                {
                    // 字典里面没有的话就新建
                    obj = new ClientObject();
                    m_ClientObjects.Add(msg.ClientKey, obj);
                    obj.ObjectTF = Instantiate(ClientObjectTF, ClientObjectTF.parent);
                    obj.ObjectTF.gameObject.SetActive(true);
                    obj.ObjectTF.position = msg.TargetPos;
                    obj.ObjectTF.rotation = Quaternion.LookRotation(msg.TargetRot);
                    obj.LastTimeStamp = msg.TimeStamp;
                    if (msg.ClientKey == m_ClientSession.ClientKey)
                    {
                        Debug.Log("当前消息地址与本机连接一致，创建服务器物体");
                        ServerObjectTF.gameObject.SetActive(true);
                    }
                }

                if (msg.TimeStamp > obj.LastTimeStamp)
                {
                    // 只要包比最后记录时间更晚就要更新数据
                    obj.StartPos = obj.ObjectTF.position;
                    obj.StartRot = obj.ObjectTF.rotation;
                    // 目标位置更新为传入的消息
                    obj.TargetPos = msg.TargetPos;
                    obj.TargetRot = Quaternion.LookRotation(msg.TargetRot);
                    
                    // 计算插值需要的数据
                    // 此时 obj.LastTimeStamp 是第一个包的时间，msg.TimeStamp 是第二个包的
                    // 加上 Mathf.Max(obj.TotalTime - obj.SimTime, 0f) 是为了补偿，如果上一个包没走完，会把剩余时间考虑进去 
                    obj.TotalTime = Mathf.Max(obj.TotalTime - obj.SimTime, 0f) + msg.TimeStamp - obj.LastTimeStamp;
                    Debug.Log($"{msg.TimeStamp} - {obj.LastTimeStamp} = {obj.TotalTime}");
                    // 如果totalTime过高则说明和服务器差距过大，不考虑补偿，直接追赶
                    if (obj.TotalTime > 1f)
                    {
                        obj.TotalTime = msg.TimeStamp - obj.LastTimeStamp;
                    }
                    // 计算完包之间的差值后更新最后时间
                    obj.SimTime = 0;
                    obj.LastTimeStamp = msg.TimeStamp;
                    
                    // 直接把服务器标记物移动到目标位置
                    if (msg.ClientKey == m_ClientSession.ClientKey)
                    {
                        ServerObjectTF.position = obj.TargetPos;
                        ServerObjectTF.rotation = obj.TargetRot;
                    }
                }
            }
        }

        // 在这里更新物体的计时器
        foreach (var pair in m_ClientObjects)
        {
            var clientObject = pair.Value;
            if (clientObject.TotalTime < Mathf.Epsilon) continue;
            clientObject.SimTime += Time.deltaTime;
            var ratio = Mathf.Clamp01(clientObject.SimTime / clientObject.TotalTime);
            //Debug.Log($"{clientObject.SimTime} / {clientObject.TotalTime} = {ratio}");
            clientObject.ObjectTF.position = Vector3.Lerp(clientObject.StartPos, clientObject.TargetPos, ratio);
            clientObject.ObjectTF.rotation = Quaternion.Slerp(clientObject.StartRot, clientObject.TargetRot, ratio);
        }
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInputAndSendInputMsg()
    {
        var inputMsg = new InputMessage();
        if (Input.GetKey(KeyCode.A))
        {
            inputMsg.MoveDir.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputMsg.MoveDir.x = 1;
        }
        if (Input.GetKey(KeyCode.W))
        {
            inputMsg.MoveDir.z = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputMsg.MoveDir.z = -1;
        }

        if (Camera.main != null)
        {
            inputMsg.MoveDir = Camera.main.transform.TransformDirection(inputMsg.MoveDir);
        }
        inputMsg.MoveDir.y = 0;
        inputMsg.MoveDir.Normalize();
        m_ClientSession.Send(inputMsg.Serialize());
    }
    
    private void OnApplicationQuit()
    {
        if (m_ClientSession != null)
        {
            m_ClientSession.Close();
            m_ClientSession = null;
        }
        else
        {
            Debug.LogError("m_ClientSession is null");
        }
    }
}
