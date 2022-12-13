using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace K12.Club.Volunteer
{
    class Permissions
    {
        public static string 新增社團 { get { return "K12.Club.Universal.NewAddClub.cs"; } }
        public static bool 新增社團權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[新增社團].Executable;
            }
        }

        public static string 複製社團 { get { return "K12.Club.Universal.CopyClub.cs"; } }
        public static bool 複製社團權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[複製社團].Executable;
            }
        }

        public static string 刪除社團 { get { return "K12.Club.Universal.DeleteClub.cs"; } }
        public static bool 刪除社團權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[刪除社團].Executable;
            }
        }

        public static string 社團基本資料 { get { return "K12.Club.Universal.ClubDetailItem.cs"; } }
        public static bool 社團基本資料權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團基本資料].Executable;
            }
        }

        public static string 社團照片 { get { return "K12.Club.Universal.ClubImageItem.cs"; } }
        public static bool 社團照片權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團照片].Executable;
            }
        }

        public static string 社團限制 { get { return "K12.Club.Universal.ClubRestrictItem.cs"; } }
        public static bool 社團限制權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團限制].Executable;
            }
        }

        public static string 社團參與學生 { get { return "K12.Club.Universal.ClubStudent.cs"; } }
        public static bool 社團參與學生權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團參與學生].Executable;
            }
        }

        public static string 未選社團學生 { get { return "K12.Club.Universal.CheckStudentIsNotInClub.cs"; } }
        public static bool 未選社團學生權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[未選社團學生].Executable;
            }
        }
        
        public static string 調整社團學生 { get { return "K12.Club.Universal.SplitClasses.cs"; } }
        public static bool 調整社團學生權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[調整社團學生].Executable;
            }
        }

        public static string 檢查批次社團鎖社 { get { return "K12.Club.Universal.MutipleLock.cs"; } }
        public static bool 檢查批次社團鎖社權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[檢查批次社團鎖社].Executable;
            }
        }

        public static string 評量項目 { get { return "K12.Club.Universal.GradingProjectConfig.cs"; } }
        public static bool 評量項目權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[評量項目].Executable;
            }
        }

        public static string 社團幹部 { get { return "K12.Club.Universal.CadresItem.cs"; } }
        public static bool 社團幹部權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團幹部].Executable;
            }
        }

        public static string 成績輸入 { get { return "K12.Club.Universal.ClubResultsInput.cs"; } }
        public static bool 成績輸入權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[成績輸入].Executable;
            }
        }

        public static string 開放選社時間 { get { return "K12.Club.Universal.OpenClubJoinDateTime.cs"; } }
        public static bool 開放選社時間權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[開放選社時間].Executable;
            }
        }

        public static string 成績輸入時間 { get { return "K12.Club.Universal.ResultsInputDateTime.cs"; } }
        public static bool 成績輸入時間權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[成績輸入時間].Executable;
            }
        }

        public static string 社團點名單 { get { return "K12.Club.Universal.ClubPointList.cs"; } }
        public static bool 社團點名單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團點名單].Executable;
            }
        }

        public static string 社團點名單_套表列印 { get { return "K12.Club.Universal.ClubPointsListForm.cs"; } }
        public static bool 社團點名單_套表列印權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團點名單_套表列印].Executable;
            }
        }

        public static string 班級學生選社_確認表 { get { return "K12.Club.Universal.ElectionSocialConfirmation.cs"; } }
        public static bool 班級學生選社_確認表_權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[班級學生選社_確認表].Executable;
            }
        }

        public static string 重覆選社檢查 { get { return "K12.Club.Universal.RepeatForm.cs"; } }
        public static bool 重覆選社檢查權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[重覆選社檢查].Executable;
            }
        }

        public static string 學期結算 { get { return "K12.Club.Universal.ClearingForm.cs"; } }
        public static bool 學期結算權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[學期結算].Executable;
            }
        }

        public static string 學生社團成績_資料項目 { get { return "K12.Club.Universal.StudentResultItem.cs"; } }
        public static bool 學生社團成績_資料項目權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[學生社團成績_資料項目].Executable;
            }
        }

        public static string 社團成績單 { get { return "K12.Club.Universal.ClubTranscript.cs"; } }
        public static bool 社團成績單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團成績單].Executable;
            }
        }

        public static string 班級社團成績單 { get { return "K12.Club.Universal.ClassClubTranscript.cs"; } }
        public static bool 班級社團成績單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[班級社團成績單].Executable;
            }
        }

        public static string 社團概況表 { get { return "K12.Club.Universal.CLUBFactsTable.cs"; } }
        public static bool 社團概況表權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團概況表].Executable;
            }
        }

        public static string 匯出社團學期成績 { get { return "K12.Club.Universal.ExportClubResult.cs"; } }
        public static bool 匯出社團學期成績權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出社團學期成績].Executable;
            }
        }

        public static string 匯出社團成績_資料介接 { get { return "K12.Club.Universal.SpecialResult.cs"; } }
        public static bool 匯出社團成績_資料介接權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出社團成績_資料介接].Executable;
            }
        }

        public static string 學生選社志願設定 { get { return "K12.Club.Universal.V_Config.cs"; } }
        public static bool 學生選社志願設定權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[學生選社志願設定].Executable;
            }
        }

        public static string 學生社團分配 { get { return "K12.Club.Universal.VolunteerForm.cs"; } }
        public static bool 學生社團分配權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[學生社團分配].Executable;
            }
        }

        public static string 社團幹部證明單 { get { return "K12.Club.Universal.CadreProveReport.cs"; } }
        public static bool 社團幹部證明單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團幹部證明單].Executable;
            }
        }

        public static string 匯出社團幹部清單 { get { return "K12.Club.Universal.ClubCadResult.cs"; } }
        public static bool 匯出社團幹部清單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出社團幹部清單].Executable;
            }
        }

        public static string 匯出社團基本資料 { get { return "K12.Club.Universal.ExportCLUBData.cs"; } }
        public static bool 匯出社團基本資料權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出社團基本資料].Executable;
            }
        }

        public static string 匯出社團志願序 { get { return "K12.Club.Universal.ExportVolunteerRecord.cs"; } }
        public static bool 匯出社團志願序權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出社團志願序].Executable;
            }
        }

        public static string 匯入社團基本資料 { get { return "K12.Club.Universal.ImportCLUBData.cs"; } }
        public static bool 匯入社團基本資料權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入社團基本資料].Executable;
            }
        }

        public static string 匯入社團志願序 { get { return "K12.Club.Universal.ImportVolunteerRecord.cs"; } }
        public static bool 匯入社團志願序權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入社團志願序].Executable;
            }
        }

        public static string 匯出社團參與學生 { get { return "C8B92AA8-8690-4833-A02D-5BFBEFF6E841"; } }
        public static bool 匯出社團參與學生權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯出社團參與學生].Executable;
            }
        }

        public static string 匯入社團參與學生 { get { return "A62ECC5A-19C3-48C7-A00F-07134BC78F75"; } }
        public static bool 匯入社團參與學生權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入社團參與學生].Executable;
            }
        }

        public static string 轉入課程 { get { return "C0A1AA4D-83C8-4F5D-84B5-24CC24FCA6C7"; } }
        public static bool 轉入課程權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[轉入課程].Executable;
            }
        }

        public static string 匯入社團幹部清單 { get { return "K12.Club.Universal.ImportClubCadres.cs"; } }
        public static bool 匯入社團幹部清單權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[匯入社團幹部清單].Executable;
            }
        }

        public static string 社團評語代碼表 { get { return "K12.Club.Universal.CommentForm.cs"; } }
        public static bool 社團評語代碼表權限
        {
            get
            {
                return FISCA.Permission.UserAcl.Current[社團評語代碼表].Executable;
            }
        }
    }
}
