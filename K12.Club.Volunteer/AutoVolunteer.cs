using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using FISCA.Data;
using System.Data;
using K12.Data;
using System.Windows.Forms;
using System.ComponentModel;
using FISCA.Presentation;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 隨機分配學生志願
    /// 分配 106 學年度 1 學期 學生社團志願
    /// 隨機分配5個社團志願
    /// </summary>
    class AutoVolunteer
    {
        public AutoVolunteer()
        {
            // List
            List<string> studentIDList = new List<string>();
            // Index clubID
            Dictionary<int, string> ClubIDDic = new Dictionary<int, string>();
            // studentID volunteer
            Dictionary<string, string> studentVolunteer = new Dictionary<string, string>();
            // 取得社團資料清單
            AccessHelper access = new AccessHelper();
            List<CLUBRecord> allClubList = access.Select<CLUBRecord>("school_year = 106 AND semester = 1");
            // 取得學生資料清單
            List<StudentRecord> sr = Student.SelectAll();
            // volunteer
            List<VolunteerRecord> vrList = new List<VolunteerRecord>();

            Exception exc = null;
            // BGW
            BackgroundWorker BGW = new BackgroundWorker() { WorkerReportsProgress = true};
            BGW.ProgressChanged += delegate(object sender,ProgressChangedEventArgs e) 
            {
                MotherForm.SetStatusBarMessage("隨機分配學生社團志願!",e.ProgressPercentage);
            };
            BGW.DoWork += delegate 
            {
                try
                {
                    // 取得所有社團 UID
                    int index = 0;
                    foreach (CLUBRecord club in allClubList)
                    {
                        ClubIDDic.Add(index++, club.UID);
                    }
                    BGW.ReportProgress(10);
                    Random random = new Random();
                    int min = 0;
                    int max = ClubIDDic.Count();
                    int c = 1;
                    // 紀錄:一般生學生ID
                    foreach (StudentRecord s in sr)
                    {
                        if (s.Status.ToString() == "一般")
                        {
                            if (studentIDList.Contains(s.ID))
                            {
                                MessageBox.Show("學生ID重複!!!!!!!!!!");
                            }
                            if (!studentIDList.Contains(s.ID))
                            {
                                studentIDList.Add(s.ID);
                            }
                        }
                    }
                    List<StudentRecord> studentRecord = Student.SelectByIDs(studentIDList);
                    // 刪除資料: 避免學生有兩筆社團志願紀錄
                    UpdateHelper qh = new UpdateHelper();
                    string deleteSQL = "DELETE FROM $k12.volunteer.universal WHERE school_year = 106 AND semester = 1";
                    qh.Execute(deleteSQL);

                    // 紀錄:新增學生隨機分配社團志願
                    foreach (StudentRecord s in studentRecord)
                    {
                        BGW.ReportProgress(10 + 90 * c / studentRecord.Count);

                        VolunteerRecord vr = new VolunteerRecord();
                        string content = "";
                        for (int i = 1; i <= 5; i++)
                        {
                            content += string.Format("<Club Index=\"{0}\" Ref_Club_ID=\"{1}\"/>", i, ClubIDDic[random.Next(min, max)]);
                        }
                        vr.RefStudentID = s.ID;
                        vr.Content = "<xml>" + content + "</xml>";
                        vr.SchoolYear = 106;
                        vr.Semester = 1;
                        vrList.Add(vr);
                        c++;
                    }
                    
                    access.SaveAll(vrList);
                }
                catch (Exception ex)
                {
                    exc = ex;
                }
                
            };
            BGW.RunWorkerCompleted += delegate 
            {
                if (exc == null)
                {
                    MessageBox.Show("隨機分配學生志願成功");
                }
                else
                {
                    throw new Exception("隨機分配學生社團志願 發生錯誤", exc);
                }
            };
            BGW.RunWorkerAsync();
        }
    }
}
