using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MOD_Club20.Report.ForHCH
{
    class Permissions
    {
        public static string 班級學生選社狀況一覽表 { get { return "MOD_Club20.Report.ForHCH"; } }
        public static bool 班級學生選社狀況一覽表權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[班級學生選社狀況一覽表].Executable;
            }
        }
    }
}
