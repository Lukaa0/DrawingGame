using AsyncNet.Tcp.Client;
using AsyncNet.Tcp.Remote;
using DrawingCL;
using GenericProtocol.Implementation;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WatsonTcp;
using TcpClient = NetCoreServer.TcpClient;

namespace TcpChatClient
{
    class ChatClient : TcpClient
    {

        public ChatClient(string address, int port) : base(address, port) { }

        public void DisconnectAndStop()
        {
            _stop = true;
            DisconnectAsync();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Chat TCP client connected a new session with Id {Id}");

        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP client disconnected a session with Id {Id}");

            // Wait for a while...
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
                ConnectAsync();
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, (int)size));
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP client caught an error with code {error}");
        }

        private bool _stop;
    }

    class Program
    {
        public static byte[] ObjectToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        static async Task Main(string[] args)
        {

            var client = new AsyncNetTcpClient("192.168.0.79", 30894);
            IRemoteTcpPeer peer = null;
            
            client.ConnectionEstablished += async (s, e) =>
            {
                peer = e.RemoteTcpPeer;
                Console.WriteLine($"Connected to [{peer.IPEndPoint}]");
                Console.WriteLine("Enter chatroom name");
                var test = new CustObj()
                {
                    Message = "idek",
                    Id = "idk",
                    IsJoining = false


                };
                var id = Console.ReadLine();
                var isJ = true;
                for (; ; )
                {

                    var obj = new CustObj()
                    {
                        Message = Console.ReadLine(),
                        Id = id,
                        IsJoining=isJ,
                        MotionEventBytes = ObjectToByteArray(test)


                    };
                    isJ = false;
                    var bytes = ObjectToByteArray(obj);
                    await peer.SendAsync(bytes);
                }
            };
            client.FrameArrived += (s, e) => {
                var obj = (CustObj)ByteArrayToObject(e.FrameData);
                Console.WriteLine($"Client received: " + $"{obj.Message}");
                
                };

            await client.StartAsync();



        }
        public static Object ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
   

}
