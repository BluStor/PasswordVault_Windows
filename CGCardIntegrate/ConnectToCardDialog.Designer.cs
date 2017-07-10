namespace CGCardIntegrate
{
    partial class ConnectToCardDialog
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tbCardName = new System.Windows.Forms.TextBox();
            this.btmOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Card name:";
            // 
            // tbCardName
            // 
            this.tbCardName.Location = new System.Drawing.Point(109, 33);
            this.tbCardName.Name = "tbCardName";
            this.tbCardName.Size = new System.Drawing.Size(398, 26);
            this.tbCardName.TabIndex = 1;
            // 
            // btmOK
            // 
            this.btmOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btmOK.Location = new System.Drawing.Point(430, 89);
            this.btmOK.Name = "btmOK";
            this.btmOK.Size = new System.Drawing.Size(77, 34);
            this.btmOK.TabIndex = 2;
            this.btmOK.Text = "OK";
            this.btmOK.UseVisualStyleBackColor = true;
            this.btmOK.Click += new System.EventHandler(this.btmOK_Click);
            // 
            // ConnectToCardDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 135);
            this.Controls.Add(this.btmOK);
            this.Controls.Add(this.tbCardName);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectToCardDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Card Dialog";
            this.UseWaitCursor = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox tbCardName;
        private System.Windows.Forms.Button btmOK;
    }
}