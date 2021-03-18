using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsyncNet.Tcp.Remote;
using AsyncNet.Tcp.Server;
using DrawingCL;
using GenericProtocol;
using GenericProtocol.Implementation;
using NetCoreServer;
using WatsonTcp;

namespace TcpChatServer
{
    class ChatSession : TcpSession
    {
        public ChatSession(TcpServer server) : base(server) { }

        protected override void OnConnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} connected!");
            // Send invite message
            string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            SendAsync(message);
            Console.WriteLine(Server.ConnectedSessions);
            var session = Server.FindSession(Id);
            Console.WriteLine(session.Id.ToString() + session.IsConnected.ToString());


        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, 0, (int)size);
            Console.WriteLine("Incoming: " + message);

            // Multicast message to all connected sessions
            SendAsync(message);

            // If the buffer starts with '!' the disconnect the current session
            if (message == "!")
                this.Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    class ChatServer : TcpServer
    {
        public ChatServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession() { return new ChatSession(this); }


        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");

        }
    }
    class Room
    {
        public string RoomName{ get; set; }
        public List<IRemoteTcpPeer> Users { get; set; }
        public Room()
        {
            Users = new List<IRemoteTcpPeer>();
        }
    }
    class Program
    {
       static  List<Room> _rooms = new List<Room>();

        static async Task Main(string[] args)
        {
            var server = new AsyncNetTcpServer(30894);
            server.ServerStarted += (s, e) => {
                Console.WriteLine($"Server started on port: " +
                 $"{e.ServerPort}");
                
                };
            
            server.ConnectionEstablished += (s, e) =>
            {
                var peer = e.RemoteTcpPeer;
                Console.WriteLine($"New connection from [{peer.IPEndPoint}]");
                
                                
            };
            
            server.FrameArrived += async (s, e) => 
            {
                var obj = (CustObj) await ByteArrayToObject(e.FrameData);
                if (obj.IsJoining)
                {
                    if (_rooms.FirstOrDefault(x=>x.RoomName==obj.Id)==null)
                    {
                        Room room = new Room();
                        room.RoomName = obj.Id;
                        room.Users = new List<IRemoteTcpPeer>();
                        room.Users.Add(e.RemoteTcpPeer);
                        _rooms.Add(room);
                    }
                    else
                    {
                        var room = _rooms.Find(x => x.RoomName == obj.Id);
                        var index = _rooms.IndexOf(room);
                        if (!room.Users.Contains(e.RemoteTcpPeer))
                        {
                            room.Users.Add(e.RemoteTcpPeer);
                            if (index != -1)
                            {
                                _rooms.RemoveAt(index);
                                _rooms.Insert(index, room);
                            }
                        }
                    }
                    Console.WriteLine("Successfully created");
                }
                else
                {
                    var room = _rooms.Find(x => x.RoomName == obj.Id);
                    var users = room.Users.Where(x => x != e.RemoteTcpPeer);
                    await server.BroadcastAsync(e.FrameData, users);

                }


            };
            await server.StartAsync(CancellationToken.None);
            for (; ; )
            {

            }

        }
        public static async Task<Object> ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                await memStream.WriteAsync(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }
    }
}