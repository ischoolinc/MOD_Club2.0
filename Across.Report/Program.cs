using FISCA;
using FISCA.Permission;
using FISCA.Presentation;
using K12.Data.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Across.Report
{
    public class Program
    {
        [MainMethod()]
        static public void Main()
        {

            #region 處理UDT Table沒有的問題

            ConfigData cd = K12.Data.School.Configuration["跨部別社團UDT載入設定"];
            bool checkClubUDT = false;

            string name = "社團UDT是否已載入_20140709";
            //如果尚無設定值,預設為
            if (string.IsNullOrEmpty(cd[name]))
            {
                cd[name] = "false";
            }

            //檢查是否為布林
            bool.TryParse(cd[name], out checkClubUDT);

            if (!checkClubUDT)
            {
                tool._A.Select<EnglishTable>("UID = '00000'");

                cd[name] = "true";
                cd.Save();
            }

            #endregion

            //RibbonBarItem InClub = FISCA.Presentation.MotherForm.RibbonBarItems["志願序社團", "其它"];
            //InClub["社團中英文管理"].Image = Properties.Resources.copy_refresh_64;
            //InClub["社團中英文管理"].Enable = Permissions.社團中英文對照表權限;
            //InClub["社團中英文管理"].Click += delegate
            //{
            //    EnglishTableForm sot = new EnglishTableForm();
            //    sot.ShowDialog();
            //};

            RibbonBarItem InStudent = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "資料統計"];
            InStudent["報表"]["社團相關報表"]["社團參與證明單(英文)"].Enable = Permissions.社團參與證明單_英文權限;
            InStudent["報表"]["社團相關報表"]["社團參與證明單(英文)"].Click += delegate
            {
                EnglishSocietiesProveSingle esps = new EnglishSocietiesProveSingle();
                esps.ShowDialog();
            };

            Catalog detail1;
            //detail1 = RoleAclSource.Instance["社團"]["功能項目"];
            //detail1.Add(new RibbonFeature(Permissions.社團中英文對照表, "社團中英文對照表"));

            detail1 = RoleAclSource.Instance["學生"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.社團參與證明單_英文, "社團參與證明單_英文"));
        }
    }
}
