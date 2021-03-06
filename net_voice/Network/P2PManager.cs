﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using net_voice.Timer;
using net_voice.Utility;
using static net_voice.Timer.MyTimer;

namespace net_voice.Network
{
    class P2PManager
    {
        public delegate void ReceiveVoiceHandler(int id, byte[] data);
        public delegate void ReceiveTextHandler(int id, byte[] data);
       
        public event ReceiveVoiceHandler m_ReceiveVoice = null;
        public event ReceiveTextHandler m_ReceiveText = null;

       /// <summary>
       /// 패킷 type 
       /// </summary>
        public static ushort ALIVE_SERVER = 2;
        public static ushort CONNECT_SERVER = 3;
        public static ushort CONNECT_SERVER_YES = 4;
        public static ushort CONNECT_SERVER_NO = 5;

        public static ushort ISALIVE_PEAR = 6;
        public static ushort ISALIVE_PEAR_YES = 7;
        public static ushort ISALIVE_PEAR_NO = 8;

        public static ushort REACEIVE_PEAR = 9;
        public static ushort ALIVE_PEAR = 10;
        public static ushort DEATH_PEAR = 11;

        public static ushort DATA_VOICE = 12;
        public static ushort DATA_TEXT = 13;


        /// <summary>
        /// status
        /// </summary>
        public static int DISCONNECTED_SERVER = 100;
        public static int CONNECTED_SERVER = 101;

        private int status = DISCONNECTED_SERVER;
        
        // id 범위
        private const int ID_MAX = 65000;        


        /// <summary>
        /// 패킷 struct
        /// </summary>
        public struct TYPE_HEADER
        {
            public ushort type;
            public ushort id;
        }
        public struct CONNECT_SERVER_HEADER
        {
            public ushort type;
            public ushort id;
            public uint private_ip;
            public int private_port;
        }
        public struct PEAR_INFO_HEADER
        {
            public ushort type;
            public ushort id;
            public uint ip;
            public int port;
        }
        public struct DATA_HEADER
        {
            public ushort type;
            public ushort id;
        }


        private MyTimer timer;
        private ServerConnect serverConnectTimer;
        private ServerAlive serverAliveTimer;

        private Random r = new Random();
        private ushort userid;
        private List<UserNode> user_list;

        private UIHandler uiHandler;

        private NetWorkIO netWorkIO;
        private Thread receiveThread;
        private Queue<ReceiveDataNode> receiveQue;     
 
        private bool run = true;      



        public P2PManager(UIHandler uiHandler)
        {
            userid = (ushort)r.Next(0, ID_MAX);
            this.uiHandler = uiHandler;

            user_list = new List<UserNode>();
            receiveQue = new Queue<ReceiveDataNode>();
            receiveThread = new Thread(receiveDRun);

            netWorkIO = new NetWorkIO();
            netWorkIO.m_ReceiveData += new NetWorkIO.ReceiveDataHandler(receiveDataToJob);

            serverConnectTimer = new ServerConnect();
            serverAliveTimer = new ServerAlive();

            timer = new MyTimer();
            timer.m_TimerEvent += new MyTimerHandler(timerEvent);
        }


        public void close()
        {
            netWorkIO.close();
            timer.close();
            run = false;
            lock (receiveQue)
            {
                Monitor.PulseAll(receiveQue);
            }
        }

        public void Run()
        {                        
            timer.AddTimer(serverConnectTimer, 5);
            receiveThread.Start();
            ConnectServer();
        }


        /// <summary>
        /// 수신받은 패킷 처리 스레드
        /// </summary>
        void receiveDRun()
        {
            while (run)
            {
                ReceiveDataNode receiveData;
                lock (receiveQue)
                {
                    while (receiveQue.Count == 0)
                    {
                        Monitor.Wait(receiveQue);
                        if (!run) return;
                    }
                    receiveData = receiveQue.Dequeue();
                }
                taskOperate(receiveData);
            }
        }

