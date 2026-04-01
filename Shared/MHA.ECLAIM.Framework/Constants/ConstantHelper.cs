using System.Security.Claims;

namespace MHA.ECLAIM.Framework.Constants
{
    public partial class ConstantHelper
    {
        #region API
        public class API
        {
            public class HomePath
            {
                public const string GetMyPendingTask = "api/Home/GetMyPendingTask";
                public const string GetMyActiveRequest = "api/Home/GetMyActiveRequest";
            }

            public class ReportPath
            {
                public const string GetReportListing = "api/Report/GetReportListing";
            }

            public class AdministrationPath
            {
                public const string InitAdministrationListing = "api/Administration/InitAdministrationListing";
            }

            public class ClaimPath
            {
                public const string SearchClaim = "api/Claim/SearchClaim";
                public const string InitClaim = "api/Claim/InitClaim";
                public const string InitClaimDisplayForm = "api/Claim/InitClaimDisplayForm";
                public const string InitApprovalForm = "api/Claim/InitApprovalForm";
                public const string RequireAmendmentRequest = "api/Approval/RequireAmendmentRequest";
                public const string ApproveRequest = "api/Approval/ApproveRequest";
                public const string RejectRequest = "api/Approval/RejectRequest";
                public const string SaveRequest = "api/Approval/SaveRequest";
                public const string SaveNewRequestForm = "api/Claim/SaveNewRequestForm";
                public const string ReSubmitClaimRequest = "api/Claim/ReSubmitClaimRequest";
                public const string SubmitClaimRequestForm = "api/Claim/SubmitClaimRequestForm";
                public const string GetAll = "api/Claim/GetAll";
                public const string InitExpensesList = "api/Claim/InitExpensesList";
                public const string DeleteClaimAttachment = "api/Claim/DeleteClaimAttachment";
                public const string SaveClaimDetails = "api/Claim/SaveClaimDetails";
                public const string DeleteClaimDetails = "api/Claim/DeleteClaimDetails";

                public const string SetVisibilitySettings = "api/Claim/SetVisibilitySettings";
                public const string GetExpenseModalInfo = "api/Claim/GetExpenseModalInfo";
            }

            public class WorkflowPath
            {
                public const string InitWorkflowHistory = "api/Workflow/InitWorkflowHistory";
                public const string InitAdminWFHistory = "api/Workflow/InitAdminWFHistory";
                public const string AddActioner = "api/Workflow/AddActioner";
                public const string ReassignActioner = "api/Workflow/ReassignActioner";
                public const string RemoveActioner = "api/Workflow/RemoveActioner";
                public const string RemoveAllActioner = "api/Workflow/RemoveAllActioner";

                public const string SetDelegation = "api/Workflow/SetDelegation";
                public const string InitNewDelegate = "api/Workflow/InitNewDelegate";
                public const string InitMyDelegate = "api/Workflow/InitMyDelegate";
                public const string DeleteDelegation = "api/Workflow/DeleteDelegation";

                //Workflow Task Report
                public const string InitAuthWorkflowReportCheck = "api/Workflow/InitAuthWorkflowReportCheck";
                public const string SearchWorflowTaskReport = "api/Workflow/SearchWorkflowTaskReport";
                public const string ExportToExcelWorkflowTaskReport = "api/Workflow/ExportToExcelWorkflowTaskReport";
            }
        }
        #endregion

        #region Delimiter
        public class Delimiter
        {
            public static char[] SemiColonDelimiter = { ',' };
        }
        #endregion

        #region Email
        public class Email
        {
            public class ExceptionMessage
            {
                public const string ConnectedPartyNotProperlyRespond = "A connection attempt failed because the connected party did not properly respond after a period of time";
                public const string OperationTimedOut = "The operation has timed out";
                public const string MailboxServerIsTooBusy = "mailbox server is too busy";
                public const string MapiExceptionRpcServerTooBusy = "MapiExceptionRpcServerTooBusy";
                public const string SenderThreadLimitExceeded = "sender thread limit exceeded";
                public const string ConcurrentConnectionLimitExceed = "Concurrent connections limit exceeded";
                public const string TemporaryServerError = "Temporary server error";
                public const string ConnectionForcibltClosed = "An existing connection was forcibly closed by the remote host";
                public const string UnableReadData = "Unable to read data from the transport connection";

                public const string SubmissionQuotaExceededException = "SubmissionQuotaExceededException";
                public const string SenderSubmissionExceeded = "sender's submission quota was exceeded";
            }

        }
        public class EmailTemplateKeyTitle
        {
            public const string WorkflowTaskReassignedNotification = "Workflow Task Reassigned Notification";
            public const string WorkflowTaskDelegatedNotification = "Workflow Task Delegated Notification";
        }
        #endregion

        #region Execption
        public class ExceptionType
        {
            public const string SaveConflict = "save conflict";
            public const string VersionConflict = "version conflict";
            public const string OperationTimedOut = "the operation has timed out";
            public const string HResult = "hresult: 0x80131904";
            public const string UnderlineClosed = "underlying connection was closed";
        }
        #endregion

