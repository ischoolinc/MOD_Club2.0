using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FISCA;
using FISCA.Presentation;
using FISCA.Permission;
using FISCA.UDT;
using FISCA.Presentation.Controls;
using System.Windows.Forms;
using K12.Data;
using K12.Data.Configuration;
using Campus.DocumentValidator;
using Campus.IRewrite.Interface;
using Campus.Import2014;
using System.Net;

namespace K12.Club.Volunteer
{
    public class Program
    {
        [MainMethod()]
        static public void Main()
        {
            ServerModule.AutoManaged("http://module.ischool.com.tw/module/138/Club_Universal/udm.xml");

            //FISCA.RTOut.WriteLine("註冊Gadget - 參加社團(學生)：" + WebPackage.RegisterGadget("Student", "fd56eafc-3601-40a0-82d9-808f72a8272b", "參加社團(學生)").Item2);
            //FISCA.RTOut.WriteLine("註冊Gadget - 社團(老師)：" + WebPackage.RegisterGadget("Teacher", "6080a7c0-60e7-443c-bad7-ecccb3a86bcf", "社團(老師)").Item2);

            #region 處理UDT Table沒有的問題
            
            //如果尚無設定值,預設
            ConfigData cd = K12.Data.School.Configuration["通用社團UDT載入設定"];
            string name = "社團UDT是否已載入_20210912";
            if (string.IsNullOrEmpty(cd[name]))
            {
                cd[name] = "false";
            }

            //檢查是否為布林
            bool checkClubUDT = false;
            bool.TryParse(cd[name], out checkClubUDT);

            //增加Comment / 評語
            tool._A.Select<ClubComment>("UID = '00000'");

            if (!checkClubUDT)
            {
                tool._A.Select<SCJoin>("UID = '00000'");
                tool._A.Select<ResultScoreRecord>("UID = '00000'");
                tool._A.Select<CLUBRecord>("UID = '00000'");
                tool._A.Select<SCJoin>("UID = '00000'");
                tool._A.Select<WeightProportion>("UID = '00000'");
                tool._A.Select<CadresRecord>("UID = '00000'");
                tool._A.Select<DTScore>("UID = '00000'");
                tool._A.Select<DTClub>("UID = '00000'");
                tool._A.Select<VolunteerRecord>("UID = '00000'");
                tool._A.Select<ConfigRecord>("UID = '00000'");

                cd[name] = "true";
                cd.Save();
            }

            #endregion

            //增加一個社團Tab
            MotherForm.AddPanel(ClubAdmin.Instance);

            //增加一個ListView
            ClubAdmin.Instance.AddView(new ExtracurricularActivitiesView());

            //驗證規則
            FactoryProvider.FieldFactory.Add(new CLUBFieldValidatorFactory());
            FactoryProvider.RowFactory.Add(new CLUBRowValidatorFactory());

            // .NET 版本預設為Ss13(已過時) ，會被擋住， 透過更正連線解決，
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            #region 毛毛蟲

            //學生社團成績
            FeatureAce UserPermission = FISCA.Permission.UserAcl.Current[Permissions.學生社團成績_資料項目];
            if (UserPermission.Editable || UserPermission.Viewable)
                K12.Presentation.NLDPanels.Student.AddDetailBulider(new FISCA.Presentation.DetailBulider<StudentResultItem>());

            //社團照片
            UserPermission = FISCA.Permission.UserAcl.Current[Permissions.社團照片];
            if (UserPermission.Editable || UserPermission.Viewable)
                ClubAdmin.Instance.AddDetailBulider(new FISCA.Presentation.DetailBulider<ClubImageItem>());

            #region 社團基本資料

            UserPermission = FISCA.Permission.UserAcl.Current[Permissions.社團基本資料];
            if (UserPermission.Editable || UserPermission.Viewable)
            {
                IClubDetailItemAPI itemB = FISCA.InteractionService.DiscoverAPI<IClubDetailItemAPI>();
                if (itemB != null)
                {
                    ClubAdmin.Instance.AddDetailBulider(itemB.CreateBasicInfo());
                }
                else
                {
                    ClubAdmin.Instance.AddDetailBulider(new FISCA.Presentation.DetailBulider<ClubDetailItem>());
                }
            }

            #endregion

            //社團限制
            UserPermission = FISCA.Permission.UserAcl.Current[Permissions.社團限制];
            if (UserPermission.Editable || UserPermission.Viewable)
                ClubAdmin.Instance.AddDetailBulider(new FISCA.Presentation.DetailBulider<ClubRestrictItem>());

            //社團學生
            UserPermission = FISCA.Permission.UserAcl.Current[Permissions.社團參與學生];
            if (UserPermission.Editable || UserPermission.Viewable)
                ClubAdmin.Instance.AddDetailBulider(new FISCA.Presentation.DetailBulider<ClubStudent>());

            //社團幹部
            UserPermission = FISCA.Permission.UserAcl.Current[Permissions.社團幹部];
            if (UserPermission.Editable || UserPermission.Viewable)
                ClubAdmin.Instance.AddDetailBulider(new FISCA.Presentation.DetailBulider<CadresItem>());

            #endregion

            #region 功能按鈕
            #region 編輯
            {
                RibbonBarItem edit = ClubAdmin.Instance.RibbonBarItems["編輯"];
                edit["新增社團"].Size = RibbonBarButton.MenuButtonSize.Large;
                edit["新增社團"].Image = Properties.Resources.health_and_leisure_add_64;
                edit["新增社團"].Enable = Permissions.新增社團權限;
                edit["新增社團"].Click += delegate
                {
                    NewAddClub insert = new NewAddClub();
                    insert.ShowDialog();
                };

                edit["複製社團"].Size = RibbonBarButton.MenuButtonSize.Large;
                edit["複製社團"].Image = Properties.Resources.rotate_64;
                edit["複製社團"].Enable = false;
                edit["複製社團"].Click += delegate
                {
                    CopyClub insert = new CopyClub();
                    insert.ShowDialog();
                };
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    edit["複製社團"].Enable = SourceCount && Permissions.複製社團權限;
                };

                edit["刪除社團"].Size = RibbonBarButton.MenuButtonSize.Large;
                edit["刪除社團"].Image = Properties.Resources.health_and_leisure_remove_64;
                edit["刪除社團"].Enable = false;
                edit["刪除社團"].Click += delegate
                {
                    DeleteClub();
                };                
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    ClubAdmin.Instance.ListPaneContexMenu["刪除社團"].Enable = SourceCount && Permissions.刪除社團權限;
                    edit["刪除社團"].Enable = SourceCount && Permissions.刪除社團權限;
                };
            } 
            #endregion
            #region 資料統計
            {
                RibbonBarItem totle = ClubAdmin.Instance.RibbonBarItems["資料統計"];
                totle["匯出"].Size = RibbonBarButton.MenuButtonSize.Large;
                totle["匯出"].Image = Properties.Resources.Export_Image;

                totle["匯出"]["匯出社團基本資料"].Enable = Permissions.匯出社團基本資料權限;
                totle["匯出"]["匯出社團基本資料"].Click += delegate
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new K12.Club.Volunteer.CLUB.ExportCLUBData();
                    K12.Club.Volunteer.CLUB.ExportStudentV2 wizard = new K12.Club.Volunteer.CLUB.ExportStudentV2(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                };

                totle["匯出"]["匯出聯課活動成績(資料介接)"].Enable = Permissions.匯出社團成績_資料介接權限;
                totle["匯出"]["匯出聯課活動成績(資料介接)"].Click += delegate
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new K12.Club.Volunteer.CLUB.SpecialResult();
                    K12.Club.Volunteer.CLUB.ExportStudentV2 wizard = new K12.Club.Volunteer.CLUB.ExportStudentV2(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                };

                totle["匯出"]["匯出社團幹部清單"].Enable = Permissions.匯出社團幹部清單權限;
                totle["匯出"]["匯出社團幹部清單"].Click += delegate
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new K12.Club.Volunteer.CLUB.ClubCadResult();
                    K12.Club.Volunteer.CLUB.ExportStudentV2 wizard = new K12.Club.Volunteer.CLUB.ExportStudentV2(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                };

                totle["匯出"]["匯出社團參與學生"].Enable = Permissions.匯出社團參與學生權限;
                totle["匯出"]["匯出社團參與學生"].Click += delegate
                {
                    (new Ribbon.Export.frmExportSCJoin()).ShowDialog();
                };

                totle["匯入"].Size = RibbonBarButton.MenuButtonSize.Large;
                totle["匯入"].Image = Properties.Resources.Import_Image;

                totle["匯入"]["匯入社團基本資料"].Enable = Permissions.匯入社團基本資料權限;
                totle["匯入"]["匯入社團基本資料"].Click += delegate
                {
                    new ImportCLUBData().Execute();
                };

                totle["匯入"]["匯入社團參與學生"].Enable = Permissions.匯入社團參與學生權限;
                totle["匯入"]["匯入社團參與學生"].Click += delegate 
                {
                    new ImportSCJoinData().Execute();
                    ClubEvents.RaiseAssnChanged();
                };

                totle["匯入"]["匯入社團幹部清單"].Enable = Permissions.匯入社團幹部清單權限;
                totle["匯入"]["匯入社團幹部清單"].Click += delegate
                {
                    new ImportClubCadres().Execute();
                    
                    ClubEvents.RaiseAssnChanged();
                };

                totle["報表"].Size = RibbonBarButton.MenuButtonSize.Large;
                totle["報表"].Image = Properties.Resources.Report;
                // 2018/01/16 羿均 註解較舊功能
                totle["報表"]["社團點名單"].Enable = false;
                totle["報表"]["社團點名單"].Click += delegate
                {
                    AssociationsPointList insert = new AssociationsPointList();
                };
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    totle["報表"]["社團點名單"].Enable = SourceCount && Permissions.社團點名單權限;
                };

