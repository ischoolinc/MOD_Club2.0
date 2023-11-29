using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Campus.DocumentValidator;
using FISCA.Data;
using System.Data;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 檢查社長副社長是否在資料上重複
    /// </summary>
    class CheckPresidentRow : IRowVaildator
    {
        private List<string> _listStudentNumber;

        QueryHelper qh = new QueryHelper();

        public CheckPresidentRow()
        {
            _listStudentNumber = new List<string>();

        }

        #region IRowVaildator 成員

        public bool Validate(IRowStream Value)
        {
            bool check = true;

            //查找系統內是不是已經有相關幹部資料

            if (Value.Contains("學年度") && Value.Contains("學期") && Value.Contains("社團名稱") && Value.Contains("幹部名稱"))
            {
                string SchoolYear = Value.GetValue("學年度");
                string Semester = Value.GetValue("學期");
                string CourseName = Value.GetValue("社團名稱");
                string CadreStudent = Value.GetValue("幹部名稱");
                if (CadreStudent == "社長" || CadreStudent == "副社長")
                {
                    string CourseKey = SchoolYear + "," + Semester + "," + CourseName;

                    if (!_listStudentNumber.Contains(CourseKey))
                    {
                        _listStudentNumber.Add(CourseKey);
                    }
                    else
                    {
                        //重複社長資料
                        check = false;
                    }
                }
            }

            return check;
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

