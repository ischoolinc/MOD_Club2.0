using FISCA.DSAUtil;
using FISCA.Presentation.Controls;
using FISCA.UDT;
using K12.Club.Volunteer;
using K12.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace K12.Club.Volunteer
{
    public partial class 批次志願功能 : BaseForm
    {
        public AccessHelper _A = new AccessHelper();

        List<CLUBRecord> ClubList { get; set; }

        int 學生選填志願數 = 0;

        public 批次志願功能()
        {
            InitializeComponent();


            List<ConfigRecord> list1 = _A.Select<ConfigRecord>("config_name='學生選填志願數'");
            學生選填志願數 = 1;
            if (list1.Count > 0)
            {
                int a = 1;
                int.TryParse(list1[0].Content, out a);
                學生選填志願數 = a;
            }

            integerInput1.Value = int.Parse(School.DefaultSchoolYear);
            integerInput2.Value = int.Parse(School.DefaultSemester);

            ClubList = _A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", integerInput1.Value, integerInput2.Value));

        }

        /// <summary>
        /// 全校亂數選社
        /// </summary>
        private void buttonX2_Click(object sender, EventArgs e)
        {
            DialogResult dr = MsgBox.Show(string.Format("確認進行全校 {0}學年度 {1}學期 亂數選社?", integerInput1.Value, integerInput2.Value), MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);

            if (dr == DialogResult.Yes)
            {
                //學生
                List<StudentRecord> StudentList = new List<StudentRecord>();
                foreach (StudentRecord each in Student.SelectAll())
                {
                    if (each.Status == StudentRecord.StudentStatus.一般 || each.Status == StudentRecord.StudentStatus.延修)
                    {
                        StudentList.Add(each);
                    }
                }

                //亂數
                RandomStudent<StudentRecord>(StudentList);
                List<VolunteerRecord> VolList = new List<VolunteerRecord>();
                StringBuilder sb_log = new StringBuilder();
                sb_log.AppendLine("學年度" + integerInput1.Value + "學期" + integerInput2.Value);

                foreach (StudentRecord each in StudentList)
                {
                    sb_log.Append("學生:" + each.Name + " ");
                    //亂數
                    RandomStudent<CLUBRecord>(ClubList);

                    VolunteerRecord vr = new VolunteerRecord();
                    vr.RefStudentID = each.ID;
                    vr.SchoolYear = integerInput1.Value;
                    vr.Semester = integerInput2.Value;

                    DSXmlHelper dsx = new DSXmlHelper("xml");
                    int x = 1;
                    foreach (CLUBRecord club in ClubList)
                    {
                        if (學生選填志願數 >= x)
                        {
                            sb_log.Append("志願" + x.ToString() + ":" + club.ClubName + " ");

                            dsx.AddElement("Club");
                            dsx.SetAttribute("Club", "Index", x.ToString());
                            dsx.SetAttribute("Club", "Ref_Club_ID", club.UID);
                            x++;
                        }
                        else
                            break;
                    }

                    sb_log.AppendLine("");
                    vr.Content = dsx.BaseElement.OuterXml;
                    VolList.Add(vr);
                }

                _A.InsertValues(VolList);

                FISCA.LogAgent.ApplicationLog.Log("[特殊歷程]", "全校亂數選社", sb_log.ToString());

                MsgBox.Show("全校亂數選社 作業完成!!");
            }
            else
            {
                MsgBox.Show("已取消");
            }
        }

        /// <summary>
        /// 亂數儀
        /// </summary>
        private void RandomStudent<T>(List<T> sList)
        {
            List<T> temp = new List<T>(sList);
            Random r = new Random();

            for (int i = 0; i < sList.Count; i++)
            {
                if (i + 1 == sList.Count)
                    break;

                int rnd = r.Next(i + 1, sList.Count - 1);
                T tmp = sList[rnd];
                sList[rnd] = sList[i];
                sList[i] = tmp;

            }
        }

        /// <summary>
        /// 清除社團參與記錄
        /// </summary>
        private void buttonX1_Click(object sender, EventArgs e)
        {
            DialogResult dr = MsgBox.Show(string.Format("確認清除 {0}學年度 {1}學期 社團參與記錄?", integerInput1.Value.ToString(), integerInput2.Value.ToString()), MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);

            if (dr == DialogResult.Yes)
            {
                List<string> list2 = new List<string>();
                List<CLUBRecord> list1 = _A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", integerInput1.Value.ToString(), integerInput2.Value.ToString()));

                foreach (CLUBRecord each in list1)
                {
                    list2.Add(each.UID);
                }
                List<SCJoin> list3 = _A.Select<SCJoin>("ref_club_id in ('" + string.Join("','", list2) + "')");

                _A.DeletedValues(list3);

                FISCA.LogAgent.ApplicationLog.Log("[特殊歷程]", "清除社團參與記錄", "已清除社團參與紀錄\n" + "學年度:" + integerInput1.Value.ToString() + " 學期:" + integerInput2.Value.ToString() + "\n\n共" + list3.Count + "筆社團參與紀錄");

                MsgBox.Show("清除社團參與記錄 作業完成!!");
            }
            else
            {
                MsgBox.Show("已取消");
            }
        }

        /// <summary>
        /// 清除志願序記錄
        /// </summary>
        private void buttonX3_Click(object sender, EventArgs e)
        {
            DialogResult dr = MsgBox.Show(string.Format("確認清除 {0}學年度 {1}學期 志願序紀錄?", integerInput1.Value.ToString(), integerInput2.Value.ToString()), MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);

            if (dr == DialogResult.Yes)
            {
                List<VolunteerRecord> list = _A.Select<VolunteerRecord>(string.Format("school_year={0} and semester={1}", integerInput1.Value.ToString(), integerInput2.Value.ToString()));
                _A.DeletedValues(list);

                FISCA.LogAgent.ApplicationLog.Log("[特殊歷程]", "清除志願序記錄", "已清除志願序記錄\n" + "學年度:" + integerInput1.Value.ToString() + " 學期:" + integerInput2.Value.ToString() + "\n\n共" + list.Count + "筆志願序紀錄");

                MsgBox.Show("清除志願序記錄 作業完成!!");
            }
            else
            {
                MsgBox.Show("已取消");
            }
        }
    }
}
