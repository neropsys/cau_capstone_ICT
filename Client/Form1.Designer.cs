﻿namespace Client
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.user = new System.Windows.Forms.TextBox();
            this.textbox1 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.chatLogBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.rightNowStr = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.getTextLogBtn = new System.Windows.Forms.Button();
            this.rightNowList = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.uploadTagBtn = new System.Windows.Forms.Button();
            this.groupChatLog = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.getRoomTagBtn = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.sendToGroupBtn = new System.Windows.Forms.Button();
            this.groupChatTextBox = new System.Windows.Forms.TextBox();
            this.tagList = new System.Windows.Forms.ListBox();
            this.label9 = new System.Windows.Forms.Label();
            this.getChatFrom0 = new System.Windows.Forms.Button();
            this.getTagButton = new System.Windows.Forms.Button();
            this.testBtn = new System.Windows.Forms.Button();
            this.userNickInput = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.bopPartyInput = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(37, 63);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(37, 92);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "disconnect";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // user
            // 
            this.user.Location = new System.Drawing.Point(201, 34);
            this.user.Name = "user";
            this.user.Size = new System.Drawing.Size(100, 21);
            this.user.TabIndex = 2;
            // 
            // textbox1
            // 
            this.textbox1.Location = new System.Drawing.Point(244, 407);
            this.textbox1.Name = "textbox1";
            this.textbox1.Size = new System.Drawing.Size(219, 21);
            this.textbox1.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(486, 405);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "send";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(599, 34);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(120, 88);
            this.listBox1.TabIndex = 5;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // chatLogBox
            // 
            this.chatLogBox.Location = new System.Drawing.Point(23, 289);
            this.chatLogBox.Name = "chatLogBox";
            this.chatLogBox.Size = new System.Drawing.Size(410, 112);
            this.chatLogBox.TabIndex = 6;
            this.chatLogBox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(201, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "userId";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(176, 410);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "chatText";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 274);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "chatLog";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(597, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "유저리스트";
            // 
            // rightNowStr
            // 
            this.rightNowStr.Location = new System.Drawing.Point(307, 34);
            this.rightNowStr.Name = "rightNowStr";
            this.rightNowStr.Size = new System.Drawing.Size(100, 21);
            this.rightNowStr.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(305, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "지금나는 태그";
            // 
            // getTextLogBtn
            // 
            this.getTextLogBtn.Location = new System.Drawing.Point(11, 120);
            this.getTextLogBtn.Name = "getTextLogBtn";
            this.getTextLogBtn.Size = new System.Drawing.Size(116, 23);
            this.getTextLogBtn.TabIndex = 13;
            this.getTextLogBtn.Text = "유저채팅로그 겟";
            this.getTextLogBtn.UseVisualStyleBackColor = true;
            this.getTextLogBtn.Click += new System.EventHandler(this.getTextLogBtn_Click);
            // 
            // rightNowList
            // 
            this.rightNowList.FormattingEnabled = true;
            this.rightNowList.ItemHeight = 12;
            this.rightNowList.Location = new System.Drawing.Point(599, 167);
            this.rightNowList.Name = "rightNowList";
            this.rightNowList.Size = new System.Drawing.Size(120, 88);
            this.rightNowList.TabIndex = 14;
            this.rightNowList.SelectedIndexChanged += new System.EventHandler(this.rightNowList_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(597, 258);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 12);
            this.label6.TabIndex = 15;
            this.label6.Text = "지금나는채팅방리스트";
            // 
            // uploadTagBtn
            // 
            this.uploadTagBtn.Location = new System.Drawing.Point(413, 34);
            this.uploadTagBtn.Name = "uploadTagBtn";
            this.uploadTagBtn.Size = new System.Drawing.Size(118, 23);
            this.uploadTagBtn.TabIndex = 16;
            this.uploadTagBtn.Text = "uploadTag";
            this.uploadTagBtn.UseVisualStyleBackColor = true;
            this.uploadTagBtn.Click += new System.EventHandler(this.createRoomBtn_Click);
            // 
            // groupChatLog
            // 
            this.groupChatLog.Location = new System.Drawing.Point(142, 125);
            this.groupChatLog.Name = "groupChatLog";
            this.groupChatLog.Size = new System.Drawing.Size(410, 112);
            this.groupChatLog.TabIndex = 17;
            this.groupChatLog.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(140, 103);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(69, 12);
            this.label7.TabIndex = 18;
            this.label7.Text = "그룹챗 로그";
            // 
            // getRoomTagBtn
            // 
            this.getRoomTagBtn.Location = new System.Drawing.Point(11, 149);
            this.getRoomTagBtn.Name = "getRoomTagBtn";
            this.getRoomTagBtn.Size = new System.Drawing.Size(125, 23);
            this.getRoomTagBtn.TabIndex = 19;
            this.getRoomTagBtn.Text = "채팅방리스트 겟";
            this.getRoomTagBtn.UseVisualStyleBackColor = true;
            this.getRoomTagBtn.Click += new System.EventHandler(this.getRoomTagBtn_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(122, 248);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 12);
            this.label8.TabIndex = 22;
            this.label8.Text = "groupchatText";
            // 
            // sendToGroupBtn
            // 
            this.sendToGroupBtn.Location = new System.Drawing.Point(456, 243);
            this.sendToGroupBtn.Name = "sendToGroupBtn";
            this.sendToGroupBtn.Size = new System.Drawing.Size(75, 23);
            this.sendToGroupBtn.TabIndex = 21;
            this.sendToGroupBtn.Text = "send";
            this.sendToGroupBtn.UseVisualStyleBackColor = true;
            this.sendToGroupBtn.Click += new System.EventHandler(this.sendToGroupBtn_Click);
            // 
            // groupChatTextBox
            // 
            this.groupChatTextBox.Location = new System.Drawing.Point(214, 245);
            this.groupChatTextBox.Name = "groupChatTextBox";
            this.groupChatTextBox.Size = new System.Drawing.Size(219, 21);
            this.groupChatTextBox.TabIndex = 20;
            // 
            // tagList
            // 
            this.tagList.FormattingEnabled = true;
            this.tagList.ItemHeight = 12;
            this.tagList.Location = new System.Drawing.Point(599, 289);
            this.tagList.Name = "tagList";
            this.tagList.Size = new System.Drawing.Size(120, 88);
            this.tagList.TabIndex = 23;
            this.tagList.SelectedIndexChanged += new System.EventHandler(this.tagList_SelectedIndexChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(602, 389);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 12);
            this.label9.TabIndex = 24;
            this.label9.Text = "지금나는태그 리스트";
            // 
            // getChatFrom0
            // 
            this.getChatFrom0.Location = new System.Drawing.Point(11, 178);
            this.getChatFrom0.Name = "getChatFrom0";
            this.getChatFrom0.Size = new System.Drawing.Size(125, 41);
            this.getChatFrom0.TabIndex = 25;
            this.getChatFrom0.Text = "유저챗인덱스0부터 겟";
            this.getChatFrom0.UseVisualStyleBackColor = true;
            this.getChatFrom0.Click += new System.EventHandler(this.getChatFrom0_Click);
            // 
            // getTagButton
            // 
            this.getTagButton.Location = new System.Drawing.Point(456, 309);
            this.getTagButton.Name = "getTagButton";
            this.getTagButton.Size = new System.Drawing.Size(75, 23);
            this.getTagButton.TabIndex = 26;
            this.getTagButton.Text = "getTag";
            this.getTagButton.UseVisualStyleBackColor = true;
            this.getTagButton.Click += new System.EventHandler(this.getTagButton_Click);
            // 
            // testBtn
            // 
            this.testBtn.Location = new System.Drawing.Point(604, 410);
            this.testBtn.Name = "testBtn";
            this.testBtn.Size = new System.Drawing.Size(75, 23);
            this.testBtn.TabIndex = 27;
            this.testBtn.Text = "send";
            this.testBtn.UseVisualStyleBackColor = true;
            this.testBtn.Click += new System.EventHandler(this.testBtn_Click);
            // 
            // userNickInput
            // 
            this.userNickInput.Location = new System.Drawing.Point(201, 61);
            this.userNickInput.Name = "userNickInput";
            this.userNickInput.Size = new System.Drawing.Size(100, 21);
            this.userNickInput.TabIndex = 28;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(133, 68);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 12);
            this.label10.TabIndex = 29;
            this.label10.Text = "userNick";
            // 
            // bopPartyInput
            // 
            this.bopPartyInput.Location = new System.Drawing.Point(307, 61);
            this.bopPartyInput.Name = "bopPartyInput";
            this.bopPartyInput.Size = new System.Drawing.Size(100, 21);
            this.bopPartyInput.TabIndex = 30;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(413, 64);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(55, 12);
            this.label11.TabIndex = 31;
            this.label11.Text = "bopParty";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(413, 88);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(118, 23);
            this.button4.TabIndex = 32;
            this.button4.Text = "uploadbopTag";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.bopPartyInput);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.userNickInput);
            this.Controls.Add(this.testBtn);
            this.Controls.Add(this.getTagButton);
            this.Controls.Add(this.getChatFrom0);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tagList);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.sendToGroupBtn);
            this.Controls.Add(this.groupChatTextBox);
            this.Controls.Add(this.getRoomTagBtn);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.groupChatLog);
            this.Controls.Add(this.uploadTagBtn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rightNowList);
            this.Controls.Add(this.getTextLogBtn);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.rightNowStr);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chatLogBox);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.textbox1);
            this.Controls.Add(this.user);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "ChatClient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox user;
        private System.Windows.Forms.TextBox textbox1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox rightNowStr;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button getTextLogBtn;
        private System.Windows.Forms.ListBox rightNowList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox chatLogBox;
        private System.Windows.Forms.Button uploadTagBtn;
        private System.Windows.Forms.RichTextBox groupChatLog;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button getRoomTagBtn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button sendToGroupBtn;
        private System.Windows.Forms.TextBox groupChatTextBox;
        private System.Windows.Forms.ListBox tagList;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button getChatFrom0;
        private System.Windows.Forms.Button getTagButton;
        private System.Windows.Forms.Button testBtn;
        private System.Windows.Forms.TextBox userNickInput;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox bopPartyInput;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button button4;
    }
}