        /// <summary>
        /// networkIO의 패킷 수신 이벤트
        /// 이벤트 발생기 수신 대기큐에 저장
        /// </summary>
        /// <param name="data"></param>
        /// <param name="remoteEP"></param>
        void receiveDataToJob(byte[] data, EndPoint remoteEP)
        {
            lock (receiveQue)
            {
                receiveQue.Enqueue(new ReceiveDataNode(data, remoteEP));
                Monitor.Pulse(receiveQue);
            }
        }


        /// <summary>
        /// 패킷 처리
        /// </summary>
        /// <param name="receiveData"></param>
        void taskOperate(ReceiveDataNode receiveData)
        {
            TYPE_HEADER type = ByteToStructure<TYPE_HEADER>(receiveData.data,0);

            // 서버에서 연결 승인
            if (type.type == CONNECT_SERVER_YES)
            {
                timer.TimerSet(serverConnectTimer);
                timer.AddTimer(serverAliveTimer, 2);
                status = CONNECTED_SERVER;
                uiHandler.UIAdd(UIHandler.COVER_UNVISIBLE, null);
            }
            //서버에서 연결 거절
            else if (type.type == CONNECT_SERVER_NO)
            {
                if (status == DISCONNECTED_SERVER)
                {
                    userid = (ushort)r.Next(0, ID_MAX);
                }
            }
            //ALIVE_SERVER 패킷 
            else if (type.type == ALIVE_SERVER)
            {
                status = CONNECTED_SERVER;
                timer.TimerSet(serverConnectTimer);
            }
            //다른 pear 연결되어있음
            else if (type.type == ISALIVE_PEAR_YES)
            {
                // pear가 서버에 연결되어있지만
                // p2p연결이 불가능(멀티 NAT의 경우 NAT가 hairpin기능을 지원하지 않을 경우)
                // 서버를 경유하는 방법을 사용해야함
            }
            //다른 pear 연결 되어있지 않음
            else if (type.type == ISALIVE_PEAR_NO)
            {
                deleteUser(type.id);
            }
            // 다른 pear정보 수신
            else if (type.type == REACEIVE_PEAR)
            {
                PEAR_INFO_HEADER pear_info = ByteToStructure<PEAR_INFO_HEADER>(receiveData.data, 0);

                IPEndPoint ep = new IPEndPoint(pear_info.ip, pear_info.port);
                UserNode user = new UserNode(pear_info.id, ep);

                addUser(user);
                timer.AddTimer(user, 1);

                sendPearAlive(user.ep);
            }
            // 다른 pear가 주기적으로 보내는 alive 패킷
            else if (type.type == ALIVE_PEAR)
            {
                UserNode user = findUser(type.id);

                if (user != null)
                {
                    timer.TimerSet(user);
                }
            }
            // 다른 pear연결 종료
            else if (type.type == DEATH_PEAR)
            {
                deleteUser(type.id);
            }
            // 다른 pear의 음성 정보
            else if (type.type == DATA_VOICE)
            {
                UserNode user = findUser(type.id);

                if (user != null)
                {
                    timer.TimerSet(user);
                }
                else
                {
                    Console.WriteLine("no user");
                }

                DATA_HEADER packet_data = ByteToStructure<DATA_HEADER>(receiveData.data, 0);
                int header_size = Marshal.SizeOf(packet_data);
                int data_size = receiveData.data.Length - Marshal.SizeOf(packet_data);

                byte[] data = new byte[data_size];

                Array.Copy(receiveData.data, header_size, data, 0, data_size);

                int index = findUserIndex(packet_data.id);
                if (!(index < 0))
                {
                    ReceiveVoice(index, data);
                }

            }
            // 다른 pear의 문자 정보
            else if (type.type == DATA_TEXT)
            {
                UserNode user = findUser(type.id);

                if (user != null)
                {
                    timer.TimerSet(user);
                }
                else
                {
                    Console.WriteLine("no user");
                }

                DATA_HEADER packet_data = ByteToStructure<DATA_HEADER>(receiveData.data, 0);
                int header_size = Marshal.SizeOf(packet_data);
                int data_size = receiveData.data.Length - Marshal.SizeOf(packet_data);

                byte[] data = new byte[data_size];

                Array.Copy(receiveData.data, header_size, data, 0, data_size);

                ReceiveText(packet_data.id, data);
            }

        }



