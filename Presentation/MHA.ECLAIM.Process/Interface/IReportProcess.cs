using MHA.ECLAIM.Entities.ViewModel.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Process.Interface
{
    public interface IReportProcess
    {
        Task<ViewModelReportListing> GetReportListing(string spHostUrl, string accessToken);
    }
}
