using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AllProveReport
{
    class Permissions
    {
        public static string 社團參與證明單 { get { return "Student_AllProveReport.cs.Aki"; } }
        public static bool 社團參與證明單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團參與證明單].Executable;
            }
        }
    }
}
