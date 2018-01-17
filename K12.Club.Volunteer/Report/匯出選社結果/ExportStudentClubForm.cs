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
        public ExportStudentClubForm()
        {
            InitializeComponent();

            // Init Combobox
            AccessHelper access = new AccessHelper();
            List<UDT.OpenSchoolYearSemester> sysList = access.Select<UDT.OpenSchoolYearSemester>();

            // SchoolYear
            schooYearCbx.Text = sysList[0].SchoolYear;
            for (int i = 0;i < 3;i++)
            {
                schooYearCbx.Items.Add(int.Parse(sysList[0].SchoolYear) - i );
            }
            // Semester
            semesterCbx.Text = sysList[0].Semester;
            semesterCbx.Items.Add(1);
            semesterCbx.Items.Add(2);
        }

        private void printBtn_Click(object sender, EventArgs e)
        {
            ExportStudentClub a = new ExportStudentClub(schooYearCbx.Text,semesterCbx.Text);
            this.Close();
        }

        private void leaveBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
