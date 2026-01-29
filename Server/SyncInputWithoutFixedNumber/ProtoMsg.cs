namespace SyncInputWithoutFixedNumber;
// 定义用于收发的消息格式
public class ProtoMsg
{
    /// <summary>
    /// 客户端发送的消息格式，记录了输入
    /// </summary>
    internal class InputMsg
    {
        public float X;
        public float Y;

        public byte[] Serialize()
        {
            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(X);
            binaryWriter.Write(Y);
            return memoryStream.ToArray();
        }

        public void Deserialize(byte[] data)
        {
            var memoryStream = new MemoryStream(data);
            var binaryReader = new BinaryReader(memoryStream);
            X = binaryReader.ReadSingle();
            Y = binaryReader.ReadSingle();
        }
    }

    /// <summary>
    /// 一个时间帧内，所有玩家的操作集合
    /// </summary>
    internal class FrameClientInputMsg
    {
        /// <summary>
        /// 基础数据单元，某玩家在某个瞬间的移动方向分量
        /// </summary>
        public class ClientInputData
        {
            public string ClientKey;
            public float X;
            public float Y;
        }
        public List<ClientInputData> ClientInputs = new();
        public int FrameCount;

        public byte[] Serialize()
        {
            var memoryStream = new MemoryStream();
            var writer = new BinaryWriter(memoryStream);
            writer.Write(FrameCount);
            writer.Write(ClientInputs.Count);
            foreach (var clientInput in ClientInputs)
            {
                writer.Write(clientInput.ClientKey);
                writer.Write(clientInput.X);
                writer.Write(clientInput.Y);
            }
            return memoryStream.ToArray();
        }

        public void Deserialize(byte[] data)
        {
            var memoryStream = new MemoryStream(data);
            var reader = new BinaryReader(memoryStream);
            FrameCount = reader.ReadInt32();
            int clientInputsLength  = reader.ReadInt32();
            for (int i = 0; i < clientInputsLength; i++)
            {
                ClientInputs.Add(new ClientInputData
                {
                    ClientKey = reader.ReadString(),
                    X = reader.ReadSingle(),
                    Y = reader.ReadSingle()
                });
            }
        }
    }
}