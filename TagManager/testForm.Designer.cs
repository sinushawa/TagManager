namespace TagManager
{
    partial class testForm
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
            this.reader_tbx = new System.Windows.Forms.TextBox();
            this.submit_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // reader_tbx
            // 
            this.reader_tbx.Location = new System.Drawing.Point(12, 45);
            this.reader_tbx.Name = "reader_tbx";
            this.reader_tbx.Size = new System.Drawing.Size(260, 20);
            this.reader_tbx.TabIndex = 0;
            // 
            // submit_btn
            // 
            this.submit_btn.Location = new System.Drawing.Point(13, 92);
            this.submit_btn.Name = "submit_btn";
            this.submit_btn.Size = new System.Drawing.Size(259, 23);
            this.submit_btn.TabIndex = 1;
            this.submit_btn.Text = "submit";
            this.submit_btn.UseVisualStyleBackColor = true;
            this.submit_btn.Click += new System.EventHandler(this.onClick);
            // 
            // testForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.submit_btn);
            this.Controls.Add(this.reader_tbx);
            this.Name = "testForm";
            this.Text = "testForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox reader_tbx;
        public System.Windows.Forms.Button submit_btn;
    }
}