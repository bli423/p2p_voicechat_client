namespace net_voice
{
    partial class Form1
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.chat_output = new System.Windows.Forms.TextBox();
            this.chat_input = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.host_list = new System.Windows.Forms.TextBox();
            this.this_ip = new System.Windows.Forms.Label();
            this.server_connect_view = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chat_output
            // 
            this.chat_output.BackColor = System.Drawing.Color.White;
            this.chat_output.Location = new System.Drawing.Point(12, 29);
            this.chat_output.Multiline = true;
            this.chat_output.Name = "chat_output";
            this.chat_output.ReadOnly = true;
            this.chat_output.Size = new System.Drawing.Size(482, 427);
            this.chat_output.TabIndex = 1;
            // 
            // chat_input
            // 
            this.chat_input.Location = new System.Drawing.Point(12, 471);
            this.chat_input.Multiline = true;
            this.chat_input.Name = "chat_input";
            this.chat_input.Size = new System.Drawing.Size(420, 57);
            this.chat_input.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.button1.Location = new System.Drawing.Point(438, 471);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 56);
            this.button1.TabIndex = 4;
            this.button1.Text = "전송";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(524, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(122, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "호스트 연결 상태";
            // 
            // host_list
            // 
            this.host_list.Location = new System.Drawing.Point(527, 68);
            this.host_list.Multiline = true;
            this.host_list.Name = "host_list";
            this.host_list.Size = new System.Drawing.Size(156, 286);
            this.host_list.TabIndex = 10;
            // 
            // this_ip
            // 
            this.this_ip.AutoSize = true;
            this.this_ip.Location = new System.Drawing.Point(12, 11);
            this.this_ip.Name = "this_ip";
            this.this_ip.Size = new System.Drawing.Size(45, 15);
            this.this_ip.TabIndex = 11;
            this.this_ip.Text = "label4";
            // 
            // server_connect_view
            // 
            this.server_connect_view.BackColor = System.Drawing.Color.LightYellow;
            this.server_connect_view.Font = new System.Drawing.Font("Gulim", 40F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.server_connect_view.Location = new System.Drawing.Point(-76, 1);
            this.server_connect_view.Name = "server_connect_view";
            this.server_connect_view.Size = new System.Drawing.Size(939, 575);
            this.server_connect_view.TabIndex = 13;
            this.server_connect_view.Text = "서버와 연결 대기중";
            this.server_connect_view.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.server_connect_view.Click += new System.EventHandler(this.server_connect_view_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(863, 585);
            this.Controls.Add(this.server_connect_view);
            this.Controls.Add(this.this_ip);
            this.Controls.Add(this.host_list);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.chat_input);
            this.Controls.Add(this.chat_output);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox chat_output;
        private System.Windows.Forms.TextBox chat_input;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox host_list;
        private System.Windows.Forms.Label this_ip;
        private System.Windows.Forms.Label server_connect_view;
    }
}

