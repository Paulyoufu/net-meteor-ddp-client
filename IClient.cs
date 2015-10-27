using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Net.DDP.Client
{
    public interface IClient
    {
        void Connect(string url);
        void Call(string methodName, string[] args);
        int Subscribe(string methodName, string[] args);
        int GetCurrentRequestId();
        void DataReceived(JObject data);
        void OnConnected();

    }
}
