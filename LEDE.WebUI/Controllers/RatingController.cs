﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LEDE.Domain.Abstract;
using LEDE.Domain.Concrete;
using LEDE.Domain.Entities;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace LEDE.WebUI.Controllers
{
    [Authorize(Roles="Faculty")]
    public class RatingController : Controller
    {
        private LEDE.Domain.Abstract.IRatingRepository db; 

        public RatingController(LEDE.Domain.Abstract.IRatingRepository repo)
        {
            this.db = repo;
        }

        public PartialViewResult GetIndexData(int SelectedUserID, int ProgramCohortID)
        {
            return PartialView(db.getTaskVersions(SelectedUserID, ProgramCohortID)); 
        }

        public ActionResult Index(bool? UploadVisible, int? VersID, int? userID, int? ProgramCohortID)
        {
            int FacultyID = Convert.ToInt32(User.Identity.GetUserId()); 
            RatingIndexModel model = db.getIndexModel(FacultyID, ProgramCohortID, userID);
            model.UploadVisible = UploadVisible ?? false;
            model.VersID = VersID;

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(RatingIndexPost post)
        {
            int FacultyID = Convert.ToInt32(User.Identity.GetUserId()); 
            RatingIndexModel model = db.getIndexModel(FacultyID, post.SelectedCohortID, post.SelectedUserID);

            return View(model);
        }

        public ActionResult Download(int DocumentID)
        {            
            Document downloadDoc = db.findDocument((int)DocumentID);            
            FileManager.DownloadDocument(downloadDoc, HttpContext);             

            return RedirectToAction("Index");
        }

        public PartialViewResult Upload(int VersID, int SelectedUserID, string User, string TaskName, int Version, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl; 
            ViewBag.Version = Version;
            ViewBag.SelectedUserID = SelectedUserID; 
            ViewBag.User = User;
            ViewBag.TaskName = TaskName; 
            return PartialView(VersID);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, int SelectedUserID, int VersID, string returnUrl)
        {
            
            if (file != null && file.ContentLength > 0)
            {
                Document feedbackUpload = new Document
                {
                    FileName = file.FileName,
                    FileSize = file.ContentLength.ToString(),
                    UploadDate = DateTime.Now,
                    Container = "user" + SelectedUserID
                };               
                
                db.saveFeedback(feedbackUpload, VersID);
                try
                {
                    FileManager.UploadDocument(feedbackUpload, file);
                    ViewBag.Message = "Upload Successfull!";
                }
                catch
                {
                    ViewBag.Message = "Upload Failed. Please Try Again.";
                }
            }

            if (returnUrl != null)
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Rate(int VersID)
        {
            RatingViewModel model = db.getRatingModel(VersID);

            return View(model);
        }

        [HttpPost]
        public ActionResult Rate(RatingViewModel post)
        {
            if (ModelState.IsValid)
            {
                db.saveCompleteRating(post);
                return RedirectToAction("Summary", "Faculty");
            }
            else
            {
                RatingViewModel model = db.getRatingModel(post.VersID);
                return View(model);
            }
        }
        
        [HttpPost]
        public ActionResult Header(RatingViewModel post)
        {
            return View("Rate", db.getRatingModel(post.VersID));
        }

        public PartialViewResult Reflection(int VersID)
        {
            RatingViewModel model = new RatingViewModel() {TaskVersion = db.getTaskVersion(VersID) };
            return PartialView(model);
        }

        public PartialViewResult Task(int VersID)
        {
            RatingViewModel model = db.getTaskRatings(VersID);
            return PartialView(model); 
        }

        public PartialViewResult Other(int VersID)
        {
            RatingViewModel model = db.getOtherRatings(VersID);
            return PartialView(model); 
        }

        [HttpPost]
        public ActionResult Other(RatingViewModel post)
        {
            CoreRating ratingToAdd;
            if (post.Rating.OtherCoreRatings != null)
            {
                int addindex = post.Rating.OtherCoreRatings.Count - 1;
                ratingToAdd = post.Rating.OtherCoreRatings[addindex];
            }
            else ratingToAdd = null;
            
            if (ratingToAdd != null && ratingToAdd.RatingID == 0 && ValidateCoreRating(ratingToAdd))
            {
                CompleteRating rating = new CompleteRating()
                {
                    OtherCoreRatings = new List<CoreRating>()
                };
                rating.OtherCoreRatings.Add(ratingToAdd); 
                db.saveTaskRating(rating, post.VersID);
            }

            RatingViewModel model = db.getOtherRatings(post.VersID);
            model.OtherVisible = true;
            return PartialView(model); 
        }

        public PartialViewResult Impact(int VersID, bool ImpactVisible = false)
        {
            RatingViewModel model = db.getImpactRatings(VersID);
            model.ImpactVisible = ImpactVisible;
            return PartialView(model);
        }

        private bool ValidateCoreRating(CoreRating rating)
        {
            if (((rating.Cscore > 0 && rating.Cscore < 4) || rating.Cscore == null) && 
                ((rating.Sscore > 0 && rating.Sscore < 4) || rating.Sscore == null) &&
                ((rating.Pscore > 0 && rating.Pscore < 4) || rating.Pscore == null))            
                return true;
            else
                return false; 
        }

        public PartialViewResult Clear(int VersID, int RatingID, string Type)
        {
            if(VersID > 0)
                db.deleteTaskRating(RatingID);
            switch (Type)
            {
                case "Task":
                    return PartialView("Task", db.getTaskRatings(VersID));
                case "Other":
                    return PartialView("Other", db.getOtherRatings(VersID));
                case "Impact":
                    break;
                case "Default":
                    break;
            }
            return PartialView("Task", db.getTaskRatings(VersID));
        }
    }
}