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
using FISCA.Data;
using System.Xml;
using FISCA.DSAUtil;

namespace K12.Club.Volunteer
{
    public partial class GradingProjectConfig : BaseForm
    {
        private AccessHelper _AccessHelper = new AccessHelper();
        private QueryHelper _QueryHelper = new QueryHelper();

        Dictionary<string, int> rowIndex = new Dictionary<string, int>();

        string PA_Name = "平時活動比例";
        string AR_Name = "出缺率比例";
        string AAS_Name = "活動力及服務比例";
        string FAR_Name = "成品成果考驗比例";

        WeightProportion wp { get; set; }

        public GradingProjectConfig()
        {
            InitializeComponent();
            List<int> cols = new List<int>() { 1 };
            Campus.Windows.DataGridViewImeDecorator dec = new Campus.Windows.DataGridViewImeDecorator(this.dataGridViewX1, cols);
        }

        private void GradingProjectConfig_Load(object sender, EventArgs e)
        {
            dataGridViewX1.Rows.Clear();

            List<WeightProportion> list = _AccessHelper.Select<WeightProportion>();
            if (list.Count == 0)
            {
                #region 預設
                //當沒有設定任何比例時,提供預設樣式
                this.Text = "社團成績評量項目(尚未設定)";
                wp = new WeightProportion();
                DataGridViewRow row;
                row = SetRow(PA_Name, "");
                rowIndex.Add(PA_Name, row.Index);

                row = SetRow(AR_Name, "");
                rowIndex.Add(AR_Name, row.Index);

                row = SetRow(AAS_Name, "");
                rowIndex.Add(AAS_Name, row.Index);

                row = SetRow(FAR_Name, "");
                rowIndex.Add(FAR_Name, row.Index);
                #endregion
            }
            else
            {
                wp = list[0];
                if (string.IsNullOrEmpty(wp.Proportion))
                {
                    #region 預設
                    //當沒有設定任何比例時,提供預設樣式
                    this.Text = "社團成績評量項目(尚未設定)";
                    wp = new WeightProportion();
                    DataGridViewRow row;
                    row = SetRow(PA_Name, "");
                    rowIndex.Add(PA_Name, row.Index);

                    row = SetRow(AR_Name, "");
                    rowIndex.Add(AR_Name, row.Index);

                    row = SetRow(AAS_Name, "");
                    rowIndex.Add(AAS_Name, row.Index);

                    row = SetRow(FAR_Name, "");
                    rowIndex.Add(FAR_Name, row.Index);
                    #endregion
                }
                else
                {
                    dataGridViewX1.Tag = wp;
                    XmlElement xml = DSXmlHelper.LoadXml(wp.Proportion);
                    foreach (XmlElement each in xml.SelectNodes("Item"))
                    {
                        DataGridViewRow row = SetRow(each.GetAttribute("Name"), each.GetAttribute("Proportion"));
                        rowIndex.Add(each.GetAttribute("Name"), row.Index);
                    }
                }
            }
        }

        private DataGridViewRow SetRow(string Name, string wp)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(dataGridViewX1);
            row.Cells[0].Value = Name;
            row.Cells[1].Value = wp;
            int index = dataGridViewX1.Rows.Add(row);
            return dataGridViewX1.Rows[index];
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (CheckData())
            {
                DSXmlHelper DSXml = new DSXmlHelper("Xml");
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("已修改評量比例");

                foreach (DataGridViewRow row in dataGridViewX1.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    XmlElement xmle = DSXml.AddElement("Item");

                    xmle.SetAttribute("Name", "" + row.Cells[0].Value);
                    xmle.SetAttribute("Proportion", "" + row.Cells[1].Value);

                    //Log
                    sb.AppendLine(string.Format("名稱「{0}」比例「{1}」", "" + row.Cells[0].Value, "" + row.Cells[1].Value));
                }

                //先刪掉
                try
                {
                    List<WeightProportion> listdelete = _AccessHelper.Select<WeightProportion>();
                    _AccessHelper.DeletedValues(listdelete);

                    //新增一組全新的
                    List<WeightProportion> list = new List<WeightProportion>();
                    wp.Proportion = DSXml.BaseElement.OuterXml;
                    list.Add(wp);
                    _AccessHelper.InsertValues(list);

                    FISCA.LogAgent.ApplicationLog.Log("社團", "修改評量比例", sb.ToString());
                }
                catch (Exception ex)
                {
                    MsgBox.Show("儲存失敗!!\n" + ex.Message);
                    SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
                    return;
                }

                MsgBox.Show("儲存成功!!");
                this.Close();
            }
            else
            {
                MsgBox.Show("資料錯誤請修正後儲存!!");
            }
        }

        //檢查每一個Row的值是否正確
        private bool CheckData()
        {
            //Cell-1必須是數字,且小於100%
            bool check = true;
            List<string> list = new List<string>();
            foreach (DataGridViewRow row in dataGridViewX1.Rows)
            {
                if (row.IsNewRow)
                    continue;

                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.ColumnIndex == 0)
                    {
                        #region 項目名稱不可重覆
                        if (list.Contains("" + cell.Value))
                        {
                            check = false;
                            row.ErrorText = "項目名稱不可重覆";
                        }
                        else
                        {
                            row.ErrorText = "";
                        }
                        list.Add("" + cell.Value);
                        #endregion

                        #region 必須有輸入名稱
                        if (string.IsNullOrEmpty("" + cell.Value))
                        {
                            check = false;
                            cell.ErrorText = "必須輸入名稱";
                        }
                        else
                        {
                            cell.ErrorText = "";
                        }
                        #endregion
                    }
                    else if (cell.ColumnIndex == 1)
                    {

                        #region 必須是數字
                        int x = 0;
                        if (!int.TryParse("" + cell.Value, out x))
                        {
                            check = false;
                            cell.ErrorText = "必須是數字";
                        }
                        else
                        {
                            cell.ErrorText = "";
                        }
                        #endregion
                    }
                }
            }

            #region 比例加總不可超過100(註解)
            //int x_1 = 100;
            //int x_2 = 0;
            //foreach (DataGridViewRow row in dataGridViewX1.Rows)
            //{
            //    if (row.IsNewRow)
            //        continue;
            //    foreach (DataGridViewCell cell in row.Cells)
            //    {
            //        if (cell.ColumnIndex == 1)
            //        {
            //            int x;
            //            int.TryParse("" + cell.Value, out x);
            //            x_2 += x;
            //        }
            //    }
            //}
            //if (x_2 > x_1)
            //{
            //    foreach (DataGridViewRow row in dataGridViewX1.Rows)
            //    {
            //        if (row.IsNewRow)
            //            continue;
            //        foreach (DataGridViewCell cell in row.Cells)
            //        {
            //            if (cell.ColumnIndex == 1)
            //            {
            //                check = false;
            //                cell.ErrorText = "比例加總不可超過100";
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (DataGridViewRow row in dataGridViewX1.Rows)
            //    {
            //        if (row.IsNewRow)
            //            continue;
            //        foreach (DataGridViewCell cell in row.Cells)
            //        {
            //            if (cell.ColumnIndex == 1)
            //            {
            //                cell.ErrorText = "";
            //            }
            //        }
            //    }
            //} 
            #endregion

            return check;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            List<WeightProportion> list = _AccessHelper.Select<WeightProportion>();
            _AccessHelper.DeletedValues(list);
        }

        private void dataGridViewX1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            CheckData();
        }
    }
}
