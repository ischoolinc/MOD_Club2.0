using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace K12.Club.Volunteer
{
    public class 一名學生
    {
        //學生
        public string student_id { get; set; }
        public string status { get; set; }
        public string student_name { get; set; }
        public string student_number { get; set; }
        public int? seat_no { get; set; }
        public GetVolunteerData.男女 gender { get; set; }
        //班級
        public string class_id { get; set; }
        public string class_name { get; set; }
        public string display_order { get; set; }
        public string grade_year { get; set; }

        //科別
        public string ref_dept_id { get; set; }
        public string dept_name { get; set; }

        //教師
        public string teacher_id { get; set; }
        public string teacher_name { get; set; }
        public string nickname { get; set; }

        public 一名學生(DataRow row)
        {
            student_id = "" + row["student_id"];
            status = "" + row["status"];
            student_name = "" + row["student_name"];
            student_number = "" + row["student_number"];

            #region 性別
            if ("" + row["gender"] == "1")
            {
                gender = GetVolunteerData.男女.男;
            }
            else if ("" + row["gender"] == "0")
            {
                gender = GetVolunteerData.男女.女;
            }
            else
            {
                gender = GetVolunteerData.男女.不限制;
            } 
            #endregion

            #region 座號
            int a;
            if (int.TryParse("" + row["seat_no"], out a))
            {
                seat_no = a;
            }
            #endregion

            class_id = "" + row["class_id"];
            class_name = "" + row["class_name"];
            display_order = "" + row["display_order"];
            grade_year = "" + row["grade_year"];

            string dept_text = "" + row["dept_name"];
            if (!string.IsNullOrEmpty(dept_text))
            {
                string[] namelist = dept_text.Split(':');
                dept_name = "" + namelist.GetValue(0);
            }

            teacher_id = "" + row["teacher_id"];
            teacher_name = "" + row["teacher_name"];
            nickname = "" + row["nickname"];
        }
    }
}
