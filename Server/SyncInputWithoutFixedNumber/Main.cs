using Common;
namespace SyncInputWithoutFixedNumber
{
    public class SyncInputWithoutFixedNumberApp : AppBase
    {
        private UDPSession m_UDPServer;
        
        protected override void OnInit()
        {
            base.OnInit();
        }

        protected override bool OnRun(float curTimestamp)
        {
            return base.OnRun(curTimestamp);
        }
    }

    internal static class SyncInputWithoutFixedNumber
    {
        public static void Main(string[] args)
        {
            var app = new SyncInputWithoutFixedNumberApp();
            app.Run();
        }
    }
}

