using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspose.Cells;
using System.IO;
using FISCA.UDT;
using K12.Data;
using FISCA.Data;
using System.Data;
using System.Windows.Forms;
using FISCA.Presentation;
using System.Diagnostics;
using System.Xml.Linq;
using System.ComponentModel;

namespace K12.Club.Volunteer.Report.匯出選社結果
{
    class ExportStudentClub
    {
        // DIC 社團ID 社團名稱
        Dictionary<string, string> clubDic = new Dictionary<string, string>();

        public ExportStudentClub(string sy, string s)
        {
            //儲存資料
            Workbook wb = new Workbook();
            Exception exc = null;
            BackgroundWorker bkw = new BackgroundWorker() { WorkerReportsProgress = true };
            bkw.ProgressChanged += delegate (object sender, ProgressChangedEventArgs e)
            {
                MotherForm.SetStatusBarMessage("匯出選社結果", e.ProgressPercentage);
            };
            bkw.DoWork += delegate
            {
                try
                {
                    bkw.ReportProgress(1);
                    AccessHelper access = new AccessHelper();
                    List<CLUBRecord> _clubList = access.Select<CLUBRecord>(/*"school_year = "+School.DefaultSchoolYear+" AND semester = "+ School.DefaultSemester*/).ToList();
                    foreach (CLUBRecord club in _clubList)
                    {
                        clubDic.Add(club.UID, club.ClubName);
                    }
                    //建立Excel範本
                    Workbook template = new Workbook(new MemoryStream(Properties.Resources.匯出選社結果_範本));
                    //template.Open(new MemoryStream(Properties.Resources.匯出選社結果_範本), FileFormatType.Excel2007Xlsx); 舊aspose寫法

                    wb.Copy(template);
                    //取得Sheet
                    Worksheet ws = wb.Worksheets[0];

                    QueryHelper qh = new QueryHelper();

                    #region SQL
                    string selectSQL = string.Format(@"
SELECT 
	student.id
    , class.class_name
    , class.grade_year
    , seat_no,name
    , student_number
    , student.ref_class_id
    , clubrecord.ref_club_id
    , clubrecord.lock
    , clubrecord.club_name
    , clubrecord.school_year
    , clubrecord.semester
    , volunteer.content
    , $k12.config.universal.content as wish_limit
FROM 
    student 
    LEFT OUTER JOIN class on class.id = student.ref_class_id
    LEFT OUTER JOIN (
        SELECT 
            scjoin.*
			, clubrecord.club_name
			, clubrecord.school_year
			, clubrecord.semester
        FROM
            $k12.clubrecord.universal AS clubrecord 
			LEFT OUTER JOIN $k12.scjoin.universal AS scjoin
				ON clubrecord.uid = scjoin.ref_club_id::bigint
        WHERE
            clubrecord.school_year = {0}
            AND clubrecord.semester = {1}
    ) AS clubrecord 
        ON clubrecord.ref_student_id::bigint = student.id
    LEFT OUTER JOIN $k12.volunteer.universal AS volunteer
        ON volunteer.ref_student_id::bigint = student.id 
            AND volunteer.school_year = {0}
            AND volunteer.semester = {1}
    LEFT OUTER JOIN $k12.config.universal ON config_name = '學生選填志願數'
WHERE 
    student.status in (1, 2) 
ORDER BY 
    class.grade_year
    , class.display_order
    , class.class_name
    , student.seat_no
    , student.id"
                    , sy, s);
                    #endregion

                    // 取得學生社團資料
                    DataTable dt = qh.Select(selectSQL);

                    bkw.ReportProgress(10);
                    int index = 1;
                    foreach (DataRow dr in dt.Rows)
                    {
                        bkw.ReportProgress(10 + 90 * index / dt.Rows.Count);
                        int wishLimit = ("" + dr["wish_limit"] == "" ? 5 : int.Parse("" + dr["wish_limit"]));
                        if (index == 1)
                        {
                            int countLimit = 6 + wishLimit;
                            for (int i = 6; i < countLimit; i++)
                            {
                                ws.Cells.CopyColumn(ws.Cells, 4, i);
                                ws.Cells[0, i].PutValue("志願" + (i - 5));
                            }
                        }
                        else
                        {
                            ws.Cells.CopyRow(ws.Cells, 1, index);
                            int countLimit = 6 + wishLimit;
                            for (int i = 0; i < countLimit; i++)
                            {
                                ws.Cells[index, i].PutValue(null);
                            }
                        }

                        ws.Cells[index, 0].PutValue("" + dr["class_name"]);
                        ws.Cells[index, 1].PutValue("" + dr["seat_no"]);
                        ws.Cells[index, 2].PutValue("" + dr["name"]);
                        ws.Cells[index, 3].PutValue("" + dr["student_number"]);
                        ws.Cells[index, 4].PutValue("" + dr["club_name"]);
                        ws.Cells[index, 5].PutValue(("" + dr["lock"]) == "true" ? "是" : "");

                        // 學生志願序
                        if (dr["content"] != null && "" + dr["content"] != "")
                        {
                            XDocument content = XDocument.Parse("" + dr["content"]);
                            List<XElement> clubList = content.Element("xml").Elements("Club").ToList();
                            int count = 6;
                            int countLimit = count + wishLimit - 1;
                            foreach (XElement club in clubList)
                            {
                                if (!clubDic.ContainsKey(club.Attribute("Ref_Club_ID").Value))
                                    continue;
                                ws.Cells[index, count].PutValue(clubDic[club.Attribute("Ref_Club_ID").Value]);
                                count++;
                                if (count > countLimit)
                                    break;
                            }
                        }
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
            };
            bkw.RunWorkerCompleted += delegate
            {
                if (exc == null)
                {
                    MotherForm.SetStatusBarMessage("匯出選社結果完成");

                    #region Excel 存檔
                    {
                        SaveFileDialog SaveFileDialog1 = new SaveFileDialog();
                        SaveFileDialog1.Filter = "Excel (*.xlsx)|*.xlsx|所有檔案 (*.*)|*.*";
                        SaveFileDialog1.FileName = "匯出選社結果";
                        try
                        {
                            if (SaveFileDialog1.ShowDialog() == DialogResult.OK)
                            {
                                wb.Save(SaveFileDialog1.FileName);
                                Process.Start(SaveFileDialog1.FileName);
                                MotherForm.SetStatusBarMessage("匯出選社結果,列印完成!!");

                            }
                            else
                            {
                                FISCA.Presentation.Controls.MsgBox.Show("檔案未儲存");
                                return;
                            }
                        }
                        catch
                        {
                            FISCA.Presentation.Controls.MsgBox.Show("檔案儲存錯誤,請檢查檔案是否開啟中!!");
                            MotherForm.SetStatusBarMessage("檔案儲存錯誤,請檢查檔案是否開啟中!!");
                        }
                    }
                    #endregion
                }
                else
                {
                    throw new Exception("匯出選社結果 發生錯誤", exc);
                }
            };
            bkw.RunWorkerAsync();
        }
    }
}
