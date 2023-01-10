using FISCA.UDT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K12.Club.Volunteer
{
    /// <summary>
    /// 評語代碼表
    /// </summary>
    [TableName("K12.ClubComment.Universal")]
    class ClubComment : ActiveRecord
    {
        /// <summary>
        /// 代碼
        /// </summary>
        [Field(Field = "code", Indexed = true)]
        public string code { get; set; }

        /// <summary>
        /// 內容
        /// </summary>
        [Field(Field = "comment", Indexed = true)]
        public string Comment { get; set; }
    }
}
