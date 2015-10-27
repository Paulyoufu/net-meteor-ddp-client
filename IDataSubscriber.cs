using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Net.DDP.Client
{
    public interface IDataSubscriber
    {
        void DataReceived(JObject data);
    }
}