        #region General
        public class DateFormat
        {
            public const string DefaultDateFormat = "dd-MMM-yyyy";
            public const string DefaultDateTimeFormat = "dd-MMM-yyyy HH:mm";
        }

        public class ItemStatus
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
        }

        public class TableColumnWidth
        {
            public const string General = "200px";
        }
        #endregion

        #region Limits and row counts
        public static int MyPagingSelectionShowLimit = 7;
        #endregion

        #region Module
        public class Module
        {
            public const string Eclaim = "Eclaim";
        }
        #endregion

        #region Message
        public class ErrorMessage
        {
            public const string NoAuthorized = "Unfortunately, you do not have permission to access {0}.";
            public const string UnexpectedErrorOccur = "An unexpected error has occurred. Please try again. If the issue persists, please contact support.";
            public const string NetworkError = "Network error. Please check your connection or VPN.";
            public const string TimedOutError = "The request timed out. Please check your internet connection and try again.";

            //Creation Form
            public const string SaveFailed = "Failed to Save. Unexpected Error Occur.";

            //New Claim Request Form
            public const string SaveError = "We could not save your request. Please try again. If the issue persists, please contact support.";
            public const string SubmitError = "We could not submit your request. Please try again. If the issue persists, please contact support.";
            //public const string SaveFailed = "Failed to save: {0}";
            public const string SubmitFailed = "Failed to submit: {0}";

            // Display Form
            public const string NotAuthorizedView = "Unfortunately, you are not authorized to view this user's {0}.";

            //Workflow
            public const string MissingWorkflowMatrix = "The system encounter an error during fetching {0} user. Please try again later.";
            public const string MissingWFDueDate = "The system encounter an error during fetching {0} due days. Please try again later.";

            //Approval Form
            public const string NonActiveTask = "This task is no longer available. It is either you have completed the task or the task has been removed.";
            public const string NotUserTask = "This task belongs to a different user.";
            public const string ApprovalError = "An error occurred during approval. Please try again later.";
            public const string TaskInProgressErrorMsg = "This task is currently being completed in another tab/browser or by another actioner. Your current changes cannot be saved. Please refresh the page to see the latest version";
            public const string RemarksForRequireAmendment = "Please provide remarks before requesting amendment.";
            public const string RemarksForReject = "Please provide remarks before rejecting the request.";

            //Claim Details
            public const string RemoveClaimDetailsWarning = "Please select <b>at least one</b> claim to delete.";
            public const string InvalidFileFormat = "Only <b>PDF, JPG or PNG</b> files are allowed.";
            public const string FileSizeLimitWarning = "Maximum allowed size is <br/><b>10 MB.</b>";
            public const string IsFileUploaded = "There is no file being uploaded.";
            public const string EffectiveDateWarning = "Transaction Date is outside the allowed date range <br/>for the selected expense subtype<br/><b>{0} - {1}</b>.";
            public const string EntitlementWarning = "Your entitlement for this claim type is not valid on the selected Transaction Date.";
            public const string ClaimLimitWarning = "Your total for this claim is {0}, which exceeds the {1} limit of {2}. Do you want to proceed?";
        }

        public class SuccessMessage
        {
            public const string SubmitRequestSuccess = "Claim request has been submitted successfully and workflow is started.";
            public const string SaveDraftSuccess = "Item has been saved successfully and is currently saved as draft.";
            public const string ApproveRequestSuccess = "The task has been approved successfully.";
            public const string RejectRequestSuccess = "The task has been rejected successfully.";
            public const string RequireAmendmentSuccess = "The task is required for amendment.";
            public const string SaveRequestSuccess = "The task has been saved successfully.";
            public const string CompleteRequestSuccess = "The task has been completed successfully.";
            public const string ResubmitRequestSuccess = "The task has been resubmitted successfully.";

            //Popup
            public const string RequireAmendmentPopup = "Request set to require amendment successfully.";
            public const string ApprovePopup = "Request approved successfully.";
            public const string RejectPopup = "Request rejected successfully.";
            public const string SavePopup = "Request saved successfully.";
            public const string CompletePopup = "Request completed successfully.";
            public const string ResubmitPopup = "Request resubmitted successfully.";
            public const string AddClaimPopup = "Claim has been added.";
        }

        public class InfoMessage
        {
            public const string EmailSentStatusInfo = "{0} {1} sent out at {2}.";
            public const string RejectRemarks = "Rejected reason: {0}";
        }
        #endregion

        #region Running Number Format
        public class RunningNumberFormatType
        {
            public const string EclaimRequestReferenceNumber = "Eclaim Request Reference Number";
        }

        public class RunningNumberFormatInstance
        {
            public const string RunningNo = "{RunningNo}";
            public const string RequestYear = "{RequestYear}";
        }
        #endregion

        #region Query String
        #endregion

