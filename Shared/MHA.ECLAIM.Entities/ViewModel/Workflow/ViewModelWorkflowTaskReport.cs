using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.Workflow
{
    public class ViewModelWorkflowReport
    {
        public bool IsAdmin { get; set; }
        public bool ShowInactiveItems { get; set; }
        public string CurrentUser { get; set; } = string.Empty;
        public string CurrentUserLogin { get; set; } = string.Empty;
        public List<string> AccessGroups { get; set; } = new();
        public List<int> AccessDepartments { get; set; } = new();
        public string MemberLogin { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int Count { get; set; }
        public int RowsPerPage { get; set; }
        //public string SortField { get; set; } = string.Empty;
        //public string SortFieldTable { get; set; } = string.Empty;
        //public string SortDirection { get; set; } = string.Empty;
        public WorkflowTaskSearchModel taskSearchModel { get; set; } = new();
        public List<WorkflowTaskReportData> WorkflowReport { get; set; } = new();
        public int Skip { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public Byte[]? ExcelFileBytes { get; set; }
        public bool IsExportToExcel { get; set; }
    }

    //Display Data in datagrid later
    public class WorkflowTaskReportData
    {
        public string ReferenceNo { get; set; } = string.Empty;
        public string EmployeeID { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public int ProcessID { get; set; }
        public string ProcessName { get; set; } = string.Empty; //i think is not needed
        public string WorkflowStage { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string AssigneeLogin { get; set; } = string.Empty;
        public string AssigneeName { get; set; } = string.Empty;
        public string AssigneeEmail { get; set; } = string.Empty;
        public DateTime? AssignedDate { get; set; }
        public DateTime? ActionedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? DaysTaken { get; set; }
        public int? TaskOverdueDays { get; set; }
        public int TaskID { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StepTemplateID { get; set; }
        public string TaskAction { get; set; } = string.Empty;
        public string StepName { get; set; } = string.Empty;
        public string InternalStepName { get; set; } = string.Empty;
    }


    public class WorkflowTaskSearchModel
    {
        //Workflow Started Date
        public DateTime? WorkflowStartedFrom { get; set; }
        public DateTime? WorkflowStartedTo { get; set; }

        public bool WorkflowCompletedEmpty { get; set; } //stupid checkbox
        public bool ActionedDateEmpty { get; set; } //stupid checkbox

        //Workflow Completed is the Person name
        public DateTime? WorkflowCompletedFrom { get; set; }
        public DateTime? WorkflowCompletedTo { get; set; }


        public DateTime? AssignedDateFrom { get; set; }
        public DateTime? AssignedDateTo { get; set; }


        public DateTime? ActionedDateFrom { get; set; }
        public DateTime? ActionedDateTo { get; set; }

        public string ReferenceNo { get; set; } = string.Empty;
        public DateTime? CompletionDate { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ActionedDate { get; set; }

        public string WorkflowType { get; set; } = string.Empty;  //should not be list
        public List<string> WorkflowStatus { get; set; } = new();

        //AsigneeEmail for taskactioner
        public string TaskActioner { get; set; } = string.Empty;
        public List<string> TaskStage { get; set; } = new();
        public List<string> TaskStatus { get; set; } = new();
        public string ProcessName { get; set; } = string.Empty;
    }
}
