using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatholabWorkList
{
    public partial class ReContainer : Form
    {
        public ReContainer()
        {
            InitializeComponent();

            this.Text = "patholab Work List";

            this.FormClosing += new FormClosingEventHandler(closeForm); ;
        }
        private void closeForm(object sender, FormClosingEventArgs e)
        {
            this.Hide();
        }
    }
}
