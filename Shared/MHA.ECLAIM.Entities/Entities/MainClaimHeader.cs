using System.ComponentModel.DataAnnotations.Schema;

namespace MHA.ECLAIM.Entities.Entities
{
    [Table("ecMainClaimHeader")]
    public class MainClaimHeader
    {
        public int ID { get; set; }
        public int ProcessID { get; set; }
        public string RequesterCompanyCode { get; set; } = string.Empty;
        public string RequesterCompanyName { get; set; } = string.Empty;
        public string RequesterEmployeeID { get; set; } = string.Empty;
        public string RequesterDepartment { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string RequesterLogin { get; set; } = string.Empty;
        public string RequesterCostCenter { get; set; } = string.Empty;

        public DateTime? RequestDate { get; set; }
        public string ReferenceNo { get; set; } = string.Empty;
        public string ClaimStatus { get; set; } = string.Empty;

        public string PurposeOfClaims { get; set; } = string.Empty;
        public string ClaimCurrency { get; set; } = string.Empty;
        public string TravelRequestFormRef { get; set; } = string.Empty;

        public string ClaimCategory { get; set; } = string.Empty;
        public int SelectedClaimCategorySpId { get; set; }
        public string ClaimEntitlementType1 { get; set; } = string.Empty;
        public int? ClaimEntitlementType1SpId { get; set; }
        public DateTime? ClaimEntitlementType1StartDate { get; set; }
        public DateTime? ClaimEntitlementType1EndDate { get; set; }
        public string ClaimEntitlementType2 { get; set; } = string.Empty;
        public int? ClaimEntitlementType2SpId { get; set; }
        public DateTime? ClaimEntitlementType2StartDate { get; set; }
        public DateTime? ClaimEntitlementType2EndDate { get; set; }
        public string CurrentClaimEntitlementType { get; set; } = string.Empty;
        public int? CurrentClaimEntitlementTypeSpId { get; set; }

        public string GeneralRemarks { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public string CreatedByLogin { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }

        public string ModifiedBy { get; set; } = string.Empty;
        public string ModifiedByLogin { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }

        public string SubmittedBy { get; set; } = string.Empty;
        public string SubmittedByLogin { get; set; } = string.Empty;
        public DateTime? SubmittedDate { get; set; }
        public string TempFolderGuid { get; set; } = string.Empty;
        public virtual ICollection<SubClaimDetails> SubClaimDetails { get; set; } = new List<SubClaimDetails>();
        public int? DepartmentID { get; set; }
    }
}
