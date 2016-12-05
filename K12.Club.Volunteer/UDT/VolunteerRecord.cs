using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace K12.Club.Volunteer
{
    [TableName("K12.Volunteer.Universal")]
    public class VolunteerRecord : ActiveRecord
    {
        /// <summary>
        /// 淺層複製CLUBRecord
        /// </summary>
        public VolunteerRecord CopyExtension()
        {
            return (VolunteerRecord)this.MemberwiseClone();
        }

        /// <summary>
        /// 學年度
        /// </summary>
        [Field(Field = "school_year", Indexed = false)]
        public int SchoolYear { get; set; }

        /// <summary>
        /// 學期
        /// </summary>
        [Field(Field = "semester", Indexed = false)]
        public int Semester { get; set; }

        /// <summary>
        /// 學生參考
        /// </summary>
        [Field(Field = "ref_student_id", Indexed = false)]
        public string RefStudentID { get; set; }

        /// <summary>
        /// 學生志願內容
        /// </summary>
        [Field(Field = "content", Indexed = false)]
        public string Content { get; set; }

    }
}