        /// <summary>
        /// timer event 
        /// 일정 시간 주기로 패킷을 전송하거나 검사해야하는 것들
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="over_time"></param>
        private void timerEvent(object obj, int over_time)
        {
            if (obj.GetType().Equals(typeof(ServerConnect)))
            {
                if (over_time < -10)
                {
                    ConnectServer();
                    status = DISCONNECTED_SERVER;
                    timer.deleteTimer(serverAliveTimer);
                    uiHandler.UIAdd(UIHandler.COVER_VISIBLE, null);
                    clearUserList();
                }
            }
            else if (obj.GetType().Equals(typeof(ServerAlive)))
            {
                timer.TimerSet(serverAliveTimer);
                SendAliveServer();
            }
            else if (obj.GetType().Equals(typeof(UserNode)))
            {
                UserNode user = (UserNode)obj;

                sendPearAlive(user.ep);
                if (over_time < -5)
                {
                    deleteUser(user.id);
                }
                else if (over_time < -2)
                {
                    SendIsAlivePear(user.id);
                }
            }
        }

        
        /// <summary>
        /// 기능별 패킷 전송 메소드
        /// </summary>
        void ConnectServer()
        {
            IPEndPoint localEndPoint = netWorkIO.getLocalEndPoint();
            
            CONNECT_SERVER_HEADER connect_server = new CONNECT_SERVER_HEADER();
            connect_server.type = CONNECT_SERVER;
            connect_server.id = userid;
            connect_server.private_ip = (uint)localEndPoint.Address.Address;
            connect_server.private_port = localEndPoint.Port;

            byte[] data = StructureToByte(connect_server);
            netWorkIO.SendDataToServer(data);
        }


        void SendAliveServer()
        {
            TYPE_HEADER alive_server = new TYPE_HEADER();
            alive_server.type = ALIVE_SERVER;
            alive_server.id = userid;

            byte[] data = StructureToByte(alive_server);
            netWorkIO.SendDataToServer(data);
        }

        void SendIsAlivePear(ushort target_id)
        {
            TYPE_HEADER isalive_pear = new TYPE_HEADER();
            isalive_pear.type = ISALIVE_PEAR;
            isalive_pear.id = target_id;

            byte[] data = StructureToByte(isalive_pear);
            netWorkIO.SendDataToServer(data);
        }

        void sendPearAlive(IPEndPoint ep)
        {
            Console.WriteLine(ep.Address.ToString());
            Console.WriteLine(ep.Port);

            TYPE_HEADER alive_pear = new TYPE_HEADER();
            alive_pear.type = ALIVE_PEAR;
            alive_pear.id = userid;

            byte[] data = StructureToByte(alive_pear);
            netWorkIO.SendData(data, ep);
        }
        public void sendVoiceData(byte[] voice)
        {
            DATA_HEADER data_header = new DATA_HEADER();
            data_header.type = DATA_VOICE;
            data_header.id = userid;

            byte[] headr = StructureToByte(data_header);
            byte[] data = new byte[headr.Length + voice.Length];

            Array.Copy(headr, 0, data, 0, headr.Length);
            Array.Copy(voice, 0, data, headr.Length, voice.Length);

            lock (user_list)
            {
                List<UserNode>.Enumerator itor = user_list.GetEnumerator();
                while (itor.MoveNext())
                {
                    UserNode user = (UserNode)itor.Current;
                    netWorkIO.SendData(data, user.ep);
                }
            }
        }

