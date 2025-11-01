using Stump.Core.Attributes;
using Stump.Server.BaseServer.Initialization;
using System;
using System.Collections.Generic;
using Fleck;
using Stump.Server.WorldServer.WebAPI.Handlers;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Newtonsoft.Json;

namespace Stump.Server.WorldServer.WebAPI
{
    public class WebServer
    {
        [Variable(definableByConfig: true, DefinableRunning = false)]
        public static bool ActiveWebSocket = false;

        [Variable(definableByConfig: true, DefinableRunning = false)]
        public static int WebAPIPort = 9000;

        [Variable(definableByConfig: true, DefinableRunning = true)]
        public static string WebAPIKey = string.Empty;

        private static string websocketUrl;
        private static WebSocketServer server;
        private static readonly Dictionary<string, IWebSocketConnection> Clients = new Dictionary<string, IWebSocketConnection>();

        [Initialization(InitializationPass.First)]
        public static void Initialize()
        {
            //try
            //{
            //    if (ActiveWebSocket)
            //    {
            //        websocketUrl = $"ws://{WorldServer.Host}:{WebAPIPort}/"; //Configure a URL aqui, após a inicialização de WorldServer.Host

            //        StartWebSocketServer(); //Inicializar o servidor WebSocket
            //    }
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception($"Cannot start WebAPI: {ex.ToString()}");
            //}
        }

        private static void StartWebSocketServer()
        {
            server = new WebSocketServer(websocketUrl);

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    var pathSegments = socket.ConnectionInfo.Path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string authKey = pathSegments[0];

                    if (string.IsNullOrWhiteSpace(authKey) || !authKey.Equals(WebAPIKey))
                    {
                        Console.WriteLine($"Unauthorized WebSocket connection from: {socket.ConnectionInfo.ClientIpAddress}");
                        socket.Close();
                        return;
                    }

                    Clients.Add(socket.ConnectionInfo.Id.ToString(), socket);
                    Console.WriteLine($"WebSocket opened: {socket.ConnectionInfo.ClientIpAddress}");
                };

                socket.OnClose = () =>
                {
                    Clients.Remove(socket.ConnectionInfo.Id.ToString());
                    Console.WriteLine($"WebSocket closed: {socket.ConnectionInfo.ClientIpAddress}");
                };

                socket.OnMessage = message =>
                {
                    //foreach (var client in Clients.Values)
                    //{
                    //    client.Send($"You sent: {message}");
                    //}

                    return;
                };

                socket.OnError = ex =>
                {
                    Console.WriteLine($"WebSocket error: {ex.Message}");
                };
            });
        }

        public static void SendoToDiscod(Character owner, int chatChannelId, string message)
        {
            WebChatHandler dataToSend = new WebChatHandler
            {
                OwnerName = owner.Name,
                OwnerRole = owner.Account.UserGroupId,
                ChatChannelId = chatChannelId,
                Message = message
            };

            string jsonData = JsonConvert.SerializeObject(dataToSend);

            foreach (var client in Clients.Values)
            {
                client.Send(jsonData);
            }
        }
    }
}