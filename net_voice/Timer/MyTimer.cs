using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections;

namespace net_voice.Timer
{
    class MyTimer
    {
        public delegate void MyTimerHandler(object obj,int over_time);

        public event MyTimerHandler m_TimerEvent = null;

        private Hashtable timerList = new Hashtable();
        private bool run = true;

        public MyTimer()
        {
            Thread TimerThread = new Thread(TimerRun);
            TimerThread.Start();
        }

        public void close()
        {
            run = false;
        }
        public void AddTimer(object obj, int time)
        {
            lock (timerList)
            {
                if (timerList.ContainsKey(obj))
                {
                    ((TimerNode)timerList[obj]).maxTimerSet(time);
                    ((TimerNode)timerList[obj]).set();
                }
                else
                {
                    timerList.Add(obj, new TimerNode(time));
                }
            }
        }

        public void TimerSet(object obj)
        {
            lock (timerList)
            {
                if (timerList.ContainsKey(obj))
                {
                    ((TimerNode)timerList[obj]).set();
                }
            }
        }



        public void deleteTimer(object obj)
        {
            lock (timerList)
            {
                if (timerList.ContainsKey(obj))
                {
                    timerList.Remove(obj);
                }
            }
        }


        private void TimerRun()
        {
            LinkedList<DictionaryEntry> EventList = new LinkedList<DictionaryEntry>();
            while (true)
            {               
                Thread.Sleep(1000);
                if (!run) return;
                lock (timerList)
                {
                    foreach (DictionaryEntry de in timerList)
                    {
                        int time = ((TimerNode)de.Value).count();
                        if (time <= 0)
                        {
                            EventList.AddLast(de);
                            
                        }
                    }

                    IEnumerator itor = EventList.GetEnumerator();

                    while (itor.MoveNext())
                    {
                        DictionaryEntry de = (DictionaryEntry)itor.Current;
                        TimerEvent(de.Key, ((TimerNode)de.Value).getCount());

                    }
                    EventList.Clear();
                }
            }
        }


        private void TimerEvent(object obj, int over_time)
        {
            if (this.m_TimerEvent != null)
            {
                this.m_TimerEvent(obj, over_time);
            }
        }


        private class TimerNode
        {
            private int maxTime;
            private int countTime;

            public TimerNode(int maxTime)
            {
                this.maxTime = maxTime;
                this.countTime = maxTime;
            }

            public void maxTimerSet(int maxTime)
            {
                this.maxTime = maxTime;
            }
            public void set()
            {
                lock (this) { 
                    countTime = maxTime;
                }
            }
            public int getCount()
            {               
                return countTime;
            }
            public int count()
            {
                lock (this)
                {
                    countTime -= 1;
                }
                return countTime;
            }
        }

    }
}
