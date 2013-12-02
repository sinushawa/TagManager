using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
namespace TagPan
{
	public class FastHost : UserControl
	{
		private IContainer components = null;
		private ElementHost elementHost;
		private FastWPFTag fastWPFTag_obj;
		public event System.EventHandler ForceRedraw;
		public FastHost()
		{
			this.InitializeComponent();
		}
		private void fastWPFTag_obj_ForceRedraw(object sender, System.EventArgs e)
		{
			this.ForceRedraw(null, null);
		}
		public void CreateAutoCompleteSource(TagPanel _tagPan)
		{
			this.fastWPFTag_obj.CreateAutoCompleteSource(_tagPan);
		}
		public void LinkParent()
		{
			this.fastWPFTag_obj.winParent = (Form)base.Parent;
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}
		private void InitializeComponent()
		{
			this.elementHost = new ElementHost();
			this.fastWPFTag_obj = new FastWPFTag();
			base.SuspendLayout();
			this.elementHost.Dock = DockStyle.Fill;
			this.elementHost.Location = new Point(0, 0);
			this.elementHost.MaximumSize = new Size(320, 30);
			this.elementHost.MinimumSize = new Size(320, 30);
			this.elementHost.Name = "elementHost";
			this.elementHost.Size = new Size(320, 30);
			this.elementHost.TabIndex = 0;
			this.elementHost.Text = "elementHost1";
			this.elementHost.Child = this.fastWPFTag_obj;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = AutoScaleMode.Font;
			base.Controls.Add(this.elementHost);
			this.MaximumSize = new Size(320, 30);
			this.MinimumSize = new Size(320, 30);
			base.Name = "FastHost";
			base.Size = new Size(320, 30);
			base.ResumeLayout(false);
		}
	}
}
