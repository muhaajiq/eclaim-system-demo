namespace MHA.ECLAIM.Entities.ViewModel.TravelRequest
{
    public class TravelRequestHeaderVM
    {
        public int? ID { get; set; }
        public int? ProcessID { get; set; }
        public string? ReferenceNo { get; set; }
        public string? CompanyCode { get; set; }
        public string? CompanyName { get; set; }
        public string? EmployeeID { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeLogin { get; set; }
        public string? Department { get; set; }
        public DateTime? DateOfRequest { get; set; }
        public string? CostCenter { get; set; }
        public string? WorkflowStatus { get; set; }
        public string? Remarks { get; set; }

        public string? CreatedBy { get; set; }
        public string? CreatedByLogin { get; set; }
        public DateTime? CreatedByDate { get; set; }

        public string? ModifiedBy { get; set; }
        public string? ModifiedByLogin { get; set; }
        public DateTime? ModifiedByDate { get; set; }

        public string? SubmittedBy { get; set; }
        public string? SubmittedByLogin { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }
}
