using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TagManager
{
    public partial class testForm : Form
    {
        public TagCenter parent;

        public testForm(TagCenter _parent)
        {
            parent = _parent;
            InitializeComponent();
        }

        private void onClick(object sender, EventArgs e)
        {
            parent.GUP.remember = this.reader_tbx.Text;
        }
    }
}
