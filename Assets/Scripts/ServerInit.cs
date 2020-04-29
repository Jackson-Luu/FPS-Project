using UnityEngine;

namespace Mirror
{
    public class ServerInit : MonoBehaviour
    {
        string[] args = System.Environment.GetCommandLineArgs();
        public int serverPort = 7777;
        public int webServerPort = 7778;

        // Use this for initialization
        void Start()
        {
            // Set port from command line
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-port"))
                {
                    serverPort = int.Parse(args[i + 1]);
                    GetComponent<TelepathyTransport>().port = (ushort)serverPort;
                    //GetComponent<Websocket.WebsocketTransport>().port = webServerPort;
                    GetComponent<NodeListServer.NodeListServerAdapter>().CurrentServerInfo.Port = serverPort;
                    NetworkManager.singleton.StartServer();
                }
            }
        }
    }
}