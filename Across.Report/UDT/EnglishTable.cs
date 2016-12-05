using FISCA.UDT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Across.Report
{
    [TableName("MOD_Club_Acrossdivisions.EnglishTable")]
    class EnglishTable : ActiveRecord
    {
        /// <summary>
        /// 社團名稱
        /// </summary>
        [Field(Field = "club_name", Indexed = true)]
        public string ClubName { get; set; }

        /// <summary>
        /// 英文名稱
        /// </summary>
        [Field(Field = "english", Indexed = true)]
        public string English { get; set; }
    }
}
