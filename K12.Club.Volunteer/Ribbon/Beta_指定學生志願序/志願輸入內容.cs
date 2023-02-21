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
using System.Xml;
using FISCA.DSAUtil;
using DevComponents.DotNetBar.Controls;
using K12.Club.Volunteer;
using FISCA.UDT;
using DevComponents.Editors;

namespace K12.Club.Volunteer
{
    public partial class 志願輸入內容 : BaseForm
    {
        /// <summary>
        /// 社團名稱:社團
        /// </summary>
        Dictionary<string, CLUBRecord> ClubDic = new Dictionary<string, CLUBRecord>();

        public AccessHelper _A = new AccessHelper();

        List<CLUBRecord> ClubList { get; set; }

        int 學生選填志願數 = 0;

        List<string> _StudentIDList = new List<string>();


        public 志願輸入內容(List<string> StudentIDList)
        {
            InitializeComponent();
            _StudentIDList = StudentIDList;
            integerInput1.Value = int.Parse(School.DefaultSchoolYear);
            integerInput2.Value = int.Parse(School.DefaultSemester);
        }

        private void 志願輸入內容_Load(object sender, EventArgs e)
        {
            List<ConfigRecord> list1 = _A.Select<ConfigRecord>("config_name='學生選填志願數'");
            學生選填志願數 = 1;
            if (list1.Count > 0)
            {
                int a = 1;
                int.TryParse(list1[0].Content, out a);
                學生選填志願數 = a;
            }

            for (int x = 1; x <= 學生選填志願數; x++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1);
                row.Cells[0].Value = "" + x;
                dataGridViewX1.Rows.Add(row);
            }

            ClubList = _A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", integerInput1.Value, integerInput2.Value));

            foreach (CLUBRecord cr in ClubList)
            {
                if (!ClubDic.ContainsKey(cr.ClubName))
                {
                    ClubDic.Add(cr.ClubName, cr);
                }
            }

            Column1.DataSource = ClubList;
            Column1.DisplayMember = "ClubName";

            //如果目標學生為1位,就顯示志願內容
            ChangeClub();
        }

        void ChangeClub()
        {
            if (_StudentIDList.Count == 1)
            {
                List<VolunteerRecord> volunteerList = tool._A.Select<VolunteerRecord>(string.Format("ref_student_id in ('{0}') and school_year={1} and semester={2}", string.Join("','", _StudentIDList), integerInput1.Value, integerInput2.Value));
                if (volunteerList.Count > 0)
                {
                    foreach (VolunteerRecord volunteer in volunteerList)
                    {
                        XmlElement dsx = K12.Data.XmlHelper.LoadXml(volunteer.Content);

                        foreach (XmlElement var in dsx.SelectNodes("Club"))
                        {
                            foreach (DataGridViewRow row in dataGridViewX1.Rows)
                            {
                                if (row.IsNewRow)
                                    continue;

                                string clubIndex = "" + row.Cells[0].Value;
                                string varIndex = "" + var.GetAttribute("Index");
                                if (clubIndex == varIndex)
                                {
                                    foreach (CLUBRecord record in ClubList)
                                    {
                                        if (record.UID == var.GetAttribute("Ref_Club_ID"))
                                        {
                                            row.Cells[1].Value = record.ClubName;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (DataGridViewRow row in dataGridViewX1.Rows)
                    {
                        if (row.IsNewRow)
                            continue;

                        row.Cells[1].Value = null;
                    }
                }
            }
        }

        /// <summary>
        /// 儲存
        /// </summary>
        private void buttonX1_Click(object sender, EventArgs e)
        {
            int x = 1;
            DSXmlHelper dsx = new DSXmlHelper("xml");

            StringBuilder sb_log = new StringBuilder();
            sb_log.AppendLine("指定 學年度" + integerInput1.Value + "學期" + integerInput2.Value);

            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow)
                    continue;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell is DataGridViewComboBoxExCell)
                    {
                        string name = "" + cell.Value;

                        if (ClubDic.ContainsKey(name))
                        {
                            CLUBRecord cr = ClubDic[name];


                            sb_log.AppendLine("社團名稱:" + cr.ClubName);

                            dsx.AddElement("Club");
                            dsx.SetAttribute("Club", "Index", x.ToString());
                            dsx.SetAttribute("Club", "Ref_Club_ID", cr.UID);
                            x++;
                        }
                    }
                }
            }

            sb_log.AppendLine("\n學生清單:");

            List<string> list = K12.Presentation.NLDPanels.Student.SelectedSource;

            List<StudentRecord> StudentList = Student.SelectByIDs(list);

            List<VolunteerRecord> VolList = new List<VolunteerRecord>();

            List<VolunteerRecord> DelList = _A.Select<VolunteerRecord>(string.Format("ref_student_id in ('{0}')", string.Join("','", list)));

            foreach (StudentRecord stud in StudentList)
            {
                sb_log.AppendLine(string.Format("班級:{0} 座號:{1} 姓名:{2}", stud.Class != null ? stud.Class.Name : "", stud.SeatNo.HasValue ? "" + stud.SeatNo.Value : "", stud.Name));


                VolunteerRecord Vol = new VolunteerRecord();
                Vol.SchoolYear = integerInput1.Value;
                Vol.Semester = integerInput2.Value;
                Vol.RefStudentID = stud.ID;
                Vol.Content = dsx.BaseElement.OuterXml;

                VolList.Add(Vol);
            }

            _A.DeletedValues(DelList); //清掉選擇學生的社團記錄
            _A.InsertValues(VolList); //新增

            FISCA.LogAgent.ApplicationLog.Log("[特殊歷程]", "學生志願指定", sb_log.ToString());

            MsgBox.Show("儲存完成!!");
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void integerInput1_ValueChanged(object sender, EventArgs e)
        {
            integerInput1.Enabled = false;
            ChangeClub();
            integerInput1.Enabled = true;
        }

        private void integerInput2_ValueChanged(object sender, EventArgs e)
        {
            integerInput2.Enabled = false;
            ChangeClub();
            integerInput2.Enabled = true;
        }
    }
}