        #region Stored Procedure
        public class StoreProcedureName
        {
            public const string GetMyPendingAndIncomingTasksByBatchByProcessNameApplicationName = "Eclaim_GetMyPendingAndIncomingTasksByBatchByProcessNameApplicationName";
            public const string GetMyPendingAndIncomingTasksCountByBatchByProcessNameApplicationName = "Eclaim_GetMyPendingAndIncomingTasksCountByBatchByProcessNameApplicationName";

            public const string GetWorkflowTaskHistoryByProcessID = "Eclaim_GetWorkflowTaskHistoryByProcessID";

            public const string WorkflowTaskReport = "EC_GetWorkflowTaskReport";
            public const string WorkflowTaskReportCount = "EC_GetWorkflowTaskReportCount";
        }

        public class StoreProcedureParameter
        {
            public const string ActionerLogin = "ActionerLogin";
            public const string ProcessName = "ProcessName";
            public const string ApplicationName = "ApplicationName";
            public const string PageNumber = "PageNumber";
            public const string RowsPerPage = "RowsPerPage";
            public const string ProcessTemplateID = "ProcessTemplateID";
            public const string WorkflowStatus = "WorkflowStatus";

            public class WorkflowTaskReport
            {
                public const string WorkflowStartedFrom = "WorkflowStartedFrom";
                public const string WorkflowStartedTo = "WorkflowStartedTo";

                public const string WorkflowCompletedEmpty = "WorkflowCompletedEmpty";
                public const string ActionedDateEmpty = "ActionedDateEmpty";

                public const string WorkflowCompletedFrom = "WorkflowCompletedFrom";
                public const string WorkflowCompletedTo = "WorkflowCompletedTo";
                public const string AssignedDateFrom = "AssignedDateFrom";
                public const string AssignedDateTo = "AssignedDateTo";
                public const string ActionedDateFrom = "ActionedDateFrom";
                public const string ActionedDateTo = "ActionedDateTo";

                public const string ReferenceNo = "ReferenceNo";
                public const string CompletionDate = "CompletionDate";
                public const string AssignedDate = "AssignedDate";
                public const string ActionedDate = "ActionedDate";

                public const string WorkflowType = "WorkflowType";
                public const string WorkflowStatus = "WorkflowStatus";
                public const string TaskActioner = "TaskActioner";
                public const string TaskStage = "TaskStage";
                public const string TaskStatus = "TaskStatus";
                public const string ProcessName = "ProcessName";

                public const string PageNumber = "PageNumber";
                public const string RowsPerPage = "RowsPerPage";
                //public const string SortField = "SortField";
                //public const string SortFieldTable = "SortFieldTable";
                //public const string SortDirection = "SortDirection";

            }
        }

        public class StoreProcedureColumn
        {
            public class WorkflowTaskReport
            {
                public const string ReferenceNo = "ReferenceNo";
                public const string EmployeeID = "EmployeeID";
                public const string CompanyName = "CompanyName";
                public const string ProcessID = "ProcessID";
                public const string ProcessName = "ProcessName";
                public const string WorkflowStage = "WorkflowStage";
                public const string StartDate = "StartDate";
                public const string CompletionDate = "CompletionDate";
                public const string AssigneeLogin = "AssigneeLogin";
                public const string AssigneeName = "AssigneeName";
                public const string AssigneeEmail = "AssigneeEmail";
                public const string AssignedDate = "AssignedDate";
                public const string ActionedDate = "ActionedDate";
                public const string DueDate = "DueDate";
                public const string DaysTaken = "DaysTaken";
                public const string TaskOverdueDays = "TaskOverdueDays";
                public const string TaskID = "TaskID";
                public const string Status = "Status";
                public const string StepTemplateID = "StepTemplateID";
                public const string TaskAction = "TaskAction";
                public const string StepName = "StepName";
                public const string InternalStepName = "InternalStepName";

            }
        }
        #endregion

        #region SharePoint

        #region Column Type
        public class SharePoint
        {
            public class ColumnType
            {
                public const string Text = "Text";
                public const string Number = "Number";
                public const string Integer = "Integer";
                public const string Boolean = "Boolean";
                public const string DateTime = "DateTime";
                public const string Lookup = "Lookup";
            }

            public class OperatorType
            {
                public const string Eq = "Eq";
                public const string Geq = "Geq";
                public const string Leq = "Leq";
                public const string Contains = "Contains";
            }
        }


        #endregion

        #region URL Template
        public class URLTemplate
        {
            public const string SiteContents = "{0}/_layouts/15/viewlsts.aspx?view=14";
            public const string SiteSettings = "{0}/_layouts/15/settings.aspx";

            public const string DisplayFormUrlTemplate = "{0}/PlaceHolder/{1}?SPHostUrl={2}";
            public const string HomePageUrlTemplate = "{0}/Home?SPHostUrl={1}";
            public const string ClaimRequestDisplayFormUrlTemplate = "{0}/ViewRequestForm?RequestId={1}&SPHostUrl={2}";
            public const string NewDelegationUrlTemplate = "{0}/NewDelegate?SPHostUrl={1}";
            public const string RemoveDelegationUrlTemplate = "{0}/RemoveDelegate?SPHostUrl={1}";

