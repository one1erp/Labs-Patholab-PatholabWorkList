//using NautToEytan;
using Patholab_Common;
using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace PatholabWorkList
{
    public class UIManager
    {
        Dictionary<string, TabWrapper> _tabs;
        private Style style;
        public event Action<ListView> BuutonsVisiblity;

        public UIManager(TabControl tabControl, DataLayer dal, Style styl)
        {
            this.style = styl;
             _tabs = new Dictionary<string, TabWrapper>();
            foreach (var tab in tabControl.Items.OfType<TabItem>())
            {
                var listview = tab.Content as ListView;
                if (listview != null)
                {

                    if (!_tabs.ContainsKey(listview.Name))
                    {
                        _tabs.Add(listview.Name, new TabWrapper(listview));
                    }
                }

            }
        }


        internal void AssignData(string name, List<PatientRow> pr)
        {

            try
            {

                _tabs[name].paitentRows = pr;
                foreach (PatientRow patient in pr)
                {
                    patient.setCheckBox(style);
                }
                if (BuutonsVisiblity != null)
                {
                    BuutonsVisiblity(_tabs[name].lv);
                }
            }
            catch (Exception e)

            {
            }


        }
        internal void FillGrid(string name)
        {
            _tabs[name].FillGrid();

        }




        internal void LoadSingleTab(string name)
        {
            foreach (var item in _tabs)
            {
                if (item.Key == name)
                {
                    _tabs[name].FillGrid();
                }
                else
                {

                }
                item.Value.SetTitle();
            }

            

        }

        internal List<PatientRow> GetRows(string name)
        {
            return _tabs[name].paitentRows;
        }

        internal void ClearData()
        {
            _tabs.Foreach(x => x.Value.ClearListView());
        }


        public class TabWrapper
        {

            public ListView lv { get; private set; }
            public List<PatientRow> paitentRows { get; set; }



            public TabWrapper(ListView lv1)
            {
                this.lv = lv1;


            }

            public void SetTitle()
            {
                TabItem tab = lv.Parent as TabItem;
                int indexParentheses = (tab.Header as string).IndexOf('(');

                if (indexParentheses != -1)
                {
                    tab.Header = (tab.Header as string).Trim().Substring(0, indexParentheses) + string.Format("({0})", paitentRows.Count);
                }
                else
                {
                    tab.Header = (tab.Header as string).Trim() + string.Format(" ({0})", paitentRows.Count);                   
                }
            }

            internal void FillGrid()
            {

                this.lv.ItemsSource = paitentRows;
            }
            internal void ClearListView()
            {
                lv.ItemsSource = null;
                this.lv.Items.Clear();
            }

        }

    }


}
