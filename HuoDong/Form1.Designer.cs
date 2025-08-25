namespace Brand
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            progressBar1 = new ProgressBar();
            webView2First = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)webView2First).BeginInit();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Dock = DockStyle.Bottom;
            progressBar1.Location = new Point(0, 814);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(1216, 23);
            progressBar1.TabIndex = 17;
            // 
            // webView2First
            // 
            webView2First.AllowExternalDrop = true;
            webView2First.CreationProperties = null;
            webView2First.DefaultBackgroundColor = Color.White;
            webView2First.Dock = DockStyle.Fill;
            webView2First.Location = new Point(0, 0);
            webView2First.Name = "webView2First";
            webView2First.Size = new Size(1216, 837);
            webView2First.TabIndex = 16;
            webView2First.ZoomFactor = 1D;
            webView2First.NavigationCompleted += webView2First_NavigationCompleted;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1216, 837);
            Controls.Add(progressBar1);
            Controls.Add(webView2First);
            MaximizeBox = false;
            Name = "Form1";
            Text = "SZLCSC 活动优惠收集";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)webView2First).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private ProgressBar progressBar1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2First;
    }
}