            public const string ReportURLTemplate = "{0}/Lists/ReportURL/AllItems.aspx";
            public const string RunningNumberFormatUrlTemplate = "{0}/Lists/Running%20Number%20Format/AllItems.aspx";
            public const string RunningNumberUrlTemplate = "{0}/Lists/Running%20Number/AllItems.aspx";

            public const string EmployeeUrlTemplate = "{0}/Lists/Employee/AllItems.aspx";
            public const string CountriesUrlTemplate = "{0}/Lists/Countries/AllItems.aspx";
            public const string CostCenterUrlTemplate = "{0}/Lists/CostCenter/AllItems.aspx";
            public const string DepartmentUrlTemplate = "{0}/Lists/Department/AllItems.aspx";
            public const string CompanyUrlTemplate = "{0}/Lists/Company/AllItems.aspx";
            public const string PositionUrlTemplate = "{0}/Lists/Position/AllItems.aspx";

            public const string ClaimEntitlementTypeUrlTemplate = "{0}/Lists/ClaimEntitlementType/AllItems.aspx";
            public const string ClaimItemApproverUrlTemplate = "{0}/Lists/Claim%20Item%20Approver/AllItems.aspx";
            public const string ClaimMasterApproverUrlTemplate = "{0}/Lists/Claim%20Master%20Approver/AllItems.aspx";

            public const string CurrencyUrlTemplate = "{0}/Lists/Currency/AllItems.aspx";
            public const string TaxCodeUrlTemplate = "{0}/Lists/TaxCode/AllItems.aspx";
            public const string GLCodeUrlTemplate = "{0}/Lists/GLCode/AllItems.aspx";

            public const string CategoryUrlTemplate = "{0}/Lists/Category/AllItems.aspx";
            public const string SubCategoryUrlTemplate = "{0}/Lists/SubCategory/AllItems.aspx";
            public const string FrequencyUrlTemplate = "{0}/Lists/Frequency/AllItems.aspx";

            public const string AttachmentPathUrlTemplate = "{0}{1}";
        }
        #endregion

        #region Site Assets URL Logo & Image Type Name

        public class SiteURLLogo
        {
            public const string LogoURL = "SiteAssets/logo.png";
        }
        
        #endregion

        #endregion

        #region SharePoint List
        public class SPList
        {
            //Maintenance list
            public const string Cities = "Cities";
            public const string ClaimCategory = "Claim Category";
            public const string ClaimEntitlementType = "Claim Entitlement Type";
            public const string ClaimItem = "Claim Item";
            public const string ClaimMasterApprover = "Claim Master Approver";
            public const string Company = "Company";
            public const string CostCenter = "Cost Center";
            public const string Countries = "Countries";
            public const string Currency = "Currency";
            public const string Department = "Department";
            public const string EmailTemplate = "Email Templates";
            public const string Employee = "Employee";
            public const string GLCode = "GL Code";
            public const string Position = "Position";
            public const string ReportUrl = "Report URL";
            public const string RunningNumber = "Running Number";
            public const string RunningNumberFormat = "Running Number Format";
            public const string States = "States";
            public const string TaxCode = "Tax Code";
            public const string TravelAgency = "Travel Agency";
            public const string Expenses = "Expenses";
            public const string CurrencyExchangeRate = "Currency Exchange Rate";

            public class Library
            {
                public const string EclaimDocuments = "Eclaim Documents";
            }
        }
        #endregion

        #region SharePoint Column
        public class SPColumn
        {
            public const string CreatedBy = "Author";
            public const string ModifiedBy = "Editor";
            public const string Created = "Created";
            public const string Modified = "Modified";
            public const string Status = "Status";
            public const string Title = "Title";
            public const string ID = "ID";

            public class CitiesList
            {
                public const string City = "City";
                public const string State = "State";
                public const string Status = "Status";
                public const string Sequence = "Sequence";
            }

            public class ClaimCategoryList
            {
                public const string Title = "Title";
                public const string Status = "Status";
            }

            public class ClaimEntitlementTypeList
            {
                public const string Title = "Title";
                public const string Status = "Status";
            }

            public class ClaimItemList
            {
                public const string Expense = "Expense";
                public const string ExpenseType = "ExpenseType";
                public const string FixedAmount = "FixedAmount";
                public const string FixedUnit = "FixedUnit";
                public const string ClaimEntitlementType = "ClaimEntitlementType";
                public const string ClaimCategory = "ClaimCategory";
                public const string EffectiveStartDate = "EffectiveStartDate";
                public const string EffectiveEndDate = "EffectiveEndDate";
                public const string Limit = "Limit";
                public const string Frequency = "Frequency";
            }

            public class ClaimMasterApproverList
            {
                public const string ClaimCategoryTitle = "ClaimCategoryTitle";
                public const string MasterApprover = "MasterApprover";
            }

