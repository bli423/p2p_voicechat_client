using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace net_voice.Network
{
    class NetWorkIO
    {
        public delegate void ReceiveDataHandler(byte[] data, EndPoint remoteEP);
        public event ReceiveDataHandler m_ReceiveData = null;

        public const int BUFF_SIZE = 1024;

        public const string server_ip = "222.118.226.197";
        public const int server_port = 7989;

        private Socket socket;              
        private IPEndPoint serverEp;
        private IPEndPoint endBind;
        private IPEndPoint localEndPoint;

        private Queue<SendDataNode> sendQue = new Queue<SendDataNode>(); // 송신 패킷 대기큐
        private bool run = true;

        public NetWorkIO()
        {
            serverEp = new IPEndPoint(IPAddress.Parse(server_ip), server_port);

            //local ip주소 설정
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Connect(serverEp);
            IPAddress iPAddress = ((IPEndPoint)socket.LocalEndPoint).Address;
            socket.Close();

            //p2p 연결을 위한 소켓 바인드
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            endBind = new IPEndPoint(IPAddress.Any, 0);
            socket.Bind(endBind);

            //local end point 지정
            localEndPoint = new IPEndPoint(iPAddress, ((IPEndPoint)socket.LocalEndPoint).Port);

            //패킷 전송, 수신 스레드 
            Thread receiveThread = new Thread(new ThreadStart(ReceiveRun));
            Thread sendThread = new Thread(new ThreadStart(SendRun));

            receiveThread.Start();
            sendThread.Start();
        }

        /// <summary>
        /// NetWorkIO 사용 종료
        /// </summary>
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




        /// <summary>
        /// 패킷 수신 스레드 
        /// </summary>
        private void ReceiveRun()
        {
            int len;
            byte[] buf = new byte[BUFF_SIZE];           

            while (run)
            {              
                try
                {
                    byte[] result_buf;
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint ep = (EndPoint)sender;

                    len = socket.ReceiveFrom(buf, 0, BUFF_SIZE, SocketFlags.None, ref ep);
                    
                    result_buf = new byte[len];
                    Array.Copy(buf, result_buf, len);                   

                    ReceiveEvent(result_buf, ep);
                }
                catch (Exception e)
                {
                    if (!run) return;
                }                
            }         
        } 
        /// <summary>
        /// 패킷 수신 이벤트 
        /// </summary>
        /// <param name="data">패킷 데이터</param>
        /// <param name="remoteEP">송신한 end point</param>
        private void ReceiveEvent(byte[] data, EndPoint remoteEP)
        {
            if (this.m_ReceiveData != null)
            {
                this.m_ReceiveData(data, remoteEP);
            }
        }




        /// <summary>
        /// 패킷 송신 스레드 
        /// </summary>
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
        /// <summary>
        /// 서버로 패킷 송신
        /// </summary>
        /// <param name="data"></param>
        public void SendDataToServer(byte[] data)
        {        
            lock (sendQue)
            {
                sendQue.Enqueue(new SendDataNode(data, serverEp));
                Monitor.Pulse(sendQue);
            }
        }
        /// <summary>
        /// 지정된 end point로 패킷 송신
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ep">지정된 end point</param>
        public void SendData(byte[] data, IPEndPoint ep)
        {
            lock (sendQue)
            {
                sendQue.Enqueue(new SendDataNode(data, ep));
                Monitor.Pulse(sendQue);
            }
        }



        /// <summary>
        /// 송신 대기큐의 node 자료구조
        /// </summary>
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
