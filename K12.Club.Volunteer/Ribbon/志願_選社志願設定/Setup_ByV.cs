using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    public class Setup_ByV
    {
        public int 學生選填志願數 { get; set; }
        public bool 社團分配優先序 { get; set; }
        public bool 已有社團記錄時 { get; set; }
        public bool 學生可經由WEB查詢選社結果 { get; set; }

        public Setup_ByV()
        {
            學生選填志願數 = 1;
            社團分配優先序 = false;
            已有社團記錄時 = false;
            學生可經由WEB查詢選社結果 = false;

            List<ConfigRecord> list1 = tool._A.Select<ConfigRecord>(string.Format("config_name='{0}'", Tn.SetupName_1));
            if (list1.Count > 0)
            {
                int a = 1;
                int.TryParse(list1[0].Content, out a);
                學生選填志願數 = a;
            }

            List<ConfigRecord> list2 = tool._A.Select<ConfigRecord>(string.Format("config_name='{0}'", Tn.SetupName_2));
            if (list2.Count > 0)
            {
                bool b = false;
                bool.TryParse(list2[0].Content, out b);
                社團分配優先序 = b;
            }

            List<ConfigRecord> list3 = tool._A.Select<ConfigRecord>(string.Format("config_name='{0}'", Tn.SetupName_3));
            if (list3.Count > 0)
            {
                bool c = false;
                bool.TryParse(list3[0].Content, out c);
                已有社團記錄時 = c;
            }

            List<ConfigRecord> list4 = tool._A.Select<ConfigRecord>(string.Format("config_name='{0}'", Tn.SetupName_4));
            if (list4.Count > 0)
            {
                bool d = false;
                bool.TryParse(list4[0].Content, out d);
                學生可經由WEB查詢選社結果 = d;
            }
        }
    }
}
