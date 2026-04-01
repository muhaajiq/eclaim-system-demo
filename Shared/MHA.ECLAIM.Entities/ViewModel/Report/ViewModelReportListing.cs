using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.Report
{
    public class ViewModelReportListing
    {
        public bool IsAuthorized { get; set; }
        public Dictionary<string, string> ReportLinks { get; set; }

        public ViewModelReportListing()
        {
            IsAuthorized = false;
            ReportLinks = new Dictionary<string, string>();
        }
    }
}