            public class CompanyList
            {
                public const string Title = "Title";
                public const string CompanyCode = "CompanyCode";
                public const string Status = "Status";
            }

            public class CostCenterList
            {
                public const string Title = "Title";
                public const string CostCenter = "CostCenter";
                public const string Status = "Choice";
            }

            public class CountriesList
            {
                public const string Title = "Title";
                public const string CountryCode = "CountryCode";
                public const string Status = "Status";
                public const string Sequence = "Sequence";
            }

            public class CurrencyList
            {
                public const string Title = "Title";
                public const string Status = "Status";
                public const string ISOCode = "ISOCode";
                public const string Symbol = "Symbol";
                public const string Country = "Country";
                public const string CurrencyName = "CurrencyName";
            }

            public class DepartmentList
            {
                public const string ID = "ID";
                public const string Title = "Title";
                public const string Admin = "Admin";
                public const string Status = "Status";
                public const string DepartmentApprover = "DepartmentApprover";
            }

            public class EmailTemplatesList
            {
                public const string Title = "Title";
                public const string Subject = "Subject";
                public const string Body = "Body";
                public const string ProcessName = "ProcessName";
                public const string InternalStepName = "InternalStepName";
                //public const string ClientOrThirdParty = "ClientOrThirdParty";
                //public const string CorrespondenceType = "CorrespondenceType";
            }

            public class EmployeeList
            {
                public const string EmployeePerson = "Employee";
                public const string EmployeeID = "EmployeeID";
                public const string Country = "Country";
                public const string Employee = "Employee";
                public const string CostCenter = "CostCenter";
                public const string Department = "Department";
                public const string CompanyCode = "CompanyCode";
                public const string CompanyName = "Company_x0020_Code_x003a_Title";
                public const string Approver1 = "Approver1";
                public const string Approver2 = "Approver2";
                public const string Approver3 = "Approver3";
                public const string ClaimEntitlement1 = "ClaimEntitlement1";
                public const string ClaimEntitlement1StartDate = "ClaimEntitlement1StartDate";
                public const string ClaimEntitlement1EndDate = "ClaimEntitlement1EndDate";
                public const string ClaimEntitlement2 = "ClaimEntitlement2";
                public const string ClaimEntitlement2StartDate = "ClaimEntitlement2StartDate";
                public const string ClaimEntitlement2EndDate = "ClaimEntitlement2EndDate";
            }

            public class GLCodeList
            {
                public const string Title = "Title";
            }

            public class PositionList
            {
                public const string Title = "Title";
            }

            public class ReportURLList
            {
                public const string Title = "Title";
                public const string URL = "URL";
            }

            public class ExpensesList
            {
                public const string Title = "Title";
                public const string ExpensesSubType = "ExpensesSubType";
                public const string ClaimCategory = "ClaimCategory";
                public const string GLCode = "GLCode";
                public const string ClaimEntitlementType = "ClaimEntitlementType";
                public const string FixedQuantity = "FixedQuantity";
                public const string FixedAmount = "FixedAmount";
                public const string Frequency = "Frequency";
                public const string EffectiveStartDate = "EffectiveStartDate";
                public const string EffectiveEndDate = "EffectiveEndDate";
                public const string ClaimLimit = "ClaimLimit";
                public const string ClaimItemApprover = "ClaimItemApprover";
            }

            public class CurrencyExchangeRate
            {
                public const string Title = "Title";
                public const string Year = "Year";
                public const string Month = "Month";
                public const string BaseCurrency = "BaseCurrency";
                public const string TargetCurrency = "TargetCurrency";
                public const string Rate = "Rate";
            }

            #region Running Number Setting
            public class RunningNumberList
            {
                public const string ID = "ID";
                public const string Title = "Title";
                public const string Format = "Format";
                public const string Prefix = "Prefix";
                public const string Number = "Number";
            }

            public class RunningNumberFormatList
            {
                public const string ID = "ID";
                public const string Title = "Title";
                public const string Format = "Format";
                public const string Prefix = "Prefix";
                public const string Autonumber = "Autonumber";
            }

            public class StatesList
            {
                public const string Title = "Title";
                public const string Country = "Country";
                public const string Status = "Status";
                public const string Sequence = "Sequence";
            }

            public class TaxCodeList
            {
                public const string Title = "Title";
            }

            public class TravelAgencyList
            {
                public const string CompanyName = "CompanyName";
                public const string RegistrationNo = "RegistrationNo";
                public const string Website = "Website";
                public const string Address = "Address";
                public const string YearInBusiness = "YearInBusiness";
                public const string ContactPersonName = "ContactPersonName";
                public const string ContactPersonEmail = "ContactPersonEmail";
                public const string ContactPersonNo = "ContactPersonNo";
                public const string Status = "Status";
            }

            #endregion

            //Workflow Due Date
            public class DueDateForWorkflow
            {
                public const string Title = "Title";
                public const string DurationDays = "DurationDays";
            }
        }
        #endregion

        #region SharePoint Column Value
        public class SPColumnValue
        {

        }
        #endregion

