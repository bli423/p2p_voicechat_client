
using net_voice.Utility;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace net_voice
{
    public partial class Form1 : Form
    {
        NetMike net_mike;
        Queue<string> logQueue;
        Queue<string> chatQueue;
        string serverlog="";
        string listlog_text = "";
        UIHandler uiHandler;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {    
            logQueue = new Queue<string>();
            chatQueue = new Queue<string>();

            uiHandler = new UIHandler();
            uiHandler.m_UIEvent += new UIHandler.UIHandlerEvent(UIEvent);

            net_mike = new NetMike(uiHandler);

            net_mike.Run();
            uiHandler.Run();
        }

        /// <summary>
        /// 프로그램 종료
        /// </summary>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            uiHandler.close();
            net_mike.close(); 
        }

        private void UIEvent(int type,object obj)
        {           
            if (type == UIHandler.TEXT)
            {
                lock (logQueue)
                {
                    chatQueue.Enqueue((string)obj + "\n");
                }
                chat_output.BeginInvoke(new MethodInvoker(chatWrite));
            }
            else if (type == UIHandler.LIST_LOG)
            {
                listlog_text = (string)obj;
                host_list.BeginInvoke(new MethodInvoker(listLogWrite));
            }
            else if (type == UIHandler.COVER_UNVISIBLE)
            {
                server_connect_view.BeginInvoke(new MethodInvoker(serverConnectViewUnVisible));
            }
            else if (type == UIHandler.COVER_VISIBLE)
            {
                server_connect_view.BeginInvoke(new MethodInvoker(serverConnectViewVisible));
            }
        }

      


        /// <summary>
        /// UI event 처리
        /// </summary>
        private void serverConnectViewVisible()
        {
            server_connect_view.Visible = true;
        }
        private void serverConnectViewUnVisible()
        {
            server_connect_view.Visible = false;
        }

        private void chatWrite()
        {
            lock (chatQueue)
            {
                try
                {
                    string text = chatQueue.Dequeue();
                    while (text != null)
                    {
                        chat_output.AppendText(text);
                        text = chatQueue.Dequeue();
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        private void listLogWrite()
        {
            host_list.Text = listlog_text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = chat_input.Text;

            net_mike.inputMessage(text);
            chat_output.AppendText("me  :  " + text+"\n");
            chat_input.Text = "";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            net_mike.mikeReset();
        }

        private void server_connect_view_Click(object sender, EventArgs e)
        {

        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        
    }
}
