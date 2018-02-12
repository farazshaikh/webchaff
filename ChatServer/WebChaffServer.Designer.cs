namespace ChatServer
{
    partial class WebChaffServer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WebChaffServer));
            this.btnListen = new System.Windows.Forms.Button();
            this.txtIp = new System.Windows.Forms.TextBox();
            this.lbIp = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnListen
            // 
            this.btnListen.Location = new System.Drawing.Point(250, 449);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(106, 23);
            this.btnListen.TabIndex = 0;
            this.btnListen.Text = "Start Listening";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // txtIp
            // 
            this.txtIp.Location = new System.Drawing.Point(87, 449);
            this.txtIp.Name = "txtIp";
            this.txtIp.Size = new System.Drawing.Size(157, 20);
            this.txtIp.TabIndex = 1;
            this.txtIp.Text = "127.0.0.1";
            // 
            // lbIp
            // 
            this.lbIp.AutoSize = true;
            this.lbIp.Location = new System.Drawing.Point(9, 456);
            this.lbIp.Name = "lbIp";
            this.lbIp.Size = new System.Drawing.Size(61, 13);
            this.lbIp.TabIndex = 2;
            this.lbIp.Text = "IP Address:";
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(12, 55);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(344, 388);
            this.txtLog.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(10, 5);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(345, 50);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // WebChaffServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 477);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lbIp);
            this.Controls.Add(this.txtIp);
            this.Controls.Add(this.btnListen);
            this.Name = "WebChaffServer";
            this.Text = "WebChaffServer";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnListen;
        private System.Windows.Forms.TextBox txtIp;
        private System.Windows.Forms.Label lbIp;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

