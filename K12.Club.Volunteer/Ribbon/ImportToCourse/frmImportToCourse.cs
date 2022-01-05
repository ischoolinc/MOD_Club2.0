using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FISCA.Presentation.Controls;
using K12.Data;
using FISCA.Data;
using FISCA;

namespace K12.Club.Volunteer
{
    public partial class frmImportToCourse : BaseForm
    {
        private QueryHelper _qh = new QueryHelper();
        private UpdateHelper _up = new UpdateHelper();
        private bool _initFinish = false;
        private string _examTemplateID;
        private string _examTemplateName;

        //2019/9/11 - Dylan
        //新增評分樣版內容
        private string _examID;
        private string _examName;

        private string _courseTagID;

        private class ClubRecord
        {
            public string ClubUID { get; set; }
            public string ClubName { get; set; }
            public string ClubLocation { get; set; }
            public string TeacherName1 { get; set; }
            public string TeacherID1 { get; set; }
            public string TeacherName2 { get; set; }
            public string TeacherID2 { get; set; }
            public string TeacherName3 { get; set; }
            public string TeacherID3 { get; set; }
            public string RefExamTemplateID { get; set; }
            public string IsImport { get; set; }
            public List<string> listStudentID { get; set; }

        }

        EventHandler eh;
        string EventCode = "課程/重新整理";

        private Dictionary<string, ClubRecord> _dicClubRecordByName = new Dictionary<string, ClubRecord>();

        public frmImportToCourse()
        {
            InitializeComponent();
        }

        private void frmImportToCourse_Load(object sender, EventArgs e)
        {
            //評分樣板
            InitExamTemplate();

            //評量名稱
            InitExam();

            //評分項目
            InitTeInclude();

            //類別
            InitTag();

            // Init SchoolYear
            int schoolYear = int.Parse(K12.Data.School.DefaultSchoolYear == "" ? null : K12.Data.School.DefaultSchoolYear);
            cbxSchoolYear.Items.Add(schoolYear - 1);
            cbxSchoolYear.Items.Add(schoolYear);
            cbxSchoolYear.Items.Add(schoolYear + 1);
            cbxSchoolYear.SelectedIndex = 1;
            // Init Semester
            int semester = int.Parse(K12.Data.School.DefaultSemester == "" ? null : K12.Data.School.DefaultSemester);
            cbxSemester.Items.Add(1);
            cbxSemester.Items.Add(2);
            cbxSemester.SelectedIndex = semester - 1;
            // Init DataGridView
            ReloadDataGridView();

            this._initFinish = true;

            eh = FISCA.InteractionService.PublishEvent(EventCode);
        }

