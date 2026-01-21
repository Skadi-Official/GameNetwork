using System.Net;
using System.Threading;

namespace MyNetwork.Core
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class MyNetworkSession
    {
        private bool m_IsClosed;    // 网络连接是否已经被关闭
        protected IPEndPoint m_Addr;// IP + 端口号，标识网络上的一个通信端点

        /// <summary>
        /// 初始化方法，需要手动调用，调用完成后需要调用start
        /// </summary>
        /// <param name="addr">目标ip</param>
        /// <param name="port">目标端口</param>
        /// <returns>子类重写的初始化是否成功</returns>
        public bool Init(string addr, int port)
        {
            // 把字符串形式的 IP 地址，解析成一个 IPAddress 对象
            m_Addr = new IPEndPoint(IPAddress.Parse(addr), port);
            return OnInit();
        }

        /// <summary>
        /// 初始化完成后的启动方法，应当被手动调用
        /// </summary>
        public void Start()
        {
            OnStart();
        }

        /// <summary>
        /// 主动断开连接时需要手动调用的方法
        /// </summary>
        public void Close()
        {
            if (m_IsClosed) return;
            m_IsClosed = true;
            OnClose();
        }

        /// <summary>
        /// 创建一个线程，创建完成后立刻执行注册的方法
        /// </summary>
        /// <param name="threadFunc">无参无返回的启动函数</param>
        /// <returns></returns>
        protected Thread CreateThread(ThreadStart threadFunc)
        {
            var t = new Thread(threadFunc)
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal
            };
            t.Start();
            return t;
        }

        /// <summary>
        /// 连接是否关闭
        /// </summary>
        /// <returns>如果关闭，返回true</returns>
        protected bool IsClosed()
        {
            return m_IsClosed;
        }
        
        #region 提供给子类重写的生命周期

        protected virtual bool OnInit()
        {
            return false;
        }
        
        protected virtual void OnStart()
        {
        }
        protected virtual void OnClose()
        {
        }

        #endregion
    }
}