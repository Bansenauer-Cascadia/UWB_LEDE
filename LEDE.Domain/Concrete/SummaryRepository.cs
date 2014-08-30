﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity; 
using System.Web;
using LEDE.Domain.Abstract;
using LEDE.Domain.Entities; 

namespace LEDE.Domain.Concrete
{
    public class SummaryRepository : ISummaryRepository
    {
        private DbContext db;

        public SummaryRepository(DbContext context)
        {
            this.db = context; 
        }

        public SummaryRepository()
        {
            this.db = new DbContext(); 
        }

        public SeminarSummary getCohortTotals(int cohortID, int userID)
        {
            return null; 
        }

        public StudentSummary getStudentTotals(int cohortID, int userID)
        {
            IEnumerable<User> cohortStudents = db.Users.Where(u => u.CohortEnrollments.Any(e => e.ProgramCohortID == cohortID) &&
                u.Roles.Any(r => r.RoleId == 1));
            int ProgramID = db.ProgramCohorts.Find(cohortID).ProgramID;

            var TaskTotals = from cr in db.CoreRatings
                             where cr.TaskRating.TaskVersion.Task.Seminar.ProgramID == ProgramID && cr.TaskRating.TaskVersion.UserID == userID
                             group cr by new { cr.TaskRating.TaskVersion.TaskID, cr.CoreTopic } into g
                             let maxVersion = g.Max(cr => cr.TaskRating.TaskVersion.Version)
                             select new
                             {
                                 CScore = g.FirstOrDefault(cr => cr.TaskRating.TaskVersion.Version == maxVersion).Cscore,
                                 SScore = g.FirstOrDefault(cr => cr.TaskRating.TaskVersion.Version == maxVersion).Sscore,
                                 PScore = g.FirstOrDefault(cr => cr.TaskRating.TaskVersion.Version == maxVersion).Pscore,
                                 TaskID = g.Key.TaskID,
                                 CoreTopic = g.Key.CoreTopic
                             };

            var UserTotals = from tt in TaskTotals
                             group tt by tt.CoreTopic into g
                             select new CoreTotal()
                             {
                                 CTotal = g.Sum(tt => tt.CScore) ?? 0,
                                 STotal = g.Sum(tt => tt.SScore) ?? 0,
                                 PTotal = g.Sum(tt => tt.PScore) ?? 0,
                                 OneCount = g.Count(tt=> tt.CScore == 1) + g.Count(tt=> tt.SScore == 1) + g.Count(tt=> tt.PScore == 1),
                                 TwoCount = g.Count(tt => tt.CScore == 2) + g.Count(tt => tt.SScore == 2) + g.Count(tt => tt.PScore == 2),
                                 ThreeCount = g.Count(tt => tt.CScore == 3) + g.Count(tt => tt.SScore == 3) + g.Count(tt => tt.PScore == 3),
                                 CoreTopic = g.Key
                             };

            StudentSummary model = new StudentSummary()
            {
                RatingsList = UserTotals.ToList(),
                User = db.Users.Find(userID),
                MaxTotal = UserTotals.Select(u => new { Total = u.CTotal + u.STotal + u.PTotal }).Max(u => u.Total),
                MaxCount = UserTotals.Select(u => new {Count = u.OneCount + u.TwoCount + u.ThreeCount }).Max(u=> u.Count)
            };

            return model;  
        }

        public IEnumerable<ProgramCohort> getCohorts()
        {
            return db.ProgramCohorts; 
        }
    }
}