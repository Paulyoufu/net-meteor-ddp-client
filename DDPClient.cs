using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Net.DDP.Client
{
    public class DDPClient:IClient
    {
        private DDPConnector _connector;
        private int _uniqueId;
        private IList<int> _sub;

        public DDPClient()
        {
            this._connector = new DDPConnector(this);
            this._sub = new List<int>();

            _uniqueId = 1;
        }

        public void Connect(string url)
        {
            _connector.Connect(url);
        }

        public void Call(string methodName, params string[] args)
        {
            string message = string.Format("\"msg\": \"method\",\"method\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", methodName,this.CreateJSonArray(args), this.NextId().ToString());
            message = "{" + message+ "}";
            _connector.Send(message);
        }

        public int Subscribe(string subscribeTo, params string[] args)
        {
            string message = string.Format("\"msg\": \"sub\",\"name\": \"{0}\",\"params\": [{1}],\"id\": \"{2}\"", subscribeTo, this.CreateJSonArray(args), this.NextId().ToString());
            message = "{" + message + "}";
            _connector.Send(message);

            return this.GetCurrentRequestId();
        }

        public int Unsubscribe(int id, params string[] args)
        {
            string message = string.Format("\"msg\": \"unsub\", \"id\": \"{0}\"", id.ToString());
            message = "{" + message + "}";
            _connector.Send(message);

            return id;
        }

        private string CreateJSonArray(params string[] args)
        {
            if (args == null)
                return string.Empty;

            StringBuilder argumentBuilder = new StringBuilder();
            string delimiter=string.Empty;
            for (int i = 0; i < args.Length; i++)
            {
                argumentBuilder.Append(delimiter);
                argumentBuilder.Append(string.Format("\"{0}\"",args[i]));
                delimiter = ",";
            }

            return argumentBuilder.ToString();
        }
        private int NextId()
        {
            return _uniqueId++;
        }

        public int GetCurrentRequestId()
        {
            return _uniqueId;
        }

        public void DataReceived(JObject data)
        {
            if (data["msg"] != null)
            {

                switch (data["msg"].ToString())
                {
                    case "connected":
                        // stuff that occurs on connection such as subscription if desired
                        //this.Subscribe("requests");
                        break;

                    case "added":
                        Console.WriteLine("Document Added: " + data["id"]);
                     
                        break;
                    case "ready":
                        Console.WriteLine("Subscription Ready: " + data["subs"].ToString());
                        break;
                    case "removed":
                        Console.WriteLine("Document Removed: " + data["id"]);
                        break;

                    default:
                        Console.WriteLine(data.ToString());
                        break;
                }
            }
        }

        public void OnConnected()
        {
           Console.WriteLine("DDP Connected.");
           // this.Subscribe("requests");
        }

        public static bool IsNullOrEmpty(JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}

