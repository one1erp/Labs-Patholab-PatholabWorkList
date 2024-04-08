using Patholab_DAL_V1;

namespace PatholabWorkList
{
    partial class ReContainer
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
            this.pathologResultEntryCls1 = new PathologResultEntry.PathologResultEntryCls();
            this.SuspendLayout();
            // 
            // pathologResultEntryCls1
            // 
            this.pathologResultEntryCls1.BackColor = System.Drawing.SystemColors.Control;
            this.pathologResultEntryCls1.Location = new System.Drawing.Point(-2, 0);
            this.pathologResultEntryCls1.Name = "pathologResultEntryCls1";
            this.pathologResultEntryCls1.Size = new System.Drawing.Size(1469, 715);
            this.pathologResultEntryCls1.TabIndex = 0;
            // 
            // ReContainer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1468, 718);
            this.Controls.Add(this.pathologResultEntryCls1);
            this.Name = "ReContainer";
            this.Text = "ReContainer";
            this.ResumeLayout(false);

        }

        #endregion

        private PathologResultEntry.PathologResultEntryCls pathologResultEntryCls1;
    }
}