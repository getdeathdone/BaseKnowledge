using UnityEngine;

namespace StructuralPatterns.Proxy
{
    public interface IServer
    {
        void Connect();
    }

    public class RealServer : IServer
    {
        public void Connect()
        {
            Debug.Log("Connecting to real server...");
        }
    }

    public class ServerProxy : IServer
    {
        private RealServer _realServer;

        public void Connect()
        {
            if (_realServer == null) _realServer = new RealServer();
            Debug.Log("Proxy: Performing access control.");
            _realServer.Connect();
        }
    }
}