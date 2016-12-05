using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using K12.Data;
using System.Data;

namespace K12.Club.Volunteer
{
    class StudentNumberStatusValidator : IFieldValidator
    {
        Dictionary<string, bool> _StudentDic { get; set; }
        public StudentNumberStatusValidator()
        {
            _StudentDic = GetStudent();
        }

        #region IFieldValidator 成員

        public string Correct(string Value)
        {
            return Value;
        }

        public string ToString(string template)
        {
            return template;
        }

        public bool Validate(string Value)
        {
            if (_StudentDic.ContainsKey(Value)) //包含此學號
            {
                return _StudentDic[Value];//True表示學生為一般生
            }
            return false;
        }

        /// <summary>
        /// 取得學生學號 vs 系統編號
        /// </summary>
        private Dictionary<string, bool> GetStudent()
        {
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            DataTable dt = tool._Q.Select("select id,student_number,status from student");
            foreach (DataRow row in dt.Rows)
            {
                string StudentID = "" + row[0];
                string Student_Number = "" + row[1];
                string Status = "" + row[2];

                if (string.IsNullOrEmpty(Student_Number))
                    continue;
                if (!dic.ContainsKey(Student_Number))
                {
                    if (Status == "1" || Status == "2")
                    {
                        //狀況是一般或延修
                        dic.Add(Student_Number, true);
                    }
                    else
                    {
                        //狀態不為一般或延修,或不存在系統
                        dic.Add(Student_Number, false);
                    }
                }
                else
                {
                    //當學號重覆於一般狀態之外時,為錯誤
                    dic[Student_Number] = false;
                }

            }
            return dic;

        }

        #endregion
    }

    class StudIsStatus
    {
        public StudIsStatus(string g, string u)
        {
            Ststus = g;
            StudentNumber = u;
        }

        public string Ststus { get; set; }
        public string StudentNumber { get; set; }
    }
}
