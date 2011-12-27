namespace RockPaperScissorsGUI
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnRock = new System.Windows.Forms.Button();
            this.btnPaper = new System.Windows.Forms.Button();
            this.btnScissors = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnRock
            // 
            this.btnRock.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRock.Location = new System.Drawing.Point(26, 199);
            this.btnRock.Name = "btnRock";
            this.btnRock.Size = new System.Drawing.Size(119, 40);
            this.btnRock.TabIndex = 0;
            this.btnRock.Text = "Rock";
            this.btnRock.UseVisualStyleBackColor = true;
            this.btnRock.Click += new System.EventHandler(this.btnRock_Click);
            // 
            // btnPaper
            // 
            this.btnPaper.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPaper.Location = new System.Drawing.Point(185, 199);
            this.btnPaper.Name = "btnPaper";
            this.btnPaper.Size = new System.Drawing.Size(121, 40);
            this.btnPaper.TabIndex = 1;
            this.btnPaper.Text = "Paper";
            this.btnPaper.UseVisualStyleBackColor = true;
            this.btnPaper.Click += new System.EventHandler(this.btnPaper_Click);
            // 
            // btnScissors
            // 
            this.btnScissors.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnScissors.Location = new System.Drawing.Point(340, 197);
            this.btnScissors.Name = "btnScissors";
            this.btnScissors.Size = new System.Drawing.Size(125, 42);
            this.btnScissors.TabIndex = 2;
            this.btnScissors.Text = "Scissors";
            this.btnScissors.UseVisualStyleBackColor = true;
            this.btnScissors.Click += new System.EventHandler(this.btnScissors_Click);
            // 
            // txtResult
            // 
            this.txtResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtResult.Location = new System.Drawing.Point(27, 108);
            this.txtResult.MaximumSize = new System.Drawing.Size(439, 90);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.Size = new System.Drawing.Size(439, 70);
            this.txtResult.TabIndex = 3;
            this.txtResult.TabStop = false;
            this.txtResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 16.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblName.Location = new System.Drawing.Point(21, 56);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(295, 32);
            this.lblName.TabIndex = 4;
            this.lblName.Text = "Rock Paper Scissors";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(27, 260);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(439, 234);
            this.txtLog.TabIndex = 5;
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(232, 31);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(126, 22);
            this.txtUser.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(229, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 17);
            this.label1.TabIndex = 7;
            this.label1.Text = "Username:";
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnect.Location = new System.Drawing.Point(364, 12);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(102, 40);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(27, 513);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(439, 43);
            this.textBox1.TabIndex = 9;
            // 
            // btnSend
            // 
            this.btnSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSend.Location = new System.Drawing.Point(364, 574);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(101, 42);
            this.btnSend.TabIndex = 10;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(27, 31);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 22);
            this.textBox2.TabIndex = 11;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 17);
            this.label2.TabIndex = 12;
            this.label2.Text = "Server IP";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 625);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnScissors);
            this.Controls.Add(this.btnPaper);
            this.Controls.Add(this.btnRock);
            this.Name = "Form1";
            this.Text = "RockPaperScissors";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRock;
        private System.Windows.Forms.Button btnPaper;
        private System.Windows.Forms.Button btnScissors;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label2;

    }
}

