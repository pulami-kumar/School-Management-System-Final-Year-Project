﻿using _19033684_Kumar_Pulami.Models.ViewModel;
using _19033684_Kumar_Pulami.Models.ViewModel.BatchGradeSubject;
using _19033684_Kumar_Pulami.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace _19033684_Kumar_Pulami.Controllers
{
    public class ClassSubjectController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.TitleName = "Assign Subject";
            ViewBag.BatchList = DropDownFilter.GetBatchList();

            List<String> gradeList = new List<String>();
            gradeList.Add("Select Grade");
            ViewBag.GradeList = gradeList;
            ClassSubjectViewModel model = new ClassSubjectViewModel();
            model.SubjectList = null;
            return View(model);
        }


        [HttpPost]
        public IActionResult Index(ClassSubjectViewModel value)
        {
            ViewBag.TitleName = "Assign Subject";
            ViewBag.BatchList = DropDownFilter.GetBatchList();
            ViewBag.GradeList = DropDownFilter.GetGradeList(value.Batch);
            value.SubjectList = GetSubjectList(value.Batch, value.Grade);
            return View(value);
        }

        public List<SubjectViewModel> GetSubjectList(int batch, int grade)
        {
            DataTable queryData;
            List<SubjectViewModel> subjecList = new List<SubjectViewModel>();
            SubjectViewModel subjectDetail;
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnection()))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand("SELECT Subject.ID, Subject.SubjectName FROM BatchGradeSubject JOIN Subject ON BatchGradeSubject.SubjectID = Subject.ID WHERE BatchGradeSubject.Grade = @grade AND BatchGradeSubject.Batch = @batch;", connection))
                {
                    command.Parameters.AddWithValue("@batch", batch);
                    command.Parameters.AddWithValue("@grade", grade);
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                    {
                        queryData = new DataTable();
                        dataAdapter.Fill(queryData);
                    }
                }
                if (queryData.Rows.Count != 0)
                {
                    foreach (DataRow row in queryData.Rows)
                    {
                        subjectDetail = new SubjectViewModel();
                        subjectDetail.SubjectID = int.Parse(row[0].ToString());
                        subjectDetail.SubjectName = row[1].ToString();
                        subjecList.Add(subjectDetail);
                    }
                }
            }
            return subjecList;
        }

        [HttpGet]
        public IActionResult AssignSubject()
        {
            ViewBag.TitleName = "Assign Subject";

            ViewBag.BatchList = DropDownFilter.GetBatchList(); 

            List<String> gradeList = new List<String>();
            gradeList.Add("Select Grade");
            ViewBag.GradeList = gradeList;


            SubjectViewModel subjectDetail = new SubjectViewModel();
            subjectDetail.SubjectID = 0;
            subjectDetail.SubjectName = "Select Subject";
            List<SubjectViewModel> subjectList = new List<SubjectViewModel>();
            subjectList.Add(subjectDetail);

            ViewBag.SubjectList = subjectList;

            return View();
        }

        [HttpPost]
        public IActionResult AssignSubject(ClassSubjectAssignViewModel value)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnection()))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand("INSERT INTO BatchGradeSubject VALUES(@batch, @grade, @subject);", connection))
                {
                    command.Parameters.AddWithValue("@batch", value.Batch);
                    command.Parameters.AddWithValue("@grade", value.Grade);
                    command.Parameters.AddWithValue("@subject", value.SubjectID);
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Subject Assigned Successfully...!";
            return RedirectToAction("Index");
        }


        public List<SubjectViewModel> GetNonAssignedSubjectList(int batch, int grade)
        {
            DataTable queryData;
            List<SubjectViewModel> subjList = new List<SubjectViewModel>();
            SubjectViewModel subjectDetail;
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnection()))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand("SELECT * FROM Subject WHERE Subject.ID NOT IN (SELECT SubjectID FROM BatchGradeSubject WHERE Batch = @batch AND Grade = @grade);", connection))
                {
                    command.Parameters.AddWithValue("@batch", batch);
                    command.Parameters.AddWithValue("@grade", grade);
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(command))
                    {
                        queryData = new DataTable();
                        dataAdapter.Fill(queryData);
                    }
                }
                if (queryData.Rows.Count != 0)
                {
                    foreach (DataRow row in queryData.Rows)
                    {
                        subjectDetail = new SubjectViewModel();
                        subjectDetail.SubjectID = int.Parse(row[0].ToString());
                        subjectDetail.SubjectName = row[1].ToString();
                        subjList.Add(subjectDetail);
                    }
                }
            }
            return subjList;
        }

        public JsonResult GetNonAssignedSubjectJSON(int batch, int grade, int subject)
        {
            return Json(GetNonAssignedSubjectList(batch, grade));
        }


        [HttpGet]
        public IActionResult Delete(int batch, int grade, int SubjectID)
        {
            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnection()))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand("DELETE FROM BatchGradeSubject WHERE Batch = @batch AND Grade = @grade AND SubjectID = @subject;", connection))
                {
                    command.Parameters.AddWithValue("@batch", batch);
                    command.Parameters.AddWithValue("@grade", grade);
                    command.Parameters.AddWithValue("@subject", SubjectID);
                    command.ExecuteNonQuery();
                }
            }

            using (SqlConnection connection = new SqlConnection(DatabaseAccess.GetConnection()))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }
                using (SqlCommand command = new SqlCommand("DELETE Marks FROM Marks JOIN Student ON Marks.StudentID = Student.ID WHERE Student.Batch = @batch AND Student.Grade = @grade AND Marks.SubjectID = @subject;", connection))
                {
                    command.Parameters.AddWithValue("@batch", batch);
                    command.Parameters.AddWithValue("@grade", grade);
                    command.Parameters.AddWithValue("@subject", SubjectID);
                    command.ExecuteNonQuery();
                }
            }
            TempData["SuccessMessage"] = "Subject Unassigned Successfully...!";
            ClassSubjectViewModel model = new ClassSubjectViewModel();
            model.Batch = batch;
            model.Grade = grade;
            return RedirectToAction("Index", model);
        }
    }
}
