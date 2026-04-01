using MHA.Framework.Core.General;
using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.Constants;
using System.Data;
using MHA.ECLAIM.Entities.ViewModel.Home;

namespace MHA.ECLAIM.Data
{
    public class HomeDA 
    {
        private readonly SQLHelper sqlHelper;

        public HomeDA()
        {
            sqlHelper = new SQLHelper(ConnectionStringHelper.GetGenericWFConnString());
        }

        public DataTable GetMyPendingTaskData(ViewModelMyPendingTask vmMyTask, Actioner currentUser, string applicationName)
        {
            int processTemplateId = ConstantHelper.WorkflowTemplateId.EClaimApprovalWorkflow;

            List<string> pendingTaskParamNames = new string[]
            {
                        ConstantHelper.StoreProcedureParameter.ActionerLogin,
                        ConstantHelper.StoreProcedureParameter.ProcessName,
                        ConstantHelper.StoreProcedureParameter.ApplicationName,
                        ConstantHelper.StoreProcedureParameter.PageNumber,
                        ConstantHelper.StoreProcedureParameter.RowsPerPage,
                        ConstantHelper.StoreProcedureParameter.ProcessTemplateID
            }.ToList();
            List<object> pendingTaskParamValues = new object[]
            {
                        currentUser.LoginName,
                        string.Empty,
                        applicationName,
                        vmMyTask.CurrentPage,
                        vmMyTask.RowsPerPage,
                        processTemplateId
            }.ToList();

            return sqlHelper.SQLExecuteAsDataSet(false, ConstantHelper.StoreProcedureName.GetMyPendingAndIncomingTasksByBatchByProcessNameApplicationName, pendingTaskParamNames, pendingTaskParamValues);
        }

        public int GetMyPendingTaskDataCount(Actioner currentUser, string applicationName)
        {
            int processTemplateId = ConstantHelper.WorkflowTemplateId.EClaimApprovalWorkflow;

            List<string> pendingTaskCountParamNames = new string[]
                    {
                        ConstantHelper.StoreProcedureParameter.ActionerLogin,
                        ConstantHelper.StoreProcedureParameter.ProcessName,
                        ConstantHelper.StoreProcedureParameter.ApplicationName,
                        ConstantHelper.StoreProcedureParameter.ProcessTemplateID
                    }.ToList();
            List<object> pendingTaskCountParamValues = new object[]
            {
                        currentUser.LoginName,
                        string.Empty,
                        applicationName,
                        processTemplateId
            }.ToList();

            return sqlHelper.SQLExecuteScalar(false, ConstantHelper.StoreProcedureName.GetMyPendingAndIncomingTasksCountByBatchByProcessNameApplicationName, pendingTaskCountParamNames, pendingTaskCountParamValues);
        }

    }
}
