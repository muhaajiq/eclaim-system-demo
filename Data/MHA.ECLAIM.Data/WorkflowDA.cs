using MHA.Framework.Core.General;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using System.Data;
using MHA.ECLAIM.Entities.ViewModel.Workflow;
namespace MHA.ECLAIM.Data
{
    public class WorkflowDA
    {
        private readonly SQLHelper sqlHelper;

        public WorkflowDA()
        {
            sqlHelper = new SQLHelper(ConnectionStringHelper.GetGenericWFConnString());
        }

        public int GetWFStepDueDateDaysFromDb(string internalStepName, string wfConnString)
        {
            string query = $@"
                SELECT {ConstantHelper.WFSQLTableFields.d_tblStep.DueDateDay}
                FROM {ConstantHelper.SQLDataTable.Table.d_tblStep} step
                WHERE step.{ConstantHelper.WFSQLTableFields.d_tblStep.InternalStepName} = @InternalStepName";

            List<string> paramNames = new List<string> { "@InternalStepName" };
            List<object> paramValues = new List<object> { internalStepName };

            int result = sqlHelper.SQLExecuteScalar(true, query, paramNames, paramValues);

            return result;
        }

        public DataTable GetWorkflowHistory(int processID)
        {
            #region Get Process ID

            //int processID = 0;

            //string query = $"SELECT * FROM {ConstantHelper.SQLDataTable.Table.Request} " +
            //                      $"WHERE {ConstantHelper.SQLDataTable.Table.RequestColumns.ID} = '{requestID}'";

            //DataTable processIdDT = sqlHelper.SQLExecuteAsDataSet(true, query, new List<string>(), new List<object>());

            //if (processIdDT != null && processIdDT.Rows.Count > 0)
            //{
            //    DataRow dr = processIdDT.Rows[0];

            //    processID = MHA.Framework.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.SQLDataTable.Table.RequestColumns.ProcessID);
            //}

            //if (processID == 0) return null;
            #endregion

            List<string> paramNames = new List<string>() { "@ProcessID" };
            List<object> paramValues = new List<object>() { processID };

            DataTable dt = sqlHelper.SQLExecuteAsDataSet(false, ConstantHelper.StoreProcedureName.GetWorkflowTaskHistoryByProcessID, paramNames, paramValues);

            return dt;
        }

        public bool IsTaskOngoing(int taskID)
        {
            string query = string.Format("SELECT COUNT(ID) AS TotalCount FROM {0} WHERE {1} = @TaskID AND {2} = @IsComplete", ConstantHelper.WFOnGoingTasks.TableName, ConstantHelper.WFOnGoingTasks.ColumnName.TaskID, ConstantHelper.WFOnGoingTasks.ColumnName.IsComplete);
            List<string> parameterNames = new List<string> { "TaskID", "IsComplete" };
            List<object> parameterValues = new List<object> { taskID, false };
            DataTable result = sqlHelper.SQLExecuteAsDataSet(true, query, parameterNames, parameterValues);

            if (result != null && result.Rows.Count > 0)
                return (FieldHelper.GetFieldValueAsNumber(result.Rows[0], "TotalCount") > 0);
            else
                return false;
        }

        public void InsertOnGoingTasks(int taskID)
        {
            List<string> columnNames = new List<string>() { ConstantHelper.WFOnGoingTasks.ColumnName.TaskID };
            List<object> columnValues = new List<object>() { taskID };
            sqlHelper.Create(ConstantHelper.WFOnGoingTasks.TableName, columnNames, columnValues);
        }

        public void UpdateOnGoingTasks(int taskID)
        {
            string query = string.Format("UPDATE {0} SET {1} = @IsComplete WHERE {2} = @TaskID AND {1} = 0 ", ConstantHelper.WFOnGoingTasks.TableName, ConstantHelper.WFOnGoingTasks.ColumnName.IsComplete, ConstantHelper.WFOnGoingTasks.ColumnName.TaskID);
            List<string> parameterNames = new List<string> { "IsComplete", "TaskID" };
            List<object> parameterValues = new List<object> { true, taskID };
            sqlHelper.SQLExecuteAsDataSet(true, query, parameterNames, parameterValues);
        }

        // Refer to Ricoh to extend this code to Eclaim business logic
        //public DataTable GetManagerTask(DateTime fromDate)
        //{
        //    List<string> paramNames = new List<string>() { "@InternalStepName", "@FromDate", "@ProcessName" };
        //    List<object> paramValues = new List<object>() { ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManagerApproval, fromDate, ConstantHelper.WorkflowName.EclaimWorkflow };

