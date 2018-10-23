using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using net_voice.Timer;
using net_voice.Utility;

namespace net_voice.Network
{
    class NetWorkIO
    {
        public delegate void ReceiveDataHandler(byte[] data, EndPoint remoteEP);

        public event ReceiveDataHandler m_ReceiveData = null;

        public const int BUFF_SIZE = 1024;

        private Socket socket;
        private IPEndPoint serverEp;
        private IPEndPoint endBind;

        private string server_ip = "222.118.226.197";
        private int server_port = 7989;


        private IPEndPoint localEndPoint;

        private Queue<SendDataNode> sendQue = new Queue<SendDataNode>();
        private bool run = true;

        public NetWorkIO()
        {
            serverEp = new IPEndPoint(IPAddress.Parse(server_ip), server_port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(serverEp);
            IPAddress iPAddress = ((IPEndPoint)socket.LocalEndPoint).Address;
            socket.Close();


            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            endBind = new IPEndPoint(IPAddress.Any, 0);
            socket.Bind(endBind);

           
            localEndPoint = new IPEndPoint(iPAddress, ((IPEndPoint)socket.LocalEndPoint).Port);


            Thread receiveThread = new Thread(new ThreadStart(ReceiveRun));
            Thread sendThread = new Thread(new ThreadStart(SendRun));

            receiveThread.Start();
            sendThread.Start();
        }
        public void close()
        {
            run = false;
            socket.Close();
            lock (sendQue)
            {
                Monitor.PulseAll(sendQue);
            }
          
        }


        public IPEndPoint getLocalEndPoint()
        {
            return localEndPoint;
        }

        private void ReceiveRun()
        {

            int len;
            byte[] buf = new byte[BUFF_SIZE];
           

            while (run)
            {
                byte[] result_buf;
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint ep = (EndPoint)sender;
                try
                {                    
                    len = socket.ReceiveFrom(buf, 0, BUFF_SIZE, SocketFlags.None, ref ep);
                    
                    result_buf = new byte[len];
                    Array.Copy(buf, result_buf, len);

                    IPEndPoint receive = new IPEndPoint(((IPEndPoint)ep).Address.Address, ((IPEndPoint)ep).Port);

                    ReceiveEvent(result_buf, receive);
                }
                catch (Exception e)
                {
                    if (!run) return;
                }                
            }         
        }

  
        
        private void ReceiveEvent(byte[] data, EndPoint remoteEP)
        {
            if (this.m_ReceiveData != null)
            {
                this.m_ReceiveData(data, remoteEP);
            }
        }



        private void SendRun()
        {
            SendDataNode send_buf;

            while (run)
            {
                lock (sendQue)
                {
                    while (sendQue.Count == 0)
                    {
                        Monitor.Wait(sendQue);
                        if (!run)
                        {
                            return;
                        }
                    }
                    send_buf = sendQue.Dequeue();
                }
                try
                {
                   
                    socket.SendTo(send_buf.data, 0, send_buf.data.Length, SocketFlags.None, send_buf.ep);
                    
                }
                catch (Exception e)
                {
                    break;
                }
            }           
        }

        public void SendDataToServer(byte[] data)
        {        
            lock (sendQue)
            {
                sendQue.Enqueue(new SendDataNode(data, serverEp));
                Monitor.Pulse(sendQue);
            }
        }
        public void SendData(byte[] data, IPEndPoint ep)
        {
            lock (sendQue)
            {
                sendQue.Enqueue(new SendDataNode(data, ep));
                Monitor.Pulse(sendQue);
            }
        }



        private class SendDataNode
        {
            public byte[] data;
            public EndPoint ep;

            public SendDataNode(byte[] data, EndPoint ep)
            {
                this.data = data;
                this.ep = ep;
            }
        }
    }
}