        public void sendTextData(byte[] text)
        {
            DATA_HEADER data_header = new DATA_HEADER();
            data_header.type = DATA_TEXT;
            data_header.id = userid;

            byte[] headr = StructureToByte(data_header);
            byte[] data = new byte[headr.Length + text.Length];

            Array.Copy(headr, 0, data, 0, headr.Length);
            Array.Copy(text, 0, data, headr.Length, text.Length);

            lock (user_list)
            {
                List<UserNode>.Enumerator itor = user_list.GetEnumerator();
                while (itor.MoveNext())
                {
                    UserNode user = (UserNode)itor.Current;
                    netWorkIO.SendData(data, user.ep);
                }
            }
        }






       /// <summary>
       /// 다른 pear들의 정보 = userList
       ///  userList 관련 메소드
       /// </summary>
  
        private UserNode findUser(int id)
        {
            lock (user_list)
            {
                List<UserNode>.Enumerator itor =  user_list.GetEnumerator();

                while (itor.MoveNext())
                {
                    if (itor.Current.id == id) return (UserNode)itor.Current;
                }

            }
            return null;
        }
        private int findUserIndex(int id)
        {
            lock (user_list)
            {
                List<UserNode>.Enumerator itor = user_list.GetEnumerator();
                int n=0;

                while (itor.MoveNext())
                {                   
                    if (itor.Current.id == id) return n;
                    n++;
                }

            }
            return -1;
        }

        private void addUser(UserNode user)
        {
            lock (user_list)
            {
                user_list.Add(user);
            }
            pearListLog();
        }
        private void deleteUser(int id)
        {

            UserNode deleteUser = null;
            int remove_index = -1;
            lock (user_list)
            {
                List<UserNode>.Enumerator itor = user_list.GetEnumerator();

                while (itor.MoveNext())
                {
                    remove_index++;
                    if (itor.Current.id == id)
                    {
                        deleteUser = (UserNode)itor.Current;
                        break;
                    }
                }

                if(deleteUser != null)
                {
                    user_list.RemoveAt(remove_index);
                    timer.deleteTimer(deleteUser);
                }
            }

            
            pearListLog();
        }
        private void clearUserList()
        {
            lock (user_list)
            {
                List<UserNode>.Enumerator itor = user_list.GetEnumerator();

                while (itor.MoveNext())
                {
                    UserNode user = itor.Current;
                    timer.deleteTimer(user);
                }
                user_list.Clear();
            }
        }

        private void pearListLog()
        {
            string log = "";
            lock (user_list)
            {
                List<UserNode>.Enumerator itor = user_list.GetEnumerator();
                while (itor.MoveNext())
                {
                    UserNode user = (UserNode)itor.Current;
                    log += user.id + ": " + user.ep.Address.ToString() + ":" + user.ep.Port + "  \n";                  
                }
                uiHandler.UIAdd(UIHandler.LIST_LOG, log);
            }          
        }

        



        /// <summary>
        /// event 처리
        /// </summary>
        private void ReceiveVoice(int id,byte[] data)
        {
            if (this.m_ReceiveVoice != null)
            {
                this.m_ReceiveVoice(id,data);
            }
        }
        private void ReceiveText(int id,byte[] data )
        {
            if (this.m_ReceiveText != null)
            {
                this.m_ReceiveText(id,data);
            }
        }
       

        
        /// <summary>
        /// dyte배열 -> 구조체 변환 
        /// 구조체 -> byte배열 변환
        /// 
        /// </summary>


        public T ByteToStructure<T>(byte[] data, int start)
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
            Marshal.Copy(data, start, ptr, Marshal.SizeOf(typeof(T)));
            T obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            return obj;
        }

        public byte[] StructureToByte(object obj)
        {
            int datasize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(datasize);
            Marshal.StructureToPtr(obj, buff, false);
            byte[] data = new byte[datasize];
            Marshal.Copy(buff, data, 0, datasize);
            Marshal.FreeHGlobal(buff);
            return data;
        }


        /// <summary>
        /// 수신 대기큐 node 구조체
        /// </summary>
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
}
