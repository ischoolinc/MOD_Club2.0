using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;

namespace K12.Club.Volunteer
{
    public partial class V_Config : BaseForm
    {
        string SetupName_1 = "學生選填志願數";
        string SetupName_2 = "社團分配優先序";
        string SetupName_3 = "已有社團記錄時";

        BackgroundWorker BGW = new BackgroundWorker();

        BackgroundWorker BGW_Save = new BackgroundWorker();

        public V_Config()
        {
            InitializeComponent();
            BGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_RunWorkerCompleted);
            BGW.DoWork += new DoWorkEventHandler(BGW_DoWork);

            BGW_Save.DoWork += new DoWorkEventHandler(BGW_Save_DoWork);
            BGW_Save.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BGW_Save_RunWorkerCompleted);
        }

        private void V_Config_Load(object sender, EventArgs e)
        {
            SetFormMode = false;
            BGW.RunWorkerAsync();
        }


        void BGW_DoWork(object sender, DoWorkEventArgs e)
        {
            Setup_ByV ByV = new Setup_ByV();           
            e.Result = ByV;
        }

        void BGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetFormMode = true;
            if (e.Error == null)
            {
                if (!e.Cancelled)
                {
                    Setup_ByV ByV = (Setup_ByV)e.Result;

                    integerInput1.Value = ByV.學生選填志願數;
                    cbMeritsX1.Checked = ByV.社團分配優先序;
                    if (ByV.已有社團記錄時)
                        cbCover.Checked = true;
                    else
                        cbSkip.Checked = true;
                }
                else
                {
                    MsgBox.Show("背景作業已被取消!");
                }
            }
            else
            {
                MsgBox.Show("取得資料發生錯誤!");
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
     
            if (!BGW_Save.IsBusy)
            {
                SetFormMode = false;

                Setup_ByV ByV = new Setup_ByV();
                ByV.學生選填志願數 = integerInput1.Value;
                ByV.社團分配優先序 = cbMeritsX1.Checked;
                if (cbCover.Checked)
                    ByV.已有社團記錄時 = true;
                else
                    ByV.已有社團記錄時 = false;

                BGW_Save.RunWorkerAsync(ByV);
            }
            else
            {
                MsgBox.Show("忙碌中,請稍後再試...");
            }
        }

        void BGW_Save_DoWork(object sender, DoWorkEventArgs e)
        {
            Setup_ByV ByV = (Setup_ByV)e.Argument;

            //全部刪除
            List<ConfigRecord> DeleteList = tool._A.Select<ConfigRecord>();
            tool._A.DeletedValues(DeleteList);

            List<ConfigRecord> InsertList = new List<ConfigRecord>();

            ConfigRecord cr = new ConfigRecord();
            cr.ConfigName = SetupName_1;
            cr.Content = ByV.學生選填志願數.ToString();
            InsertList.Add(cr);

            cr = new ConfigRecord();
            cr.ConfigName = SetupName_2;
            cr.Content = ByV.社團分配優先序.ToString();
            InsertList.Add(cr);

            cr = new ConfigRecord();
            cr.ConfigName = SetupName_3;
            if (ByV.已有社團記錄時)
                cr.Content = "True";
            else
                cr.Content = "False";
            InsertList.Add(cr);

            tool._A.InsertValues(InsertList);
        }

        void BGW_Save_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SetFormMode = true;
            if (e.Error == null)
            {
                if (!e.Cancelled)
                {
                    MsgBox.Show("儲存成功!!");
                    this.Close();
                }
                else
                {

                    MsgBox.Show("背景作業已被取消!");
                }
            }
            else
            {
                MsgBox.Show("取得資料發生錯誤!");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        bool SetFormMode
        {
            set
            {
                btnSave.Enabled = value;
                integerInput1.Enabled = value;
                cbMeritsX1.Enabled = value;
                cbCover.Enabled = value;
                cbSkip.Enabled = value;
            }
        }
    }
}
