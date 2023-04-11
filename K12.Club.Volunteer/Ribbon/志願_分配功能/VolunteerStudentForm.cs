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
    public partial class VolunteerStudentForm : BaseForm
    {
        社團志願分配的Row _VolRow { get; set; }

        /// <summary>
        /// 開始背景模式
        /// 取得學生選填的志願序內容
        /// </summary>
        BackgroundWorker BGW = new BackgroundWorker();

        Dictionary<string, CLUBRecord> ClubDic { get; set; }

        List<string> StudentIDList = new List<string>();

        Setup_ByV _By_V { get; set; }
        //人為設定選社學年
        string _seting_school_year = "";

        //人為設定選社學期
        string _seting_school_semester = "";
        public VolunteerStudentForm(社團志願分配的Row VolRow, string seting_school_year, string seting_school_semester, Setup_ByV By_V)
        {
            InitializeComponent();
            _seting_school_year = seting_school_year;
            _seting_school_semester = seting_school_semester;
            _VolRow = VolRow;
            _By_V = By_V;
        }

        private void VolunteerStudentForm_Load(object sender, EventArgs e)
        {
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);

            dataGridViewX1.Enabled = false;
            this.Text = "學生選填明細(資料取得中...)";
            BGW.RunWorkerAsync();
        }

        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            #region 整理出社團基本資料

            List<string> ClubIDList = new List<string>();

            foreach (VolunteerRecord vr in _VolRow._Volunteer.Values)
            {
                if (!string.IsNullOrEmpty(vr.Content))
                {
                    XmlElement xml = XmlHelper.LoadXml(vr.Content);
                    foreach (XmlElement each in xml.SelectNodes("Club"))
                    {
                        if (!ClubIDList.Contains(each.GetAttribute("Ref_Club_ID")))
                        {
                            ClubIDList.Add(each.GetAttribute("Ref_Club_ID"));
                        }
                    }
                }
            }

            ClubDic = tool.GetClub(ClubIDList);

            #endregion
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataGridViewX1.Enabled = true;
            this.Text = "學生選填明細";

            if (!e.Cancelled)
            {
                if (e.Error == null)
                {
                    SetColumn();

                    #region 建立Row

                    //取得該班級的學生基本資料
                    List<一名學生> list = _VolRow._StudentDic.Values.ToList();
                    //依座號排序
                    list.Sort(tool.SortMergeList);

                    foreach (一名學生 each in list)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dataGridViewX1);
                        row.Cells[0].Value = each.seat_no.HasValue ? each.seat_no.Value.ToString() : ""; //座號
                        row.Cells[1].Value = each.student_name; //姓名
                        row.Cells[2].Value = each.gender; //姓名


                        if (_VolRow._Volunteer.ContainsKey(each.student_id))
                        {
                            #region 必須有填志願才會被填入社團資料
                            //學生基本資料
                            VolunteerRecord obj = _VolRow._Volunteer[each.student_id];

                            //ClubDic社團是從選社志願內的資料
                            //因此它為0時,就表示多數數資料都沒有填志願
                            if (ClubDic.Count != 0)
                            {
                                //取得單一學生志願序選填狀況
                                if (!string.IsNullOrEmpty(obj.Content))
                                {
                                    XmlElement Element = XmlHelper.LoadXml(obj.Content);
                                    foreach (XmlElement xml in Element.SelectNodes("Club"))
                                    {
                                        //所選填的必須只有設定之數量
                                        int ClubIndex = 0;
                                        int.TryParse(xml.GetAttribute("Index"), out ClubIndex);
                                        if (ClubIndex <= _By_V.學生選填志願數 && ClubIndex != 0)
                                        {
                                            string clubID = xml.GetAttribute("Ref_Club_ID");
                                            //是否包含此社團
                                            if (ClubDic.ContainsKey(clubID))
                                            {
                                                CLUBRecord cr = ClubDic[clubID];
                                                //+3是因為由第 3 Column起始
                                                row.Cells[ClubIndex + 2].Value = cr.ClubName;
                                                row.Cells[ClubIndex + 2].ReadOnly = true;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }

                        if (_VolRow._SCJDic.ContainsKey(each.student_id))
                        {
                            #region 當此學生有社團參與記錄時

                            SCJoin scj = _VolRow._SCJDic[each.student_id];
                            if (_VolRow._ClubDic.ContainsKey(scj.RefClubID))
                            {
                                CLUBRecord club = _VolRow._ClubDic[scj.RefClubID];
                                row.Cells[1].Value += "(" + club.ClubName + ")";

                                if (scj.Lock)
                                {
                                    foreach (DataGridViewCell cell in row.Cells)
                                    {
                                        cell.Style.BackColor = Color.GreenYellow;
                                    }
                                }
                                else
                                {
                                    foreach (DataGridViewCell cell in row.Cells)
                                    {
                                        cell.Style.BackColor = Color.Yellow;
                                    }
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            StudentIDList.Add(each.student_id);
                        }

                        dataGridViewX1.Rows.Add(row);
                    }


                    #endregion
                }
                else
                {
                    MsgBox.Show("已發生錯誤!!\n" + e.Error.Message);
                }
            }
            else
            {
                MsgBox.Show("資料取得動作已取消");
            }
        }

        /// <summary>
        /// 依據設定檔,建立Column
        /// </summary>
        private void SetColumn()
        {
            for (int x = 1; x <= _By_V.學生選填志願數; x++)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = "ByVColumn" + x;
                column.HeaderText = "社團" + x;
                column.Width = 100;
                dataGridViewX1.Columns.Add(column);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 未選社學生加入待處理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            K12.Presentation.NLDPanels.Student.AddToTemp(StudentIDList);
        }

        private void 清空學生待處理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            K12.Presentation.NLDPanels.Student.RemoveFromTemp(K12.Presentation.NLDPanels.Student.TempSource);
        }

        private void btnSendClubAll_Click(object sender, EventArgs e)
        {
            StringBuilder sb_tipc = new StringBuilder();
            sb_tipc.AppendLine("本功能將會對班級中");
            sb_tipc.AppendLine("有成功加入社團之學生");
            sb_tipc.AppendLine("發送「選社結果通知」結果電子報表");

            DialogResult dr = MsgBox.Show(string.Format(sb_tipc.ToString(), dataGridViewX1.SelectedRows.Count), MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                //電子報表介面
                ReportTeacherForm rtForm = new ReportTeacherForm(new List<社團志願分配的Row>() { _VolRow }, _seting_school_year, _seting_school_semester, _By_V);
                rtForm.ShowDialog();
            }
        }
    }
}
