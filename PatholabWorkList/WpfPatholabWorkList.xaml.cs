using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Oracle.ManagedDataAccess.Client;
using Patholab_Common;
using Patholab_DAL_V1;
using Patholab_XmlService;
using PathologResultEntry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using forms = System.Windows.Forms;




namespace PatholabWorkList
{

    public partial class WpfPatholabWorkList : UserControl
    {
        #region fields
        private DataLayer dal;
        ListView currentListView;
        ReContainer pathologEntryContainer;
        PathologResultEntryCls pre;
        INautilusServiceProvider sp;
        INautilusUser ntlsUser;
        INautilusDBConnection ntlsCon;
        public static bool DEBUG = false;
        private long loggedInUser;
        private string loggedInUserName;
        private string loggedInUserFullName;
        private State _state;
        OracleConnection _oraCon;

        #endregion

        #region ctor
        public WpfPatholabWorkList(DataLayer _dal, INautilusServiceProvider _sp, INautilusUser _ntlsUser, IExtensionWindowSite2 _ntlsSite, INautilusDBConnection _ntlsCon, State _windowState)
        {
            InitializeComponent();

            Mouse.OverrideCursor = Cursors.Wait;
            var Z = tabControl1.SelectedItem;
            dal = _dal;
            sp = _sp;
            ntlsUser = _ntlsUser;
            ntlsCon = _ntlsCon;
            _state = _windowState;

            _oraCon = _dal.GetOracleConnection(_ntlsCon);

            loggedInUser = Convert.ToInt64(ntlsUser.GetOperatorId());
            loggedInUserName = ntlsUser.GetOperatorName().Trim();

            var q = (from item in dal.FindBy<OPERATOR>(op => op.OPERATOR_ID == loggedInUser) select new { item.FULL_NAME }).FirstOrDefault();
            loggedInUserFullName = q.FULL_NAME;
            init();
            Mouse.OverrideCursor = null;

        }



        public WpfPatholabWorkList()//for debug only
        {
            InitializeComponent();


        }
        public void initDebug()
        {


            DEBUG = true;
            loggedInUser = 1;// 121;
            loggedInUserName = "lims_sys";//Dima ";
            loggedInUserFullName = "Lims System User";
            dal = new DataLayer();
            dal.MockConnect();
            init();


        }

        ~WpfPatholabWorkList()
        {

        }
        UIManager _uiManager;

