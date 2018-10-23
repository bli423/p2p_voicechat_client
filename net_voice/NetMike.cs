using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using net_voice.Media.Wave;
using net_voice.Network;
using net_voice.Timer;
using System.Threading;
using net_voice.Utility;

namespace net_voice
{
    
    class NetMike
    {
      
        private WaveIn wave_in;
        private WaveOut[] wave_outList;


        private P2PManager p2PManager;
        private Queue<byte[]> chatQueue;   

        private const int wave_out_amount = 16;

        private Thread textDataSend;
        private UIHandler uiHandler;

        private bool run = true;

        public NetMike(UIHandler uiHandler)
        {
            this.uiHandler = uiHandler;
            p2PManager = new P2PManager(uiHandler);
            p2PManager.m_ReceiveText += new P2PManager.ReceiveTextHandler(textReceive);
            p2PManager.m_ReceiveVoice += new P2PManager.ReceiveVoiceHandler(voiceReceive);
            chatQueue = new Queue<byte[]>();

            textDataSend = new Thread(sendDataRun);
           

            wave_in = new WaveIn(WaveIn.Devices[0], 8000, 16, 1, 400);
            wave_in.BufferFull += new BufferFullHandler(waveIn_BufferFull);

            wave_outList = new WaveOut[wave_out_amount];

            for (int i = 0; i < wave_outList.Length; i++)
            {
                wave_outList[i] = new WaveOut(WaveOut.Devices[0], 8000, 16, 1);
            }
        }

        public void close()
        {
            p2PManager.close();
            wave_in.Dispose();
            run = false;
            lock (chatQueue)
            {
                Monitor.PulseAll(chatQueue);
            }
        }

        public void Run()
        {
            wave_in.Start();
            p2PManager.Run();
            textDataSend.Start();  
        }

        /// <summary>
        /// 음성 데이터 수집 이벤트
        /// </summary>
        /// <param name="buffer"></param>
        private void waveIn_BufferFull(byte[] buffer)
        {
            byte[] data = new byte[buffer.Length];
            Array.Copy(buffer,0, data,0, data.Length);            
            
            p2PManager.sendVoiceData(data);     
        }

        /// <summary>
        /// 다른 pear에게 수신한 문자 정보 UI handler에 넘겨줌
        /// </summary>
        private void textReceive(int n, byte[] buffer)
        {
            uiHandler.UIAdd(UIHandler.TEXT, n + " :  "+  Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        ///  다른 pear에게 수신한 음성데이터 출력 
        /// </summary>
        private void voiceReceive(int n,byte[] buffer)
        {           
            wave_outList[n].Play(buffer, 0, buffer.Length);
        }
               
        /// <summary>
        /// 마이크 초기화
        /// </summary>
        public void mikeReset()
        {
            wave_in.Stop();
            wave_in.Dispose();
            wave_in = new WaveIn(WaveIn.Devices[0], 8000, 16, 1, 400);
            wave_in.BufferFull += new BufferFullHandler(waveIn_BufferFull);
            wave_in.Start();        
        }


        /// <summary>
        /// 문자 데이터 전송 대기큐 등록
        /// </summary>
        /// <param name="text"></param>
        public void inputMessage(string text)
        {
            byte[] data = Encoding.UTF8.GetBytes(text);
            lock (chatQueue)
            {
                chatQueue.Enqueue(data);
                Monitor.Pulse(chatQueue);
            }            
           
        }

        /// <summary>
        /// 문자 데이터 전송 스레드
        /// </summary>
        private void sendDataRun()
        {
            byte[] data = null;
            while (run)
            {
                lock (chatQueue)
                {
                    while (chatQueue.Count == 0)
                    {
                        Monitor.Wait(chatQueue);
                        if (!run) return;
                    }
                    data = chatQueue.Dequeue();
                }
                p2PManager.sendTextData(data);
            }
        }
       
    }   
}
