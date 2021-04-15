namespace K12.Club.Volunteer
{
    partial class CopyClub
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
            this.lbHelp1 = new DevComponents.DotNetBar.LabelX();
            this.lbSchoolYear = new DevComponents.DotNetBar.LabelX();
            this.lbSemester = new DevComponents.DotNetBar.LabelX();
            this.intSchoolYear = new DevComponents.Editors.IntegerInput();
            this.intSemester = new DevComponents.Editors.IntegerInput();
            this.btnStart = new DevComponents.DotNetBar.ButtonX();
            this.btnClose = new DevComponents.DotNetBar.ButtonX();
            this.checkBoxX1 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.checkBoxX2 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.checkBoxX3 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.labelX2 = new DevComponents.DotNetBar.LabelX();
            this.lbHelp2 = new DevComponents.DotNetBar.LabelX();
            this.checkBoxX4 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.checkBoxX5 = new DevComponents.DotNetBar.Controls.CheckBoxX();
            this.labelX1 = new DevComponents.DotNetBar.LabelX();
            ((System.ComponentModel.ISupportInitialize)(this.intSchoolYear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.intSemester)).BeginInit();
            this.SuspendLayout();
            // 
            // lbHelp1
            // 
            this.lbHelp1.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lbHelp1.BackgroundStyle.Class = "";
            this.lbHelp1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbHelp1.Location = new System.Drawing.Point(12, 283);
            this.lbHelp1.Name = "lbHelp1";
            this.lbHelp1.Size = new System.Drawing.Size(352, 98);
            this.lbHelp1.TabIndex = 5;
            this.lbHelp1.Text = "說明：\r\n  1. 本功能將會複製社團至下一個學年度學期\r\n      (目標學年期已自動加一學期,複製開始前請確認)\r\n  2. 如果下學期已有相同社團,會略過" +
    "此社團不做變動\r\n  3. 加入社團幹部將保留幹部身分";
            // 
            // lbSchoolYear
            // 
            this.lbSchoolYear.AutoSize = true;
            this.lbSchoolYear.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lbSchoolYear.BackgroundStyle.Class = "";
            this.lbSchoolYear.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbSchoolYear.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbSchoolYear.Location = new System.Drawing.Point(12, 42);
            this.lbSchoolYear.Name = "lbSchoolYear";
            this.lbSchoolYear.Size = new System.Drawing.Size(85, 24);
            this.lbSchoolYear.TabIndex = 1;
            this.lbSchoolYear.Text = "目標學年度";
            // 
            // lbSemester
            // 
            this.lbSemester.AutoSize = true;
            this.lbSemester.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lbSemester.BackgroundStyle.Class = "";
            this.lbSemester.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbSemester.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbSemester.Location = new System.Drawing.Point(204, 42);
            this.lbSemester.Name = "lbSemester";
            this.lbSemester.Size = new System.Drawing.Size(38, 24);
            this.lbSemester.TabIndex = 3;
            this.lbSemester.Text = "學期";
            // 
            // intSchoolYear
            // 
            this.intSchoolYear.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.intSchoolYear.BackgroundStyle.Class = "DateTimeInputBackground";
            this.intSchoolYear.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.intSchoolYear.ButtonFreeText.Shortcut = DevComponents.DotNetBar.eShortcut.F2;
            this.intSchoolYear.Location = new System.Drawing.Point(110, 42);
            this.intSchoolYear.MaxValue = 999;
            this.intSchoolYear.MinValue = 90;
            this.intSchoolYear.Name = "intSchoolYear";
            this.intSchoolYear.ShowUpDown = true;
            this.intSchoolYear.Size = new System.Drawing.Size(80, 25);
            this.intSchoolYear.TabIndex = 0;
            this.intSchoolYear.Value = 90;
            // 
            // intSemester
            // 
            this.intSemester.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.intSemester.BackgroundStyle.Class = "DateTimeInputBackground";
            this.intSemester.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.intSemester.ButtonFreeText.Shortcut = DevComponents.DotNetBar.eShortcut.F2;
            this.intSemester.Location = new System.Drawing.Point(252, 42);
            this.intSemester.MaxValue = 2;
            this.intSemester.MinValue = 1;
            this.intSemester.Name = "intSemester";
            this.intSemester.ShowUpDown = true;
            this.intSemester.Size = new System.Drawing.Size(80, 25);
            this.intSemester.TabIndex = 1;
            this.intSemester.Value = 1;
            // 
            // btnStart
            // 
            this.btnStart.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnStart.AutoSize = true;
            this.btnStart.BackColor = System.Drawing.Color.Transparent;
            this.btnStart.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnStart.Location = new System.Drawing.Point(211, 384);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 25);
            this.btnStart.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "開始";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnClose
            // 
            this.btnClose.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnClose.AutoSize = true;
            this.btnClose.BackColor = System.Drawing.Color.Transparent;
            this.btnClose.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnClose.Location = new System.Drawing.Point(292, 384);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 25);
            this.btnClose.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "關閉";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // checkBoxX1
            // 
            this.checkBoxX1.AutoSize = true;
            this.checkBoxX1.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.checkBoxX1.BackgroundStyle.Class = "";
            this.checkBoxX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.checkBoxX1.Location = new System.Drawing.Point(30, 154);
            this.checkBoxX1.Name = "checkBoxX1";
            this.checkBoxX1.Size = new System.Drawing.Size(100, 20);
            this.checkBoxX1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.checkBoxX1.TabIndex = 4;
            this.checkBoxX1.Text = "加入<b><u>一般社員</u></b>";
            this.checkBoxX1.CheckedChanged += new System.EventHandler(this.checkBoxX1_CheckedChanged);
            // 
            // checkBoxX2
            // 
            this.checkBoxX2.AutoSize = true;
            this.checkBoxX2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.checkBoxX2.BackgroundStyle.Class = "";
            this.checkBoxX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.checkBoxX2.Location = new System.Drawing.Point(30, 128);
            this.checkBoxX2.Name = "checkBoxX2";
            this.checkBoxX2.Size = new System.Drawing.Size(232, 20);
            this.checkBoxX2.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.checkBoxX2.TabIndex = 3;
            this.checkBoxX2.Text = "加入<b><u>其他社團幹部(社長/副社長以外)</u></b>";
            this.checkBoxX2.CheckedChanged += new System.EventHandler(this.checkBoxX2_CheckedChanged);
            // 
            // checkBoxX3
            // 
            this.checkBoxX3.AutoSize = true;
            this.checkBoxX3.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.checkBoxX3.BackgroundStyle.Class = "";
            this.checkBoxX3.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.checkBoxX3.Location = new System.Drawing.Point(30, 102);
            this.checkBoxX3.Name = "checkBoxX3";
            this.checkBoxX3.Size = new System.Drawing.Size(126, 20);
            this.checkBoxX3.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.checkBoxX3.TabIndex = 2;
            this.checkBoxX3.Text = "加入<b><u>社長、副社長</u></b>";
            // 
            // labelX2
            // 
            this.labelX2.AutoSize = true;
            this.labelX2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX2.BackgroundStyle.Class = "";
            this.labelX2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX2.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelX2.Location = new System.Drawing.Point(12, 72);
            this.labelX2.Name = "labelX2";
            this.labelX2.Size = new System.Drawing.Size(131, 24);
            this.labelX2.TabIndex = 12;
            this.labelX2.Text = "加入現有社團學生";
            // 
            // lbHelp2
            // 
            this.lbHelp2.AutoSize = true;
            this.lbHelp2.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.lbHelp2.BackgroundStyle.Class = "";
            this.lbHelp2.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbHelp2.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbHelp2.Location = new System.Drawing.Point(12, 12);
            this.lbHelp2.Name = "lbHelp2";
            this.lbHelp2.Size = new System.Drawing.Size(54, 24);
            this.lbHelp2.TabIndex = 1;
            this.lbHelp2.Text = "學年度";
            // 
            // checkBoxX4
            // 
            this.checkBoxX4.AutoSize = true;
            this.checkBoxX4.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.checkBoxX4.BackgroundStyle.Class = "";
            this.checkBoxX4.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.checkBoxX4.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.checkBoxX4.Checked = true;
            this.checkBoxX4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxX4.CheckValue = "Y";
            this.checkBoxX4.Location = new System.Drawing.Point(30, 226);
            this.checkBoxX4.Name = "checkBoxX4";
            this.checkBoxX4.Size = new System.Drawing.Size(147, 21);
            this.checkBoxX4.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.checkBoxX4.TabIndex = 13;
            this.checkBoxX4.Text = "依照原鎖定狀態複製";
            // 
            // checkBoxX5
            // 
            this.checkBoxX5.AutoSize = true;
            this.checkBoxX5.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.checkBoxX5.BackgroundStyle.Class = "";
            this.checkBoxX5.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.checkBoxX5.CheckBoxStyle = DevComponents.DotNetBar.eCheckBoxStyle.RadioButton;
            this.checkBoxX5.Location = new System.Drawing.Point(30, 253);
            this.checkBoxX5.Name = "checkBoxX5";
            this.checkBoxX5.Size = new System.Drawing.Size(80, 21);
            this.checkBoxX5.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.checkBoxX5.TabIndex = 14;
            this.checkBoxX5.Text = "全部鎖定";
            // 
            // labelX1
            // 
            this.labelX1.AutoSize = true;
            this.labelX1.BackColor = System.Drawing.Color.Transparent;
            // 
            // 
            // 
            this.labelX1.BackgroundStyle.Class = "";
            this.labelX1.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.labelX1.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.labelX1.Location = new System.Drawing.Point(12, 194);
            this.labelX1.Name = "labelX1";
            this.labelX1.Size = new System.Drawing.Size(162, 22);
            this.labelX1.TabIndex = 15;
            this.labelX1.Text = "設定選社鎖定狀態<font color=\"#ED1C24\">(New)</font>";
            // 
            // CopyClub
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 421);
            this.Controls.Add(this.labelX1);
            this.Controls.Add(this.checkBoxX5);
            this.Controls.Add(this.checkBoxX4);
            this.Controls.Add(this.labelX2);
            this.Controls.Add(this.checkBoxX3);
            this.Controls.Add(this.checkBoxX2);
            this.Controls.Add(this.checkBoxX1);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.intSemester);
            this.Controls.Add(this.intSchoolYear);
            this.Controls.Add(this.lbSemester);
            this.Controls.Add(this.lbHelp2);
            this.Controls.Add(this.lbSchoolYear);
            this.Controls.Add(this.lbHelp1);
            this.DoubleBuffered = true;
            this.Name = "CopyClub";
            this.Text = "複製社團";
            this.Load += new System.EventHandler(this.CopyClub_Load);
            ((System.ComponentModel.ISupportInitialize)(this.intSchoolYear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.intSemester)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevComponents.DotNetBar.LabelX lbHelp1;
        private DevComponents.DotNetBar.LabelX lbSchoolYear;
        private DevComponents.DotNetBar.LabelX lbSemester;
        private DevComponents.Editors.IntegerInput intSchoolYear;
        private DevComponents.Editors.IntegerInput intSemester;
        private DevComponents.DotNetBar.ButtonX btnStart;
        private DevComponents.DotNetBar.ButtonX btnClose;
        private DevComponents.DotNetBar.Controls.CheckBoxX checkBoxX1;
        private DevComponents.DotNetBar.Controls.CheckBoxX checkBoxX2;
        private DevComponents.DotNetBar.Controls.CheckBoxX checkBoxX3;
        private DevComponents.DotNetBar.LabelX labelX2;
        private DevComponents.DotNetBar.LabelX lbHelp2;
        private DevComponents.DotNetBar.Controls.CheckBoxX checkBoxX4;
        private DevComponents.DotNetBar.Controls.CheckBoxX checkBoxX5;
        private DevComponents.DotNetBar.LabelX labelX1;
    }
}