        #region SharePoint Group & Permission
        public class SPSecurityGroup
        {
            public const string SDCClaimsAdmin = "SDC Claims Admin";
            public const string HeadOfDepartment = "Head of Department";
            public const string HRDepartment = "HR Department";
            public const string FinanceDepartment = "Finance Department";
            public const string DepartmentAdmin = "Department Admin";
            public const string Employee = "Employee";
            public const string RWCClaimsMembers = "RWC - Claims Dev Members";
        }
        #endregion

        #region Workflow
        public class WorkflowName
        {
            public const string EclaimWorkflow = "Eclaim Workflow";
        }

        public class WorkflowStatus
        {
            public const string PendingReportingManager1Approval = "Pending Reporting Manager 1 Approval";
            public const string PendingReportingManager2Approval = "Pending Reporting Manager 2 Approval";
            public const string PendingReportingManager3Approval = "Pending Reporting Manager 3 Approval";
            public const string PendingClaimItemApproval = "Pending Claim Item Approval";
            public const string PendingFinalApproval = "Pending Final Approval";
            public const string PendingOriginatorResubmission = "Pending Originator Resubmission";

            public const string COMPLETED = "Completed";
            public const string REJECTED = "Rejected";
            public const string TERMINATED = "Terminated";
            public const string GO_TO = "GoTo";
            public const string ERROR = "Error";
            public const string DRAFT = "Draft";

            public const string APPROVED = "Approved";
            public const string IN_PROGRESS = "In Progress";
        }

        public class TaskStatus
        {
            public const string Completed = "Completed";
            public const string IN_Progress = "In Progress";
            public const string Reassigned = "Reassigned";
            public const string Removed = "Removed";
        }

        public class WorkflowStepName
        {
            public class EClaimWorkflow
            {
                public const string PendingOriginatorResubmission = "PendingOriginatorResubmission";
                public const string PendingReportingManager1Approval = "PendingReportingManager1Approval";
                public const string PendingReportingManager2Approval = "PendingReportingManager2Approval";
                public const string PendingReportingManager3Approval = "PendingReportingManager3Approval";
                public const string PendingClaimItemApproval = "PendingClaimItemApproval";
                public const string PendingFinalApproval = "PendingFinalApproval";

                public const string Completed = "Completed";
                public const string Rejected = "Rejected";
                public const string Terminated = "Terminated";
                public const string Draft = "Draft";
            }
        }

        public class WorkflowTemplateId
        {
            public const int EClaimApprovalWorkflow = 1;
        }

        public class WorkflowActionName
        {
            public const string Complete = "Complete";
            public const string Approve = "Approve";
            public const string Reject = "Reject";
            public const string Default = "Default";
            public const string RequireAmendment = "Require Amendment";
            public const string Resubmit = "Resubmit";
            //public const string Requote = "Requote";

            //Approval Form Save Draft Action (Not a workflow action)
            public const string Save = "Save";
        }

        public class WorkflowManagementAction
        {
            public const string AddNew = "Add New";
            public const string Reassign = "Reassign";
            public const string Remove = "Remove";
            public const string RemoveAll = "Remove All";
        }

        public class WorkflowDTColumn
        {
            public class WorkflowHistory
            {
                public const string ProcessID = "ProcessID";
                public const string ProcessName = "ProcessName";
                public const string ReferenceKey = "ReferenceKey";
                public const string InternalStepName = "InternalStepName";
                public const string EncryptParam = "EncryptParam";
                public const string StepName = "StepName";
                public const string AssigneeLogin = "AssigneeLogin";
                public const string AssigneeName = "AssigneeName";
                public const string AssigneeEmail = "AssigneeEmail";
                public const string AssignedDate = "AssignedDate";
                public const string DueDate = "DueDate";
                public const string Status = "Status";
                public const string ActionedDate = "ActionedDate";
                public const string ActionedBy = "ActionedBy";
                public const string ActionedByName = "ActionedByName";
                public const string ActionName = "ActionName";
                public const string Comments = "Comments";
                public const string TaskURL = "TaskURL";
                public const string TaskID = "TaskID";
                public const string ExtendedDays = "ExtendedDays";
            }

            public class MyPendingTask
            {
                public const string ProcessID = "ProcessID";
                public const string ProcessName = "ProcessName";
                public const string OriginatorLogin = "OriginatorLogin";
                public const string ReferenceKey = "ReferenceKey";
                public const string OriginatorName = "OriginatorName";
                public const string ProcessStartDate = "ProcessStartDate";
                public const string KeywordsXML = "KeywordsXML";
                public const string EncryptParam = "EncryptParam";
                public const string ApplicationName = "ApplicationName";
                public const string StepName = "StepName";
                public const string TaskURL = "TaskURL";
                public const string TaskID = "TaskID";
                public const string AssigneeLogin = "AssigneeLogin";
                public const string AssigneeEmail = "AssigneeEmail";
                public const string AssigneeName = "AssigneeName";
                public const string AssignedDate = "AssignedDate";
                public const string DueDate = "DueDate";
                public const string ExtendedDays = "ExtendedDays";
                public const string IsDelegatedTask = "IsDelegatedTask";
                public const string ActionerLogin = "ActionerLogin";
                public const string ActionerName = "ActionerName";
                public const string TaskStatus = "Status";
            }

