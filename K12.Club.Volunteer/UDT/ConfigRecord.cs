using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA.UDT;

namespace K12.Club.Volunteer
{
    //專門用來儲存
    //使用者相關設定檔
    [TableName("K12.Config.Universal")]
    class ConfigRecord : ActiveRecord
    {
        /// <summary>
        /// 設定名稱
        /// </summary>
        [Field(Field = "config_name", Indexed = false)]
        public string ConfigName { get; set; }

        /// <summary>
        /// 設定內容
        /// </summary>
        [Field(Field = "content", Indexed = false)]
        public string Content { get; set; }
    }
}