        /// <summary>
        /// 評分樣板
        /// </summary>
        private void InitExamTemplate()
        {
            string sql = @"
SELECT
    *
FROM
    exam_template
WHERE 
    name = '社團評量(社團模組)'
";
            DataTable dt = this._qh.Select(sql);

            if (dt.Rows.Count > 0)
            {
                this._examTemplateID = "" + dt.Rows[0]["id"];
                this._examTemplateName = "" + dt.Rows[0]["name"];
            }
            else
            {
                try
                {
                    string insertSQl = @"
WITH insert_data as (
    INSERT into exam_template(
        name , allow_upload
    )VALUES(
        '社團評量(社團模組)', '0'
    )
    RETURNING *
)
SELECT
    *
FROM 
    insert_data
";
                    DataTable insertDt = this._qh.Select(insertSQl);

                    this._examTemplateID = "" + insertDt.Rows[0]["id"];
                    this._examTemplateName = "" + insertDt.Rows[0]["name"];
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 評量名稱
        /// </summary>
        private void InitExam()
        {
            string sql = @"
SELECT
    *
FROM
    exam
WHERE 
    exam_name = '社團評量'
";
            DataTable dt = this._qh.Select(sql);

            if (dt.Rows.Count > 0)
            {
                this._examID = "" + dt.Rows[0]["id"];
                this._examName = "" + dt.Rows[0]["exam_name"];
            }
            else
            {
                try
                {
                    string insertSQl = @"
WITH insert_data as (
    INSERT into exam(
        exam_name
    )VALUES(
        '社團評量'
    )
    RETURNING *
)
SELECT
    *
FROM 
    insert_data
";
                    DataTable insertDt = this._qh.Select(insertSQl);

                    this._examID = "" + insertDt.Rows[0]["id"];
                    this._examName = "" + insertDt.Rows[0]["exam_name"];
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 評分項目
        /// </summary>
        private void InitTeInclude()
        {
            string sql = @"
SELECT
    *
FROM
    te_include
WHERE 
    ref_exam_template_id='{0}'
";
            DataTable dt = this._qh.Select(string.Format(sql, _examTemplateID));

            if (dt.Rows.Count > 0)
            {
                //不予處理
            }
            else
            {
                try
                {
                    string insertSQl = @"
WITH insert_data as (
    INSERT into te_include(
        ref_exam_template_id , ref_exam_id , weight , use_score , use_text , extension
    )VALUES(
        '{0}' , '{1}' , '{2}' , '{3}' , '{4}' , '{5}'
    )
    RETURNING *
)
SELECT
    *
FROM 
    insert_data
";
                    DataTable insertDt = this._qh.Select(
                        string.Format(insertSQl, _examTemplateID, _examID, "100", "1" , "0", @"<Extension><UseScore>是</UseScore><UseEffort>否</UseEffort><UseText>否</UseText></Extension>")
                        );
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }

        }

        /// <summary>
        /// 類別
        /// </summary>
        private void InitTag()
        {
            string sql = @"
SELECT
    *
FROM
    tag
WHERE
    prefix = '聯課活動'
    AND category = 'Course'
";
            DataTable dt = this._qh.Select(sql);

            if (dt.Rows.Count > 0)
            {
                this._courseTagID = "" + dt.Rows[0]["id"];
            }
            else
            {
                try
                {
                    string insertSql = @"
WITH insert_data AS(
    INSERT INTO tag(
        prefix
        ,name
        , category
    ) VALUES(
        '聯課活動'
        ,'社團'
        , 'Course'
    )
    RETURNING *
)
SELECT
    *
FROM
    insert_data
";
                    DataTable insertDt = this._qh.Select(insertSql);
                    this._courseTagID = "" + insertDt.Rows[0]["id"];
                }
                catch (Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }
        }

        private void ReloadDataGridView()
        {
            this.SuspendLayout();
            {
                this._dicClubRecordByName.Clear();
                dataGridViewX1.Rows.Clear();

                #region SQL
                string sql = string.Format(@"
WITH club_data AS(
    SELECT
		club.uid
       ,  club.club_name
        , club.location
        , club.ref_teacher_id
        , club.ref_teacher_id_2
        , club.ref_teacher_id_3
        , teacher1.teacher_name AS teacher_name1
        , teacher2.teacher_name AS teacher_name2
        , teacher3.teacher_name AS teacher_name3
    FROM
        $k12.clubrecord.universal AS club
        LEFT OUTER JOIN teacher AS teacher1
            ON teacher1.id = club.ref_teacher_id::BIGINT
        LEFT OUTER JOIN teacher AS teacher2
            ON teacher2.id = club.ref_teacher_id_2::BIGINT
        LEFT OUTER JOIN teacher AS teacher3
            ON teacher3.id = club.ref_teacher_id_3::BIGINT
    WHERE
        club.school_year = {0}
        AND club.semester = {1}
) , course_data AS(
    SELECT
        course.*
    FROM
        course
        LEFT OUTER JOIN tag_course
            ON tag_course.ref_course_id = course.id
        LEFT OUTER JOIN tag 
            ON tag.id = tag_course.ref_tag_id
    WHERE
        school_year = {0}
        AND semester = {1}
)   
SELECT
    club_data.*
	, target_student.ref_student_id
    , CASE WHEN course_data.id IS NOT NULL
        THEN '是'
        ELSE '否'
        END AS is_import
FROM
    club_data
    LEFT OUTER JOIN course_data
        ON course_data.course_name = club_data.club_name
	LEFT OUTER JOIN(
		SELECT 
			scj.ref_club_id
			 , scj.ref_student_id
		FROM
			$k12.scjoin.universal  AS scj
			LEFT OUTER JOIN student
				ON student.id = scj.ref_student_id::BIGINT
		WHERE
			scj.ref_club_id::BIGINT IN(
				SELECT uid FROM club_data
			)
			AND student.status IN (1,2)
	)	target_student
		ON target_student.ref_club_id::BIGINT = club_data.uid
ORDER BY
    club_data.club_name

                ", cbxSchoolYear.SelectedItem.ToString(), cbxSemester.SelectedItem.ToString());
                #endregion
                DataTable dt = this._qh.Select(sql);

                #region 資料整理
                foreach (DataRow row in dt.Rows)
                {
                    if (!this._dicClubRecordByName.ContainsKey("" + row["club_name"]))
                    {
                        ClubRecord data = new ClubRecord();
                        data.ClubUID = "" + row["uid"];
                        data.ClubName = "" + row["club_name"];
                        data.ClubLocation = "" + row["location"];
                        data.TeacherID1 = "" + row["ref_teacher_id"];
                        data.TeacherName1 = "" + row["teacher_name1"];
                        data.TeacherID2 = "" + row["ref_teacher_id_2"];
                        data.TeacherName2 = "" + row["teacher_name2"];
                        data.TeacherID3 = "" + row["ref_teacher_id_3"];
                        data.TeacherName3 = "" + row["teacher_name3"];
                        data.RefExamTemplateID = this._examTemplateID;
                        data.IsImport = "" + row["is_import"];
                        data.listStudentID = new List<string>();

                        this._dicClubRecordByName.Add("" + row["club_name"], data);

                    }
                    if (!string.IsNullOrEmpty("" + row["ref_student_id"]))
                    {
                        this._dicClubRecordByName["" + row["club_name"]].listStudentID.Add("" + row["ref_student_id"]);
                    }
                }
                #endregion

                #region 填入資料
                foreach (string clubName in this._dicClubRecordByName.Keys)
                {
                    DataGridViewRow dgvrow = new DataGridViewRow();
                    dgvrow.CreateCells(dataGridViewX1);

                    int col = 0;
                    dgvrow.Cells[col++].Value = clubName;
                    dgvrow.Cells[col++].Value = _dicClubRecordByName[clubName].listStudentID.Count;
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].TeacherName1;
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].TeacherName2;
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].TeacherName3;
                    //dgvrow.Cells[col++].Value = this._examTemplateName; 新改版社團轉入課程 取消此欄位
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].IsImport;

                    dgvrow.Tag = _dicClubRecordByName[clubName].ClubUID;

                    dataGridViewX1.Rows.Add(dgvrow);
                }
                #endregion
            }
            this.ResumeLayout();
        }

        private void cbxSchoolYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._initFinish)
            {
                ReloadDataGridView();
            }
        }

        private void cbxSemester_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this._initFinish)
            {
                ReloadDataGridView();
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (dataGridViewX1.SelectedRows.Count > 0)
            {
                List<string> listSelectedClubName = new List<string>();
                foreach (DataGridViewRow dgvrow in dataGridViewX1.SelectedRows)
                {
                    string selectedClub = string.Format("「{0}」", "" + dgvrow.Cells[0].Value);
                    listSelectedClubName.Add(selectedClub);
                }
                DialogResult result = MsgBox.Show(string.Format("確定將{0}社團資料轉入課程?", string.Join(",", listSelectedClubName)), "提醒", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    DataTable dtCourse = new DataTable();
                    List<string> listCourseData = new List<string>();
                    List<string> skipIDList = new List<string>(); //跳過的社團編號
                    #region 建立課程，並取回課程資料
                    {
                        
                        #region 資料整理
                        foreach (DataGridViewRow dgvrow in dataGridViewX1.SelectedRows)
                        {
                            if (dgvrow.Index > -1)
                            {
                                string clubName = "" + dgvrow.Cells[0].Value;

                                if (this._dicClubRecordByName[clubName].IsImport == "否")
                                {
                                    string data = string.Format(@"
SELECT
    '{0}'::TEXT AS course_name
    , {1}::BIGINT AS ref_teacher_id    
    , {2}::SMALLINT AS school_year
    , {3}::SMALLINT AS semester
    , '{4}'::CHARACTER VARYING AS subject
    , {5}::INTEGER AS score_calc_flag
    , {6}::BIT(1) AS not_included_in_credit
    , {7}::BIT(1) AS not_included_in_calc
                                ", clubName.Replace("'", "''")
                                    , this._dicClubRecordByName[clubName].TeacherID1 == "" ? "null" : this._dicClubRecordByName[clubName].TeacherID1
                                    , cbxSchoolYear.SelectedItem.ToString()
                                    , cbxSemester.SelectedItem.ToString()
                                    , clubName
                                    , "2"
                                    , "1"
                                    , "1"
        );
                                    listCourseData.Add(data);
                                }
                                else
                                {
                                    // 若選擇的 dgvrow中 已經有加入下學期課程的社團，將之放到待處理社團 供使用者處理
                                    skipIDList.Add("" + dgvrow.Tag);
                                }

                            }

                        }
                        #endregion
                        if (listCourseData.Count > 0)
                        {
                            #region SQL
                            string sql = string.Format(@"
WITH data_row AS(
    {0}
) , insert_data AS(
    INSERT INTO course(
        course_name
        , ref_teacher_id        
        , school_year
        , semester
        , subject
        , score_calc_flag
        , not_included_in_credit
        , not_included_in_calc
    )
    SELECT
        course_name
        , ref_teacher_id        
        , school_year
        , semester
        , subject
        , score_calc_flag
        , not_included_in_credit
        , not_included_in_calc
    FROM
        data_row
    RETURNING * 
)
SELECT
    *
FROM
    insert_data
            ", string.Join("UNION ALL", listCourseData));
                            #endregion
                            dtCourse = this._qh.Select(sql);
                        }
                        else
                        {
                            if (skipIDList.Count > 0)
                            {
                                // 加入待處理
                                DialogResult result2 = MsgBox.Show(string.Format("重覆之課程/社團筆數「{0}」\n", dataGridViewX1.SelectedRows.Count - listCourseData.Count) +
                                    "是否將重複之社團加入待處理?", "提醒", MessageBoxButtons.YesNo);
                                if (result2 == DialogResult.Yes)
                                {
                                    ClubAdmin.Instance.AddToTemp(skipIDList);
                                }
                            }

                            return;
                        }
                    }
                    #endregion

                    #region 轉入課程修課學生、轉入課程老師、新建課程貼上聯課活動標籤、轉入高雄社團上課地點
                    {
                        List<string> listSCAttend = new List<string>();
                        List<string> listCourseTeacher = new List<string>();
                        List<string> listDataRow = new List<string>();
                        List<string> listClubLocation = new List<string>();

                        #region 資料整理
                        foreach (DataRow row in dtCourse.Rows)
                        {
                            string courseName = "" + row["course_name"];
                            string courseID = "" + row["id"];
                            // 修課學生資料
                            foreach (string studentID in this._dicClubRecordByName[courseName].listStudentID)
                            {
                                string scData = string.Format(@"
SELECT
    {0}::BIGINT AS ref_student_id
    , {1}::BIGINT AS ref_course_id
                                ", studentID, courseID);
                                listSCAttend.Add(scData);
                            }

                            List<string> CheckTeacherID = new List<string>();
                            // 課程老師資料
                            for (int i = 1; i <= 3; i++)
                            {
                                string teacherID = "";
                                switch (i)
                                {
                                    case 1:
                                        teacherID = this._dicClubRecordByName[courseName].TeacherID1;
                                        break;
                                    case 2:
                                        teacherID = this._dicClubRecordByName[courseName].TeacherID2;
                                        break;
                                    case 3:
                                        teacherID = this._dicClubRecordByName[courseName].TeacherID3;
                                        break;
                                }

                                if (!string.IsNullOrEmpty(teacherID))
                                {
                                    string ctData = string.Format(@"
SELECT
    {0}::BIGINT AS ref_teacher_id
    , {1}::BIGINT AS ref_course_id
    , {2}::SMALLINT AS sequence
                                ", teacherID, courseID, i);

                                    string ctCheck = teacherID + "_" + courseID;
                                    if (!CheckTeacherID.Contains(ctCheck))
                                    {
                                        listCourseTeacher.Add(ctData);
                                    }
                                    
                                    CheckTeacherID.Add(ctCheck);
                                }
                            }
                            // 聯課活動標籤
                            {
                                string tagData = string.Format(@"
SELECT
    {0}::BIGINT AS ref_course_id
    , {1}::BIGINT AS ref_tag_id
                        ", "" + row["id"]
                               , this._courseTagID);

                                listDataRow.Add(tagData);
                            }
                            // 上課地點
                            {
                                string location = this._dicClubRecordByName[courseName].ClubLocation;
                                string locationData = string.Format(@"
SELECT
    '{0}'::TEXT AS associationid
    , '{1}'::TEXT AS schoolyear
    , '{2}'::TEXT AS semester
    , '{3}'::TEXT AS address
                            ", courseID, cbxSchoolYear.SelectedItem.ToString(), cbxSemester.SelectedItem.ToString(), location);

                                listClubLocation.Add(locationData);
                            }
                        }
                        #endregion

                        // 轉入課程修課學生
                        if (listSCAttend.Count > 0)
                        {
                            #region SQL
                            string sql = string.Format(@"
WITH data_row AS(
    {0}
) 
INSERT INTO sc_attend(
    ref_student_id
    , ref_course_id
)
SELECT
    ref_student_id
    , ref_course_id
FROM
    data_row
                ", string.Join("UNION ALL", listSCAttend));
                            #endregion
                            this._up.Execute(sql);
                        }
                        // 轉入課程老師
                        if (listCourseTeacher.Count > 0)
                        {
                            #region SQL
                            string sql = string.Format(@"
WITH data_row AS(
    {0}
)
INSERT INTO tc_instruct(
    ref_teacher_id
    , ref_course_id
    , sequence
)
SELECT
    ref_teacher_id
    , ref_course_id
    , sequence
FROM
    data_row
                        ", string.Join("UNION ALL", listCourseTeacher));
                            #endregion
                            this._up.Execute(sql);
                        }
                        // 新建課程貼上聯課活動標籤
                        if (listDataRow.Count > 0)
                        {
                            #region SQL
                            string sql = string.Format(@"
WITH data_row AS(
    {0}
)
INSERT INTO tag_course(
    ref_course_id
    , ref_tag_id
)
SELECT
    ref_course_id
    , ref_tag_id
FROM
    data_row
                    ", string.Join("UNION ALL", listDataRow));
                            #endregion
                            this._up.Execute(sql);
                        }

                        //2019/9/4 - 如果有上課地點Table,才新增上課地點
                        string sql_check = "select * from _udt_table where name='jhschool.association.udt.address'";
                        QueryHelper q = new QueryHelper();
                        DataTable dt = q.Select(sql_check);
                        if (dt.Rows.Count > 0)
                        {
                            // 轉入高雄社團上課地點
                            if (listClubLocation.Count > 0)
                            {
                                #region SQL
                                string sql = string.Format(@"
                        WITH data_row AS(
                            {0}
                        ) , update_data AS(
                            UPDATE $jhschool.association.udt.address SET
                                associationid = data_row.associationid
                                , schoolyear = data_row.schoolyear
                                , semester = data_row.semester
                                , address = data_row.address
                            FROM    
                                data_row
                            WHERE
                                data_row.associationid = $jhschool.association.udt.address.associationid
                        ) 
                        INSERT INTO $jhschool.association.udt.address(
                            associationid
                            , schoolyear
                            , semester
                            , address
                        )
                        SELECT
                            data_row.associationid
                            , data_row.schoolyear
                            , data_row.semester
                            , data_row.address
                        FROM
                            data_row
                            LEFT OUTER JOIN $jhschool.association.udt.address AS club
                                ON club.associationid = data_row.associationid
                        WHERE
                            club.uid IS NULL
                                                    ", string.Join("UNION ALL", listClubLocation));
                                #endregion
                                this._up.Execute(sql);
                            }
                        }
                    }
                    #endregion


                    //Log - By Dylan 2019/8/26
                    StringBuilder sb_log = new StringBuilder();
                    sb_log.AppendLine("社團轉入課程：");
                    sb_log.Append(string.Format("學年度「{0}」", cbxSchoolYear.SelectedItem.ToString()));
                    sb_log.AppendLine(string.Format("學期「{0}」", cbxSemester.SelectedItem.ToString()));
                    sb_log.AppendLine("社團清單：");
                    foreach (string each in listSelectedClubName)
                    {
                        sb_log.AppendLine(string.Format("{0}", each));
                    }

                    FISCA.LogAgent.ApplicationLog.Log("社團", "轉入課程", sb_log.ToString());

                    //處理產品社團,當使用轉入課程功能時
                    //需要引發
                    //1.國中課程 or 高中社團 的更新事件
                    //2.引發高雄社團更新事件
                    //2019/9/10 - Dylan
                    eh(null, EventArgs.Empty);
            

                    if (skipIDList.Count > 0)
                    {
                        // 加入待處理
                        DialogResult result2 = MsgBox.Show(string.Format("轉入成功資料筆數「{0}」，重複之課程/社團筆數「{1}」\n", listCourseData.Count, dataGridViewX1.SelectedRows.Count - listCourseData.Count) +
                            "是否將重複之社團加入待處理?", "提醒", MessageBoxButtons.YesNo);
                        if (result2 == DialogResult.Yes)
                        {
                            ClubAdmin.Instance.AddToTemp(skipIDList);
                        }
                    }
                    
                    ReloadDataGridView();
                }
            }
            else
            {
                MsgBox.Show("請選擇要轉入課程的社團資料!");
            }
        }

        private void btnLeave_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
