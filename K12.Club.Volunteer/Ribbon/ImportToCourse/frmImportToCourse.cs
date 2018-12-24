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
using JHSchool.Association;

namespace K12.Club.Volunteer
{
    public partial class frmImportToCourse : BaseForm
    {
        private QueryHelper _qh = new QueryHelper();
        private UpdateHelper _up = new UpdateHelper();
        private bool _initFinish = false;
        private string _examTemplateID;
        private string _examTemplateName;
        private string _courseTagID;

        private class ClubRecord
        {
            public string ClubName { get; set; }
            public string RefTeacherName { get; set; }
            public string RefTeacherID { get; set; }
            public string RefExamTemplateID { get; set; }
            public string IsImport { get; set; }
            public List<string> listStudentID { get; set; }
        }
        private Dictionary<string, ClubRecord> _dicClubRecordByName = new Dictionary<string, ClubRecord>();

        public frmImportToCourse()
        {
            InitializeComponent();
        }

        private void frmImportToCourse_Load(object sender, EventArgs e)
        {
            // Init exam_template
            InitExamTemplate();

            // Init tag
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
        }

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
WITH insert_data (
    INSERT INTO exam_template(
        name 
    )VALUES(
        '社團評量(社團模組)'
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
                catch(Exception ex)
                {
                    MsgBox.Show(ex.Message);
                }
            }
        }

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
        , category
    ) VALUES(
        '聯課活動'
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
        , teacher.teacher_name
    FROM
        $k12.clubrecord.universal AS club
        LEFT OUTER JOIN teacher
            ON teacher.id = club.ref_teacher_id::BIGINT
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
        AND tag.category = 'Course'
        AND tag.prefix = '聯課活動'
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

                ", cbxSchoolYear.SelectedItem.ToString(),cbxSemester.SelectedItem.ToString()); 
                #endregion
                DataTable dt = this._qh.Select(sql);

                #region 資料整理
                foreach (DataRow row in dt.Rows)
                {
                    if (!this._dicClubRecordByName.ContainsKey("" + row["club_name"]))
                    {
                        ClubRecord data = new ClubRecord();
                        data.ClubName = "" + row["club_name"];
                        data.RefTeacherID = "" + row["ref_teacher_id"];
                        data.RefTeacherName = "" + row["teacher_name"];
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
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].listStudentID.Count;
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].RefTeacherName;
                    dgvrow.Cells[col++].Value = this._examTemplateName;
                    dgvrow.Cells[col++].Value = this._dicClubRecordByName[clubName].IsImport;

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
            DataTable dtCourse = new DataTable(); ;

            #region 建立課程，並取回課程資料
            {
                List<string> listCourseData = new List<string>();
                #region 資料整理
                foreach (string clubName in this._dicClubRecordByName.Keys)
                {
                    // 取得尚未轉入課程的資料
                    if (this._dicClubRecordByName[clubName].IsImport == "否")
                    {
                        string data = string.Format(@"
SELECT
    '{0}'::TEXT AS course_name
    , {1}::BIGINT AS ref_teacher_id
    , {2}::BIGINT AS ref_exam_template_id 
    , {3}::SMALLINT AS school_year
    , {4}::SMALLINT AS semester
                        ", clubName
                        , this._dicClubRecordByName[clubName].RefTeacherID == "" ? "null" : this._dicClubRecordByName[clubName].RefTeacherID
                        , this._dicClubRecordByName[clubName].RefExamTemplateID
                        , cbxSchoolYear.SelectedItem.ToString()
                        , cbxSemester.SelectedItem.ToString()
                    );
                        listCourseData.Add(data);
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
        , ref_exam_template_id
        , school_year
        , semester
    )
    SELECT
        course_name
        , ref_teacher_id
        , ref_exam_template_id
        , school_year
        , semester
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
                    MsgBox.Show("沒有資料可以轉入課程!");
                    return;
                }
            }
           
            #endregion

            #region 轉入修課學生
            {
                List<string> listSCAttend = new List<string>();

                #region 資料整理
                foreach (DataRow row in dtCourse.Rows)
                {
                    string courseName = "" + row["course_name"];
                    string courseID = "" + row["id"];

                    foreach (string studentID in this._dicClubRecordByName[courseName].listStudentID)
                    {
                        string data = string.Format(@"
SELECT
    {0}::BIGINT AS ref_student_id
    , {1}::BIGINT AS ref_course_id
                    ", studentID, courseID);

                        listSCAttend.Add(data);
                    }
                } 
                #endregion
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
            }
            #endregion

            #region 新建課程貼上聯課活動標籤
            {
                if (dtCourse.Rows.Count > 0)
                {
                    List<string> listDataRow = new List<string>();

                    foreach (DataRow row in dtCourse.Rows)
                    {
                        string data = string.Format(@"
SELECT
    {0}::BIGINT AS ref_course_id
    , {1}::BIGINT AS ref_tag_id
                        ","" + row["id"]
                        , this._courseTagID);

                        listDataRow.Add(data);
                    }

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
            }
            #endregion

            MsgBox.Show("資料轉入成功");
            // 高雄社團頁籤資料Reload
            AssnAdmin.Instance.BGW1.RunWorkerAsync();
            // 課程頁籤資料Reload
            JHSchool.Course.Instance.SyncAllBackground();
            //K12.Presentation.NLDPanels.Course.RefillListPane();
            ReloadDataGridView();
        }

        private void btnLeave_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
