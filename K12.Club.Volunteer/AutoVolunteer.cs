using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;
using FISCA.Data;
using System.Data;
using K12.Data;
namespace K12.Club.Volunteer
{
    class AutoVolunteer
    {
        public AutoVolunteer()
        {
            // Index clubID
            Dictionary<int, string> ClubIDDic = new Dictionary<int, string>();
            // studentID volunteer
            Dictionary<string, string> studentVolunteer = new Dictionary<string, string>();

            // 取得所有社團 UID
            AccessHelper access = new AccessHelper();
            List<CLUBRecord>allClubList =  access.Select<CLUBRecord>("school_year = 106 AND semester = 1");
            int index = 0;
            foreach (CLUBRecord club in allClubList)
            {
                ClubIDDic.Add(index++,club.UID);
            }

            Random random = new Random();
            int min = 0;
            int max = ClubIDDic.Count();

            List<StudentRecord> sr = Student.SelectAll();
            // 取得所有在校學生ID
            //QueryHelper qh = new QueryHelper();
            //string selectSQL = "SELECT*FROM student WHERE status = 1";
            //DataTable dt = qh.Select(selectSQL);

            // volunteer
            List<VolunteerRecord> vrList = new List<VolunteerRecord>();

            int c = 1;
            foreach (StudentRecord s in sr)
            {
                if (s.Status.ToString() == "一般")
                {
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
                }
                
            }
            //vrList.SaveAll();
            access.SaveAll(vrList);
        }
    }
}