            public class MyActiveRequest
            {
                public const string RequestID = "RequestID";
                public const string ReferenceNo = "ReferenceNo";
                public const string EmployeeID = "EmployeeID";
                public const string EmployeeName = "EmployeeName";
                public const string RequestType = "ClaimCategory";
                public const string WorkflowStatus = "WorkflowStatus";
                public const string Submitted = "Submitted";
            }
        }

        public class WorkflowKeywords
        {
            public class Common
            {
                public const string SPHostURL = "{SPHostUrl}";
                public const string SPWebURL = "{SPWebURL}";
                public const string DocTitle = "{DocTitle}";
                public const string DocNo = "{DocNo}";
                public const string Rev = "{Rev}";
                public const string WorkflowName = "{WorkflowName}";
                public const string WorkflowDueDate = "{WorkflowDueDate}";

                public const string RequestID = "{RequestID}";
                public const string RequestRefNo = "{RequestRefNo}";
                public const string ClaimCategory = "{ClaimCategory}";
                public const string EmployeeId = "{EmployeeId}";
                public const string EmployeeName = "{EmployeeName}";
                public const string ViewRequestLink = "{ViewRequestLink}";
                public const string Remarks = "{Remarks}";

                public const string WorkflowCycleDueDays = "{WorkflowCycleDueDays}";
                public const string AG_TimeZoneName = "{AG_TimeZoneName}";
                public const string ViewFormLink = "{ViewFormLink}";
                public const string AppAccessToken = "{AppAccessToken}";

                public const string Originator = "{Originator}";
                public const string OriginatorSubmittedByName = "{OriginatorSubmittedByName}";
                public const string OriginatorSubmittedByLogin = "{OriginatorSubmittedByLogin}";
                public const string OriginatorSubmittedByEmail = "{OriginatorSubmittedByEmail}";
            }

            public class TaskURL
            {
                public const string ProcessID = "{ProcessID}";
                public const string TaskID = "{TaskID}";
            }
        }

        public class WorkflowMatrix
        {
            public const string ReportingManager1 = "Reporting Manager 1";
            public const string ReportingManager2 = "Reporting Manager 2";
            public const string ReportingManager3 = "Reporting Manager 3";
            public const string ClaimItemApprover = "Claim Item Approver";
            public const string HRDepartment = "HR Department";
            public const string FinanceDepartment = "Finance Department";
            public const string SDCClaimsAdmin = "SDC Claims Admin";
            public const string HeadOfDepartment = "Head of Department";
        }

        #endregion

        #region Excel
        public class Excel
        {
            public const string DisplayFields = "DisplayFields";
            public const string InternalFields = "InternalFields";
            public const string DateFields = "DateFields";
        }
        #endregion

        #region Workflow On Going Tasks
        public class WFOnGoingTasks
        {
            public const string TableName = "WFOngoingTasks";
            public class ColumnName
            {
                public const string ID = "ID";
                public const string TaskID = "TaskID";
                public const string IsComplete = "IsComplete";
            }
        }
        #endregion

        #region Generic WF SQL
        public class WFSQLTableFields
        {
            public class d_tblStep
            {
                public const string StepName = "StepName";
                public const string InternalStepName = "InternalStepName";
                public const string StepOrder = "StepOrder";
                public const string DueDateDay = "DueDateDay";
                public const string EmailNotification = "EmailNotification";
                public const string EmailToAssignee = "EmailToAssignee";
                public const string EmailToOriginator = "EmailToOriginator";
                public const string EmailCCOriginator = "EmailCCOriginator";
                public const string EmailNotificationSubject = "EmailNotificationSubject";
                public const string EmailNotificationBody = "EmailNotificationBody";

                public const string TaskURL = "TaskURL";
                public const string LastStep = "LastStep";
                public const string EmailOnlyStep = "EmailOnlyStep";
                public const string CodeOnlyStep = "CodeOnlyStep";
                public const string AssemblyName = "AssemblyName";
                public const string ClassName = "ClassName";
                public const string MethodName = "MethodName";
                public const string ProcessID = "ProcessID";
                public const string StepID = "StepID";
            }

            public class d_tblProcess
            {
                public const string ProcessID = "ProcessID";
                public const string ProcessName = "ProcessName";
                public const string EncryptParam = "EncryptParam";
            }

            public class i_tblTask
            {
                public const string ProcessID = "ProcessID";
                public const string TaskID = "TaskID";
                public const string ActionID = "ActionID";
                public const string Status = "Status";
                public const string Comments = "Comments";
                public const string ActionedDate = "ActionedDate";
                public const string AssignedDate = "AssignedDate";
                public const string DueDate = "DueDate";
                public const string AssigneeEmail = "AssigneeEmail";
                public const string AssigneeLogin = "AssigneeLogin";
                public const string ActionedBy = "ActionedBy";
                public const string StepTemplateID = "StepTemplateID";
            }
        }
        #endregion

