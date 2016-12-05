using Campus.DocumentValidator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.General.ImportClubScore
{
    public class Utility
    {
        /// <summary>
        /// 透過輸入的欄位名稱, 取得匯入資料的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetIRowValueString(IRowStream row, string name)
        {
            if (row.Contains(name))
            {
                if (string.IsNullOrEmpty(row.GetValue(name)))
                    return "";

                return row.GetValue(name).Trim();
            }
            else
                return "";
        }

        /// <summary>
        /// 透過輸入的欄位名稱, 取得匯入資料的值, 並轉成int
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetIRowValueInt(IRowStream row, string name)
        {
            if (row.Contains(name))
            {
                if (string.IsNullOrEmpty(row.GetValue(name)))
                    return -1;
                int retVal;
                if (int.TryParse(row.GetValue(name).Trim(), out retVal))
                    return retVal;
                else
                    return -1;
            }
            else
                return -1;
        }

        /// <summary>
        /// 透過輸入的欄位名稱, 取的匯入資料的值, 並轉成decimal
        /// </summary>
        /// <param name="row"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static decimal? GetIRowValueDecimal(IRowStream row, string name)
        {
            if (row.Contains(name))
            {
                if (string.IsNullOrEmpty(row.GetValue(name)))
                    return null;
                decimal retVal;
                if (decimal.TryParse(row.GetValue(name).Trim(), out retVal))
                    return retVal;
                else
                    return null;
            }
            else
                return null;
        }

        /// <summary>
        /// 檢查 "學號, 學年度, 學期, 社團名稱"有沒有資料
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static bool CheckRowData(IRowStream row)
        {
            string[] checkAry = { Global._ColStudentNumber, Global._ColScholYear,
                                    Global._ColSemester, Global._ColClubName };
            
            // 檢查 "學號, 學年度, 學期, 社團名稱"有沒有資料
            foreach (string str in checkAry)
            {
                string tmp = Utility.GetIRowValueString(row, str);
                if (string.IsNullOrEmpty(tmp)) return false;
            }

            return true;
        }

        /// <summary>
        /// 找出是否有已存在資料
        /// </summary>
        /// <returns></returns>
        public static DAO.ResultScoreRecord GetClubScoreRecord(List<DAO.ResultScoreRecord> clubScoreList,
                                                                string studentId, int schoolYear, int semester, string clubName)
        {
            foreach (DAO.ResultScoreRecord rec in clubScoreList)
            {
                if (rec.RefStudentID == studentId && rec.SchoolYear == schoolYear &&
                        rec.Semester == semester && rec.ClubName == clubName)
                {
                    return rec;
                }
            }

            return null;
        }

        public static DAO.ResultScoreRecord CopyResultScoreRecord(DAO.ResultScoreRecord rec)
        {
            DAO.ResultScoreRecord newRec = new DAO.ResultScoreRecord();
            newRec.CadreName = rec.CadreName;
            newRec.ClubName = rec.ClubName;
            newRec.Deleted = rec.Deleted;
            // newRec.DSConnection = rec.DSConnection;
            // newRec.RecordStatus = rec.RecordStatus;
            newRec.RefClubID = rec.RefClubID;
            newRec.RefSCJoinID = rec.RefSCJoinID;
            newRec.RefStudentID = rec.RefStudentID;
            newRec.ResultScore = rec.ResultScore;
            newRec.SchoolYear = rec.SchoolYear;
            newRec.Semester = rec.Semester;
            // newRec.UID = rec.UID;

            return newRec;
        }
    }
}
