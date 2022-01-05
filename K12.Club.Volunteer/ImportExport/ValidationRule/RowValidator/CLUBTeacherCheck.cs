using Campus.DocumentValidator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K12.Club.Volunteer
{
    class CLUBTeacherCheck : IRowVaildator
    {


        public CLUBTeacherCheck()
        {

        }

        #region IRowVaildator 成員

        public bool Validate(IRowStream Value)
        {
            List<string> TeacherList = new List<string>();
            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("老師1"))
            {
                if (!string.IsNullOrEmpty(Value.GetValue("老師1")))
                {
                    string SchoolYear = Value.GetValue("學年度").Trim();
                    string Semester = Value.GetValue("學期");
                    string ClubName = Value.GetValue("社團名稱").Trim();
                    string TeacherName1 = Value.GetValue("老師1").Trim();
                    string ClubKey = SchoolYear + "," + Semester + "," + ClubName + "," + TeacherName1;

                    if (!TeacherList.Contains(ClubKey))
                    {
                        TeacherList.Add(ClubKey);
                    }
                }
            }

            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("老師2"))
            {
                if (!string.IsNullOrEmpty(Value.GetValue("老師2")))
                {
                    string SchoolYear = Value.GetValue("學年度").Trim();
                    string Semester = Value.GetValue("學期").Trim();
                    string ClubName = Value.GetValue("社團名稱").Trim();
                    string TeacherName2 = Value.GetValue("老師2").Trim();
                    string ClubKey = SchoolYear + "," + Semester + "," + ClubName + "," + TeacherName2;

                    if (!TeacherList.Contains(ClubKey))
                    {
                        TeacherList.Add(ClubKey);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("老師3"))
            {
                if (!string.IsNullOrEmpty(Value.GetValue("老師3")))
                {
                    string SchoolYear = Value.GetValue("學年度").Trim();
                    string Semester = Value.GetValue("學期").Trim();
                    string ClubName = Value.GetValue("社團名稱").Trim();
                    string TeacherName3 = Value.GetValue("老師3").Trim();
                    string ClubKey = SchoolYear + "," + Semester + "," + ClubName + "," + TeacherName3;

                    if (!TeacherList.Contains(ClubKey))
                    {
                        TeacherList.Add(ClubKey);
                    }
                    else
                    {
                        return false;
                    }
                }
            }



            return true;
        }

        /// <summary>
        /// 沒有提供自動修正
        /// </summary>
        public string Correct(IRowStream Value)
        {
            return string.Empty;
        }

        /// <summary>
        /// 傳回預設樣版
        /// </summary>
        public string ToString(string template)
        {
            return template;
        }

        #endregion
    }
}
