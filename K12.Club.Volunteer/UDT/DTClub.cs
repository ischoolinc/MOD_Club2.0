using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace K12.Club.Volunteer
{
    //設定學校的開始選社時間
    [TableName("K12.DTClub.Universal")]
    class DTClub : ActiveRecord
    {
        /// <summary>
        /// 年級
        /// </summary>
        [Field(Field = "gradeyear", Indexed = false)]
        public int GradeYear { get; set; }

        ///// <summary>
        ///// 開始時間
        ///// </summary>
        //[Field(Field = "startDate", Indexed = false)]
        //public DateTime? Start { get; set; }

        ///// <summary>
        ///// 結束時間
        ///// </summary>
        //[Field(Field = "endDate", Indexed = false)]
        //public DateTime? End { get; set; }


        /// <summary>
        /// 開始時間1
        /// </summary>
        [Field(Field = "startDate1", Indexed = false)]
        public DateTime? Start1 { get; set; }

        /// <summary>
        /// 結束時間1
        /// </summary>
        [Field(Field = "endDate1", Indexed = false)]
        public DateTime? End1 { get; set; }

        /// <summary>
        /// 階段1模式
        /// </summary>
        [Field(Field = "Stage1_Mode", Indexed = false)]
        public string Stage1_Mode { get; set; }


        /// <summary>
        /// 開始時間1
        /// </summary>
        [Field(Field = "startDate2", Indexed = false)]
        public DateTime? Start2 { get; set; }

        /// <summary>
        /// 結束時間1
        /// </summary>
        [Field(Field = "endDate2", Indexed = false)]
        public DateTime? End2 { get; set; }

        /// <summary>
        /// 階段2模式
        /// </summary>
        [Field(Field = "Stage2_Mode", Indexed = false)]
        public string Stage2_Mode { get; set; }

    }
}
