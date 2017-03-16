using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FISCA.Presentation.Controls;
using DevComponents.DotNetBar;
using FISCA.UDT;
using FISCA.Data;
using System.Drawing.Drawing2D;
using K12.Data;

namespace K12.Club.Volunteer.Ribbon.檢查_批次社團鎖社
{
    public partial class MutipleLockForm : BaseForm
    {
        //UDT物件
        private static AccessHelper _AccessHelper = new AccessHelper();
        private static QueryHelper _QueryHelper = new QueryHelper();
        
        //社團資料
        private Dictionary<string, CLUBRecord> clubDic = new Dictionary<string, CLUBRecord>();
        //社團鎖社資料
        private Dictionary<string, ClubLockData> clublockdataDic = new Dictionary<string, ClubLockData>();
        //SCJoin List 用來取得、上傳 學生參與、鎖社社團 資料
        private List<SCJoin> scjList = new List<SCJoin>();

        //目標社團學年度
        private string targetCulbSchoolYear ="";
        //目標社團學期
        private string targetCulbSchoolSemester="";

        BackgroundWorker BGW_ClubDataLoad = new BackgroundWorker();
        BackgroundWorker BGW_ClubLock = new BackgroundWorker();

        int lockedClubCount;
        int lockedStudentCount;

        public MutipleLockForm()
        {
            InitializeComponent();

            string defaultschoolyear = K12.Data.School.DefaultSchoolYear;
            string defaultschoolsemester = K12.Data.School.DefaultSemester;


            // 學年為 上下加減一學年
            comboBoxEx1.Items.Add(""+(int.Parse(K12.Data.School.DefaultSchoolYear)-1));
            comboBoxEx1.Items.Add(K12.Data.School.DefaultSchoolYear);
            comboBoxEx1.Items.Add(""+(int.Parse(K12.Data.School.DefaultSchoolYear)+1));
            
            // 學期為1、2
            comboBoxEx2.Items.Add("1");
            comboBoxEx2.Items.Add("2");

            BGW_ClubDataLoad.DoWork += new DoWorkEventHandler(BGW_ClubDataLoad_DoWork);
            BGW_ClubDataLoad.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_ClubDataLoad_RunWorkerCompleted);

