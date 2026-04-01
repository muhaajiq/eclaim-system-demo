using MHA.Framework.Core.SP;
using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Entities.ViewModel.RunningNumber;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Entities.ViewModel.Workflow;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.SharePoint.Client;
using System.Data;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class ConvertEntitiesHelper
    {
        private static readonly JSONAppSettings appSettings;

        static ConvertEntitiesHelper()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        #region Running Number
        public static RunningNumber ConvertRunningNumberObject(ListItem li)
        {
            RunningNumber rnObj = new RunningNumber();

            rnObj.Format = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.RunningNumberList.Format);
            rnObj.ID = FieldHelper.GetFieldValueAsNumber(li, ConstantHelper.SPColumn.RunningNumberList.ID);
            rnObj.Number = FieldHelper.GetFieldValueAsNumber(li, ConstantHelper.SPColumn.RunningNumberList.Number);
            rnObj.Prefix = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.RunningNumberList.Prefix);
            rnObj.Title = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.RunningNumberList.Title);

            return rnObj;
        }

        public static RunningNumberFormat ConvertRunningNumberFormatObject(ListItem li)
        {
            RunningNumberFormat rnfObj = new RunningNumberFormat();

            rnfObj.Autonumber = FieldHelper.GetFieldValueAsBoolean(li, ConstantHelper.SPColumn.RunningNumberFormatList.Autonumber);
            rnfObj.Format = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.RunningNumberFormatList.Format);
            rnfObj.ID = FieldHelper.GetFieldValueAsNumber(li, ConstantHelper.SPColumn.RunningNumberFormatList.ID);
            rnfObj.Prefix = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.RunningNumberFormatList.Prefix);
            rnfObj.Title = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.RunningNumberFormatList.Title);

            return rnfObj;
        }
        #endregion

        #region Pending Task
        public static MyPendingTask ConvertMyPendingTaskObj(DataRow dr, ClientContext context)
        {
            string keywordsXML = dr[ConstantHelper.WorkflowDTColumn.MyPendingTask.KeywordsXML] + "";
            ProcessKeywords keywords = new ProcessKeywords(keywordsXML);
            MyPendingTask myTaskObj = new MyPendingTask();

            DateTime? dueDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.DueDate);
            if (dueDate != DateTime.MinValue)
            {
                myTaskObj.DueDate = dueDate;
                bool useSPTimeZone = false;
                bool.TryParse(appSettings.AG_UseSPTimeZoneforDBDateTimeColumn, out useSPTimeZone);
                myTaskObj.DueDateDisplay = FieldHelper.GetFieldValueAsDateTimeStringWithSPTimeZone(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.DueDate, useSPTimeZone, context, appSettings.DefaultDateFormat);
            }

            myTaskObj.StepName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.StepName);
            myTaskObj.TaskID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.TaskID);
            myTaskObj.ProcessID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WFSQLTableFields.i_tblTask.ProcessID);

            // Additional fields extracted from keywords XML
            myTaskObj.WorkflowName = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.WorkflowName);
            myTaskObj.RequestID = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.RequestID);
            myTaskObj.RequestReferenceNo = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.RequestRefNo);
            myTaskObj.ClaimCategory = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.ClaimCategory);
            myTaskObj.EmployeeId = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.EmployeeId);
            myTaskObj.EmployeeName = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.EmployeeName);

            // If is incoming task, URL set to view request form
            string taskUrl = dr[ConstantHelper.WFSQLTableFields.d_tblStep.TaskURL].ToString();
            taskUrl = taskUrl
                .Replace(ConstantHelper.WorkflowKeywords.TaskURL.ProcessID, dr[ConstantHelper.WFSQLTableFields.i_tblTask.ProcessID].ToString())
                .Replace(ConstantHelper.WorkflowKeywords.TaskURL.TaskID, dr[ConstantHelper.WFSQLTableFields.i_tblTask.TaskID].ToString());
            dr[ConstantHelper.WFSQLTableFields.d_tblStep.TaskURL] = taskUrl;

            //TODO
            if (myTaskObj.TaskID <= 0)
            {
                // Set Task URL
                myTaskObj.TaskURL = string.Format(
                        ConstantHelper.URLTemplate.ClaimRequestDisplayFormUrlTemplate,
                        appSettings.RemoteAppURL.TrimEnd('/'),
                        myTaskObj.RequestID,
                        context.Web.Url);

                myTaskObj.IsUpcomingTask = true;
            }
            else
            {
                myTaskObj.TaskURL = appSettings.RemoteAppURL.TrimEnd('/') + MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.TaskURL);
            }

            //string actionerName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.AssigneeName);
            //if (string.IsNullOrEmpty(actionerName))
            //{
            //    actionerName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.MyPendingTask.ActionerName);
            //}
            //myTaskObj.ActionerName = actionerName;
            //myTaskObj.ModifiedDate = DateTimeHelper.GetCurrentDateTime();

            myTaskObj.WorkflowDueDate = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.WorkflowDueDate);

            return myTaskObj;
        }
        #endregion

        #region SharePoint Maintenance List
        #endregion

        public static List<WorkflowHistory> ConvertWorkflowHistoryObj(DataTable dt)
        {
            List<WorkflowHistory> list = new List<WorkflowHistory>();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    WorkflowHistory obj = new WorkflowHistory();

                    obj.Action = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.ActionName);
                    obj.ActionedBy = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.ActionedByName);
                    obj.AssigneeLogin = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.AssigneeLogin);
                    obj.AssigneeName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.AssigneeName);
                    obj.AssigneeEmail = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.AssigneeEmail);
                    obj.Comments = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.Comments);
                    obj.Status = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.Status);
                    obj.StepName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.StepName);
                    obj.TaskID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.TaskID);
                    obj.TaskURL = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.TaskURL);
                    obj.ProcessName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.ProcessName);

                    if (MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.ActionedDate) != DateTime.MinValue)
                    {
                        DateTime actionDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.ActionedDate);
                        actionDate = DateTimeHelper.ConvertToLocalDateTime(actionDate);
                        obj.ActionedDate = actionDate.ToString(appSettings.DefaultDateTimeFormat);
                    }

                    if (MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.AssignedDate) != DateTime.MinValue)
                    {
                        DateTime assignedDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.AssignedDate);
                        assignedDate = DateTimeHelper.ConvertToLocalDateTime(assignedDate);
                        obj.AssignedDate = assignedDate.ToString(appSettings.DefaultDateTimeFormat);
                    }

                    if (MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.DueDate) != DateTime.MinValue)
                    {
                        DateTime dueDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.WorkflowDTColumn.WorkflowHistory.DueDate);
                        dueDate = DateTimeHelper.ConvertToLocalDateTime(dueDate);
                        obj.DueDate = dueDate.ToString(appSettings.DefaultDateTimeFormat);
                    }

                    list.Add(obj);
                }
            }

            return list;
        }

        #region Workflow Status Report
        public static WorkflowTaskReportData ConvertWorkflowTaskReportObj(DataRow dr)
        {
            WorkflowTaskReportData obj = new();
            obj.ReferenceNo = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.ReferenceNo);
            obj.EmployeeID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.EmployeeID);
            obj.CompanyName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.CompanyName);
            obj.ProcessID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.ProcessID);
            obj.ProcessName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.ProcessName);
            obj.WorkflowStage = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.WorkflowStage);
            obj.StartDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.StartDate);
            obj.CompletionDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.CompletionDate);
            obj.AssigneeLogin = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.AssigneeLogin);
            obj.AssigneeName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.AssigneeName);
            obj.AssigneeEmail = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.AssigneeEmail);
            obj.AssignedDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.AssignedDate);
            obj.ActionedDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.ActionedDate);
            obj.DueDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.DueDate);
            obj.DaysTaken = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.DaysTaken);
            obj.TaskOverdueDays = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.TaskOverdueDays);
            obj.TaskID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.TaskID);
            obj.Status = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.Status);
            obj.StepTemplateID = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsNumber(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.StepTemplateID);
            obj.TaskAction = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.TaskAction);
            obj.StepName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.StepName);
            obj.InternalStepName = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.WorkflowTaskReport.InternalStepName);

            //New Stuff
            //obj.PurposeOfRequest = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsString(dr, ConstantHelper.StoreProcedureColumn.TravelListing.PurposeOfRequest);
            //obj.CreatedByDate = MHA.Framework.Core.General.FieldHelper.GetFieldValueAsDateTime(dr, ConstantHelper.StoreProcedureColumn.TravelListing.CreatedByDate);


            return obj;
        }
        #endregion

        #region PeoplePicker
        public static PeoplePickerUser ConvertPeoplePickerUser(User user)
        {
            if (user != null)
            {
                PeoplePickerUser ppUser = new PeoplePickerUser();
                ppUser.Name = user.Title;
                ppUser.Login = user.LoginName;
                ppUser.Email = user.Email;
                ppUser.LookupId = user.Id;

                return ppUser;
            }
            else
            {
                return null;
            }
        }

        public static PeoplePickerUser ConvertPeoplePickerUser(User user, ClientContext clientContext)
        {
            if (user != null)
            {
                clientContext.Load(user);
                clientContext.ExecuteQueryWithIncrementalRetry();

                PeoplePickerUser ppUser = new PeoplePickerUser();
                ppUser.Name = user.Title;
                ppUser.Login = user.LoginName;
                ppUser.Email = user.Email;
                ppUser.LookupId = user.Id;

                return ppUser;
            }
            else
            {
                return null;
            }
        }

        public static List<PeoplePickerUser> ConvertFieldUserValueToPeoplePicker(FieldUserValue fieldUserValue, ClientContext clientContext)
        {
            var result = new List<PeoplePickerUser>();

            if (fieldUserValue != null)
            {
                User spUser = clientContext.Web.GetUserById(fieldUserValue.LookupId);
                var ppUser = ConvertPeoplePickerUser(spUser, clientContext);
                if (ppUser != null)
                {
                    result.Add(ppUser);
                }
            }

            return result;
        }
        #endregion
    }
}
