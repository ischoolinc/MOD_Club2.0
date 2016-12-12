using FISCA.Data;
using FISCA.UDT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.General.ImportClubScore
{
    public static class Global
    {
        public static readonly bool IsDebug = true;

        public static AccessHelper _A = new AccessHelper();
        public static QueryHelper _Q = new QueryHelper();

        public static readonly string NewLine = "\r\n";

        #region 匯入的欄位名稱
        /// <summary>
        /// 學號
        /// </summary>
        public static readonly string _ColStudentNumber = "學號";
        /// <summary>
        /// 學年度
        /// </summary>
        public static readonly string _ColScholYear = "學年度";
        /// <summary>
        /// 學期
        /// </summary>
        public static readonly string _ColSemester = "學期";
        /// <summary>
        /// 社團名稱
        /// </summary>
        public static readonly string _ColClubName = "社團名稱";
        /// <summary>
        /// 學期成績
        /// </summary>
        public static readonly string _ColClubScore = "學期成績";
        /// <summary>
        /// 幹部名稱
        /// </summary>
        public static readonly string _ColCadreName = "幹部名稱";
        /// <summary>
        /// 社團評等
        /// </summary>
        public static readonly string _ColClubLevel = "社團評等";
        
        #endregion

        /// <summary>
        /// 所有學生學號與ID的暫存 key:StudentNumber; value:StudentID
        /// </summary>
        public static Dictionary<string, string> _AllStudentNumberIDTemp = new Dictionary<string, string>();
    }
}
