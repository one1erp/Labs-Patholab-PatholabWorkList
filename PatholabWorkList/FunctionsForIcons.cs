using Patholab_DAL_V1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Patholab_Common;

namespace PatholabWorkList
{
    public static class FunctionsForIcons
    {
        public static DataLayer dal { get; set; }

    
        public static List<System.Windows.Controls.Image> iconsList = null;

        public static List<System.Windows.Controls.Image> setIcons(PatientRow patient)
        {

            iconsList = new List<System.Windows.Controls.Image>();
            RemarksIcon(patient.hasRemarksIcon);
            secondInspectionIcon(patient.hasSecondInspectionIcon);
            ColorsIcon(patient.hasColorsIcon, patient.hasFinishColorsIcon, patient.unscanned_slides_number);
            ExtraRequestsIcon(patient.hasExtraRequestsIcon, patient.hasFinishExtraRequestsIcon);
            AdvisorIcon(patient.SentToConsultationIcon, patient.FinishToConsultationIcon);
            ScannedIcon(patient.hasSlidedsScannedIcon);
            ExtraMaterialIcon(patient.hasExtraMaterialIcon,patient.hasFinishExtraMaterialIcon);

            //Liat asked to cancel.
            //ScannedReferenceIcon(patient.HasAnyAttachdDocs);
            //ClinicalDetailsIcon(patient.ClinicalDiagnosis);

            return iconsList;
        }

        private static void ClinicalDetailsIcon(string clinicalDiagnosis)
        {
            if (clinicalDiagnosis != null)
            {
                if (clinicalDiagnosis != string.Empty)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.clinicalDetailsIcon);
                    img.ToolTip = new ToolTip() { Content = "הוזנו פרטים קליניים" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
        }

        private static void ScannedReferenceIcon(bool? hasAnyAttachdDocs)
        {
            if (hasAnyAttachdDocs != null)
            {
                if ((bool)hasAnyAttachdDocs)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.scannedReferenceIcon);
                    img.ToolTip = new ToolTip() { Content = "קיימת הפנייה סרוקה" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
          
        }

        private static void ExtraMaterialIcon(bool hasExtraMaterialIcon, bool hasFinishExtraMaterialIcon)
        {
            if (hasExtraMaterialIcon)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();

              img.Source = loadBitmap(PatholabWorkList.Properties.Resources.extraMaterial);
                img.ToolTip = new ToolTip() { Content = "קיימת בקשה לחומר נוסף" };
                ToolTipService.SetInitialShowDelay(img, 700);

                iconsList.Add(img);
            }

            if (hasFinishExtraMaterialIcon)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();

     img.Source = loadBitmap(PatholabWorkList.Properties.Resources.fnishextraMaterial);
                img.ToolTip = new ToolTip() { Content = "הבקשה לחומר נוסף טופלה" };
                ToolTipService.SetInitialShowDelay(img, 700);

                iconsList.Add(img);
            }
        }

        private static void ScannedIcon(bool hasSlidedsScannedIcon)
        {
            if (hasSlidedsScannedIcon)
            {
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                img.Source = loadBitmap(PatholabWorkList.Properties.Resources.Scanned);
                img.ToolTip = new ToolTip() { Content = "כל הסליידים נסרקו ומוכנים לצפייה" };
                ToolTipService.SetInitialShowDelay(img, 700);

                iconsList.Add(img);
            }
        }

        public static void secondInspectionIcon(bool hasSecondInspectionIcon)
        {
            try
            {
                if (hasSecondInspectionIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.secondInspection);
                    img.ToolTip = new ToolTip() { Content = "חתימה שנייה" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);

                MessageBox.Show("Error checking for second inspection." + Environment.NewLine + ex.Message);
            }
        }

        public static void MultipleChecksIcon(bool hasMultipleIcon)
        {
            try
            {
                if (hasMultipleIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.multiple);
                    img.ToolTip = new ToolTip() { Content = "בדיקות מרובות" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);

                MessageBox.Show("Error checking for multiple inspections." + Environment.NewLine + ex.Message);
            }
        }

        public static void RemarksIcon(bool hasRemarksIcon)
        {
            try
            {
                if (hasRemarksIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.remarks);
                    img.ToolTip = new ToolTip() { Content = "קיימות הערות" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);

                MessageBox.Show("Error getting remarks icon." + Environment.NewLine + ex.Message);
            }
        }

        public static void AdvisorIcon(bool hasAdvisorIcon, bool hasFinishAdvisorIcon)
        {
            try
            {
                if (hasAdvisorIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.advisorWait);
                    img.ToolTip = new ToolTip() { Content = "נשלח להתייעצות" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }

                if (hasFinishAdvisorIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.advisorFinish);
                    img.ToolTip = new ToolTip() { Content = "חזר מהתייעצות" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);

                MessageBox.Show("Error getting consultation icon." + Environment.NewLine + ex.Message);
            }
        }

        public static void ExtraRequestsIcon(bool hasExtraRequestsIcon, bool hasFinishExtraRequestsIcon)
        {
            try
            {
                if (hasExtraRequestsIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.Rescan);
                    img.ToolTip = new ToolTip() { Content = "קיימת בקשה לסריקה חוזרת" };
                    ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }

                if (hasFinishExtraRequestsIcon)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.FinishRescan);
                    img.ToolTip = new System.Windows.Controls.ToolTip() { Content = "הבקשה לסריקה חוזרת טופלה" };
                    System.Windows.Controls.ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);

                MessageBox.Show("Error getting extra requests icon." + Environment.NewLine + ex.Message);
            }
        }

        public static void ColorsIcon(bool hasColorsIcon, bool hasFinishColorsIcon, int? unscanned_slides_number)
        {
            try
            {
                if (hasFinishColorsIcon && unscanned_slides_number == 0)
                {
                    System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                    img.Source = loadBitmap(PatholabWorkList.Properties.Resources.has_finish_colors);
                    img.ToolTip = new System.Windows.Controls.ToolTip() { Content = "הסליידים עברו צביעה ומוכנים לסריקה" };
                    System.Windows.Controls.ToolTipService.SetInitialShowDelay(img, 700);

                    iconsList.Add(img);
                }
                else
                {
                    if (hasColorsIcon || hasFinishColorsIcon)
                    {
                        System.Windows.Controls.Image img = new System.Windows.Controls.Image();

                        img.Source = loadBitmap(PatholabWorkList.Properties.Resources.has_colors);
                        img.ToolTip = new ToolTip() { Content = "קיימת בקשה לצביעות" };
                        ToolTipService.SetInitialShowDelay(img, 700);

                        iconsList.Add(img);
                    }
                }
                

                
            }
            catch (Exception ex)
            {
                Logger.WriteLogFile(ex);

                MessageBox.Show("Error getting extra requests icon." + Environment.NewLine + ex.Message);
            }
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
    }
}
