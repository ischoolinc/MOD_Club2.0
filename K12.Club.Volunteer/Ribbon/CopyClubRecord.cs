using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    class CopyClubRecord
    {

        //社團
        public CLUBRecord _Club { get; set; }

        //社團學生參與清單
        public List<SCJoin> _scj { get; set; }

        //社團學生幹部
        public List<CadresRecord> _Cadres { get; set; }

        //新社團
        public CLUBRecord _new_Club { get; set; }

        //新社團學生參與清單
        public List<SCJoin> _new_scj { get; set; }

        //新社團學生幹部
        public List<CadresRecord> _new_Cadres { get; set; }

        public CopyClubRecord(CLUBRecord club)
        {
            _Club = club;
            //建立一個社團 / 社團學生的物件
            //並且傳入新社團的相關資料

            //本物件的功能
            //就是傳入新社團的ID
            //而本物件依據ID來建立學生記錄


        }

        /// <summary>
        /// 設定本社團的參與學生
        /// </summary>
        /// <param name="scj"></param>
        public void SetSCJ(List<SCJoin> scj)
        {
            _scj = scj;
        }

        /// <summary>
        /// 設定本社團的社團幹部
        /// </summary>
        /// <param name="scj"></param>
        public void SetSCJ(List<CadresRecord> cadres)
        {
            _Cadres = cadres;
        }

        /// <summary>
        /// 取得新的參與學生清單
        /// </summary>
        public List<SCJoin> GetNewSCJoinList()
        {
            List<SCJoin> list = new List<SCJoin>();
            if (_new_Club != null && _scj.Count != 0)
            {
                foreach (SCJoin each in _scj)
                {
                    SCJoin scj = new SCJoin();
                    scj.RefStudentID = each.RefStudentID;
                    scj.RefClubID = _new_Club.UID;
                    scj.Lock = each.Lock; //鎖定
                    list.Add(scj);
                }
            }
            return list;
        }

        /// <summary>
        /// 取得新的社團幹部對象
        /// </summary>
        public List<CadresRecord> GetNewCadresList()
        {
            List<CadresRecord> list = new List<CadresRecord>();
            if (_new_Club != null && _Cadres.Count != 0)
            {
                foreach (CadresRecord each in _Cadres)
                {
                    CadresRecord cadre = new CadresRecord();
                    cadre.RefStudentID = each.RefStudentID; //學生
                    cadre.CadreName = each.CadreName; //幹部名稱
                    cadre.RefClubID = _new_Club.UID;

                    list.Add(cadre);
                }
            }
            return list;
        }
    }
}
