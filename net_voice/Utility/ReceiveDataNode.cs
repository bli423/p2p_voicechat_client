using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace net_voice.Utility
{
    class ReceiveDataNode
    {
        public byte[] data;
        public EndPoint fromEndPoint;
        private EndPoint remoteEP;

        public ReceiveDataNode(byte[] data, EndPoint fromEndPoint)
        {
            this.data = data;
            this.fromEndPoint = fromEndPoint;
        }
    }
}