        //checking role
        private bool chkPermission()
        {
            var permittedQuery = from item in dal.FindBy<LIMS_ROLE>(OPE => OPE.NAME == "PATHOLOG_MANAGER")
                                 select item.OPERATORs1;
            foreach (var item1 in permittedQuery.ToList())
            {
                foreach (var op in item1)
                {
                    if (op.NAME == loggedInUserName)
                        return true;
                }
            }
            return false;
        }
        private void init()
        {
            try
            {
                var style = Resources["myCheckboxStyle"] as Style;
                _uiManager = new UIManager(tabControl1, dal, style);
                LoadData();
                LoadOrgans();

                // Remove old tabs
                switch (_state)
                {
                    case State.Manager:
                        SetManagerPage();
                        break;
                    case State.Patholog:
                        SetPTGPage();
                        break;
                    case State.Bank:
                        SetBankPage();
                        break;
                    default:
                        if (DEBUG)
                        {
                            SetBankPage();
                        }
                        break;
                }



                initializeData();




            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void LoadOrgans()
        {
            Thread test = new Thread(() =>
            {
                this.Dispatcher.BeginInvoke((Action)(() =>
                {
                    List<PatientRow> rows = _uiManager.GetRows(currentListView.Name);
                    List<string> l_v_organs = new List<string>();


                    l_v_organs.Add("כלל האיברים");
                    var organNamesList = rows.Select(patient => patient.FirstOrgan).Distinct().ToList();

                    if (organNamesList != null)
                    {
                        organNamesList.Sort();
                        l_v_organs.AddRange(organNamesList);
                    }

                    CmbOrgan.SelectionChanged -= CmbOrgan_SelectionChanged;
                    CmbOrgan.ItemsSource = null;
                    CmbOrgan.ItemsSource = l_v_organs;
                    CmbOrgan.SelectedIndex = -1;

                    CmbOrgan.SelectionChanged += CmbOrgan_SelectionChanged;

                }));

            });
            test.Start();

        }



        private void SetManagerPage()
        {
            _state = State.Manager;
            labelTitle.Content = "מסך מנהל";

            // הסרת כלל הטאבים
            tabControl1.Items.Clear();

            // Add new items
            tabControl1.Items.Add(tabItem5);
            tabControl1.Items.Add(tabItem6);


            LoadDoctors();

            //כפתורים ופקדים
            buttonBank.Visibility = Visibility.Collapsed;
            buttonMovToManager.Visibility = Visibility.Collapsed;
            buttonDistribute.Visibility = Visibility.Collapsed;
            buttonAdvise.Visibility = Visibility.Collapsed;

            currentListView = lv_manager;


        }
        private void SetBankPage()
        {
            _state = State.Bank;

            labelTitle.Content = "בנק המקרים";

            // הסרת כלל הטאבים
            tabControl1.Items.Clear();

            //פתיחת הטאבים הרצויים לעמוד זה
            tabControl1.Items.Add(tabItem2);
            tabControl1.Items.Add(tabItem5);
            tabControl1.Items.Add(tabItem6);

            tabControl1.Items.Remove(tabItem5);
            tabControl1.Items.Remove(tabItem6);


            //כפתורים ופקדים
            buttonBank.Visibility = Visibility.Collapsed;
            buttonDistribute.Visibility = Visibility.Collapsed;
            buttonAdvise.Visibility = Visibility.Collapsed;
            PTGList.Visibility = Visibility.Collapsed;
            buttonBank.Visibility = Visibility.Collapsed;
            buttonDistribute.Visibility = Visibility.Collapsed;
            buttonAdvise.Visibility = Visibility.Collapsed;
            buttonCancelAssociation.Visibility = Visibility.Collapsed;

            buttonMovToPTG.Visibility = Visibility.Visible;
            ORGANList.Visibility = Visibility.Visible;
            buttonAssociation.Visibility = Visibility.Visible;
            buttonMovToManager.Visibility = Visibility.Visible;

            currentListView = lv_All_Cases;

            SaveTime(currentListView.Name);
            LoadOrgans();


        }
        private void SetPTGPage()
        {
            _state = State.Patholog;

            //_uiManager_BuutonsVisiblity();
            labelTitle.Content = "רשימת עבודה";

            // הסרת כלל הטאבים
            tabControl1.Items.Clear();

            // Add new items
            tabControl1.Items.Add(tabItem1);
            tabControl1.Items.Add(tabItem3);
            tabControl1.Items.Add(tabItem4);


            currentListView = lv_my_cases;

            //כפתורים ופקדים
            buttonAssociation.Visibility = Visibility.Collapsed;
            buttonMovToManager.Visibility = Visibility.Collapsed;
            buttonBank.Visibility = Visibility.Visible;
            buttonCancelAssociation.Visibility = Visibility.Collapsed;
            buttonMovToPTG.Visibility = Visibility.Collapsed;
            

            PTGList.Visibility = Visibility.Collapsed;
            ORGANList.Visibility = Visibility.Collapsed;

            SaveTime(currentListView.Name);
        }



        #endregion

        #region Init Tabs Methods

        private void initializeData()
        {
            initDal();


            pathologEntryContainer = new ReContainer();
            pre = pathologEntryContainer.Controls[0] as PathologResultEntryCls;

            //////////Load  in another  thread
            this.Dispatcher.BeginInvoke(

                DispatcherPriority.ContextIdle,
                new Action(delegate ()
                {
                    if (DEBUG)
                    {
                        pre.DEBUG = true;
                        pre.runByWfDebug(sp, loggedInUserName, loggedInUser);

                    }
                    else
                    {
                        pre.runByWf(sp, dal);
                    }

                }));



        }


        private void initDal()
        {
            FunctionsForIcons.dal = dal;
            PatientRow.initDal(dal);
        }







        // this function sets the list of patients to the listview rows. Each patient in a different row.
        private void FillGrid(List<PatientRow> Patients, ListView listView)
        {
            try
            {
                _uiManager.FillGrid(listView.Name);
                GridView grid = listView.View as GridView;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region control's events



        private void _uiManager_BuutonsVisiblity()
        {
            checkboxChecked.Clear();
            AssigningDATA();
            buttonDistribute.Visibility = (currentListView == lv_revision || currentListView == lv_Distribution) ? Visibility.Visible : Visibility.Collapsed;
            buttonSelectSpecificRows.Visibility = (currentListView == lv_my_cases ) ? Visibility.Collapsed : Visibility.Visible;
            buttonAdvise.Visibility = _state == State.Manager ? buttonDistribute.Visibility : Visibility.Collapsed;

            if (_state == State.Manager && currentListView == lv_manager)
            {
                buttonCancelAssociation.Visibility = Visibility.Visible;
            }
            else
            {

                if (_state == State.Manager && currentListView == lv_All_Cases_m)
                    buttonCancelAssociation.Visibility = Visibility.Collapsed;
            }

            rowsCounter = 0;

            buttonSelectAll.Visibility = (currentListView == lv_my_cases ? Visibility.Collapsed : Visibility.Visible);

            if (currentListView == lv_revision)
            {
                buttonDistribute.Content = "סיים ייעוץ";
            }
            if (currentListView == lv_Distribution)
            {

                buttonDistribute.Content = "הפץ";
            }

            if (currentListView == lv_manager)
            {
                PTGList.Visibility = Visibility.Visible;
            }

        }



        #endregion

        #region Button Clicks




        private void buttonOpenPreviewLetter_Click(object sender, RoutedEventArgs e)
        {
            PatientRow item;
            try
            {
                item = currentListView.SelectedItem as PatientRow;
            }
            catch (Exception ex)
            {
                item = null;
            }

            if (item != null)
            {
                var sdg = dal.FindBy<SDG>(s => s.SDG_ID == item.sdgId).FirstOrDefault();
                if (sdg != null)
                {
                    pre.runPreviewLetter(sdg);
                }
                else
                {
                    MessageBox.Show(string.Format("Unable to find sdg (sdg_id: {0}).", item.sdgId));
                }

            }
        }


        private void buttonDistribute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button != null)
                {
                    if (button.Content.ToString().Contains("הפץ"))
                        distribute();
                    else
                    {
                        finishAdvise();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonRefresh_Click(object sender, RoutedEventArgs e)
        {

            ReloadData(currentListView.Name);
        }




        // open pathologResultEntry for the clicked row (clicked patient)
        protected void HandlePatientDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {

                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                Mouse.OverrideCursor = Cursors.Wait;
                var item = sender as ListViewItem;
                PatientRow patient = null;

                if (item != null && item.IsSelected)
                {
                    patient = item.Content as PatientRow;
                }


                if (patient != null)
                {
                    if (pathologEntryContainer.Controls.Count == 1)
                    {


                        forms.Form fullScreenForm = new forms.Form();
                        pathologEntryContainer.WindowState = forms.FormWindowState.Maximized;
                        pre.Dock = forms.DockStyle.Fill;
                        pathologEntryContainer.Controls.Add(pre);



                        pre.setSnomed();
                        pre.LoadPatient(patient.sdgName, dal);

                        Mouse.OverrideCursor = null;
                        watch.Stop();

                        Logger.WriteLogFile("Execution Time on pathologEntryContainer: \n  Milliseconds:" + watch.ElapsedMilliseconds + "\n Seconds:" + watch.ElapsedMilliseconds / 1000 + "Test");
                        pathologEntryContainer.ShowDialog();
                        //close prient
                        Logger.WriteLogFile("close patient file start");
                        pre.Open_Patient_File(loggedInUserName, null);
                        //Reload data from DB
                        ReloadData(currentListView.Name);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        List<PatientRow> checkboxChecked = new List<PatientRow>();

        int rowsCounter = 0;



        #endregion

        private void distribute()
        {
            try
            {
                if (DEBUG)
                {
                    MessageBox.Show("Distribution is not allowed on debug mode. ", "ccc", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }

                //בדיקה האם נבחרו מקרים
                if (checkboxChecked.Count <= 0)
                {
                    forms.MessageBox.Show("חובה לבחור מקרים ");
                }
                else
                {
                    int countDistributions = 0;
                    //מעבר על כל האובייקטים בשורות המסומנות
                    foreach (PatientRow pr in checkboxChecked)
                    {
                        try
                        {
                            //ביצוע השינוי הדרוש ושמירת הנתונים
                            FireEventXmlHandler authorizeSdg = new FireEventXmlHandler(sp, "Set SDG Status");
                            authorizeSdg.CreateFireEventXml("SDG", pr.sdgId, "ToAuthoriseDoctor");
                            bool res = authorizeSdg.ProcssXml();
                            //בדיקה האם בוצעו הפעולות הנדרשות
                            if (!res)
                            {
                                MessageBox.Show(authorizeSdg.ErrorResponse);
                                continue;
                            }
                            else
                            {
                                countDistributions++;

                                var sdgToChange = dal.FindBy<SDG>(sdg => sdg.SDG_ID == pr.sdgId).FirstOrDefault();

                                if (sdgToChange != null)
                                {
                                    sdgToChange.SDG_USER.U_WEEK_NBR = 999;
                                    dal.InsertToSdgLog(pr.sdgId, "PTG.WL_D", !DEBUG ? (long)ntlsCon.GetSessionId() : 1, "מסך רשימת עבודה - הפצה");
                                    dal.SaveChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("error while distributing:" + Environment.NewLine + ex.Message);
                        }

                    }

                    //בדיקה האם בוצעו הפעולות הנדרשות
                    if (countDistributions > 0)
                    {

                        MessageBox.Show(string.Format("{0} {1}.", countDistributions, countDistributions > 1 ? "rows were distributed" : "row was distributed"));
                        ReloadData(currentListView.Name);
                    }

                }
                //ניקוי רשימת האובייקטים המסומנים
                checkboxChecked.Clear();
                AssigningDATA();


            }
            catch (Exception ex)
            {
                checkboxChecked.Clear();
                AssigningDATA();
                MessageBox.Show(ex.Message);
            }
        }


        private string selectedOrgan;

        private void CmbOrgan_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            selectedOrgan = CmbOrgan.SelectedItem as string;

            LoadData(currentListView.Name, selectedOrgan);
        }

        private void LoadDoctors()
        {

            var qDoctors =
             dal.FindBy<OPERATOR>(o => (o.LIMS_ROLE.NAME == "DOCTOR" || o.LIMS_ROLE.NAME == "Cytoscreener") && o.OPERATOR_USER.U_IS_DIGITAL_PATHOLOG == "T")
                              .Include(a => a.LIMS_ROLE)
                                .Include(x => x.OPERATOR_USER).OrderBy(x => x.NAME);

            var Doctors = qDoctors.ToList();//.First().OPERATOR_USER.U_HEBREW_NAMEOPERATOR_ID;



            CmbPatholog.DisplayMemberPath = "FULL_NAME";
            CmbPatholog.SelectedValuePath = "OPERATOR_ID";
            CmbPatholog.ItemsSource = Doctors;
            CmbPatholog.SelectedIndex = -1;


        }

        //AE CODE
        private OPERATOR selectedOperator;


        private void CmbPatholog_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            selectedOperator = CmbPatholog.SelectedItem as OPERATOR;
        }

        private void finishAdvise()
        {

            try
            {

                loggedInUser = !DEBUG ? Convert.ToInt64(ntlsUser.GetOperatorId()) : 1;
                loggedInUserName = !DEBUG ? ntlsUser.GetOperatorName().Trim() : "lims_sys";

                List<U_EXTRA_REQUEST_DATA_USER> consultations = null;
                int counter = 0;

                //בדיקה האם נבחרו מקרים
                if (checkboxChecked.Count <= 0)
                {
                    forms.MessageBox.Show("חובה לבחור מקרים ");
                }
                else
                {
                    //מעבר על רשימת האובייקטים המסומנים
                    foreach (PatientRow item in checkboxChecked)
                    {

                        if (DEBUG)
                        {
                            consultations = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(a => a.U_SLIDE_NAME != null && a.U_SLIDE_NAME.Substring(0, 1).Equals("B") &&
                                        a.U_SLIDE_NAME.Substring(0, 10).Equals(item.sdgName) && a.U_EXTRA_REQUEST.NAME.Contains("Consult") &&
                                        a.U_STATUS.Equals("V") && a.U_REQUEST_DETAILS.Trim().Contains(loggedInUserFullName)).ToList();
                        }
                        else
                        {
                            if (_state == State.Manager)
                            {
                                consultations = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(a => a.U_SLIDE_NAME != null && a.U_SLIDE_NAME.Substring(0, 1).Equals("B") &&
                                a.U_SLIDE_NAME.Substring(0, 10).Equals(item.sdgName) && a.U_EXTRA_REQUEST.NAME.Contains("Consult") &&
                                a.U_STATUS.Equals("V")).ToList();
                            }
                            else
                            {
                                //מה זה בא לעשות?
                                if (_state == State.Bank)
                                {
                                    var currentSDG = dal.FindBy<SDG>(x => x.NAME == item.PatholabNum);
                                    var currentSDG_USER = dal.FindBy<SDG_USER>(a => a.SDG_ID == currentSDG.FirstOrDefault().SDG_ID);
                                    currentSDG_USER.FirstOrDefault().U_PATHOLOG = loggedInUser;
                                }
                                else
                                {
                                    consultations = dal.FindBy<U_EXTRA_REQUEST_DATA_USER>(a => a.U_SLIDE_NAME != null && a.U_SLIDE_NAME.Substring(0, 1).Equals("B") &&
                                        a.U_SLIDE_NAME.Substring(0, 10).Equals(item.sdgName) && a.U_EXTRA_REQUEST.NAME.Contains("Consult") &&
                                        a.U_STATUS.Equals("V") && a.U_REQUEST_DETAILS.Trim().Contains(loggedInUserFullName)).ToList();
                                }
                            }
                            if (consultations != null && consultations.Count > 0)
                            {
                                foreach (var item1 in consultations)
                                {
                                    item1.U_STATUS = "L";
                                    item1.U_LAB_ON = DateTime.Now;
                                    var id = item1.U_EXTRA_REQUEST_ID;
                                    var sdg_id = dal.FindBy<U_EXTRA_REQUEST_USER>(a => a.U_EXTRA_REQUEST_ID == id).FirstOrDefault().U_SDG_ID;
                                    long sdg_id1 = sdg_id.Value;
                                    dal.InsertToSdgLog(sdg_id1, "PTG.CC", !DEBUG ? (long)ntlsCon.GetSessionId() : 1, "מסך פתולוג - סיום התייעצות");
                                    dal.SaveChanges();
                                    counter++;
                                }

                            }
                        }


                    }
                    forms.MessageBox.Show(counter + " התייעצויות הסתיימו");
                    ReloadData(currentListView.Name);
                    checkboxChecked.Clear();
                    AssigningDATA();
                }


            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void ReloadData(string name)
        {            
            _uiManager.ClearData();

            //Reload data from DB
            LoadData();
            SaveTime(name);
            ChangeRowsNum(name);
        }

        #region Sort methods

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection;
        void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked.Content.ToString() == "שייך אלי" || headerClicked.Content.ToString() == "שייך לפתולוג")
                    {
                        // Skip the sorting logic for this column
                        return;
                    }
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string sortBy = headerClicked.Tag.ToString();

                    Sort(sortBy, direction, currentListView);

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction, ListView listView)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView.ItemsSource);
            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
        #endregion

        #region filter methods

        Dictionary<GridViewColumnHeader, string> dictFilter = new Dictionary<GridViewColumnHeader, string>();
        GridViewColumnHeader currentFilteredHeader;
        formFilter filterForm;
        CollectionView view;
        string txtFilter = string.Empty;
        bool hasFilter = false;
        private void GridViewColumnHeader_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            currentFilteredHeader = sender as GridViewColumnHeader;
            displayContextMenu();
        }

        private void displayContextMenu()
        {
            try
            {
                forms.ContextMenuStrip contexMenu = new forms.ContextMenuStrip();
                forms.ToolStripItem itemAddFiler;
                forms.ToolStripItem itemRemoveFilter;

                if (!hasFilter)
                {
                    itemAddFiler = contexMenu.Items.Add("Filter");
                    itemAddFiler.Click += new EventHandler(addFilter);
                }
                else
                {
                    itemAddFiler = contexMenu.Items.Add("Add Another Filter");
                    itemAddFiler.Click += new EventHandler(addFilter);
                    itemRemoveFilter = contexMenu.Items.Add("Remove Filter");
                    itemRemoveFilter.Click += new EventHandler(removeFilter);
                }

                contexMenu.Show(forms.Cursor.Position);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool UserFilter(object item)
        {
            try
            {
                if (String.IsNullOrEmpty(textBoxScan.Text))
                    return true;
                else
                    return ((item as PatientRow).PatholabNum.IndexOf(textBoxScan.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private void addFilter(object sender, EventArgs e)
        {
            try
            {


                using (filterForm = new formFilter(currentFilteredHeader.Content as string))
                {
                    forms.DialogResult result = filterForm.ShowDialog();
                    if (result == forms.DialogResult.OK)
                    {
                        txtFilter = filterForm.filterSentence;

                        try
                        {
                            if (!dictFilter.ContainsKey(currentFilteredHeader))
                            {
                                dictFilter.Add(currentFilteredHeader, txtFilter);
                            }
                            else
                            {
                                dictFilter[currentFilteredHeader] = txtFilter;
                            }
                        }
                        catch (Exception)
                        {
                        }

                        view = (CollectionView)CollectionViewSource.GetDefaultView(currentListView.ItemsSource);
                        view.Filter = UserFilter;
                        hasFilter = true;

                        var item = sender as forms.ToolStripItem;
                        if (item != null)
                        {
                            item.Click -= addFilter;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void removeFilter(object sender, EventArgs e)
        {
            try
            {


                view = (CollectionView)CollectionViewSource.GetDefaultView(currentListView.ItemsSource);
                view.Filter = clearFilter;
                hasFilter = false;
                dictFilter.Clear();

                var item = sender as forms.ToolStripItem;
                if (item != null)
                {
                    item.Click -= removeFilter;
                }

                clearScanControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // this function will iterate on all rows of the listview and because it is returning true - all rows will be visible.
        private bool clearFilter(object item)
        {
            return true;
        }


        #endregion

        private void listView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                ListView lv = sender as ListView;
                double width = currentListView.ActualWidth;
                var columns = getCurrentGridViewColumns(lv);
                double widthPerColumn = width / (columns.Count() > 0 ? columns.Count() : 1);
                foreach (var header in columns)
                {
                    header.Width = widthPerColumn;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private System.Windows.Controls.GridViewColumnCollection getCurrentGridViewColumns(ListView listView)
        {
            GridView grid = listView.View as GridView;
            GridViewColumnHeader col = grid.Columns[0].Header as GridViewColumnHeader;

            if (grid != null)
            {
                return grid.Columns;
            }

            return null;
        }

        private void textBoxScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                buttonScan_Click(null, null);
            }
        }


        System.Drawing.Color redColor = System.Drawing.Color.Red;
        private void buttonScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                view = (CollectionView)CollectionViewSource.GetDefaultView(currentListView.ItemsSource);
                view.Filter = UserFilter;
                CollectionViewSource.GetDefaultView(currentListView.ItemsSource).Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void clearScanControls()
        {
            textBoxScan.Text = string.Empty;
        }







        List<PatientRow> GeneralPR_list = new List<PatientRow>();
        List<PatientRow> MyCasesList = new List<PatientRow>();
        private int numOfRowsMC_list;
        List<PatientRow> AllCasesList = new List<PatientRow>();
        private int numOfRowsAC_list;
        List<PatientRow> RevisionsList = new List<PatientRow>();
        private int numOfRowsR_list;
        List<PatientRow> DistributionsList = new List<PatientRow>();
        private int numOfRowsD_list;
        List<PatientRow> ManagerList = new List<PatientRow>();
        private int numOfRowsM_list;

        List<PatientRow> OrganList = new List<PatientRow>();
        private int numOfRowsChecked;

        private void SaveTime(string key)
        {
            rowsCounter = 0;
            _uiManager.LoadSingleTab(key);
        }

        //טעינה מהDB



        private void LoadData()
        {
            Logger.WriteLogFile("Start Load Data");

            GeneralPR_list = new List<PatientRow>();

            var query = "select * from lims.PATHOLOG_WORKLIST";

            GeneralPR_list = dal.FetchDataFromDB(query, reader =>
            {
                return new PatientRow
                {
                    sdgId = Convert.ToInt32(reader["SDG_ID"]),
                    sdgName = reader["SDGNAME"] != DBNull.Value ? reader["SDGNAME"].ToString() : null,
                    FullPtientName = reader["PTIENT_NAME"] != DBNull.Value ? reader["PTIENT_NAME"].ToString() : null,
                    status = reader["SDGSTATUS"] != DBNull.Value ? reader["SDGSTATUS"].ToString() : null,
                    ShouldDistribute = reader["U_WEEK_NBR"] != DBNull.Value && (Convert.ToInt32(reader["U_WEEK_NBR"]) == 907 || Convert.ToInt32(reader["U_WEEK_NBR"]) == 909 || Convert.ToInt32(reader["U_WEEK_NBR"]) == 908),
                    Scanned_on = reader["SCANNED_ON"] != DBNull.Value ? Convert.ToDateTime(reader["SCANNED_ON"]) : (DateTime?)null,
                    U_WEEK_NBR = reader["U_WEEK_NBR"] != DBNull.Value ? Convert.ToDecimal(reader["U_WEEK_NBR"]) : (decimal?)null,
                    InConsult = reader["IS_ADVISOR"] != DBNull.Value && reader["IS_ADVISOR"].ToString().Equals("T") ? true : false,
                    isDigit = reader["IS_DIGITAL"] != DBNull.Value ? reader["IS_DIGITAL"].ToString() : null,
                    SentToConsultationIcon = reader["IS_ADVISOR"] != DBNull.Value && reader["IS_ADVISOR"].ToString().Equals("T") && reader["U_PATHOLOG"] != DBNull.Value && Convert.ToDecimal(reader["U_PATHOLOG"]) == loggedInUser ? true : false,
                    FinishToConsultationIcon = reader["HAS_FINISH_ADVISE"] != DBNull.Value && reader["HAS_FINISH_ADVISE"].ToString().Equals("T") && reader["U_PATHOLOG"] != DBNull.Value && Convert.ToDecimal(reader["U_PATHOLOG"]) == loggedInUser ? true : false,
                    PriorityNumber = reader["U_PRIORITY"] != DBNull.Value ? Convert.ToDecimal(reader["U_PRIORITY"]) : (decimal?)null,
                    PriorityTxt = reader["PRIO_TXT"] != DBNull.Value ? reader["PRIO_TXT"].ToString() : null,
                    PathologId = reader["U_PATHOLOG"] != DBNull.Value ? Convert.ToInt32(reader["U_PATHOLOG"]) : (int?)null,
                    Patholog = reader["PATHOLOG_FULL_NAME"] != DBNull.Value ? reader["PATHOLOG_FULL_NAME"].ToString() : null,
                    Date = reader["U_SEND_ON"] != DBNull.Value ? Convert.ToDateTime(reader["U_SEND_ON"]) : (DateTime?)null,
                    PatholabNum = reader["U_PATHOLAB_NUMBER"] != DBNull.Value ? reader["U_PATHOLAB_NUMBER"].ToString() : null,
                    ClinicalDiagnosis = reader["DIAGNOSIS"] != DBNull.Value ? reader["DIAGNOSIS"].ToString() : null,
                    AllOrgans = reader["ORGANS"] != DBNull.Value ? reader["ORGANS"].ToString() : null,
                    NumBlocks = reader["NUM_OF_BLOCKS"] != DBNull.Value ? Convert.ToInt32(reader["NUM_OF_BLOCKS"]) : (int?)null,
                    Colors = reader["COLORS"] != DBNull.Value ? reader["COLORS"].ToString() : null,
                    NumColors = reader["NUM_OF_COLORS"] != DBNull.Value ? Convert.ToInt32(reader["NUM_OF_COLORS"]) : (int?)null,
                    hasRemarksIcon = reader["HAS_REMARKS"] != DBNull.Value && reader["HAS_REMARKS"].ToString().Equals("T") ? true : false,
                    hasMultipleIcon = reader["HAS_MULTIPLE"] != DBNull.Value && reader["HAS_MULTIPLE"].ToString().Equals("T") ? true : false,
                    hasSecondInspectionIcon = reader["HAS_SECOND_INSPECTION"] != DBNull.Value && reader["HAS_SECOND_INSPECTION"].ToString().Equals("T") ? true : false,
                    hasExtraRequestsIcon = reader["HAS_RESCAN"] != DBNull.Value && reader["HAS_RESCAN"].ToString().Equals("T") ? true : false,
                    hasFinishExtraRequestsIcon = reader["HAS_FINISH_RESCAN"] != DBNull.Value && reader["HAS_FINISH_RESCAN"].ToString().Equals("T") ? true : false,
                    hasColorsIcon = reader["HAS_COLORS"] != DBNull.Value && reader["HAS_COLORS"].ToString().Equals("T") ? true : false,
                    hasFinishColorsIcon = reader["HAS_FINISH_COLORS"] != DBNull.Value && reader["HAS_FINISH_COLORS"].ToString().Equals("T") ? true : false,
                    GlassType = reader["U_GLASS_TYPE"] != DBNull.Value ? Convert.ToDecimal(reader["U_GLASS_TYPE"]) : (decimal?)null,
                    Revision = reader["REVISION"] != DBNull.Value ? reader["REVISION"].ToString() : null,
                    ADVISOROPERATOR = reader["ADVISOROPERATOR"] != DBNull.Value ? reader["ADVISOROPERATOR"].ToString() : null,
                    hasSlidedsScannedIcon = reader["HAS_SLIDES_SCANED"] != DBNull.Value && reader["HAS_SLIDES_SCANED"].ToString().Equals("T") ? true : false,
                    hasExtraMaterialIcon = reader["HAS_EXTRA_MATERIAL"] != DBNull.Value && reader["HAS_EXTRA_MATERIAL"].ToString().Equals("T") ? true : false,
                    hasFinishExtraMaterialIcon = reader["HAS_FINISH_EXTRA_MATERIAL"] != DBNull.Value && reader["HAS_FINISH_EXTRA_MATERIAL"].ToString().Equals("T") ? true : false,
                    RemarksRequest = reader["REMARKSREQUESTS"] != DBNull.Value ? reader["REMARKSREQUESTS"].ToString() : null,
                    unscanned_slides_number = reader["UNSCANNED_SLIDES_NUMBER"] != DBNull.Value ? Convert.ToInt32(reader["UNSCANNED_SLIDES_NUMBER"]) : (int?)null,
                    empty_blocks_number = reader["EMPTY_BLOCKS_NUMBER"] != DBNull.Value ? Convert.ToInt32(reader["EMPTY_BLOCKS_NUMBER"]) : (int?)null,
                    HasAnyAttachdDocs = reader["HAS_ATTACHED_DOCS"] != DBNull.Value && reader["HAS_ATTACHED_DOCS"].ToString().Equals("T") ? true : false
                };
            });

            Logger.WriteLogFile($"End Load Data {GeneralPR_list.Count()} rows in list");

            AssigningDATA();

        }
        private void AssigningDATA()
        {

            //מנהל
            ManagerList = GeneralPR_list.Where(x => x.PathologId != null && x.isDigit == "T" && x.status != "A").OrderBy(patient => patient.PathologId).ThenBy(patient => patient.Date).ToList();
            numOfRowsM_list = ManagerList.Count;
            //בנק מקרים

            //debug mode
            AllCasesList = GeneralPR_list.Where(x => x.PathologId == null).OrderBy(patient => patient.Date).ToList();

            //real mode 
            //AllCasesList = GeneralPR_list.Where(x => x.PathologId == null && x.unscanned_slides_number == 0 && x.HasAnyAttachdDocs == true && x.ClinicalDiagnosis != string.Empty).OrderBy(patient => patient.Date).ToList();
            numOfRowsAC_list = AllCasesList.Count;
            //פתולוג משויך

            MyCasesList = GeneralPR_list.Where(x => x.PathologId == loggedInUser && (x.ShouldDistribute == false)).OrderBy(patient => patient.Date).ToList();
            numOfRowsMC_list = MyCasesList.Count;
            RevisionsList = GeneralPR_list.Where(x => (x.PathologId == loggedInUser && x.Revision != null && !x.shouldDistribute) || (x.InConsult && x.ADVISOROPERATOR == loggedInUserFullName)).OrderBy(patient => !patient.InConsult).ThenBy(patient => patient.Date).ToList();
            numOfRowsR_list = RevisionsList.Count;
            DistributionsList = GeneralPR_list.Where(x => x.PathologId == loggedInUser && x.ShouldDistribute).OrderBy(patient => patient.Date).ToList();
            numOfRowsD_list = DistributionsList.Count;

            _uiManager.AssignData(lv_Distribution.Name, DistributionsList);
            _uiManager.AssignData(lv_All_Cases.Name, AllCasesList);
            _uiManager.AssignData(lv_revision.Name, RevisionsList);
            _uiManager.AssignData(lv_my_cases.Name, MyCasesList);
            _uiManager.AssignData(lv_manager.Name, ManagerList);
            _uiManager.AssignData(lv_All_Cases_m.Name, AllCasesList);

        }

        private void LoadData(string key, string organ)
        {

            if (organ.Equals("כלל האיברים"))
            {
                switch (key)
                {
                    case "lv_All_Cases":
                        {
                            _uiManager.AssignData(lv_All_Cases.Name, AllCasesList);
                            break;
                        }
                    case "lv_manager":
                        {
                            _uiManager.AssignData(lv_manager.Name, ManagerList);
                            break;
                        }
                    case "lv_All_Cases_m":
                        {
                            _uiManager.AssignData(lv_All_Cases_m.Name, AllCasesList);
                            break;
                        }
                }

            }
            else
            {
                switch (key)
                {
                    case "lv_All_Cases":
                        {
                            OrganList = AllCasesList.Where(x => (x.FirstOrgan != null && x.FirstOrgan.Equals(organ)) && x.GlassType.HasValue).OrderBy(patient => patient.Date).ToList();
                            _uiManager.AssignData(lv_All_Cases.Name, OrganList);
                            break;
                        }
                    case "lv_All_Cases_m":
                        {
                            OrganList = AllCasesList.Where(x => (x.FirstOrgan != null && x.FirstOrgan.Equals(organ)) && x.GlassType.HasValue).OrderBy(patient => patient.Date).ToList();
                            _uiManager.AssignData(lv_All_Cases_m.Name, OrganList);
                            break;
                        }
                    case "lv_manager":
                        {
                            OrganList = ManagerList.Where(x => (x.FirstOrgan != null && x.FirstOrgan.Equals(organ)) && x.GlassType.HasValue).OrderBy(patient => patient.Date).ToList();
                            _uiManager.AssignData(lv_manager.Name, OrganList);
                            break;
                        }
                }

            }


            _uiManager.LoadSingleTab(key);

            Logger.WriteLogFile("End Load Data");


        }



        public void CloseQuery()
        {
            try
            {
                pre.Leave_Context();
            }
            catch (Exception e)
            {
                Logger.WriteLogFile("error in close query: " + e.Message);
            }
        }

        private void lv_my_cases_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void lv_revision_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }


        private void headerPriopity2_Click(object sender, RoutedEventArgs e)
        {

        }
        private void buttonMovToPTG_Click(object sender, RoutedEventArgs e)
        {

            SetPTGPage();

        }
        private void buttonMovToM_Click(object sender, RoutedEventArgs e)
        {
            if (chkPermission())
            {
                SetManagerPage();
            }
            else
            {
                forms.MessageBox.Show("גישה מוגבלת");
            }
        }
        private void buttonBank_Click(object sender, RoutedEventArgs e)
        {
            SetBankPage();
        }

        private void ManagerAssociation()
        {
            int counter = 0;

            try
            {

                //בדיקה האם נבחרו מקרים
                if (checkboxChecked.Count <= 0 || selectedOperator == null)
                {
                    forms.MessageBox.Show("חובה לבחור פתולוג ומקרים לשיוך ");
                }
                else
                {
                    //מעבר על כל האובייקטים בשורות המסומנות
                    foreach (PatientRow item in checkboxChecked)
                    {
                        var currentSdg = dal.FindBy<SDG>(a => a.SDG_ID == item.sdgId).FirstOrDefault();
                        //ביצוע השינוי הדרוש ושמירת הנתונים

                        var currentSdgUser = currentSdg.SDG_USER;
                        currentSdgUser.U_PATHOLOG = selectedOperator.OPERATOR_ID;

                        //שמירת תאריך השליחה בכל הסליידים
                        var currentSdgSampleList = currentSdg.SAMPLEs;
                        foreach (var sample in currentSdgSampleList)
                        {
                            var currentSdgAliquotsList = sample.ALIQUOTs;
                            foreach (var aliquot in currentSdgAliquotsList)
                            {
                                aliquot.ALIQUOT_USER.U_SEND_TO_PATHOLOG_ON = DateTime.Now;
                            }
                        }

                        var msg = $"מסך מנהל - שיוך ל {selectedOperator.FULL_NAME}";
                        dal.InsertToSdgLog(currentSdg.SDG_ID, "PTG.MA", !DEBUG ? (long)ntlsCon.GetSessionId() : 1, msg);
                        dal.SaveChanges();
                        counter++;


                    }

                    MessageBox.Show(counter + " שיוכים בוצעו בהצלחה");

                    ReloadData(currentListView.Name);

                    //ניקוי רשימת האובייקטים המסומנים
                    checkboxChecked.Clear();
                    UpdateCheckedRowsCount(0);
                    AssigningDATA();
                }

            }
            catch (Exception a)
            {
                checkboxChecked.Clear();
                UpdateCheckedRowsCount(0);
                AssigningDATA();
                forms.MessageBox.Show("error in manager association");
                Logger.WriteLogFile(a);
            }
        }
        //AE CODE   
        private void BankAssociation()
        {
            int counter = 0;
            try
            {
                var loggedInUser = !DEBUG ? Convert.ToInt64(ntlsUser.GetOperatorId()) : 1;
                string loggedInUserName = !DEBUG ? ntlsUser.GetOperatorName().Trim() : "lims_sys";
                //בדיקה האם נבחרו מקרים
                if (checkboxChecked.Count <= 0)
                {
                    forms.MessageBox.Show("חובה לבחור מקרים ");
                }
                else
                {
                    //מעבר על כל האובייקטים בשורות המסומנות
                    foreach (PatientRow item in checkboxChecked)
                    {
                        //ביצוע השינוי הדרוש ושמירת הנתונים
                        var currentSDG = dal.FindBy<SDG>(a => a.SDG_ID == item.sdgId).FirstOrDefault();
                        //שיוך לפתולוג
                        var currentSDG_USER = currentSDG.SDG_USER;
                        currentSDG_USER.U_PATHOLOG = loggedInUser;

                        //שמירת תאריך השליחה בכל הסליידים
                        var currentSDG_sampleList = currentSDG.SAMPLEs;
                        foreach (var sample in currentSDG_sampleList)
                        {
                            var currentSDG_aliquotsList = sample.ALIQUOTs;
                            foreach (var aliquot in currentSDG_aliquotsList)
                            {
                                aliquot.ALIQUOT_USER.U_SEND_TO_PATHOLOG_ON = DateTime.Now;
                            }
                        }
                        var msg = $"בנק המקרים - שיוך ל {loggedInUserName}";
                        dal.InsertToSdgLog(currentSDG.SDG_ID, "PTG.BA", !DEBUG ? (long)ntlsCon.GetSessionId() : 1, msg);
                        dal.SaveChanges();
                        counter++;
                    }
                    //בדיקה האם בוצעו הפעולות הנדרשות
                    if (counter == checkboxChecked.Count)
                    {
                        MessageBox.Show(counter + " שיוכים בוצעו בהצלחה");
                        ReloadData(currentListView.Name);
                    }

                    //ניקוי רשימת האובייקטים המסומנים
                    checkboxChecked.Clear();
                    UpdateCheckedRowsCount(0);
                    AssigningDATA();
                }
            }

            catch (Exception ex)
            {
                checkboxChecked.Clear();
                UpdateCheckedRowsCount(0);
                AssigningDATA();
                forms.MessageBox.Show("error");
                Logger.WriteLogFile(ex);
            }
        }

        private void ManagerAssociationCanceling()
        {
            int counter = 0;

            try
            {

                //בדיקה האם נבחרו מקרים
                if (checkboxChecked.Count <= 0)
                {
                    forms.MessageBox.Show("חובה לבחור מקרים ");
                }
                else
                {
                    //מעבר על כל האובייקטים בשורות המסומנות
                    foreach (PatientRow item in checkboxChecked)
                    {
                        var currentSDG = dal.FindBy<SDG>(a => a.SDG_ID == item.sdgId).FirstOrDefault();
                        //ביצוע השינוי הדרוש ושמירת הנתונים

                        var currentSDG_USER = currentSDG.SDG_USER;
                        currentSDG_USER.U_PATHOLOG = null;
                        var msg = $"מסך מנהל - ביטול שיוך ל {currentSDG_USER.PATHOLOG.FULL_NAME}";
                        dal.InsertToSdgLog(currentSDG.SDG_ID, "PTG.MCA", !DEBUG ? (long)ntlsCon.GetSessionId() : 1, msg);
                        dal.SaveChanges();
                        counter++;

                    }
                    //בדיקה האם בוצעו הפעולות הנדרשות

                    MessageBox.Show(counter.ToString() + " שיוכים בוטלו");
                    ReloadData(currentListView.Name);

                    //ניקוי רשימת האובייקטים המסומנים
                    checkboxChecked.Clear();
                    UpdateCheckedRowsCount(0);
                    AssigningDATA();
                }

            }
            catch (Exception a)
            {
                checkboxChecked.Clear();
                UpdateCheckedRowsCount(0);
                AssigningDATA();
                forms.MessageBox.Show("error in manager association canceling");
                Logger.WriteLogFile(a);
            }
        }

        private void btnCancelAssociation_Click(object sender, RoutedEventArgs e)
        {

            if (_state == State.Manager)
            {
                ManagerAssociationCanceling();
            }

        }
        private void buttonAssociation_Click(object sender, RoutedEventArgs e)
        {

            if (_state == State.Bank)
            {
                BankAssociation();
            }
            else
            {
                if (_state == State.Manager)
                {
                    ManagerAssociation();
                }
            }

        }


        private void tabControl1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TabControl tabControl = e.Source as TabControl;

            if (tabControl != null)
            {
                var tabItem = tabControl.SelectedItem as TabItem;
                if (tabItem != null)
                {
                    var listView = tabItem.Content as ListView;
                    if (listView != null)
                    {
                        currentListView = listView;
                        _uiManager_BuutonsVisiblity();
                        _uiManager.LoadSingleTab(listView.Name);
                        ChangeRowsNum(listView.Name);
                        buttonUnselectAll_Click(null, null);

                    }
                }
            }

        }

        private List<PatientRow> selectedPatientRows = new List<PatientRow>();
        private double _session_id;

        public static string callingClass { get; private set; }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (PatientRow item in e.RemovedItems)
                selectedPatientRows.Remove(item);

            foreach (PatientRow item in e.AddedItems)
                selectedPatientRows.Add(item);
        }

        private void buttonSelectSpecificRows_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in selectedPatientRows)
            {
                if (!checkboxChecked.Contains(item))
                {
                    checkboxChecked.Add(item);
                    item.IsChecked = true;
                    numOfRowsChecked += 1;
                }
            }
            selectedPatientRows.Clear();

            UpdateCheckedRowsCount(numOfRowsChecked);
            AssigningDATA();
            SaveTime(currentListView.Name);
        }

        private void buttonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<PatientRow> rows = _uiManager.GetRows(currentListView.Name);
                checkboxChecked = rows;
                UpdateCheckedRowsCount(rows.Count);

                foreach (var item in _uiManager.GetRows(currentListView.Name))
                    item.IsChecked = true;

                AssigningDATA();
                SaveTime(currentListView.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void buttonUnselectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateCheckedRowsCount(0);
                checkboxChecked.Clear();
                selectedPatientRows.Clear();
                foreach (var item in _uiManager.GetRows(currentListView.Name))
                    item.IsChecked = false;
                AssigningDATA();
                SaveTime(currentListView.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateCheckedRowsCount(int count)
        {
            numOfRowsChecked = count;
            lblCheckedRowsNum.Content = count > 0 ? $"נבחרו {count} מתוך" : string.Empty;
        }
        private void ChangeRowsNum(string name)
        {
            string s = " מקרים ";

            numOfRowsAC_list = currentListView.Items.Count;

            switch (name)
            {
                case "lv_manager":
                    {
                        lblRowsNum.Content = numOfRowsM_list + s;
                        break;
                    }
                case "lv_All_Cases_m":
                    {
                        lblRowsNum.Content = numOfRowsAC_list + s;
                        break;
                    }
                case "lv_All_Cases":
                    {
                        lblRowsNum.Content = numOfRowsAC_list + s;
                        break;
                    }
                case "lv_my_cases":
                    {
                        lblRowsNum.Content = numOfRowsMC_list + s;
                        break;
                    }
                case "lv_Distribution":
                    {
                        lblRowsNum.Content = numOfRowsD_list + s;
                        break;
                    }
                case "lv_revision":
                    {
                        lblRowsNum.Content = numOfRowsR_list + s;
                        break;
                    }
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

            CheckBox checkBox = (CheckBox)sender;
            PatientRow item = (PatientRow)checkBox.DataContext;
            item.IsChecked = true;

            checkboxChecked.Add(item);
            numOfRowsChecked += 1;
            UpdateCheckedRowsCount(numOfRowsChecked);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            PatientRow item = (PatientRow)checkBox.DataContext;
            item.IsChecked = false;

            checkboxChecked.Remove(item);
            numOfRowsChecked -= 1;
            UpdateCheckedRowsCount(numOfRowsChecked);
        }

        private void lv_manager_MouseMove(object sender, MouseEventArgs e)
        {
            GridViewRowPresenter presenter = FindVisualParent<GridViewRowPresenter>(e.OriginalSource as DependencyObject);

            if (presenter != null)
            {
                int rowIndex = currentListView.Items.IndexOf(presenter.DataContext);

                Point relativePosition = e.GetPosition(presenter);
                double left = 0;

                for (int i = 0; i < presenter.Columns.Count; i++)
                {
                    GridViewColumn column = presenter.Columns[i];
                    if (relativePosition.X >= left && relativePosition.X < left + column.ActualWidth)
                    {
                        // Found the column based on mouse position
                        int columnIndex = i;
                        var rowData = (PatientRow)currentListView.Items[rowIndex];
                        switch (columnIndex)
                        {
                            case 7:
                                {
                                    rowData.ToolTipDetails = rowData.ClinicalDiagnosis;
                                    break;
                                }

                            default:
                                {
                                    rowData.ToolTipDetails = null;
                                    break;
                                }
                        }
                        Console.WriteLine($"Mouse over Row: {rowIndex}, Column: {columnIndex}");
                        return;
                    }
                    left += column.ActualWidth;
                }
            }
        }
        private T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T parent)
                {
                    return parent;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }


    }
    public class WordCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string content)
            {
                string[] words = content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return words.Length > 6;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}





