using MHA.Framework.Core.Workflow.BO;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class DelegationObject : DelegationInstance
    {
        public bool IsAbleToRemove { get; set; }
        public bool Active { get; set; }

        public int DelegationID { get; set; }
        public int ProcessTemplateID { get; set; }
        public string ProcessName { get; set; }
        public string ApplicationName { get; set; }

        public string DelegationFrom { get; set; }
        public string DelegationFromFriendlyName { get; set; }
        public string DelegationFromEmail { get; set; }
        public string DelegationTo { get; set; }
        public string DelegationToFriendlyName { get; set; }
        public string DelegationToEmail { get; set; }
        public DateTime DelegationStartDate { get; set; }
        public DateTime DelegationEndDate { get; set; }
    }
}
