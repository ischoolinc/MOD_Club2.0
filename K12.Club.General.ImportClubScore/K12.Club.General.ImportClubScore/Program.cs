using Campus.DocumentValidator;
using FISCA.Permission;
using FISCA.Presentation;
using K12.Club.General.ImportClubScore.ImportExport.Import.匯入社團學期成績;
using K12.Club.General.ImportClubScore.ImportExport.ValidationRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K12.Club.General.ImportClubScore
{
    public class Program
    {
        [FISCA.MainMethod]
        public static void main()
        {
            #region 自訂驗證規則
            FactoryProvider.FieldFactory.Add(new CLUBScoreFieldValidatorFactory());
            #endregion

            RibbonBarButton btnReport = MotherForm.RibbonBarItems["學生", "資料統計"]["匯入"];
            btnReport["社團相關匯入"]["匯入社團學期成績"].Enable = Permissions.IsEnableCLUBScoreImport;

            btnReport["社團相關匯入"]["匯入社團學期成績"].Click += delegate
            {
                // 準備所有一般生的學生ID, 之後驗證資料時會用到
                Global._AllStudentNumberIDTemp = DAO.FDQuery.GetAllStudenNumberDict();
                FrmImportClubScore frmImport = new FrmImportClubScore();
                frmImport.Execute();
            };

            // 在權限畫面出現"匯入社團學期成績"權限
            Catalog catalog1 = RoleAclSource.Instance["學生"]["匯出/匯入"];
            catalog1.Add(new RibbonFeature(Permissions.KeyCLUBScoreImport, "匯入社團學期成績"));
        }
    }
}