        #region SQL DataTable
        public class SQLDataTable
        {

            public class Table
            {
                public const string d_tblStep = "d_tblStep";
                public const string MasterTrackingClaims = "MasterTrackingClaims";
                public const string EClaimDetails = "EClaimDetails";
                public class MasterTrackingClaimsColumns
                {
                    public const string ID = "ID";
                    public const string RequesterCompanyCode = "RequesterCompanyCode";
                    public const string RequesterCompanyName = "RequesterCompanyName";
                    public const string RequesterEmployeeID = "RequesterEmployeeID";
                    public const string RequesterDepartment = "RequesterDepartment";
                    public const string RequesterName = "RequesterName";
                    public const string RequesterCostCenter = "RequesterCostCenter";
                    public const string PayeeCompanyCode = "PayeeCompanyCode";
                    public const string PayeeCompanyName = "PayeeCompanyName";
                    public const string PayeeEmployeeID = "PayeeEmployeeID";
                    public const string PayeeDepartment = "PayeeDepartment";
                    public const string PayeeName = "PayeeName";
                    public const string PayeeCostCenter = "PayeeCostCenter";
                    public const string DateOfRequest = "DateOfRequest";
                    public const string ReferenceNo = "ReferenceNo";
                    public const string ClaimStatus = "ClaimStatus";
                    public const string PurposeOfClaims = "PurposeOfClaims";
                    public const string CurrencyOfTheClaim = "CurrencyOfTheClaim";
                    public const string TravelRequestFormReference = "TravelRequestFormReference";
                    public const string ClaimCategory = "ClaimCategory";
                    public const string CreatedBy = "CreatedBy";
                    public const string CreatedDate = "CreatedDate";
                    public const string CreatedByLogin = "CreatedByLogin";
                    public const string ModifiedBy = "ModifiedBy";
                    public const string ModifiedDate = "ModifiedDate";
                    public const string ModifiedByLogin = "ModifiedByLogin";

                }

                public class ClaimRequestColumns
                {
                    public const string ID = "ID";
                    public const string FinalEmployeeDetailsID = "FinalEmployeeDetailsID";
                    public const string ReferenceNo = "ReferenceNo";
                    public const string WorkflowStatus = "WorkflowStatus";
                    public const string ProcessID = "ProcessID";
                    public const string Changes = "Changes";
                    public const string Created = "Created";
                    public const string CreatedBy = "CreatedBy";
                    public const string CreatedByLogin = "CreatedByLogin";
                    public const string Modified = "Modified";
                    public const string ModifiedBy = "ModifiedBy";
                    public const string ModifiedByLogin = "ModifiedByLogin";
                    public const string Submitted = "Submitted";
                    public const string SubmittedBy = "SubmittedBy";
                    public const string SubmittedByLogin = "SubmittedByLogin";
                    public const string RequestType = "ClaimCategory";
                }
            }


        }
        #endregion

        #region Request Form
        public class RequestForm
        {
            public class WorkflowNewRequest
            {
                public const string ReferenceNoEmpty = "Not Available";
                public const string WorkflowStatusEmpty = "Not Available";
                public const string ReferenceNoDraft = "-";
                public const string WorkflowStatusDraft = "Draft";
            }

            public class ClaimCategory
            {
                public const string HR = "HR";
                public const string Finance = "Finance";
                public const string TravelHR = "Travel HR";
                public const string TravelFinance = "TravelFinanceHR";
            }
        }
        #endregion

        #region Approval Form
        public class ApprovalForm
        {
            public class ApprovalFormTitle
            {
                public const string ReportingManagerApproval = "Reporting Manager Approval";
                public const string ClaimItemApproval = "Claim Item Approval";
                public const string Resubmission = "Resubmission";
                public const string FinalApproval = "Final Approval";
            }
        }
        #endregion

        #region General
        public class Sorting
        {
            public const string GridAscending = "Ascending";
            public const string GridDescending = "Descending";

            public const string SQLAscending = "ASC";
            public const string SQLDescending = "DESC";
        }

        public class ParameterQuery
        {
            public const string SPHostUrl = "SPHostUrl";
            public const string RequestId = "RequestId";
            public const string ProcessId = "ProcessId";
            public const string TaskId = "TaskId";
        }

        public class PageHeaderFormType
        {
            public const string CreateRequestForm = "CreateRequestForm";
            public const string ViewRequestForm = "ViewRequestForm";
            public const string ApprovalForm = "ApprovalForm";
        }
        #endregion

        #region Permission Config Function
        public class PermissionConfigFunction
        {
            public class Request
            {
                public const string CreateNewRequest = "Create New Claim Request";
                public const string DisplayForm = "Display Form";
            }
        }
        #endregion
    }
}