            BGW_ClubLock.DoWork += new DoWorkEventHandler(BGW_ClubLock_DoWork);
            BGW_ClubLock.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_ClubLock_RunWorkerCompleted);
                        
        }


        void BGW_ClubDataLoad_DoWork(object sender, DoWorkEventArgs e)
        {            
            //社團
            clubDic = GetClubRecord();

            //社團鎖社資料
            clublockdataDic = GetClubLockDataRecord();
                            
        }

        void BGW_ClubDataLoad_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            comboBoxEx1.Enabled = true;
            comboBoxEx2.Enabled = true;
            checkBox1.Enabled = true;
            buttonX1.Enabled = true;
            buttonX2.Enabled = true;

            if (e.Cancelled)
            {
                MsgBox.Show("背景作業已取消...");
            }
            else 
            {
                FillClubDataGridView();            
            }
        }

        void BGW_ClubLock_DoWork(object sender, DoWorkEventArgs e)
        {
            //儲存鎖社結果
            scjList.SaveAll();
        }

        void BGW_ClubLock_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            comboBoxEx1.Enabled = true;
            comboBoxEx2.Enabled = true;
            checkBox1.Enabled = true;
            buttonX1.Enabled = true;
            buttonX2.Enabled = true;

            if (e.Cancelled)
            {
                MsgBox.Show("背景作業已取消...");
            }
            else
            {
                MsgBox.Show("本次共鎖了"+lockedClubCount+"個社團，共"+lockedStudentCount+"個學生。");
                BGW_ClubDataLoad.RunWorkerAsync();
            }
        }

        private void comboBoxEx1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.Text != "" && comboBoxEx2.Text != "") 
            {
                targetCulbSchoolYear = comboBoxEx1.Text;
                targetCulbSchoolSemester = comboBoxEx2.Text;

                comboBoxEx1.Enabled = false;
                comboBoxEx2.Enabled = false;
                checkBox1.Enabled = false;
                buttonX1.Enabled = false;
                buttonX2.Enabled = false;

                BGW_ClubDataLoad.RunWorkerAsync();
                                         
            }            
        }

        private void comboBoxEx2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEx1.Text != "" && comboBoxEx2.Text!="")
            {
                targetCulbSchoolYear = comboBoxEx1.Text;
                targetCulbSchoolSemester = comboBoxEx2.Text;

                comboBoxEx1.Enabled = false;
                comboBoxEx2.Enabled = false;
                checkBox1.Enabled = false;
                buttonX1.Enabled = false;
                buttonX2.Enabled = false;

                BGW_ClubDataLoad.RunWorkerAsync();                
            }
        }

        /// <summary>
        /// 取得選擇社團的資料
        /// </summary>
        public Dictionary<string, CLUBRecord> GetClubRecord()
        {
            Dictionary<string, CLUBRecord> dic = new Dictionary<string, CLUBRecord>();

            try
            {                
                foreach (CLUBRecord each in _AccessHelper.Select<CLUBRecord>("school_year =" + targetCulbSchoolYear + " and semester="+targetCulbSchoolSemester))
                {
                    if (!dic.ContainsKey(each.UID))
                    {
                        dic.Add(each.UID, each);
                    }
                }
                return dic;
            }
            catch (Exception ex)
            {                
                        
            }            
               return dic;
        }

        /// <summary>
        /// 整理選擇社團鎖社的資料
        /// </summary>
        public Dictionary<string, ClubLockData> GetClubLockDataRecord()
        {
            Dictionary<string, ClubLockData> dic = new Dictionary<string, ClubLockData>();

            List<string> ref_club_id_list = new List<string>();
            
            foreach (var club in clubDic) 
            {
                ClubLockData cld = new ClubLockData();

                ref_club_id_list.Add(club.Key);

                cld.ClubName = club.Value.ClubName;

                cld.ClubID = club.Key;

                dic.Add(club.Key, cld);
            }
                        
            try
            {
                scjList = _AccessHelper.Select<SCJoin>(string.Format("ref_club_id in('{0}') ", string.Join("','", ref_club_id_list)));                              
            }
            catch (Exception ex)
            {

            }

            foreach(SCJoin scj in scjList)
            {
                foreach(var clublockdata in dic)
                {
                    if (scj.RefClubID == clublockdata.Key) 
                    {                        
                        clublockdata.Value.TotalStudentCount++;
                        if (scj.Lock)
                        {                        
                            clublockdata.Value.AlreadyLockCount++;
                        }
                    }
                }                                        
            }

            return dic;
        }

        private void FillClubDataGridView() 
        {
            //清除舊資料
            dataGridViewX1.Rows.Clear();

            foreach (var club in clublockdataDic)
            {
                DataGridViewRow dr = new DataGridViewRow();

                dr.CreateCells(dataGridViewX1);

                dr.Cells[0].Value = false;
                dr.Cells[1].Value = club.Value.ClubName;
                dr.Cells[2].Value = club.Value.AlreadyLockCount;
                dr.Cells[3].Value = club.Value.TotalStudentCount;

                dr.Tag = club.Value.ClubID;

                // 未所有學生鎖社， 標紅字
                if (club.Value.TotalStudentCount > club.Value.AlreadyLockCount)
                {                
                    dr.Cells[2].Style.ForeColor = Color.Red;
                }
                                                    
                dataGridViewX1.Rows.Add(dr);                        
            }

            checkBox1.Checked = false;
                                            
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            foreach(DataGridViewRow dr in dataGridViewX1.Rows)
            {
                dr.Cells[0].Value = checkBox1.Checked;                                    
            }
        }

        //鎖社
        private void buttonX1_Click(object sender, EventArgs e)
        {
            comboBoxEx1.Enabled = false;
            comboBoxEx2.Enabled = false;
            checkBox1.Enabled = false;
            buttonX1.Enabled = false;
            buttonX2.Enabled = false;

            lockedClubCount = 0;
            lockedStudentCount = 0;

            foreach (DataGridViewRow dr in dataGridViewX1.Rows)
            {
                if (Boolean.Parse(""+dr.Cells[0].Value))
                {
                    lockedClubCount++;
                    foreach (SCJoin scj in scjList)
                    { 
                        if(scj.RefClubID == ""+dr.Tag)
                        {
                            if(scj.Lock == false)
                            {
                                scj.Lock = true;

                                lockedStudentCount++;
                            }                            
                        }                                        
                    }                                
                }            
            }
            BGW_ClubLock.RunWorkerAsync();
        }

        //關閉
        private void buttonX2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
                
    }
}
