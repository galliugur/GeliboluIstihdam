using System;
using System.Collections.Generic;

namespace GeliboluIstihdam.Areas.Employer.Models
{
    public class JobPostingsViewModel
    {
        public List<JobPosting> JobPostings { get; set; }
    }

    public class JobPosting
    {
        public string Title { get; set; }
        public DateTime PublishDate { get; set; }
        public int ApplicationCount { get; set; }
        public string Status { get; set; }
    }
}