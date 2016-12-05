using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using K12.Data;

namespace K12.Club.Volunteer
{
    public partial class SCJReMove : BaseForm
    {
        public SCJReMove()
        {
            InitializeComponent();
        }

        private void SCJReMove_Load(object sender, EventArgs e)
        {
            labelX2.Text = string.Format("[{0}]學年度,第[{1}]學期", School.DefaultSchoolYear, School.DefaultSemester);
        }

        private void btnOkCleart_Click(object sender, EventArgs e)
        {
            //大寫'LOG'
            if (textBoxX1.Text == "LOG")
            {
                DialogResult dr = MsgBox.Show("確認清除[社團參與記錄]?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", School.DefaultSchoolYear, School.DefaultSemester));
                    List<string> list2 = new List<string>();
                    foreach (CLUBRecord each in ClubList)
                    {
                        list2.Add(each.UID);
                    }
                    List<SCJoin> list3 = tool._A.Select<SCJoin>("ref_club_id in ('" + string.Join("','", list2) + "')");

                    tool._A.DeletedValues(list3);

                    MsgBox.Show("已清除共[" + list3.Count + "]筆 社團參與記錄");
                }
                else
                {
                    MsgBox.Show("已取消清除作業!!");
                    this.Close();
                }
            }
            else
            {
                MsgBox.Show("核可碼錯誤!!");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
