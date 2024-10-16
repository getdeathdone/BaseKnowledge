using UnityEngine;

namespace StructuralPatterns.Proxy
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            IServer server = new ServerProxy();
            server.Connect();
        }
    }
}