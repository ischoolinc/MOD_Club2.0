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

namespace K12.Club.Volunteer
{
    public partial class VolunteerForm : BaseForm
    {
        /// <summary>
        /// 社團分配時,需取得設定資料
        /// 還是依據志願數來決定實際可分配的志願數
        /// </summary>
        Setup_ByV ByV { get; set; }

        /// <summary>
        /// 開始背景模式
        /// 取得學生選填的志願序內容
        /// </summary>
        BackgroundWorker BGW = new BackgroundWorker();

        BackgroundWorker BGW_Run = new BackgroundWorker();

        GetVolunteer VoluntObj { get; set; }

        List<StudentMergeClub> StudentVolList { get; set; }

        public VolunteerForm()
        {
            InitializeComponent();
        }

        private void VolunteerForm_Load(object sender, EventArgs e)
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

            BGW_Run.DoWork += new DoWorkEventHandler(BGW_Run_DoWork);
            BGW_Run.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_Run_RunWorkerCompleted);

            SetFromIsBeg = false;
            this.Text = "學生社團分配(資料取得中...)";
            BGW.RunWorkerAsync();
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            //取得學生本期的志願序內容
            VoluntObj = new GetVolunteer();
            //取得學生社團志願選填內容
            StudentVolList = GetVolObj(VoluntObj);



        }

        /// <summary>
        /// 取得學生社團志願選填內容
        /// </summary>
        private List<StudentMergeClub> GetVolObj(GetVolunteer VoluntObj)
        {
            List<StudentMergeClub> list = new List<StudentMergeClub>();
            foreach (VolunteerRecord each in VoluntObj.VolList)
            {
                //是否包含此學生
                if (VoluntObj.StudentDic.ContainsKey(each.RefStudentID))
                {
                    StudentRecord sr = VoluntObj.StudentDic[each.RefStudentID];
                    StudentMergeClub obj = new StudentMergeClub(sr);

                    XmlElement Element = XmlHelper.LoadXml(each.Content);
                    foreach (XmlElement xml in Element.SelectNodes("Club"))
                    {
                        string clubID = xml.GetAttribute("Ref_Club_ID");
                        //是否包含此社團
                        if (VoluntObj.ClubDic.ContainsKey(clubID))
                        {
                            obj._CLUBList.Add(VoluntObj.ClubDic[clubID]);
                        }
                    }
                    list.Add(obj);
                }
            }
            return list;
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetFromIsBeg = true;
            this.Text = "學生社團分配";

            //加入Column
            SetColumn();

            foreach (StudentMergeClub each in StudentVolList)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridViewX1);

                row.Cells[0].Value = string.IsNullOrEmpty(each._Student.RefClassID) ? "" : each._Student.Class.Name;
                row.Cells[1].Value = each._Student.SeatNo.HasValue ? each._Student.SeatNo.Value.ToString() : "";
                row.Cells[2].Value = each._Student.Name;

                int x = 3;
                int index = 0;
                foreach (CLUBRecord club in each._CLUBList)
                {
                    index++;
                    if (index <= ByV.學生選填志願數)
                    {
                        row.Cells[x].Value = club.ClubName;
                        x++;
                    }
                }

                dataGridViewX1.Rows.Add(row);
            }
        }

        /// <summary>
        /// 依據設定檔,建立Column
        /// </summary>
        private void SetColumn()
        {
            ByV = new Setup_ByV();
            for (int x = 1; x <= ByV.學生選填志願數; x++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = "ByVColumn" + x;
                column.HeaderText = "社團" + x;
                column.Width = 100;
                dataGridViewX1.Columns.Add(column);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //開始進行分配


            //社團分配做法
            //1.取得本學期學生 - ?
            //2.取得學生本學期是否已修社

            //3.取得學生選填志願記錄
            //4.取得當期社團
            //5.選填內容與社團記錄合併顯示於畫面上

            //必須符合相關的社團限制
            //1.選社人數上限
            //2.男女限制
            //3.科別限制
            //4.

            //如果該生為選社鎖定,則不予處理



        }

        void BGW_Run_DoWork(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        void BGW_Run_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            MsgBox.Show("分配完成!!");

            throw new NotImplementedException();
        }

        bool SetFromIsBeg
        {
            set
            {
                dataGridViewX1.Enabled = value;
                buttonX3.Enabled = value;
                checkBoxX1.Enabled = value;
                buttonX1.Enabled = value;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