                totle["報表"]["社團點名單(套表列印)"].Enable = false;
                totle["報表"]["社團點名單(套表列印)"].Click += delegate
                {
                    ClubPointsListForm insert = new ClubPointsListForm();
                    insert.ShowDialog();
                };
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    totle["報表"]["社團點名單(套表列印)"].Enable = SourceCount && Permissions.社團點名單_套表列印權限;
                };

                totle["報表"]["社團成績單"].Enable = false;
                totle["報表"]["社團成績單"].Click += delegate
                {
                    MsgBox.Show("說明:成績單部分資料需透過結算產生.\n如幹部紀錄,學期成績");
                    ClubTranscript insert = new ClubTranscript();
                };
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    totle["報表"]["社團成績單"].Enable = SourceCount && Permissions.社團成績單權限;
                };

                totle["報表"]["社團概況表"].Enable = Permissions.社團概況表權限;
                totle["報表"]["社團概況表"].Click += delegate
                {
                    CLUBFactsTable insert = new CLUBFactsTable();
                    insert.ShowDialog();
                };
            }
            #endregion
            #region 學生選社
            {
                RibbonBarItem oder = ClubAdmin.Instance.RibbonBarItems["學生選社"];

                oder["開放選社時間"].Size = RibbonBarButton.MenuButtonSize.Medium;
                oder["開放選社時間"].Image = Properties.Resources.time_frame_refresh_128;
                oder["開放選社時間"].Enable = Permissions.開放選社時間權限;
                oder["開放選社時間"].Click += delegate
                {
                    OpenClubJoinDateTime insert = new OpenClubJoinDateTime();
                    insert.ShowDialog();
                };
                // 2018/01/16 羿均 因應弘文高中需求新增
                oder["匯出選社結果"].Size = RibbonBarButton.MenuButtonSize.Medium;
                oder["匯出選社結果"].Image = Properties.Resources.Export_Image;
                oder["匯出選社結果"].Enable = Permissions.學生選社志願設定權限;
                oder["匯出選社結果"].Click += delegate
                {
                    Report.匯出選社結果.ExportStudentClubForm e = new Report.匯出選社結果.ExportStudentClubForm();
                    e.ShowDialog();
                };

                // 2018/1/15 羿均 此為社團2.0開發工具: 隨機填入學生社團志願
                //RibbonBarItem test = ClubAdmin.Instance.RibbonBarItems["測試資料"];
                //test["隨機填入學生志願"].Size = RibbonBarButton.MenuButtonSize.Medium;
                //test["隨機填入學生志願"].Image = Properties.Resources.group_up_64;
                //test["隨機填入學生志願"].Enable = true;
                //test["隨機填入學生志願"].Click += delegate
                //{
                //    AutoVolunteer a = new AutoVolunteer();
                //};

                oder["選社志願設定"].Size = RibbonBarButton.MenuButtonSize.Medium;
                oder["選社志願設定"].Image = Properties.Resources.presentation_a_config_64;
                oder["選社志願設定"].Enable = Permissions.學生選社志願設定權限;
                oder["選社志願設定"].Click += delegate
                {
                    V_Config v = new V_Config();
                    v.ShowDialog();
                };

                oder["志願分配作業"].Size = RibbonBarButton.MenuButtonSize.Medium;
                oder["志願分配作業"].Image = Properties.Resources.group_up_64;
                oder["志願分配作業"].Enable = Permissions.學生社團分配權限;
                oder["志願分配作業"].Click += delegate
                {
                    //是診斷模式 是超級使用者 按下Shift
                    if (FISCA.RTContext.IsDiagMode && FISCA.Authentication.DSAServices.IsSysAdmin && Control.ModifierKeys == Keys.Shift)
                    {
                        //一個社團選社資料清空功能
                        //SCJReMove move = new SCJReMove();
                        //move.ShowDialog();

                        批次志願功能 betaForm = new 批次志願功能();
                        betaForm.ShowDialog();
                    }
                    else
                    {
                        VolunteerClassForm form = new VolunteerClassForm();
                        DialogResult dr = form.ShowDialog();
                        if (dr == DialogResult.Yes)
                        {
                            FISCA.Presentation.MotherForm.SetStatusBarMessage("社團資料已重新讀取");
                            ClubEvents.RaiseAssnChanged();
                        }
                    }
                };
            }
            #endregion
            #region 檢查
            {
                RibbonBarItem check = ClubAdmin.Instance.RibbonBarItems["檢查"];
                check["未參與社團檢查"].Size = RibbonBarButton.MenuButtonSize.Medium;
                check["未參與社團檢查"].Image = Properties.Resources.group_help_64;
                check["未參與社團檢查"].Enable = Permissions.未參與社團學生權限;
                check["未參與社團檢查"].Click += delegate
                {
                    CheckStudentIsNotInClub insert = new CheckStudentIsNotInClub();
                    insert.ShowDialog();
                };

                check["重覆選社檢查"].Size = RibbonBarButton.MenuButtonSize.Medium;
                check["重覆選社檢查"].Image = Properties.Resources.meeting_64;
                check["重覆選社檢查"].Enable = Permissions.重覆選社檢查權限;
                check["重覆選社檢查"].Click += delegate
                {
                    RepeatForm insert = new RepeatForm();
                    insert.ShowDialog();
                };

                check["調整社團學生"].Size = RibbonBarButton.MenuButtonSize.Medium;
                check["調整社團學生"].Image = Properties.Resources.layers_64;
                check["調整社團學生"].Enable = false;
                check["調整社團學生"].Click += delegate
                {
                    if (ClubAdmin.Instance.SelectedSource.Count > 7)
                    {
                        MsgBox.Show("所選社團大於7個\n本功能最多僅處理7個社團!!");
                    }
                    else if (ClubAdmin.Instance.SelectedSource.Count < 2)
                    {
                        MsgBox.Show("使用調整社團學生功能\n必須2個以上社團!!");
                    }
                    else
                    {
                        SplitClasses insert = new SplitClasses();
                        insert.ShowDialog();
                    }
                };

                check["檢查/批次社團鎖社"].Size = RibbonBarButton.MenuButtonSize.Medium;
                check["檢查/批次社團鎖社"].Image = Properties.Resources.layers_64;
                check["檢查/批次社團鎖社"].Enable = Permissions.檢查批次社團鎖社權限; 
                check["檢查/批次社團鎖社"].Click += delegate
                {                    
                    Ribbon.檢查_批次社團鎖社.MutipleLockForm mutiplelock = new Ribbon.檢查_批次社團鎖社.MutipleLockForm();

                    mutiplelock.ShowDialog();
                                           
                };


                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    check["調整社團學生"].Enable = SourceCount && Permissions.調整社團學生權限;
                };
            }
            #endregion
            #region 成績
            {
                RibbonBarItem Results = ClubAdmin.Instance.RibbonBarItems["成績"];
                Results["成績輸入"].Size = RibbonBarButton.MenuButtonSize.Medium;
                Results["成績輸入"].Image = Properties.Resources.marker_fav_64;
                Results["成績輸入"].Enable = false;
                Results["成績輸入"].Click += delegate
                {
                    ClubResultsInput insert = new ClubResultsInput();
                    insert.ShowDialog();
                };
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    Results["成績輸入"].Enable = SourceCount && Permissions.成績輸入權限;
                };

                Results["評量比例"].Size = RibbonBarButton.MenuButtonSize.Medium;
                Results["評量比例"].Image = Properties.Resources.barchart_64;
                Results["評量比例"].Enable = Permissions.評量項目權限;
                Results["評量比例"].Click += delegate
                {
                    GradingProjectConfig insert = new GradingProjectConfig();
                    insert.ShowDialog();
                };

                Results["學期結算"].Size = RibbonBarButton.MenuButtonSize.Medium;
                Results["學期結算"].Image = Properties.Resources.brand_write_64;
                Results["學期結算"].Enable = false;
                Results["學期結算"].Click += delegate
                {
                    IClubClearingFormAPI itemK = FISCA.InteractionService.DiscoverAPI<IClubClearingFormAPI>();
                    if (itemK != null)
                    {
                        itemK.CreateBasicForm().ShowDialog();
                    }
                    else
                    {
                        ClearingForm insert = new ClearingForm();
                        insert.ShowDialog();
                    }
                };
                ClubAdmin.Instance.SelectedSourceChanged += delegate
                {
                    //是否選擇大於0的社團
                    bool SourceCount = (ClubAdmin.Instance.SelectedSource.Count > 0);
                    Results["學期結算"].Enable = SourceCount && Permissions.學期結算權限;
                };

                Results["成績輸入時間"].Size = RibbonBarButton.MenuButtonSize.Medium;
                Results["成績輸入時間"].Image = Properties.Resources.time_frame_refresh_128;
                Results["成績輸入時間"].Enable = Permissions.成績輸入時間權限;
                Results["成績輸入時間"].Click += delegate
                {
                    ResultsInputDateTime insert = new ResultsInputDateTime();
                    insert.ShowDialog();
                };

                Results["社團評語代碼表"].Size = RibbonBarButton.MenuButtonSize.Medium;
                Results["社團評語代碼表"].Image = Properties.Resources.admissions_ok_64;
                Results["社團評語代碼表"].Enable = Permissions.社團評語代碼表權限;
                Results["社團評語代碼表"].Click += delegate
                {
                    CommentForm form = new CommentForm();
                    form.ShowDialog();
                };
            }
            #endregion
            #region 課程
            {
                RibbonBarItem course = ClubAdmin.Instance.RibbonBarItems["課程"];
                course["轉入課程"].Size = RibbonBarButton.MenuButtonSize.Medium;
                course["轉入課程"].Image = Properties.Resources.library_up_64;
                course["轉入課程"].Enable = Permissions.轉入課程權限;
                course["轉入課程"].Click += delegate
                {
                    frmImportToCourse form = new frmImportToCourse();
                    form.ShowDialog();
                };

            }
            #endregion
            
            #region 右鍵選單
            ClubAdmin.Instance.NavPaneContexMenu["重新整理"].Click += delegate
                {
                    ClubEvents.RaiseAssnChanged();
                };
            ClubAdmin.Instance.ListPaneContexMenu["刪除社團"].Enable = false;
            ClubAdmin.Instance.ListPaneContexMenu["刪除社團"].Click += delegate
            {
                DeleteClub();
            }; 
            #endregion
            #endregion

            #region 學生功能按鈕
            {
                RibbonBarItem Print = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "資料統計"];
                Print["匯出"]["社團相關匯出"]["匯出社團學期成績"].Enable = Permissions.匯出社團學期成績權限;
                Print["匯出"]["社團相關匯出"]["匯出社團學期成績"].Click += delegate
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new ExportStudentClubResult();
                    ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                };
            }
            {
                RibbonBarItem Print = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "資料統計"];

                Print["匯出"]["社團相關匯出"]["匯出社團志願序"].Enable = Permissions.匯出社團志願序權限;
                Print["匯出"]["社團相關匯出"]["匯出社團志願序"].Click += delegate
                {
                    SmartSchool.API.PlugIn.Export.Exporter exporter = new K12.Club.Volunteer.CLUB.ExportVolunteerRecord();
                    ExportStudentV2 wizard = new ExportStudentV2(exporter.Text, exporter.Image);
                    exporter.InitializeExport(wizard);
                    wizard.ShowDialog();
                };
            }
            {
                RibbonBarItem Print = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "資料統計"];
                Print["匯入"]["社團相關匯入"]["匯入社團志願序"].Enable = Permissions.匯入社團志願序權限;
                Print["匯入"]["社團相關匯入"]["匯入社團志願序"].Click += delegate
                {
                    new ImportVolunteerMPG().Execute();
                };
            }
            {
                RibbonBarItem Print = FISCA.Presentation.MotherForm.RibbonBarItems["學生", "資料統計"];
                Print["報表"]["社團相關報表"]["社團幹部證明單"].Enable = Permissions.社團幹部證明單權限;
                Print["報表"]["社團相關報表"]["社團幹部證明單"].Click += delegate
                {
                    CadreProveReport cpr = new CadreProveReport();
                    cpr.ShowDialog();
                };
            }
            #endregion

            #region 班級功能按鈕
            {
                RibbonBarItem InClass = FISCA.Presentation.MotherForm.RibbonBarItems["班級", "資料統計"];
                InClass["報表"]["社團相關報表"]["班級學生選社同意確認單"].Enable = false;
                InClass["報表"]["社團相關報表"]["班級學生選社同意確認單"].Click += delegate
                {
                    ElectionForm insert = new ElectionForm();
                    insert.ShowDialog();
                };

                InClass["報表"]["社團相關報表"]["班級社團成績單"].Enable = false;
                InClass["報表"]["社團相關報表"]["班級社團成績單"].Click += delegate
                {
                    ClassClubTranscript insert = new ClassClubTranscript();
                    insert.ShowDialog();
                };

                K12.Presentation.NLDPanels.Class.SelectedSourceChanged += delegate
                {
                //是否選擇大於0的社團
                bool SourceCount = (K12.Presentation.NLDPanels.Class.SelectedSource.Count > 0);

                    bool a = (SourceCount && Permissions.班級學生選社_確認表_權限);
                    InClass["報表"]["社團相關報表"]["班級學生選社同意確認單"].Enable = a;


                    bool b = (SourceCount && Permissions.班級社團成績單權限);
                    InClass["報表"]["社團相關報表"]["班級社團成績單"].Enable = b;

                };
            }
            #endregion

            #region 登錄權限代碼

            //是否能夠只用單一代碼,決定此模組之使用
            Catalog detail1;
            detail1 = RoleAclSource.Instance["社團"]["功能按鈕"];
            detail1.Add(new RibbonFeature(Permissions.新增社團, "新增社團"));
            detail1.Add(new RibbonFeature(Permissions.複製社團, "複製社團"));
            detail1.Add(new RibbonFeature(Permissions.刪除社團, "刪除社團"));
            detail1.Add(new RibbonFeature(Permissions.成績輸入, "成績輸入"));
            detail1.Add(new RibbonFeature(Permissions.評量項目, "評量比例"));
            detail1.Add(new RibbonFeature(Permissions.學期結算, "學期結算"));
            detail1.Add(new RibbonFeature(Permissions.未參與社團學生, "未參與社團學生"));
            detail1.Add(new RibbonFeature(Permissions.調整社團學生, "調整社團學生"));
            detail1.Add(new RibbonFeature(Permissions.檢查批次社團鎖社, "檢查/批次社團鎖社"));
            detail1.Add(new RibbonFeature(Permissions.開放選社時間, "開放選社時間"));
            detail1.Add(new RibbonFeature(Permissions.成績輸入時間, "成績輸入時間"));
            detail1.Add(new RibbonFeature(Permissions.重覆選社檢查, "重覆選社檢查"));
            detail1.Add(new RibbonFeature(Permissions.轉入課程, "轉入課程"));
            detail1.Add(new RibbonFeature(Permissions.社團評語代碼表, "社團評語代碼表"));

            //志願序獨有
            detail1.Add(new RibbonFeature(Permissions.學生選社志願設定, "學生選社志願設定"));
            detail1.Add(new RibbonFeature(Permissions.學生社團分配, "學生社團分配"));

            detail1 = RoleAclSource.Instance["社團"]["匯出/匯入"];
            detail1.Add(new RibbonFeature(Permissions.匯出社團基本資料, "匯出社團基本資料"));
            detail1.Add(new RibbonFeature(Permissions.匯出社團幹部清單, "匯出社團幹部清單"));
            detail1.Add(new RibbonFeature(Permissions.匯出社團成績_資料介接, "匯出社團學期成績(資料介接)"));
            detail1.Add(new RibbonFeature(Permissions.匯出社團參與學生, "匯出社團參與學生"));

            //匯入
            detail1.Add(new RibbonFeature(Permissions.匯入社團基本資料, "匯入社團基本資料"));
            detail1.Add(new RibbonFeature(Permissions.匯入社團參與學生, "匯入社團參與學生"));
            detail1.Add(new RibbonFeature(Permissions.匯入社團幹部清單, "匯入社團幹部清單"));

            detail1 = RoleAclSource.Instance["社團"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.社團點名單, "社團點名單"));
            detail1.Add(new RibbonFeature(Permissions.社團點名單_套表列印, "社團點名單(套表列印)"));
            detail1.Add(new RibbonFeature(Permissions.社團成績單, "社團成績單"));
            detail1.Add(new RibbonFeature(Permissions.社團概況表, "社團概況表"));

            detail1 = RoleAclSource.Instance["社團"]["資料項目"];
            detail1.Add(new DetailItemFeature(Permissions.社團基本資料, "基本資料"));
            detail1.Add(new DetailItemFeature(Permissions.社團照片, "社團照片"));
            detail1.Add(new DetailItemFeature(Permissions.社團限制, "社團限制"));
            detail1.Add(new DetailItemFeature(Permissions.社團參與學生, "參與學生"));
            detail1.Add(new DetailItemFeature(Permissions.社團幹部, "社團幹部"));

            detail1 = RoleAclSource.Instance["班級"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.班級學生選社_確認表, "班級學生選社同意確認單"));
            detail1.Add(new RibbonFeature(Permissions.班級社團成績單, "班級社團成績單"));

            detail1 = RoleAclSource.Instance["學生"]["匯出/匯入"];
            detail1.Add(new RibbonFeature(Permissions.匯出社團學期成績, "匯出社團學期成績"));
            detail1.Add(new RibbonFeature(Permissions.匯出社團志願序, "匯出社團志願序"));
            detail1.Add(new RibbonFeature(Permissions.匯入社團志願序, "匯入社團志願序"));

            detail1 = RoleAclSource.Instance["學生"]["資料項目"];
            detail1.Add(new DetailItemFeature(Permissions.學生社團成績_資料項目, "社團成績"));

            detail1 = RoleAclSource.Instance["學生"]["報表"];
            detail1.Add(new RibbonFeature(Permissions.社團幹部證明單, "社團幹部證明單"));
            #endregion

            ClubAdmin.Instance.SelectedSourceChanged += delegate
            {
                FISCA.Presentation.MotherForm.SetStatusBarMessage("選擇「" + ClubAdmin.Instance.SelectedSource.Count + "」個社團");
            };
        }

        static private void DeleteClub()
        {
            DialogResult dr = MsgBox.Show("確認刪除所選社團?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
            if (dr == DialogResult.Yes)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("已刪除選擇社團：");
                List<CLUBRecord> ClubList = tool._A.Select<CLUBRecord>(UDT_S.PopOneCondition("UID", ClubAdmin.Instance.SelectedSource));
                foreach (CLUBRecord each in ClubList)
                {
                    sb.AppendLine(string.Format("學年度「{0}」學期「{1}」社團名稱「{2}」", each.SchoolYear.ToString(), each.Semester.ToString(), each.ClubName));
                }

                List<SCJoin> SCJList = tool._A.Select<SCJoin>(UDT_S.PopOneCondition("ref_club_id", ClubAdmin.Instance.SelectedSource));
                if (SCJList.Count != 0)
                {
                    MsgBox.Show("刪除社團必須清空社團參與學生!");
                    return;
                }

                try
                {
                    tool._A.DeletedValues(ClubList);
                }
                catch (Exception ex)
                {
                    MsgBox.Show("社團刪除失敗!!\n" + ex.Message);
                    SmartSchool.ErrorReporting.ReportingService.ReportException(ex);
                    return;

                }
                FISCA.LogAgent.ApplicationLog.Log("社團", "刪除社團", sb.ToString());
                MsgBox.Show("社團刪除成功!!");
                ClubEvents.RaiseAssnChanged();
            }
            else
            {
                MsgBox.Show("已中止刪除社團操作!!");
            }
        }
    }
}
