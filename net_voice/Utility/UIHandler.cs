using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace net_voice.Utility
{
    class UIHandler
    {
        public delegate void UIHandlerEvent(int type, object value);
        public event UIHandlerEvent m_UIEvent = null;


        public static int USER_LOG = 1;
        public static int SERVER_LOG = 2;
        public static int TEXT = 3;
        public static int LIST_LOG = 4;
        public static int COVER_VISIBLE = 5;
        public static int COVER_UNVISIBLE = 7;
        private bool run = true;

        private Queue<UICommand> UIHandlerQue;
        Thread uiThread;

        public UIHandler()
        {
            UIHandlerQue = new Queue<UICommand>();
            uiThread = new Thread(UIQueThreadRun);
        }

        public void close()
        {
            run = false;
            lock (UIHandlerQue)
            {
                Monitor.PulseAll(UIHandlerQue);
            }
        }
        public void Run()
        {
            uiThread.Start();
        }

        public void UIAdd(int type, object value)
        {
            lock (UIHandlerQue)
            {
                UIHandlerQue.Enqueue(new UICommand(type, value));
                Monitor.Pulse(UIHandlerQue);
            }
        }

        private void UIQueThreadRun()
        {
            Thread.Sleep(1000);
            while (run)
            {
                UICommand uICommand;
                lock (UIHandlerQue)
                {
                    while(UIHandlerQue.Count == 0)
                    {
                        Monitor.Wait(UIHandlerQue);
                        if (!run) return;
                    }
                    uICommand = UIHandlerQue.Dequeue();
                    uiEvent(uICommand.type, uICommand.value);
                }
            }
        }


        private void uiEvent(int type, object value)
        {
            if(this.m_UIEvent != null)
            {
                this.m_UIEvent(type, value);
            }
        }
    }


    class UICommand
    {
        public int type;
        public object value;

        public UICommand(int type, object value)
        {
            this.type = type;
            this.value = value;
        }

    }
}
