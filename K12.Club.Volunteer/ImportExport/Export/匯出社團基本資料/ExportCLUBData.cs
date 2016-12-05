using System.Collections.Generic;
using System.Xml;
using FISCA.UDT;
using SmartSchool.API.PlugIn;
using K12.Data;
using System.Data;
using FISCA.DSAUtil;
using System.Text;

namespace K12.Club.Volunteer.CLUB
{
    class ExportCLUBData : SmartSchool.API.PlugIn.Export.Exporter
    {
        Dictionary<string, string> TeacherDic { get; set; }
        //建構子
        public ExportCLUBData()
        {
            this.Image = null;
            this.Text = "匯出社團基本資料";
        }

        //覆寫
        public override void InitializeExport(SmartSchool.API.PlugIn.Export.ExportWizard wizard)
        {
            List<string> FieldList = new List<string>();
            FieldList.Add("學年度"); //目前欄位
            FieldList.Add("學期"); //目前欄位
            FieldList.Add("社團名稱"); //目前欄位
            FieldList.Add("代碼");
            FieldList.Add("場地");
            FieldList.Add("類型");

            FieldList.Add("老師1");
            FieldList.Add("老師2");
            FieldList.Add("老師3");
            FieldList.Add("簡介");
            FieldList.Add("限制:性別");
            FieldList.Add("限制:科別");
            FieldList.Add("限制:一年級人數");
            FieldList.Add("限制:二年級人數");
            FieldList.Add("限制:三年級人數");
            FieldList.Add("限制:人數上限");

            //取得教師
            TeacherDic = GetTeacher();



            wizard.ExportableFields.AddRange(FieldList);

            wizard.ExportPackage += (sender, e) =>
            {
                //取得學生清單

                AccessHelper helper = new AccessHelper();

                string strCondition = string.Join("','", e.List);
                List<CLUBRecord> records = helper.Select<CLUBRecord>("uid in ('" + strCondition + "')");

                for (int i = 0; i < records.Count; i++)
                {
                    RowData row = new RowData();
                    row.ID = records[i].UID;

                    string teacher1 = "";
                    string teacher2 = "";
                    string teacher3 = "";
                    if (TeacherDic.ContainsKey(records[i].RefTeacherID))
                        teacher1 = TeacherDic[records[i].RefTeacherID];

                    if (TeacherDic.ContainsKey(records[i].RefTeacherID2))
                        teacher2 = TeacherDic[records[i].RefTeacherID2];

                    if (TeacherDic.ContainsKey(records[i].RefTeacherID3))
                        teacher3 = TeacherDic[records[i].RefTeacherID3];

                    foreach (string field in e.ExportFields)
                    {
                        if (wizard.ExportableFields.Contains(field))
                        {
                            switch (field)
                            {
                                case "學年度": row.Add(field, "" + records[i].SchoolYear); break;
                                case "學期": row.Add(field, "" + records[i].Semester); break;
                                case "社團名稱": row.Add(field, records[i].ClubName); break;
                                case "代碼": row.Add(field, records[i].ClubNumber); break;
                                case "場地": row.Add(field, records[i].Location); break;
                                case "類型": row.Add(field, records[i].ClubCategory); break;
                                case "老師1": row.Add(field, teacher1); break;
                                case "老師2": row.Add(field, teacher2); break;
                                case "老師3": row.Add(field, teacher3); break;
                                case "簡介": row.Add(field, records[i].About); break;
                                case "限制:性別": row.Add(field, records[i].GenderRestrict); break;
                                case "限制:科別": row.Add(field, GetRestrict(records[i].DeptRestrict)); break;
                                case "限制:一年級人數": row.Add(field, records[i].Grade1Limit.HasValue ? "" + records[i].Grade1Limit.Value : ""); break;
                                case "限制:二年級人數": row.Add(field, records[i].Grade2Limit.HasValue ? "" + records[i].Grade2Limit.Value : ""); break;
                                case "限制:三年級人數": row.Add(field, records[i].Grade3Limit.HasValue ? "" + records[i].Grade3Limit.Value : ""); break;
                                case "限制:人數上限": row.Add(field, records[i].Limit.HasValue ? "" + records[i].Limit.Value : ""); break;
                            }
                        }
                    }
                    e.Items.Add(row);
                }
            };
        }

        /// <summary>
        /// 取得教師名稱
        /// </summary>
        /// <param name="TeacherID"></param>
        /// <returns></returns>
        private Dictionary<string, string> GetTeacher()
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            DataTable dt = tool._Q.Select("select id,teacher_name,nickname from teacher where status=1");

            foreach (DataRow row in dt.Rows)
            {
                string id = "" + row["id"];
                string teacher_name = "" + row["teacher_name"];
                string nickname = "" + row["nickname"];
                if (!dic.ContainsKey(id))
                {
                    if (string.IsNullOrEmpty(nickname))
                        dic.Add(id, teacher_name);
                    else
                        dic.Add(id, teacher_name + "(" + nickname + ")");
                }
            }

            return dic;
        }

        /// <summary>
        /// 傳入科別內容,回傳以"/"號分隔的科別資料
        /// </summary>
        private string GetRestrict(string RestrictXml)
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(RestrictXml))
            {
                DSXmlHelper dsx = new DSXmlHelper();
                dsx.Load(RestrictXml);
                foreach (XmlElement xml in dsx.BaseElement.SelectNodes("Dept"))
                {
                    list.Add(xml.InnerText);
                }
            }

            return string.Join("/", list);
        }
    }
}