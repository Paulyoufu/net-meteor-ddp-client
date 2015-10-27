using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Net.DDP.Client
{
    internal class DDPConnector
    {
        private WebSocket _socket;
        private string _url=string.Empty;
        private int _isWait = 0;
        private IClient _client;
        private string _session = string.Empty;
        
        public bool isConnected = false;

        public DDPConnector(IClient client)
        {
            this._client = client;

        }

        public void Connect(string url)
        {
            if (_url == string.Empty)
                _url = "ws://" + url + "/websocket";

            if (_socket == null)
            {
                Console.WriteLine("Creating WebSocket...");
                _socket = new WebSocket(_url);
       
                _socket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(socket_MessageReceived);
                _socket.Opened += new EventHandler(_socket_Opened);
                _socket.Closed += new EventHandler(_socket_Closed);
                _socket.Error += _socket_Error;
            }

            Console.WriteLine("Connecting...");

            //_socket.AutoSendPingInterval = 30;
            //_socket.EnableAutoSendPing = true;

            if (!isConnected && _socket.State != WebSocketState.Open)
            {
                _socket.Open();
                _isWait = 1;
                this.Wait();
            }
            Console.WriteLine("Connected.");
            this._client.OnConnected();
        }

        void _socket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            Console.WriteLine("Error: " + e.Exception.Message);
            
            Console.WriteLine(this._socket.State);

            Thread.Sleep(3000);

            if (this._socket.State == WebSocketState.Connecting)
            {
                //this._socket.Close();
                this.isConnected = false;
            }

            if (!this.isConnected)
            {

                Console.WriteLine("Reconnecting...");

                this._isWait = 1;
                this.Connect(this._url);
            }
            //_socket.Close();
            //throw new NotImplementedException();
        }

        public void Close()
        {
            _socket.Close();
        }

        public void Send(string message)
        {
            _socket.Send(message);
        }

        void _socket_Opened(object sender, EventArgs e)
        {
            this.isConnected = true;
            this.Send("{\"msg\":\"connect\", \"version\":\"1\", \"support\": [\"1\"]}");
            _isWait = 0;
        }


        void _socket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("Disconnected!");
            this.isConnected = false;
            Thread.Sleep(3000);
            Console.WriteLine("Reconnecting...");
            this._isWait = 1;
            this.Connect(this._url);
        }

        void socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {

            //Console.WriteLine(e.Message);

            JObject jObj = JObject.Parse(e.Message);

            if (jObj["msg"] != null)
            {
                switch (jObj["msg"].ToString())
                {
                    case "ping":
                        this.Send("{\"msg\": \"pong\"}");
                        Console.WriteLine("Ping? Pong!");
                        break;

                    default:
                        this._client.DataReceived(jObj);
                        break;
                }

            }

        }

        private void Wait()
        {
            // wait for reconnect on disconnect
            while (_isWait != 0)
            {
                System.Threading.Thread.Sleep(100);
            }
        }

    }
}
