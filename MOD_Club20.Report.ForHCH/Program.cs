using FISCA;
using FISCA.Permission;
using FISCA.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MOD_Club20.Report.ForHCH
{

    public class Program
    {
        [MainMethod()]
        static public void Main()
        {
            string ReportName = "社團參與學生清單(新竹高中)";
            RibbonBarItem InClass = FISCA.Presentation.MotherForm.RibbonBarItems["班級", "資料統計"];
            InClass["報表"]["社團相關報表"][ReportName].Enable = false;
            InClass["報表"]["社團相關報表"][ReportName].Click += delegate
            {
                ElectionForm ef = new ElectionForm();
                ef.ShowDialog();

            };

            K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
            {
                //是否選擇大於0的社團
                bool SourceCount = (K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0);

                bool a = (SourceCount && Permissions.班級學生選社狀況一覽表權限);
                InClass["報表"]["社團相關報表"][ReportName].Enable = a;

            };

            Catalog detail1= RoleAclSource.Instance["班級"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.班級學生選社狀況一覽表, ReportName));
        }
    }
}
