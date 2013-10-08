using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Max;
using MaxCustomControls;

namespace TagManager
{
    public partial class testForm : MaxForm
    {
        public TagCenter parent;

        public testForm(TagCenter _parent)
        {
            parent = _parent;
            InitializeComponent();
        }

        private void onClick(object sender, EventArgs e)
        {

        }
    }
}
