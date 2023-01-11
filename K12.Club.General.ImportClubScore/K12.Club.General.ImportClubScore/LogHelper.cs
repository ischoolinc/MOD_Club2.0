using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.General.ImportClubScore
{
    public class LogHelper
    {
        
        private Dictionary<string, ClubScorePair> clubScorePairDic = new Dictionary<string,ClubScorePair>();

        /// <summary>
        /// Key: UID, Value: 原始跟更新後的社團學期成績
        /// </summary>
        public Dictionary<string, ClubScorePair> ClubScorePairDic
        {
            get
            {
                return clubScorePairDic;
            }
        }

        /// <summary>
        /// 新增尚未更新的原始資料
        /// </summary>
        /// <param name="rec"></param>
        public void SaveOldRecForLog(DAO.ResultScoreRecord rec)
        {
            if (!clubScorePairDic.ContainsKey(rec.UID))
                clubScorePairDic.Add(rec.UID, new ClubScorePair());
            clubScorePairDic[rec.UID]._OldRec = Utility.CopyResultScoreRecord(rec);
        }

        /// <summary>
        /// 新增更新後的資料
        /// </summary>
        /// <param name="rec"></param>
        public void SaveNewRecForLog(DAO.ResultScoreRecord rec)
        {
            if (!clubScorePairDic.ContainsKey(rec.UID))
                clubScorePairDic.Add(rec.UID, new ClubScorePair());
            clubScorePairDic[rec.UID]._NewRec = Utility.CopyResultScoreRecord(rec);
        }

        /// <summary>
        /// 取得新增的log字串
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="studentNumber"></param>
        /// <returns></returns>
        public string ComposeInsertLogString(DAO.ResultScoreRecord rec, string studentNumber)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Global._ColScholYear).Append("「").Append(rec.SchoolYear).Append("」");
            sb.Append(Global._ColSemester).Append("「").Append(rec.Semester).Append("」");
            sb.Append(Global._ColClubName).Append("「").Append(rec.ClubName).Append("」");
            sb.Append(Global.NewLine);

            sb.Append(Global._ColStudentNumber).Append("「").Append(studentNumber).Append("」");
            sb.Append(Global.NewLine);

            if (rec.ResultScore.HasValue)
            {
                sb.Append(Global._ColClubScore).Append("「").Append(rec.ResultScore.Value).Append("」");
                sb.Append(Global.NewLine);
            }
            else
            {
                sb.Append(Global._ColClubScore).Append("「").Append("」");
                sb.Append(Global.NewLine);
            }

            sb.Append(Global._ColCadreName).Append("「").Append(rec.CadreName).Append("」");
            // 評語
            sb.Append(Global._ColComment).Append("「").Append(rec.Comment).Append("」");
            sb.Append(Global.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// 取得更新的log字串
        /// </summary>
        /// <param name="recUid"></param>
        /// <param name="studentNumber"></param>
        /// <returns></returns>
        public string ComposeUpdateLogString(ClubScorePair pair, string studentNumber)
        {
            //檢查與確認資料是否被修改
            StringBuilder sb = new StringBuilder();

            sb.Append(Global._ColScholYear).Append("「").Append(pair._NewRec.SchoolYear).Append("」");
            sb.Append(Global._ColSemester).Append("「").Append(pair._NewRec.Semester).Append("」");
            sb.Append(Global._ColClubName).Append("「").Append(pair._NewRec.ClubName).Append("」");
            //評語
            sb.Append(Global._ColComment).Append("「").Append(pair._NewRec.Comment).Append("」");
            sb.Append(Global.NewLine);

            sb.Append(Global._ColStudentNumber).Append("「").Append(studentNumber).Append("」");
            sb.Append(Global.NewLine);


            if (pair._OldRec.ResultScore != pair._NewRec.ResultScore)
                sb.AppendLine(ByOne(Global._ColClubScore, pair._OldRec.ResultScore, pair._NewRec.ResultScore));

            if (pair._OldRec.CadreName != pair._NewRec.CadreName)
                sb.AppendLine(ByOne(Global._ColCadreName, pair._OldRec.CadreName, pair._NewRec.CadreName));

            // 評語
            if (pair._OldRec.Comment != pair._NewRec.Comment)
                sb.AppendLine(ByOne(Global._ColComment, pair._OldRec.Comment, pair._NewRec.Comment));

            return sb.ToString();
            
        }

        private string ByOne(string name, string oldValue, string newValue)
        {
            return string.Format("「{0}」由「{1}」修改為「{2}」", name, oldValue, newValue);
        }

        private string ByOne(string name, decimal? oldValue, decimal? newValue)
        {
            return ByOne(name, oldValue.HasValue ? "" + oldValue.Value : "", newValue.HasValue ? "" + newValue.Value : "");
        }
    }

    public class ClubScorePair
    {
        public DAO.ResultScoreRecord _OldRec;
        public DAO.ResultScoreRecord _NewRec;
    }
}
