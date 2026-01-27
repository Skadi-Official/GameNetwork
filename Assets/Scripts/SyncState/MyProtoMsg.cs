using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SyncState
{
        /// <summary>
        /// 发送给服务器的移动方向类
        /// </summary>
        class InputMessage
        {
            public Vector3 MoveDir;

            public byte[] Serialize()
            {
                var ms = new MemoryStream();
                var writer = new BinaryWriter(ms);
                writer.Write(MoveDir.x);
                writer.Write(MoveDir.y);
                writer.Write(MoveDir.z);
                return ms.ToArray();
            }

            public void Deserialize(byte[] bytes)
            {
                var reader = new BinaryReader(new MemoryStream(bytes));
                MoveDir.x = reader.ReadSingle();
                MoveDir.y = reader.ReadSingle();
                MoveDir.z = reader.ReadSingle();
            }
        }


        class StateMessage
        {
            public string ClientKey;    // 用来记录客户端ip和端口
            public Vector3 TargetPos;
            public Vector3 TargetRot;
            public float TimeStamp;
            public byte[] Serialize()
            {
                var ms = new MemoryStream();
                var writer = new BinaryWriter(ms);
                writer.Write(ClientKey);
                writer.Write(TargetPos.x);
                writer.Write(TargetPos.y);
                writer.Write(TargetPos.z);
                writer.Write(TargetRot.x);
                writer.Write(TargetRot.y);
                writer.Write(TargetRot.z);
                writer.Write(TimeStamp);
                return ms.ToArray();
            }

            /// <summary>
            /// 反序列化得到的数据并写入StateMessage中
            /// </summary>
            /// <param name="bytes"></param>
            public void Deserialize(byte[] bytes)
            {
                var reader = new BinaryReader(new MemoryStream(bytes));
                ClientKey =  reader.ReadString();
                TargetPos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                TargetRot = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                TimeStamp = reader.ReadSingle(); 
            }
        }
}
    