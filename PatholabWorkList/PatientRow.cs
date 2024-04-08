using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PatholabWorkList
{
    public class PatientRow //: INotifyPropertyChanged
    {
        public long sdgId { get; set; }
        public string ptientName { get; set; }
        public string FullPtientName {
            get { return "ת.ז: " + ptientName; } 
            set {ptientName = value;}

        }
        public string RemarksRequest { get; set; }

        public string sdgName { get; set; }
        public string isDigit { get; set; }

        public string status { get; set; }
        private static DataLayer dal;


        public List<Image> images
        {
            get { return getIcons(); }
        }

        private string priority;
        public Decimal? PriorityNumber
        {
            get { return priority != null ? Convert.ToDecimal(priority) : 1; }
            set { priority = value.ToString(); }
        }

        public string PriorityTxt { get; set; }

    
        public string Priority
        {
            get { return getPriorityString(); }
        }

        public string Patholog
        {
            get;
            set;
        }


        public DateTime? scanned_on = null;
        public DateTime? Scanned_on
        {
            get
            {
                try
                {
                    return scanned_on;

                }
                catch (Exception e)
                {

                    System.Windows.Forms.MessageBox.Show(e.Message);
                    return scanned_on;

                }
            }
            set
            {
                scanned_on = value;
            }
        }




        DateTime? date = null;
        public DateTime? Date
        {
            get
            {
                return date.HasValue ? date.Value : new DateTime();
            }
            set { date = value; }
        }

        private string patholabNum;
        public string PatholabNum
        {
            get { return patholabNum; }
            set { patholabNum = value; }
        }

        private string clinicalDiagnosis;
        public string ClinicalDiagnosis
        {

            get { return clinicalDiagnosis; }
            set { clinicalDiagnosis = value != null ? value : string.Empty; }

        }

        private string colors;
        public string Colors
        {
            get { return colors; }
            set { colors = value; }
        }

        private Decimal? _numColors = null;
        public Decimal? NumColors
        {
            get
            {
                return _numColors;
            }
            set
            {
                _numColors = value;
            }
        }

        private Decimal? _numBlocks;
        public Decimal? NumBlocks
        {
            get { return _numBlocks; }
            set { _numBlocks = value; }
        }

        public string AllOrgans { get; set; }

        public string FirstOrgan
        {
            get
            {
                if (AllOrgans != null)
                {
                    var split = AllOrgans.Split(new char[] { ',' }, 2);
                    return split[0];
                }

                return null;
            }
        }

        public List<CheckBox> checkBoxDistribution { get; set; }
        public List<CheckBox> checkBoxAdvise { get; set; }
        public List<CheckBox> checkBoxAssociation { get; set; }


        public static void initDal(DataLayer i_Dal)
        {
            dal = i_Dal;
        }

        private bool inConsult = false;
        public bool InConsult
        {
            get { return inConsult; }
            set { inConsult = value; }
        }

        

        public bool shouldDistribute = false;
        public bool ShouldDistribute
        {
            get { return shouldDistribute; }
            set { shouldDistribute = value; }
        }

        public BitmapSource portalPath { get { return getPortal(); } }

        public BitmapSource sectraPath { get { return getSectra(); } }

        public BitmapSource previewLetterPath { get { return getPreviewLetter(); } }

        public BitmapSource checkBoxUnchecked { get { return checkBoxUncheckedImage(); } }

        public BitmapSource checkBoxMouseOver { get { return checkBoxMouseOverImage(); } }

        public BitmapSource checkBoxChecked { get { return checkBoxCheckedImage(); } }


        public bool hasRemarksIcon { get; set; }
        public bool hasMultipleIcon { get; set; }
        public bool hasSecondInspectionIcon { get; set; }
        public bool hasExtraRequestsIcon { get; set; }
        public bool hasFinishExtraRequestsIcon { get; set; }
        public bool SentToConsultationIcon { get; set; }
        public bool FinishToConsultationIcon { get; set; }
        public bool hasColorsIcon { get; set; }
        public bool hasSlidedsScannedIcon { get; set; }
        public bool hasFinishColorsIcon { get; set; }
        public bool hasExtraMaterialIcon { get; set; }
        public bool hasFinishExtraMaterialIcon { get; set; }

        public int? unscanned_slides_number { get; set; }
        public int? empty_blocks_number { get; set; }


        public decimal? GlassType { get; set; }


        private static Dictionary<string, string> dict = new Dictionary<string, string>();
        private static Dictionary<string, string> PriorityDict
        {
            get
            {
                if (dict.Count() < 1)
                {
                    PHRASE_HEADER header = dal.FindBy<PHRASE_HEADER>(ph => ph.NAME.Equals("Priority")).FirstOrDefault();
                    if (header != null)
                    {
                        foreach (PHRASE_ENTRY entry in header.PHRASE_ENTRY)
                        {
                            try
                            {
                                dict.Add(entry.PHRASE_NAME, entry.PHRASE_DESCRIPTION);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }

                return dict;
            }
        }


        public PatientRow()
        {
        }


        public BitmapSource getSectra()
        {
            return loadBitmap(PatholabWorkList.Properties.Resources.sectra);
        }


        public BitmapSource getPortal()
        {
            return loadBitmap(PatholabWorkList.Properties.Resources.portal);
        }

        public BitmapSource getPreviewLetter()
        {
            return loadBitmap(PatholabWorkList.Properties.Resources.extraDocument);
        }
   


        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);
        private static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ip,
                    IntPtr.Zero,
                   Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        public BitmapSource checkBoxUncheckedImage()
        {
            return loadBitmap(PatholabWorkList.Properties.Resources.white_square);
        }

        public BitmapSource checkBoxMouseOverImage()
        {
            return loadBitmap(PatholabWorkList.Properties.Resources.black_square);
        }

        public BitmapSource checkBoxCheckedImage()
        {
            return loadBitmap(PatholabWorkList.Properties.Resources.checked_checkbox);
        }

        List<Image> getIcons()
        {
            return FunctionsForIcons.setIcons(this);
        }
        public bool IsChecked { get; set; }
       
        private string getPriorityString()
        {
            if (priority != null)
            {
                if (PriorityDict.ContainsKey(priority))
                {
                    return PriorityDict[priority];
                }
                else
                {
                    return "רגיל";
                }
            }

            return "רגיל";
        }

        public string ToolTipDetails { get; set; }
        
        public void setCheckBox(Style parent)
        {
            checkBoxDistribution = new List<CheckBox>();
            checkBoxAdvise = new List<CheckBox>();
            checkBoxAssociation = new List<CheckBox>();

            CheckBox cb = new CheckBox();
            checkBoxAssociation.Add(cb);

            if (InConsult)
            {
                cb.Style = parent;

                checkBoxAdvise.Add(cb);
            }

            if (ShouldDistribute)
            {
                cb.Style = parent;

                checkBoxDistribution.Add(cb);
            }
        }


        public string Revision { get; set; }
        public string ADVISOROPERATOR { get; set; }
        public long? PathologId { get; internal set; }
        public decimal? U_WEEK_NBR { get; internal set; }
        public bool? HasAnyAttachdDocs { get; internal set; }
    }


   public enum State
    {
        Patholog,Manager,Bank
    }
}
