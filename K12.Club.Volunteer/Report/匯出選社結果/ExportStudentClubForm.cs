using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using FISCA.UDT;

namespace K12.Club.Volunteer.Report.匯出選社結果
{
    public partial class ExportStudentClubForm : BaseForm
    {
        private AccessHelper _access = new AccessHelper();

        public ExportStudentClubForm()
        {
            InitializeComponent();

            // Init Combobox
            List<UDT.OpenSchoolYearSemester> listSys = this._access.Select<UDT.OpenSchoolYearSemester>();

            // SchoolYear
            schooYearCbx.Text = listSys[0].SchoolYear;
            for (int i = 0;i < 3;i++)
            {
                schooYearCbx.Items.Add(int.Parse(listSys[0].SchoolYear) - i );
            }
            // Semester
            semesterCbx.Text = listSys[0].Semester;
            semesterCbx.Items.Add(1);
            semesterCbx.Items.Add(2);
        }

        private void printBtn_Click(object sender, EventArgs e)
        {
            // 檢查是否有學生選設志願設定
            List<ConfigRecord> listConfig = this._access.Select<ConfigRecord>("config_name = '學生選填志願數'");
            if (listConfig.Count > 0)
            {
                ExportStudentClub a = new ExportStudentClub(schooYearCbx.Text, semesterCbx.Text);
                this.Close();
            }
            else
            {
                DialogResult result = MsgBox.Show("尚未設定學生選設志願設定，無法匯出學生選社結果。\r\n 是否開始起「學生選設定志願設定」功能?"
                    , "提醒",MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    V_Config v = new V_Config();
                    v.ShowDialog();
                }
                this.Close();
            }
        }

        private void leaveBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
