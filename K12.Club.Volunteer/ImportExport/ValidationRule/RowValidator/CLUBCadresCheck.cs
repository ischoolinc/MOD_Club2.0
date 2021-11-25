using Campus.DocumentValidator;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K12.Club.Volunteer
{
    class CLUBCadresCheck : IRowVaildator
    {

        /// <summary>
        /// 學年度,學期,社團名稱 / 是否包含此學生
        /// </summary>
        Dictionary<string, List<string>> ClubNameDic { get; set; }

        private Task mTask;
        public CLUBCadresCheck()
        {
            //檢查學號(學生)是否存在這個社團內
            ClubNameDic = new Dictionary<string, List<string>>();
            mTask = Task.Factory.StartNew(() =>
            {
                DataTable dt = tool._Q.Select(@"select clubrecord.uid,scjoin.ref_student_id,student.student_number,
clubrecord.school_year,clubrecord.semester,clubrecord.club_name
from $k12.clubrecord.universal clubrecord
join $k12.scjoin.universal scjoin on clubrecord.uid=cast(scjoin.ref_club_id as integer)
join student on student.id=cast(scjoin.ref_student_id as integer)");

                foreach (DataRow row in dt.Rows)
                {
                    string school_year = "" + row["school_year"];
                    string semester = "" + row["semester"];
                    string club_name = "" + row["club_name"];
                    string student_number = "" + row["student_number"];


                    string ClubKey = school_year + "_" + semester + "_" + club_name;
                    if (!ClubNameDic.ContainsKey(ClubKey))
                    {
                        ClubNameDic.Add(ClubKey, new List<string>());
                    }
                    ClubNameDic[ClubKey].Add(student_number);
                }
            });

        }


        public bool Validate(IRowStream Value)
        {
            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("學號"))
            {
                string school_year = Value.GetValue("學年度");
                string semester = Value.GetValue("學期");
                string club_name = Value.GetValue("社團名稱");
                string student_number = Value.GetValue("學號");
                string ClubKey = school_year + "_" + semester + "_" + club_name;

                mTask.Wait();

                if (ClubNameDic.ContainsKey(ClubKey))
                {
                    if (ClubNameDic[ClubKey].Contains(student_number))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public string Correct(IRowStream Value)
        {
            return string.Empty;
        }

        public string ToString(string template)
        {
            return template;
        }
    }
}
