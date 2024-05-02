using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Patholab_DAL_V1;
using System.Runtime.InteropServices;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using System.Diagnostics.Eventing.Reader;
using Oracle.ManagedDataAccess.Client;

namespace PatholabWorkList
{
    [ComVisible(true)]
    [ProgId("PatholabWorkList.PatholabWorkList")]
    public partial class PatholabWorkListHost : UserControl, IExtensionWindow
    {

        #region Private members

        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;
        private DataLayer dal = null;
        private OracleConnection oraCon;
        private WpfPatholabWorkList workList;
        State _windowState;
        bool DEBUG = false;
        bool isManager = false;
        bool isBank = false;

        #endregion

        public PatholabWorkListHost()
        {
            try
            {
                InitializeComponent();
                this.Disposed += PatholabWorkList_Disposed;
                BackColor = Color.FromName("Control");
                this.Dock = DockStyle.Fill;
                this.AutoSize = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Logger.WriteLogFile(e);
            }
        }

        private void activateWorkListWindow()
        {
            try
            {
                dal = new DataLayer();
                if (DEBUG)
                {
                    workList = new WpfPatholabWorkList();
                    workList.initDebug();
                    WpfPatholabWorkList.DEBUG = true;
                    dal.MockConnect();
                }
                else
                {
                    dal.Connect(_ntlsCon);
                    workList = new WpfPatholabWorkList(dal, sp, _ntlsUser, _ntlsSite, _ntlsCon, _windowState);
                    elementHost1.Child = workList;
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Logger.WriteLogFile(ex);

            }
        }
  
        void PatholabWorkList_Disposed(object sender, EventArgs e)
        {
            GC.Collect();
        }

        public bool CloseQuery()
        {
            DialogResult res = MessageBox.Show(@"?האם אתה בטוח שברצונך לצאת ", "Patholab Work List", MessageBoxButtons.YesNo);

            if (res == DialogResult.Yes)
            {
                if (dal != null)
                {
                    dal.Close();
                    dal = null;
                }
                if (_ntlsSite != null) _ntlsSite = null;

                //    if (connection != null) connection.Close();

                this.Dispose();
                workList.CloseQuery();
                return true;
            }
            else
            {
                return false;
            }
        }

        public WindowRefreshType DataChange()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public WindowButtonsType GetButtons()
        {
            return LSExtensionWindowLib.WindowButtonsType.windowButtonsNone;
        }

        public void Internationalise()
        {
        }

        public void PreDisplay()
        {
            if (!DEBUG)
                _ntlsUser = Utils.GetNautilusUser(sp);

            activateWorkListWindow();
        }

        public void RestoreSettings(int hKey)
        {
        }

        public bool SaveData()
        {
            return true;
        }

        public void SaveSettings(int hKey)
        {
        }

        public void SetParameters(string parameters)
        {


            if (parameters.ToLower() == "manager")
            {
                isManager = true;
                _windowState = State.Manager;

            }
            else
            {
                if (parameters.ToLower() == "bank")
                {
                    isBank = true;
                    _windowState = State.Bank;


                }
                else
                {
                    _windowState = State.Patholog;

                }
            }


        }

        public void SetServiceProvider(object serviceProvider)
        {
            sp = serviceProvider as NautilusServiceProvider;
            _ntlsCon = Utils.GetNtlsCon(sp);
        }

        public void SetSite(object site)
        {
            _ntlsSite = (IExtensionWindowSite2)site;
            _ntlsSite.SetWindowInternalName("Patholab Work List");
            _ntlsSite.SetWindowRegistryName("Patholab_Work_List");
            _ntlsSite.SetWindowTitle("Patholab Work List");
        }

        public void Setup()
        {
        }

        public WindowRefreshType ViewRefresh()
        {
            return LSExtensionWindowLib.WindowRefreshType.windowRefreshNone;
        }

        public void refresh()
        {
        }




    }

}