        //    DataTable dt = sqlHelper.SQLExecuteAsDataSet(false, ConstantHelper.StoreProcedureName.GetPendingReportingManagerApprovalTasksByProcessName, paramNames, paramValues);

        //    return dt;
        //}

        #region WorkflowTaskReport


        public ViewModelWorkflowReport GetWorkflowTaskReport(ViewModelWorkflowReport vm)
        {
            try
            {
                List<WorkflowTaskReportData> taskList = new();

                // Convert list filters to comma-separated strings for STRING_SPLIT in SQL
                string workflowStatuses = string.Join(",", vm.taskSearchModel.WorkflowStatus);
                string taskStatuses = string.Join(",", vm.taskSearchModel.TaskStatus);
                string taskStages = string.Join(",", vm.taskSearchModel.TaskStage);


                // Stored procedure parameter names
                List<string> paramNames = new List<string>
                {
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowStartedFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowStartedTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowCompletedEmpty,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDateEmpty,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowCompletedFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowCompletedTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.AssignedDateFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.AssignedDateTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDateFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDateTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ReferenceNo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.CompletionDate,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.AssignedDate,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDate,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowType,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowStatus,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.TaskActioner,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.TaskStage,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.TaskStatus,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ProcessName,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.PageNumber,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.RowsPerPage
                };

                // Stored procedure parameter values
                List<object> paramValues = new List<object>
                {
                    vm.taskSearchModel.WorkflowStartedFrom,
                    vm.taskSearchModel.WorkflowStartedTo,
                    vm.taskSearchModel.WorkflowCompletedEmpty,
                    vm.taskSearchModel.ActionedDateEmpty,
                    vm.taskSearchModel.WorkflowCompletedFrom,
                    vm.taskSearchModel.WorkflowCompletedTo,
                    vm.taskSearchModel.AssignedDateFrom,
                    vm.taskSearchModel.AssignedDateTo,
                    vm.taskSearchModel.ActionedDateFrom,
                    vm.taskSearchModel.ActionedDateTo,
                    string.IsNullOrWhiteSpace(vm.taskSearchModel.ReferenceNo) ? null : vm.taskSearchModel.ReferenceNo,
                    vm.taskSearchModel.CompletionDate,
                    vm.taskSearchModel.AssignedDate,
                    vm.taskSearchModel.ActionedDate,
                    string.IsNullOrWhiteSpace(vm.taskSearchModel.WorkflowType) ? null : vm.taskSearchModel.WorkflowType,
                    string.IsNullOrWhiteSpace(workflowStatuses) ? null : workflowStatuses,
                    string.IsNullOrWhiteSpace(vm.taskSearchModel.TaskActioner) ? null : vm.taskSearchModel.TaskActioner,
                    string.IsNullOrWhiteSpace(taskStages) ? null : taskStages,
                    string.IsNullOrWhiteSpace(taskStatuses) ? null : taskStatuses,
                    string.IsNullOrWhiteSpace(vm.taskSearchModel.ProcessName) ? null : vm.taskSearchModel.ProcessName,
                    vm.PageNumber,
                    vm.RowsPerPage
                };

                // Execute SP
                DataTable dt = sqlHelper.SQLExecuteAsDataSet(false, ConstantHelper.StoreProcedureName.WorkflowTaskReport, paramNames, paramValues);

                foreach (DataRow row in dt.Rows)
                {
                    WorkflowTaskReportData obj = new();
                    obj = ConvertEntitiesHelper.ConvertWorkflowTaskReportObj(row);

                    taskList.Add(obj);
                }

                vm.WorkflowReport = taskList;
                // Parameter names for count SP
                List<string> countParamNames = new List<string>
                {
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowStartedFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowStartedTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowCompletedEmpty,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDateEmpty,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowCompletedFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowCompletedTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.AssignedDateFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.AssignedDateTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDateFrom,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDateTo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ReferenceNo,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.CompletionDate,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.AssignedDate,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ActionedDate,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowType,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.WorkflowStatus,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.TaskActioner,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.TaskStage,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.TaskStatus,
                    ConstantHelper.StoreProcedureParameter.WorkflowTaskReport.ProcessName
                };
             
                List<object> countParamValues = paramValues.Take(countParamNames.Count).ToList();

                // Execute the count SP
                int totalRows = sqlHelper.SQLExecuteScalar(
                    false,
                    ConstantHelper.StoreProcedureName.WorkflowTaskReportCount,
                    countParamNames,
                    countParamValues
                );
                vm.Count = totalRows; 
            }
            catch (Exception)
            {
                throw; 
            }

            return vm;
        }

        #endregion
    
}
}
