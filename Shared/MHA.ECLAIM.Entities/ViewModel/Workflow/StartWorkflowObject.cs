using MHA.ECLAIM.Entities.ViewModel.Shared;
using System.Runtime.Serialization;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class StartWorkflowObject
    {
        #region WF Actioners
        public PeoplePickerUser Originator { get; set; }
        public List<PeoplePickerUser> ReportingManager1 { get; set; }
        public List<PeoplePickerUser> ReportingManager2 { get; set; }
        public List<PeoplePickerUser> ReportingManager3 { get; set; }
        public List<PeoplePickerUser> ClaimItemApprover { get; set; }
        public List<PeoplePickerUser> FinalApprover { get; set; }
        #endregion

        #region WF Actioner Due Days
        public int PendingOriginatorResubmissionDueDays { get; set; }
        public int PendingReportingManager1DueDays { get; set; }
        public int PendingReportingManager2DueDays { get; set; }
        public int PendingReportingManager3DueDays { get; set; }
        public int PendingClaimItemApproverDueDays { get; set; }
        public int PendingFinalApproverDueDays { get; set; }
        public bool UseDefaultDueDays { get; set; }
        #endregion

        #region WF Actioner Due Dates
        public string PendingOriginatorResubmissionDueDate { get; set; }
        public string PendingReportingManager1DueDate { get; set; }
        public string PendingReportingManager2DueDate { get; set; }
        public string PendingReportingManager3DueDate { get; set; }
        public string PendingClaimItemApproverDueDate { get; set; }
        public string PendingFinalApproverDueDate { get; set; }
        #endregion

        [DataMember]
        public int ProcessId { get; set; }

        public StartWorkflowObject()
        {
            ReportingManager1 = new List<PeoplePickerUser>();
            ReportingManager2 = new List<PeoplePickerUser>();
            ReportingManager3 = new List<PeoplePickerUser>();
            ClaimItemApprover = new List<PeoplePickerUser>();
            FinalApprover = new List<PeoplePickerUser>();
        }
    }
}
