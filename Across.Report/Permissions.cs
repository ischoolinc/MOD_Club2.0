using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Across.Report
{
    class Permissions
    {
        public static string 社團中英文對照表 { get { return "MOD_Club_Acrossdivisions.EnglishTableForm.cs"; } }
        public static bool 社團中英文對照表權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團中英文對照表].Executable;
            }
        }

        public static string 社團參與證明單_英文 { get { return "MOD_Club_Acrossdivisions.EnglishSocietiesProveSingle.cs"; } }
        public static bool 社團參與證明單_英文權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團參與證明單_英文].Executable;
            }
        }
    }
}
