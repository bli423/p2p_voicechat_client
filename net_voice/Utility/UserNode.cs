using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace net_voice.Utility
{
    class UserNode
    {
        public ushort id;
        public IPEndPoint ep;

        public UserNode(ushort id, IPEndPoint ep)
        {
            this.id = id;
            this.ep = ep;
        }
    }
}
