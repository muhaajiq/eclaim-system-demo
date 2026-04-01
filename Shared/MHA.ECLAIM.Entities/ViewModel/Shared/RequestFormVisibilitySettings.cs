using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    public class RequestFormVisibilitySettings
    {
        //Buttons
        public bool ShowRequireAmendment { get; set; } = false;
        public bool ShowApprove { get; set; } = false;
        public bool ShowReject { get; set; } = false;
        public bool ShowSave { get; set; } = false;
        public bool ShowSubmit { get; set; } = false;
        public bool ShowClose { get; set; } = false;
        public bool ShowWorkFlowHistory { get; set; } = false;
        public bool EnableRemarks { get; set; } = false;
    }
}
