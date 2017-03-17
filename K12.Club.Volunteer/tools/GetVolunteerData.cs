using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using K12.Data;
using System.Data;
using FISCA.UDT;
using FISCA.Presentation.Controls;

namespace K12.Club.Volunteer
{
    static public class GetVolunteerData
    {
        
        
        /// <summary>
        /// 取得本學年度學期的社團清單
        /// </summary>
        static public Dictionary<string, CLUBRecord> GetSchoolYearClub()
        {

            AccessHelper _AccessHelper = new AccessHelper();
            List<UDT.OpenSchoolYearSemester> opensemester = new List<UDT.OpenSchoolYearSemester>();

            //人為設定選社學年
            string seting_school_year = "";

            //人為設定選社學期
            string seting_school_semester = "";

            opensemester = _AccessHelper.Select<UDT.OpenSchoolYearSemester>();

            //填入之前的紀錄
            if (opensemester.Count > 0)
            {
                seting_school_year = opensemester[0].SchoolYear;
                seting_school_semester = opensemester[0].Semester;
            }
            else 
            {
                MsgBox.Show("沒有設定 選社學年、選社學期，請至'選社開放時間'功能內設定。");
                            
            }


            Dictionary<string, CLUBRecord> dic = new Dictionary<string, CLUBRecord>();

            //舊的 ClubList 會載入 系統系統學期的社團清單
            //List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", School.DefaultSchoolYear, School.DefaultSemester));

            //新的 是載入 人為設定選社學年、學期
            List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>(string.Format("school_year={0} and semester={1}", seting_school_year, seting_school_semester));


            foreach (CLUBRecord club in ClubList)
            {
                if (!dic.ContainsKey(club.UID))
                {
                    dic.Add(club.UID, club);
                }
            }
            return dic;
        }

        /// <summary>
        /// 取得傳入的學生ID清單
        /// </summary>
        static public Dictionary<string, StudentRecord> GetStudent(List<string> StudentIDList)
        {
            Dictionary<string, StudentRecord> dic = new Dictionary<string, StudentRecord>();

            List<StudentRecord> StudentList = Student.SelectByIDs(StudentIDList);
            foreach (StudentRecord sr in StudentList)
            {
                if (!dic.ContainsKey(sr.ID))
                {
                    dic.Add(sr.ID, sr);
                }
            }
            return dic;
        }

        /// <summary>
        /// 依據本學年度學期
        /// 取得學生所填的志願序清單
        /// </summary>
        static public Dictionary<string, VolunteerRecord> GetVolunteerDic()
        {
            AccessHelper _AccessHelper = new AccessHelper();
            List<UDT.OpenSchoolYearSemester> opensemester = new List<UDT.OpenSchoolYearSemester>();

            //人為設定選社學年
            string seting_school_year = "";

            //人為設定選社學期
            string seting_school_semester = "";

            opensemester = _AccessHelper.Select<UDT.OpenSchoolYearSemester>();

            //填入之前的紀錄
            if (opensemester.Count > 0)
            {
                seting_school_year = opensemester[0].SchoolYear;
                seting_school_semester = opensemester[0].Semester;
            }
            else
            {
                MsgBox.Show("沒有設定 選社學年、選社學期，請至'選社開放時間'功能內設定。");

            }

            Dictionary<string, VolunteerRecord> dic = new Dictionary<string, VolunteerRecord>();
            List<VolunteerRecord> VolList = new List<VolunteerRecord>();

            StringBuilder sb = new StringBuilder();

            //舊的  會載入 系統系統學期的社團清單
            //sb.AppendFormat("school_year={0} and semester={1}", School.DefaultSchoolYear, School.DefaultSemester);

            //新的 是載入 人為設定選社學年、學期
            sb.AppendFormat("school_year={0} and semester={1}", seting_school_year, seting_school_semester);


            VolList = tool._A.Select<VolunteerRecord>(sb.ToString());

            foreach (VolunteerRecord each in VolList)
            {
                if (!dic.ContainsKey(each.RefStudentID))
                {
                    dic.Add(each.RefStudentID, each);
                }
            }

            return dic;
        }

        /// <summary>
        /// 取得學生所填的志願序清單
        /// </summary>
        static public List<VolunteerRecord> GetStudentVolunteerDic(List<string> StudentIDList)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("ref_student_id in ('{0}')", string.Join("','", StudentIDList));
            List<VolunteerRecord> VolList = tool._A.Select<VolunteerRecord>(sb.ToString());
            return VolList;
        }

        /// <summary>
        /// 取得學生基本資料
        /// 學生系統編號/狀態/姓名/座號
        /// 班級ID/班級名稱/年級/班級序號
        /// 老師ID/老師名稱/老師暱稱
        /// (狀態為一般 or 延修)
        /// </summary>
        static public List<一名學生> GetStudentData()
        {
            List<一名學生> StudentList = new List<一名學生>();
            StringBuilder sb = new StringBuilder();
            sb.Append("select student.id as student_id,student.status,student.name as student_name,student.seat_no,student.gender,");
            sb.Append("class.id as class_id,class.class_name,class.grade_year,class.display_order,");
            sb.Append("teacher.id as teacher_id,teacher.teacher_name,teacher.nickname,");
            sb.Append("dept.name as dept_name ");
            sb.Append("from student join class on student.ref_class_id=class.id ");
            sb.Append("left join teacher on class.ref_teacher_id=teacher.id ");
            sb.Append("left join dept on class.ref_dept_id=dept.id ");
            sb.Append("where student.status in (1,2)");

            //取得學生選填志願序
            DataTable dt = tool._Q.Select(sb.ToString());

            foreach (DataRow each in dt.Rows)
            {
                一名學生 obj = new 一名學生(each);
                StudentList.Add(obj);
            }

            return StudentList;
        }

        /// <summary>
        /// 整理一名學生字典
        /// </summary>
        static public Dictionary<string, 一名學生> GetStudentDic(List<一名學生> StudentList)
        {
            Dictionary<string, 一名學生> dic = new Dictionary<string, 一名學生>();
            foreach (一名學生 each in StudentList)
            {
                if (!dic.ContainsKey(each.student_id))
                {
                    dic.Add(each.student_id, each);
                }
            }
            return dic;
        }

        /// <summary>
        /// 取得目前系統內
        /// 依據傳入的社團ID清單
        /// 取得社團相關記錄
        /// </summary>
        static public Dictionary<string, SCJoin> GetSCJDic(List<string> ClubIDList)
        {
            Dictionary<string, SCJoin> dic = new Dictionary<string, SCJoin>();

            List<SCJoin> list = tool._A.Select<SCJoin>(string.Format("ref_club_id in('{0}')", string.Join("','", ClubIDList)));

            foreach (SCJoin each in list)
            {
                if (!dic.ContainsKey(each.RefStudentID))
                {
                    dic.Add(each.RefStudentID, each);
                }

            }
            return dic;
        }

        /// <summary>
        /// 取得志願數
        /// </summary>
        /// <returns></returns>
        static public int GetVolumnteerCount()
        {
            int 學生選填志願數 = 1;
            List<ConfigRecord> list1 = tool._A.Select<ConfigRecord>(string.Format("config_name='{0}'", Tn.SetupName_1));
            if (list1.Count > 0)
            {
                int a = 1;
                int.TryParse(list1[0].Content, out a);
                學生選填志願數 = a;
            }
            return 學生選填志願數;
        }

        public enum 男女 { 男, 女, 不限制 }
    }
}
