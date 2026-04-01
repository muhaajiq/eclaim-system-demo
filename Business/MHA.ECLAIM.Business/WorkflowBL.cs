using MHA.Framework.Core.SP;
using MHA.Framework.Core.Workflow.BL;
using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Data;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Entities.ViewModel.Workflow;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.TRAVELREQUEST.Framework.Helpers;
using Microsoft.SharePoint.Client;
using System.Data;
using System.Net.Mail;
using LogHelper = MHA.ECLAIM.Framework.Helpers.LogHelper;

namespace MHA.ECLAIM.Business
{
    public class WorkflowBL
    {
        private static readonly JSONAppSettings appSettings;

        static WorkflowBL()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        #region Workflow History
        public async Task<PartialModelWorkflowHistory> InitWorkflowHistory(int processID, string spHostUrl, string accessToken)
        {
            PartialModelWorkflowHistory modelWFHist = new PartialModelWorkflowHistory();
            try
            {
                if (processID != -1)
                {
                    modelWFHist.WorkflowHistoryList = GetWorkflowHistory(processID);
                    modelWFHist.ProcessID = processID + "";

                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    ProcessInstance processInstance = wfBL.GetProcessInstance(processID);

                    if (processInstance.CompletionDate == DateTime.MinValue)
                    {
                        string attributeName = string.Format("<Key>{0}</Key><Value>", ConstantHelper.WorkflowKeywords.Common.WorkflowDueDate);
                        int startIndex = processInstance.KeywordsXML.IndexOf(attributeName);

                        if (startIndex > -1)
                        {
                            string workflowDueDate = processInstance.KeywordsXML.Substring(startIndex + attributeName.Length);
                            modelWFHist.WorkflowDueDate = workflowDueDate.Substring(0, workflowDueDate.IndexOf("</Value>"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - InitWorkflowHistory Error: " + ex.ToString());
            }
            return modelWFHist;
        }

        public async Task<PartialModelAdminWorkflowHistory> InitAdminWFHistory(int processID, string spHostUrl, string accessToken)
        {
            PartialModelAdminWorkflowHistory obj = new PartialModelAdminWorkflowHistory();
            try
            {
                if (processID > 0)
                {
                    obj.WorkflowHistoryList = GetWorkflowHistory(processID);
                    obj.ProcessID = processID + "";
                    obj.isWFRunnning = WorkflowHelper.IsWorkflowRunning(processID);
                    //InstanceDL
                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    ProcessInstance processInstance = wfBL.GetProcessInstance(processID);

                    if (processInstance.CompletionDate == DateTime.MinValue)
                    {
                        string attributeName = string.Format("<Key>{0}</Key><Value>", ConstantHelper.WorkflowKeywords.Common.WorkflowDueDate);
                        int startIndex = processInstance.KeywordsXML.IndexOf(attributeName);

                        if (startIndex > -1)
                        {
                            string workflowDueDate = processInstance.KeywordsXML.Substring(startIndex + attributeName.Length);
                            obj.WorkflowDueDate = workflowDueDate.Substring(0, workflowDueDate.IndexOf("</Value>"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - InitAdminWFHistory Error: " + ex.ToString());
            }
            return obj;
        }

        public List<WorkflowHistory> GetWorkflowHistory(int processID)
        {
            LogHelper logHelper = new LogHelper();
            List<WorkflowHistory> wfHistoryList = new List<WorkflowHistory>();

            try
            {
                WorkflowDA workflowDA = new WorkflowDA();
                DataTable wfDT = workflowDA.GetWorkflowHistory(processID);
                List<WorkflowHistory> workflowList = ConvertEntitiesHelper.ConvertWorkflowHistoryObj(wfDT); ;

                foreach (WorkflowHistory item in workflowList)
                {
                    item.ShowRemoveLink = ShowRemoveAction(item.Status);
                    item.ShowRemoveCheckbox = ShowRemoveAction(item.Status);

                    wfHistoryList.Add(item);
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage("WorkflowBL - GetWorkflowHistory Error: " + ex.ToString());
            }
            return wfHistoryList;
        }
        #endregion

        #region Submit - Launch Workflow
        public static string StartWorkflow(StartWorkflowObject startWFObject, MainClaimHeaderVM vm, string spHostUrl, ClientContext clientContext, string accessToken, ref int processId)
        {
            string errorMessage = string.Empty;
            string rerAccountEmail = appSettings.rerAccountEmail;
            string rerAccountPassword = appSettings.rerAccountPassword;

            string WFConnString = ConnectionStringHelper.GetGenericWFConnString();

            try
            {
                #region Workflow Matrix

                #region Reporting Manager Approvers
                ListItem? employeeListItem = GetCurrentEmployeeItemWithApprovers(clientContext);

                if (employeeListItem != null)
                {
                    var fiedlUserReportingManager1 = FieldHelper.GetFieldUserValue(employeeListItem, ConstantHelper.SPColumn.EmployeeList.Approver1);
                    var fiedlUserReportingManager2 = FieldHelper.GetFieldUserValue(employeeListItem, ConstantHelper.SPColumn.EmployeeList.Approver2);
                    var fiedlUserReportingManager3 = FieldHelper.GetFieldUserValue(employeeListItem, ConstantHelper.SPColumn.EmployeeList.Approver3);

                    startWFObject.ReportingManager1 = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(fiedlUserReportingManager1, clientContext);
                    startWFObject.ReportingManager2 = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(fiedlUserReportingManager2, clientContext);
                    startWFObject.ReportingManager3 = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(fiedlUserReportingManager3, clientContext);
                }
                #endregion

                #region Claim Item Approvers
                foreach (var detail in vm.SubClaimDetails)
                {
                    var approverItems = GetClaimItemApprovers(clientContext, detail.ExpensesSubtype);

                    foreach (var item in approverItems)
                    {
                        var claimItemApprover = FieldHelper.GetFieldUserValue(item, ConstantHelper.SPColumn.ExpensesList.ClaimItemApprover);
                        var peoplePickers = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(claimItemApprover, clientContext);

                        if (peoplePickers != null && peoplePickers.Any())
                        {
                            startWFObject.ClaimItemApprover.AddRange(peoplePickers);
                        }
                    }
                }

                // Remove duplicates by login name
                startWFObject.ClaimItemApprover = startWFObject.ClaimItemApprover
                    .GroupBy(u => u.Login)
                    .Select(g => g.First())
                    .ToList();
                #endregion

                #region Final Approver - HR or Finance

                switch (vm.ClaimCategory)
                {
                    case ConstantHelper.RequestForm.ClaimCategory.HR:
                        startWFObject.FinalApprover = SharePointHelper.GetUsersFromSharePointGroup(clientContext, ConstantHelper.WorkflowMatrix.HRDepartment);
                        Require(ConstantHelper.WorkflowMatrix.HRDepartment, startWFObject.FinalApprover);
                        break;

                    case ConstantHelper.RequestForm.ClaimCategory.Finance:
                        startWFObject.FinalApprover = SharePointHelper.GetUsersFromSharePointGroup(clientContext, ConstantHelper.WorkflowMatrix.FinanceDepartment);
                        Require(ConstantHelper.WorkflowMatrix.FinanceDepartment, startWFObject.FinalApprover);
                        break;

                    default:
                        throw new Exception("Invalid claim category specified.");
                }
                #endregion

                Require(ConstantHelper.WorkflowMatrix.ReportingManager1, startWFObject.ReportingManager1);
                Require(ConstantHelper.WorkflowMatrix.ClaimItemApprover, startWFObject.ClaimItemApprover);
                #endregion

                #region Workflow Due Days
                WorkflowDA wfDA = new WorkflowDA();
                startWFObject.PendingOriginatorResubmissionDueDays = wfDA.GetWFStepDueDateDaysFromDb(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingOriginatorResubmission, WFConnString);
                startWFObject.PendingReportingManager1DueDays = wfDA.GetWFStepDueDateDaysFromDb(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager1Approval, WFConnString);
                startWFObject.PendingReportingManager2DueDays = wfDA.GetWFStepDueDateDaysFromDb(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager2Approval, WFConnString);
                startWFObject.PendingReportingManager3DueDays = wfDA.GetWFStepDueDateDaysFromDb(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager3Approval, WFConnString);
                startWFObject.PendingClaimItemApproverDueDays = wfDA.GetWFStepDueDateDaysFromDb(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingClaimItemApproval, WFConnString);
                startWFObject.PendingFinalApproverDueDays = wfDA.GetWFStepDueDateDaysFromDb(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingFinalApproval, WFConnString);
                #endregion

                //Working Days
                List<DayOfWeek> daysOfWeek = new List<DayOfWeek>
                {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                };

                //Calculate Stage Due Date
                DateTime workflowDueDate = DateTimeHelper.GetCurrentDateTime();
                startWFObject = ModifyEClaimTaskDueDate(startWFObject, ref workflowDueDate, daysOfWeek);

                List<int> reminderDays = new List<int>()
                {
                    startWFObject.PendingReportingManager1DueDays,
                    startWFObject.PendingReportingManager2DueDays,
                    startWFObject.PendingReportingManager3DueDays,
                    startWFObject.PendingClaimItemApproverDueDays,
                    startWFObject.PendingFinalApproverDueDays,
                    startWFObject.PendingOriginatorResubmissionDueDays
                };

                startWFObject.UseDefaultDueDays = true;

                #region Contruct Email
                MailAddressCollection notificationToMails = new MailAddressCollection();
                EmailHelper.AddEmail(startWFObject.Originator.Email, ref notificationToMails);
                #endregion

                //Check if there is workflow running
                bool isWorkflowRunning = WorkflowHelper.IsWorkflowRunning(vm.ReferenceNo);
                if (isWorkflowRunning)
                {
                    throw new Exception("This EClaim request have active workflow running and cannot be launched again. Please try again later.");
                }

                //Step 1 - Set stage actioners
                StageActioners stageActioners = SetWFActioners(startWFObject, vm);

                // Step 1.1 - Final Stage
                stageActioners.NextStage(ConstantHelper.WorkflowStepName.EClaimWorkflow.Completed, new MailAddressCollection(), notificationToMails);
                stageActioners.NextStage(ConstantHelper.WorkflowStepName.EClaimWorkflow.Rejected, new MailAddressCollection(), notificationToMails);

                //Step 2 - Set keywords
                ProcessKeywords processKeywords = SetWFProcessKeywords(vm, startWFObject, spHostUrl, ConstantHelper.WorkflowName.EclaimWorkflow, workflowDueDate, reminderDays, rerAccountEmail, rerAccountPassword, accessToken);

                //Step 3 - Start Workflow
                Actioner Originator = WorkflowHelper.ConstructActioner(startWFObject.Originator);
                string applicationName = ProjectHelper.GetRelativeUrlFromUrl(spHostUrl);
                MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                processId = wfBL.StartWorkflow(ConstantHelper.WorkflowName.EclaimWorkflow, applicationName, Originator, processKeywords, stageActioners, vm.GeneralRemarks);
                startWFObject.ProcessId = processId;
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - StartWorkflow Error: " + ex.ToString());
                errorMessage = string.Format("Error: {0}", ex.Message);
            }

            return errorMessage;
        }

        private static StageActioners SetWFActioners(StartWorkflowObject startWFObject, MainClaimHeaderVM vm)
        {
            StageActioners stageActioners = new StageActioners(ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager1Approval, startWFObject.PendingReportingManager1DueDays);
            if (startWFObject.ReportingManager1.Count > 0)
            {
                WorkflowHelper.AddStageActionerFromPeoplePickerList(ref stageActioners, startWFObject.ReportingManager1);
            }
            WorkflowHelper.AddStage(ref stageActioners, ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager2Approval, startWFObject.PendingReportingManager2DueDays, startWFObject.ReportingManager2);
            WorkflowHelper.AddStage(ref stageActioners, ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager3Approval, startWFObject.PendingReportingManager3DueDays, startWFObject.ReportingManager3);
            WorkflowHelper.AddStage(ref stageActioners, ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingClaimItemApproval, startWFObject.PendingClaimItemApproverDueDays, startWFObject.ClaimItemApprover);
            WorkflowHelper.AddStage(ref stageActioners, ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingFinalApproval, startWFObject.PendingFinalApproverDueDays, startWFObject.FinalApprover);

            return stageActioners;
        }

        public static void Require(string roleName, List<PeoplePickerUser> list)
        {
            if (list == null || list.Count == 0)
            {
                string message = string.Format(ConstantHelper.ErrorMessage.MissingWorkflowMatrix, roleName);
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage(message);
                throw new Exception(message);
            }
        }

        public static StartWorkflowObject ModifyEClaimTaskDueDate(StartWorkflowObject startWorkflowObject, ref DateTime workflowDueDate, List<DayOfWeek> daysOfWeek)
        {
            DateTime currentDate = DateTimeHelper.GetCurrentDateTime();

            //1.Pending Reporting Manager 1
            if (startWorkflowObject.ReportingManager1.Count > 0)
            {
                currentDate = DateTimeHelper.GetNextWorkingDate(daysOfWeek, currentDate, startWorkflowObject.PendingReportingManager1DueDays);
                startWorkflowObject.PendingReportingManager1DueDate = currentDate.ToString(appSettings.DefaultDateFormat);
            }
            //2.Pending Reporting Manager 2
            if (startWorkflowObject.ReportingManager2.Count > 0)
            {
                currentDate = DateTimeHelper.GetNextWorkingDate(daysOfWeek, currentDate, startWorkflowObject.PendingReportingManager2DueDays);
                startWorkflowObject.PendingReportingManager2DueDate = currentDate.ToString(appSettings.DefaultDateFormat);
            }
            //3.Pending Reporting Manager 3
            if (startWorkflowObject.ReportingManager3.Count > 0)
            {
                currentDate = DateTimeHelper.GetNextWorkingDate(daysOfWeek, currentDate, startWorkflowObject.PendingReportingManager3DueDays);
                startWorkflowObject.PendingReportingManager3DueDate = currentDate.ToString(appSettings.DefaultDateFormat);
            }
            //4.Pending Claim Item Approver
            if (startWorkflowObject.ClaimItemApprover.Count > 0)
            {
                currentDate = DateTimeHelper.GetNextWorkingDate(daysOfWeek, currentDate, startWorkflowObject.PendingClaimItemApproverDueDays);
                startWorkflowObject.PendingClaimItemApproverDueDate = currentDate.ToString(appSettings.DefaultDateFormat);
            }
            //5.Pending Final Approver
            if (startWorkflowObject.FinalApprover.Count > 0)
            {
                currentDate = DateTimeHelper.GetNextWorkingDate(daysOfWeek, currentDate, startWorkflowObject.PendingFinalApproverDueDays);
                startWorkflowObject.PendingFinalApproverDueDate = currentDate.ToString(appSettings.DefaultDateFormat);
            }
            //6.Pending Originator Resubmission
            if (startWorkflowObject.Originator != null)
            {
                currentDate = DateTimeHelper.GetNextWorkingDate(daysOfWeek, currentDate, startWorkflowObject.PendingOriginatorResubmissionDueDays);
                startWorkflowObject.PendingOriginatorResubmissionDueDate = currentDate.ToString(appSettings.DefaultDateFormat);
            }

            workflowDueDate = currentDate;
            return startWorkflowObject;
        }

        private static ProcessKeywords SetWFProcessKeywords(MainClaimHeaderVM vm, StartWorkflowObject startWFObject, string spHostUrl, string workflowName, DateTime workflowDueDate, List<int> reminderDays, string rerAccountEmail, string rerAccountPassword, string appAccessToken)
        {
            string smtp = appSettings.AG_EMAILHOST;
            string smtp_Port = appSettings.AG_EMAILPORT;
            string emailFrom = appSettings.AG_EMAILFROM;
            string emailPassword = string.Empty;
            string encKey = appSettings.AG_ENCKEY;
            string emailDefailtCred = appSettings.AG_EMAILDEFAULTCREDENTIAL;
            string emailUseSSL = appSettings.AG_EMAILUSESSL;
            string emailUseDefailtCred = appSettings.AG_EMAILUSEDEFAULTCREDENTIAL;

            if (!string.IsNullOrEmpty(rerAccountEmail))
                emailFrom = rerAccountEmail;

            if (!string.IsNullOrEmpty(rerAccountPassword))
                emailPassword = rerAccountPassword;

            if (String.IsNullOrEmpty(smtp) || String.IsNullOrEmpty(smtp_Port) || String.IsNullOrEmpty(emailFrom))
                throw new ArgumentNullException("SMTP host name, SMTP port number and Email address sender in webconfig value cannot be null");
            int emailPort = int.Parse(smtp_Port);

            ProcessKeywords processKeywords = new ProcessKeywords(vm.ReferenceNo, spHostUrl, smtp, emailPort, emailFrom, encKey, emailDefailtCred, emailUseSSL, emailUseDefailtCred, emailFrom, emailPassword);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.SPHostURL, spHostUrl);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.AppAccessToken, appAccessToken);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.SPWebURL, appSettings.RemoteAppURL);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.WorkflowName, workflowName);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.WorkflowDueDate, workflowDueDate.ToString(appSettings.DefaultDateFormat));

            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.RequestID, vm.ID.ToString());
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.RequestRefNo, vm.ReferenceNo);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.ClaimCategory, vm.ClaimCategory);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.EmployeeId, vm.RequesterEmployeeID);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.EmployeeName, vm.RequesterName);

            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.Originator, startWFObject.Originator?.Name);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.OriginatorSubmittedByName, vm.SubmittedBy);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.OriginatorSubmittedByLogin, vm.SubmittedByLogin);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.OriginatorSubmittedByEmail, vm.SubmittedByEmail);

            //TimeZone for Working Day Calculation
            string timeZoneName = appSettings.CentralTimeZone;
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.AG_TimeZoneName, timeZoneName);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.WorkflowCycleDueDays, string.Join(",", reminderDays));

            string viewRequestLink = string.Format(ConstantHelper.URLTemplate.ClaimRequestDisplayFormUrlTemplate, appSettings.RemoteAppURL.TrimEnd('/'), vm.ID, spHostUrl);
            processKeywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.ViewRequestLink, viewRequestLink);

            return processKeywords;
        }

        public static bool CompleteAWorkflowStep(string processID, string taskID, string remarks, string actionName, string spHostUrl, string accessToken)
        {
            bool completeSuccess = false;
            int _ProcessID = -1;
            int _TaskID = -1;

            if (int.TryParse(processID, out _ProcessID) && int.TryParse(taskID, out _TaskID))
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);
                    string wfConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(wfConnString);
                    wfBL.ActionTask(_TaskID, _ProcessID, actionName, actioner, remarks + "");
                    completeSuccess = true;
                }
            }
            else
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - CompleteAWorkflowStep Error: Failed To Update AppAccessToken");
            }

            return completeSuccess;
        }

        public static int GetNewDueDay(string currentStep, string workflowCycleDueDays)
        {
            int dueDay = -1;
            try
            {
                if (!string.IsNullOrEmpty(workflowCycleDueDays))
                {
                    List<int> dueDays = workflowCycleDueDays.Split(',').Select(x => int.Parse(x)).ToList();
                    // todo:
                    switch (currentStep)
                    {
                        case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager1Approval:
                            dueDay = dueDays[0];
                            break;
                        case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager2Approval:
                            dueDay = dueDays[1];
                            break;
                        case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager3Approval:
                            dueDay = dueDays[2];
                            break;
                        case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingClaimItemApproval:
                            dueDay = dueDays[3];
                            break;
                        case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingFinalApproval:
                            dueDay = dueDays[4];
                            break;
                        case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingOriginatorResubmission:
                            dueDay = dueDays[5];
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorfklowBL - GetNewDueDay error : " + ex.ToString());
            }

            return dueDay;
        }

        #endregion

        #region Workflow Code (Used by workflow engine)
        public string UpdateWFStage(string refkey, int processID, string processXML)
        {
            string curStep = string.Empty;
            string result = "Default";
            ClaimRequestDA da = new ClaimRequestDA();

            try
            {
                ProcessKeywords keywords = new ProcessKeywords(processXML);
                String connString = ConnectionStringHelper.GetGenericWFConnString();
                MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(connString);

                string spHostUrl = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.SPHostURL);
                string appAccessToken = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.AppAccessToken);
                string requestID = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.RequestID);
                int intRequestID = int.Parse(requestID);
                string currentStep = wfBL.GetCurrentStepName(processID);

                if (curStep.Equals(ConstantHelper.WorkflowStatus.TERMINATED))
                {
                    result = String.Empty;
                }

                da.UpdateRequestWFAsync(currentStep, intRequestID);

            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL UpdateWFStage Error : " + ex.ToString());
            }
            return result;
        }
        #endregion

        #region Workflow Actioners
        public async Task<bool> AddActioner(int processID, List<PeoplePickerUser> newActioner, string spHostURL, string accessToken, string appAccessToken)
        {
            LogHelper logHelper = new LogHelper();
            bool actionSuccess = false;
            try
            {
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(ctx);
                    PeoplePickerUser currentUser = ConvertEntitiesHelper.ConvertPeoplePickerUser(curUser);

                    if (WorkflowHelper.IsWorkflowRunning(processID))
                    {
                        actionSuccess = UpdateActioner(processID, newActioner, currentUser, spHostURL, accessToken, appAccessToken, false);
                    }
                    else
                    {
                        logHelper.LogMessage("WorkflowBL Error - Add Actioner Failed - Selected Workflow is not running : " + processID);
                        actionSuccess = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(string.Format("WorkflowBL - Add Actioner Error: {0}", ex));
            }
            return actionSuccess;
        }

        private static bool UpdateActioner(int processID, List<PeoplePickerUser> newActioner, PeoplePickerUser currentUser, string spHostUrl, string accessToken, string appAccessToken, bool isReassign, bool addTaskNow = true)
        {
            bool ActionSuccess = false;
            List<Actioner> Users = new List<Actioner>();
            Users = WorkflowHelper.ContructActionerList(newActioner);
            Actioner wfCurrentUser = new Actioner();
            wfCurrentUser = WorkflowHelper.ConstructActioner(currentUser);

            string ConnString = ConnectionStringHelper.GetGenericWFConnString();
            MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(ConnString);
            ProcessInstance instance = wfBL.GetProcessInstance(processID);

            wfBL.AddOrReassignActionersForStep(processID, instance.CurStepTemplateID, Users, isReassign, addTaskNow, wfCurrentUser);

            ActionSuccess = true;
            return ActionSuccess;
        }

        public async Task<bool> ReassignActioner(int processID, int taskID, PeoplePickerUser newActioner, string spHostUrl, string accessToken, string comments)
        {
            bool actionSuccess = false;
            LogHelper logHelper = new LogHelper();

            try
            {
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(ctx);
                    PeoplePickerUser currentUser = ConvertEntitiesHelper.ConvertPeoplePickerUser(curUser);
                    Actioner actionBy = WorkflowHelper.ConstructActioner(currentUser);
                    Actioner newWFActioner = WorkflowHelper.ConstructActioner(newActioner);
                    string connString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(connString);
                    ProcessInstance processInstance = wfBL.GetProcessInstance(processID);

                    var curTask = wfBL.GetTask(taskID);

                    PeoplePickerUser oldActioner = new PeoplePickerUser()
                    {
                        Email = curTask.Assignee?.Email,
                        Login = curTask.Assignee?.LoginName,
                        Name = curTask.Assignee?.Name
                    };

                    if (!wfBL.IsTaskCompleted(taskID))
                    {
                        if (!string.IsNullOrEmpty(comments))
                            comments = $"Reassigned to {newActioner.Name}. Remarks: {comments}";
                        else
                            comments = $"Reassigned to {newActioner.Name}";

                        int newTaskId = wfBL.ReassignTaskAndGetTaskIDForAdmin(taskID, actionBy, newWFActioner, comments);

                        if (newTaskId > -1) actionSuccess = true;

                        List<PeoplePickerUser> newUsers = new List<PeoplePickerUser>();
                        newUsers.Add(newActioner);
                        PeoplePickerUser taskOwner = WorkflowHelper.GetTaskOwnerByTaskID(taskID, wfBL);
                        List<PeoplePickerUser> taskOwnerList = new List<PeoplePickerUser>() { taskOwner };

                        //Get Delegate To
                        DelegationBL delegationBL = new DelegationBL(connString);
                        List<DelegationInstance> delegationInstancesList = delegationBL.GetDelegationsToUsers(new List<Actioner>() { newWFActioner }, processInstance.ApplicationName, processInstance.ProcessTemplateID);
                        DelegationInstance delegationInstance = delegationInstancesList?.FirstOrDefault(x => x.Active);

                        //Construct Delegatee
                        PeoplePickerUser delegatTo = new PeoplePickerUser();
                        if (delegationInstance != null)
                        {
                            string email = delegationInstance.DelegationToEmail;
                            string loginName = delegationInstance.DelegationTo;
                            string name = delegationInstance.DelegationToFriendlyName;

                            delegatTo.Email = email;
                            delegatTo.Login = loginName;
                            delegatTo.Name = name;
                        }

                        ProcessKeywords keywords = new ProcessKeywords(processInstance.KeywordsXML);
                        string reqRefNo = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.RequestRefNo);
                        string claimCategory = keywords.GetKeywordValue(ConstantHelper.WorkflowKeywords.Common.ClaimCategory);

                        SendGeneralWorkflowEmailNotification(ConstantHelper.EmailTemplateKeyTitle.WorkflowTaskReassignedNotification, newActioner, oldActioner, currentUser, delegatTo, DateTime.MinValue, DateTime.MinValue, spHostUrl, accessToken, reqRefNo, claimCategory);
                    }
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(string.Format("Workflow BL - Reassign Actioner Error: {0}", ex));
            }

            return actionSuccess;
        }

        public async Task<bool> RemoveActioner(int processID, int taskID, string workflowName, string remarks, string spHostUrl, string accessToken, string appAccessToken)
        {
            bool actionSuccess = false;
            PeoplePickerUser currentUser = new PeoplePickerUser();

            try
            {
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(ctx);
                    currentUser = ConvertEntitiesHelper.ConvertPeoplePickerUser(curUser);
                }

                string connString = ConnectionStringHelper.GetGenericWFConnString();
                MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(connString);

                if (!wfBL.IsTaskCompleted(taskID))
                {
                    //Determine workflow name
                    string actionName = string.Empty;
                    if (!string.IsNullOrEmpty(workflowName))
                    {
                        if (workflowName.Equals(ConstantHelper.WorkflowName.EclaimWorkflow))
                        {
                            string currentStep = wfBL.GetCurrentStepInternalName(processID);
                            switch (currentStep)
                            {
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager1Approval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager2Approval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager3Approval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingClaimItemApproval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingFinalApproval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                default:
                                    throw new DataException("Invalid Workflow Step Name");
                            }
                        }
                        else
                            throw new DataException("Invalid Workflow Name");

                        Actioner actioner = new Actioner();
                        actioner = WorkflowHelper.ConstructActioner(currentUser);

                        wfBL.RemoveTask(processID, taskID, actionName, actioner, remarks);
                        actionSuccess = true;
                    }
                    else
                        throw new DataException("Workflow Type cannot be null or empty.");
                }
                else
                {
                    throw new Exception("Selected task already completed.");
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage(string.Format("WorkflowBLHelper - RemoveActioner Error: {0}", ex.ToString()));
            }

            return actionSuccess;
        }

        public async Task<bool> RemoveAllActioner(int processID, List<int> taskIDList, List<String> workflowNameList, string remarks, string spHostUrl, string accessToken, string appAccessToken)
        {
            bool actionSuccess = false;
            PeoplePickerUser currentUser = new PeoplePickerUser();

            try
            {
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(ctx);
                    currentUser = ConvertEntitiesHelper.ConvertPeoplePickerUser(curUser);
                }

                //Determine workflow name
                string actionName;
                Actioner actioner = new Actioner();
                actioner = WorkflowHelper.ConstructActioner(currentUser);

                string ConnString = ConnectionStringHelper.GetGenericWFConnString();
                MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(ConnString);
                for (var i = 0; i < taskIDList.Count; i++)
                {
                    actionName = string.Empty;
                    if (!string.IsNullOrEmpty(workflowNameList[i]))
                    {
                        if (workflowNameList.Equals(ConstantHelper.WorkflowName.EclaimWorkflow))
                        {
                            string currentStep = wfBL.GetCurrentStepInternalName(processID);
                            switch (currentStep)
                            {
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager1Approval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager2Approval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingReportingManager3Approval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingClaimItemApproval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingFinalApproval:
                                    actionName = ConstantHelper.WorkflowActionName.Approve;
                                    break;
                                case ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingOriginatorResubmission:
                                    actionName = ConstantHelper.WorkflowActionName.Resubmit;
                                    break;
                                default:
                                    throw new DataException("Invalid Workflow Step Name"); // todo: further dev
                            }
                        }
                        else
                            throw new DataException("Invalid Workflow Name.");

                        wfBL.RemoveTask(processID, taskIDList[i], actionName, actioner, remarks);
                        actionSuccess = true;
                    }
                    else
                        throw new DataException("Workflow Type cannot be null or empty.");
                }

                actionSuccess = true;
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage(string.Format("WorkflowBLHelper - RemoveAllActioner Error: {0}", ex.ToString()));
            }

            return actionSuccess;
        }

        public static bool AddActionerForPendingCompletion(string processID, string taskID, User curUser, ClientContext context, MainClaimHeaderVM vm)
        {
            var logHelper = new LogHelper();
            bool actionSuccess = false;

            if (string.IsNullOrEmpty(processID) || string.IsNullOrEmpty(taskID))
            {
                logHelper.LogMessage("Invalid input parameters for Pending Completion.");
                return false;
            }

            try
            {
                if (curUser == null)
                {
                    logHelper.LogMessage("Failed to load the current user.");
                    return false;
                }

                var curActioner = new Actioner(curUser.LoginName, curUser.Title, curUser.Email);
                string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                if (wfBL.CheckIsMyTask(Convert.ToInt32(taskID), curActioner))
                {
                    logHelper.LogMessage($"Task ID {taskID} is not assigned to the current user.");
                    return false;
                }

                int.TryParse(processID, out int processIDInt);
                MHA.Framework.Core.Workflow.DL.InstanceDL instanceDL = new MHA.Framework.Core.Workflow.DL.InstanceDL(WFConnString);
                string currentStep = wfBL.GetCurrentStepInternalName(processIDInt);

                ProcessKeywords keywords = wfBL.GetKeywords(processIDInt);

                // todo:
                //string pendingAcknowledgement = ConstantHelper.WorkflowStepName.EClaimWorkflow.PendingAcknowledgement;
                //StepInstance stepInstance = instanceDL.GetStepInstanceByStepName(processIDInt, pendingAcknowledgement);

                //// assign to 
                //List<Actioner> empActionerList = new List<Actioner>();
                //Actioner actioner = new Actioner
                //{
                //    LoginName = vm.EmployeeDetails.EmployeeLogin,
                //    Name = vm.EmployeeDetails.EmployeeName,
                //    Email = vm.EmployeeDetails.EmployeeEmail
                //};
                //empActionerList.Add(actioner);

                //wfBL.UpdateActionersForStep(processIDInt, stepInstance.InternalStepName, empActionerList, true, true, curActioner);
                actionSuccess = true;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"AddActionerForPendingCompletion encountered an error: {ex.Message}\n{ex.StackTrace}");
            }
            return actionSuccess;
        }

        #endregion

        #region Email Notifications
        private static void SendGeneralWorkflowEmailNotification(string emailTitle, PeoplePickerUser newActioner, PeoplePickerUser oldActioner, PeoplePickerUser currentUser, PeoplePickerUser delegator, DateTime startDate, DateTime endDate, string spHostUrl, string accessToken, string reqRefNo, string reqType)
        {
            string emailSubject = string.Empty;
            string emailBody = string.Empty;
            EmailTemplateBL.GetEmailTemplateByTemplateTitle(emailTitle, ref emailSubject, ref emailBody, spHostUrl, accessToken);
            if (!string.IsNullOrEmpty(emailSubject) && !string.IsNullOrEmpty(emailBody))
            {
                //Initialize the CC Email Address Collection
                MailAddressCollection emailCC = new MailAddressCollection();

                string homePageURL = string.Format(ConstantHelper.URLTemplate.HomePageUrlTemplate, appSettings.RemoteAppURL.TrimEnd('/'), spHostUrl);

                //Build email subject
                if (emailTitle.Equals(ConstantHelper.EmailTemplateKeyTitle.WorkflowTaskReassignedNotification))
                {
                    emailSubject = BuildReassignedEmailSubject(emailSubject);
                    emailBody = BuildReassignedEmailBody(emailBody, homePageURL, newActioner, oldActioner, currentUser, reqRefNo, reqType);

                    //Push the delegatee to CC
                    if (delegator != null && !string.IsNullOrEmpty(delegator.Email))
                        emailCC.Add(new MailAddress(delegator.Email));
                }
                else if (emailTitle.Equals(ConstantHelper.EmailTemplateKeyTitle.WorkflowTaskDelegatedNotification))
                {
                    emailSubject = BuildDelegatedEmailSubject(emailSubject, delegator);
                    emailBody = BuildDelegatedEmailBody(emailBody, homePageURL, newActioner, delegator, startDate, endDate);
                }
                else
                    throw new Exception("Invalid email notification title has been selected");

                MailAddressCollection emailTo = new MailAddressCollection();
                if (!string.IsNullOrEmpty(newActioner.Email))
                    emailTo.Add(new MailAddress(newActioner.Email));

                EmailHelper.SendEmailWithSender(appSettings.rerAccountEmail, appSettings.rerAccountPassword, emailTo, emailCC, emailSubject, emailBody, new List<System.Net.Mail.Attachment>());
            }
        }

        private static string BuildReassignedEmailSubject(string emailSubject)
        {
            emailSubject = emailSubject.Replace("{ProjectName}", ConstantHelper.Module.Eclaim);
            return emailSubject;
        }

        private static string BuildReassignedEmailBody(string emailBody, string homePageURL, PeoplePickerUser newActioner, PeoplePickerUser oldActioner, PeoplePickerUser currentUser, string reqRefNo, string reqType)
        {
            emailBody = emailBody.Replace("{ProjectName}", ConstantHelper.Module.Eclaim);
            emailBody = emailBody.Replace("{HomePageURL}", homePageURL);
            emailBody = emailBody.Replace("{NewActioner}", newActioner.Name);
            emailBody = emailBody.Replace("{OldActioner}", oldActioner.Name);
            emailBody = emailBody.Replace("{Assigner}", currentUser.Name);

            emailBody = emailBody.Replace("{ReqRefNo}", reqRefNo);
            emailBody = emailBody.Replace("{ReqType}", reqType);
            return emailBody;
        }

        private static string BuildDelegatedEmailSubject(string emailSubject, PeoplePickerUser delegator)
        {
            emailSubject = emailSubject.Replace("{ProjectName}", ConstantHelper.Module.Eclaim);
            emailSubject = emailSubject.Replace("{Delegator}", delegator.Name);
            return emailSubject;
        }

        private static string BuildDelegatedEmailBody(string emailBody, string homePageURL, PeoplePickerUser newActioner, PeoplePickerUser delegator, DateTime startDate, DateTime endDate)
        {
            emailBody = emailBody.Replace("{ProjectName}", ConstantHelper.Module.Eclaim);
            emailBody = emailBody.Replace("{HomePageURL}", homePageURL);
            emailBody = emailBody.Replace("{Delegator}", delegator.Name);
            emailBody = emailBody.Replace("{NewActioner}", newActioner.Name);
            emailBody = emailBody.Replace("{StartDate}", startDate.ToString(appSettings.DefaultDateFormat));
            emailBody = emailBody.Replace("{EndDate}", endDate.ToString(appSettings.DefaultDateFormat));
            return emailBody;
        }
        #endregion

        #region General Functions
        private bool ShowRemoveAction(string status)
        {
            bool isShow = false;

            bool isInprogress = status.Equals(ConstantHelper.WorkflowStatus.IN_PROGRESS);

            if (isInprogress)
            {
                isShow = true;
            }

            return isShow;
        }

        private static ListItem? GetCurrentEmployeeItemWithApprovers(ClientContext clientContext)
        {
            User currentUser = SharePointHelper.GetCurrentUser(clientContext);

            string[] viewFields = {
                ConstantHelper.SPColumn.EmployeeList.Approver1,
                ConstantHelper.SPColumn.EmployeeList.Approver2,
                ConstantHelper.SPColumn.EmployeeList.Approver3
            };

            string queryCondition = $@"
            <Eq>
                <FieldRef Name='{ConstantHelper.SPColumn.EmployeeList.EmployeePerson}' LookupId='TRUE'/>
                <Value Type='Integer'>{currentUser.Id}</Value>
            </Eq>";

            return GeneralQueryHelper
                .GetSPItems(clientContext, ConstantHelper.SPList.Employee, queryCondition, viewFields)
                ?.FirstOrDefault();
        }

        private static IEnumerable<ListItem> GetClaimItemApprovers(ClientContext clientContext, string expensesSubType)
        {
            string[] viewFields =
            {
                ConstantHelper.SPColumn.ExpensesList.ClaimItemApprover
            };

            string queryCondition = $@"
            <Eq>
                <FieldRef Name='{ConstantHelper.SPColumn.ExpensesList.ExpensesSubType}' />
                <Value Type='Text'>{expensesSubType}</Value>
            </Eq>";

            return GeneralQueryHelper
                .GetSPItems(clientContext, ConstantHelper.SPList.Expenses, queryCondition, viewFields)
                ?? Enumerable.Empty<ListItem>();
        }
        #endregion

        #region Lock Method
        private static object workflowLock = new object();
        public static bool CheckIsTaskNotStarted(string taskId, bool isMyTask)
        {
            bool result = false;
            if (isMyTask && !string.IsNullOrEmpty(taskId) && int.TryParse(taskId, out int _taskId))
            {
                try
                {
                    WorkflowDA workflowDA = new WorkflowDA();
                    lock (workflowLock)
                    {
                        //Check is exist in table
                        if (!workflowDA.IsTaskOngoing(_taskId))
                        {
                            //Insert New Row in table
                            workflowDA.InsertOnGoingTasks(_taskId);
                            result = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper logHelper = new LogHelper();
                    logHelper.LogMessage("Error WorkflowBL - CheckIsTaskNotStarted TaskID : " + taskId + " :" + ex.ToString());
                }
            }
            return result;
        }

        public static bool CloseRunningTask(string taskId, bool isNotSkip)
        {
            bool result = false;
            if (isNotSkip)
            {
                if (!string.IsNullOrEmpty(taskId) && int.TryParse(taskId, out int _taskId))
                {
                    try
                    {
                        WorkflowDA workflowDA = new WorkflowDA();
                        lock (workflowLock)
                        {
                            //Check is exist in table
                            workflowDA.UpdateOnGoingTasks(_taskId);
                            result = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper logHelper = new LogHelper();
                        logHelper.LogMessage("WorkflowBL - CloseRunningTask Error: TaskID : " + taskId + " :" + ex.ToString());
                    }
                }
            }
            return result;
        }

        #endregion

        #region Delegation
        public async Task<ViewModelMyDelegate> InitMyDelegate(string spHostURL, string accessToken)
        {
            ViewModelMyDelegate vmMyDelegate = new ViewModelMyDelegate();

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(clientContext);

                    vmMyDelegate.IsAdmin = SharePointHelper.IsUserInGroup(clientContext, curUser.LoginName, ConstantHelper.SPSecurityGroup.SDCClaimsAdmin);

                    PeoplePickerUser curUserObj = new PeoplePickerUser();
                    curUserObj.Email = curUser.Email;
                    curUserObj.Login = curUser.LoginName;
                    curUserObj.Name = curUser.Title;

                    if (vmMyDelegate.IsAdmin)
                        vmMyDelegate.DelegationList = GetAllDelegationList(spHostURL, clientContext);
                    else
                        vmMyDelegate.DelegationList = GetMyDelegationList(curUserObj, spHostURL, clientContext);
                }

                if (vmMyDelegate.DelegationList?.Count > 0)
                    vmMyDelegate.HasItem = true;

                vmMyDelegate.DateFormat = appSettings.DefaultDateFormat;
                vmMyDelegate.NewDelegationURL = string.Format(ConstantHelper.URLTemplate.NewDelegationUrlTemplate, appSettings.RemoteAppURL, spHostURL);
                vmMyDelegate.RemoveDelegationURL = string.Format(ConstantHelper.URLTemplate.NewDelegationUrlTemplate, appSettings.RemoteAppURL, spHostURL);

            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - InitMyDelegate Error: " + ex.ToString());
            }

            return vmMyDelegate;
        }

        public List<DelegationObject> GetAllDelegationList(string spHostURL, ClientContext clientContext)
        {
            List<DelegationInstance> instanceList = new List<DelegationInstance>();
            List<DelegationObject> resultList = new List<DelegationObject>();

            try
            {
                string ConnString = ConnectionStringHelper.GetGenericWFConnString();
                DelegationBL delegateBl = new DelegationBL(ConnString);

                instanceList = delegateBl.GetAllDelegations(ProjectHelper.GetRelativeUrlFromUrl(spHostURL));

                DateTime currentDateTime = DateTimeHelper.GetCurrentDateTime();

                if (instanceList != null && instanceList.Count > 0)
                {
                    foreach (DelegationInstance obj in instanceList)
                    {
                        if(obj.ProcessTemplateID == ConstantHelper.WorkflowTemplateId.EClaimApprovalWorkflow)
                        {
                            obj.DelegationStartDate = SPDateTimeHelper.ConvertFromUTCWithSPTimeZone(obj.DelegationStartDate, clientContext);
                            obj.DelegationEndDate = SPDateTimeHelper.ConvertFromUTCWithSPTimeZone(obj.DelegationEndDate, clientContext);

                            DelegationObject delegateObj = new DelegationObject();
                            delegateObj.Active = obj.Active;
                            delegateObj.ApplicationName = obj.ApplicationName;
                            delegateObj.DelegationEndDate = obj.DelegationEndDate;
                            delegateObj.DelegationFrom = obj.DelegationFrom;
                            delegateObj.DelegationFromEmail = obj.DelegationFromEmail;
                            delegateObj.DelegationFromFriendlyName = obj.DelegationFromFriendlyName;
                            delegateObj.DelegationID = obj.DelegationID;
                            delegateObj.DelegationStartDate = obj.DelegationStartDate;
                            delegateObj.DelegationTo = obj.DelegationTo;
                            delegateObj.DelegationToEmail = obj.DelegationToEmail;
                            delegateObj.DelegationToFriendlyName = obj.DelegationToFriendlyName;
                            delegateObj.ProcessName = obj.ProcessName;
                            delegateObj.ProcessTemplateID = obj.ProcessTemplateID;
                            delegateObj.IsAbleToRemove = true;

                            if (currentDateTime.Date <= obj.DelegationEndDate.Date)
                            {
                                resultList.Add(delegateObj);
                            }
                        }
                       
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - GetAllDelegationList Error: " + ex.ToString());
            }
            return resultList;
        }

        public List<DelegationObject> GetMyDelegationList(PeoplePickerUser curUser, string spHostURL, ClientContext clientContext)
        {
            List<DelegationInstance> instanceList = new List<DelegationInstance>();
            List<DelegationObject> resultList = new List<DelegationObject>();

            try
            {
                string ConnString = ConnectionStringHelper.GetGenericWFConnString();
                DelegationBL delegateBl = new DelegationBL(ConnString);
                Actioner actioner = new Actioner(curUser.Login, curUser.Name, curUser.Email);
                instanceList = delegateBl.GetMyDelegations(actioner, ProjectHelper.GetRelativeUrlFromUrl(spHostURL));
                DateTime currentDate = DateTimeHelper.GetCurrentDateTime();

                if (instanceList != null && instanceList.Count > 0)
                {
                    foreach (DelegationInstance obj in instanceList)
                    {
                        if (obj.ProcessTemplateID == ConstantHelper.WorkflowTemplateId.EClaimApprovalWorkflow)
                        {
                            obj.DelegationStartDate = SPDateTimeHelper.ConvertFromUTCWithSPTimeZone(obj.DelegationStartDate, clientContext);
                            obj.DelegationEndDate = SPDateTimeHelper.ConvertFromUTCWithSPTimeZone(obj.DelegationEndDate, clientContext);

                            DelegationObject delegateObj = new DelegationObject();
                            delegateObj.Active = obj.Active;
                            delegateObj.ApplicationName = obj.ApplicationName;
                            delegateObj.DelegationEndDate = obj.DelegationEndDate;
                            delegateObj.DelegationFrom = obj.DelegationFrom;
                            delegateObj.DelegationFromEmail = obj.DelegationFromEmail;
                            delegateObj.DelegationFromFriendlyName = obj.DelegationFromFriendlyName;
                            delegateObj.DelegationID = obj.DelegationID;
                            delegateObj.DelegationStartDate = obj.DelegationStartDate;
                            delegateObj.DelegationTo = obj.DelegationTo;
                            delegateObj.DelegationToEmail = obj.DelegationToEmail;
                            delegateObj.DelegationToFriendlyName = obj.DelegationToFriendlyName;
                            delegateObj.ProcessName = obj.ProcessName;
                            delegateObj.ProcessTemplateID = obj.ProcessTemplateID;
                            if (delegateObj.DelegationFromFriendlyName.Equals(curUser.Name))
                                delegateObj.IsAbleToRemove = true;
                            else
                                delegateObj.IsAbleToRemove = false;

                            if (currentDate.Date <= obj.DelegationEndDate.Date)
                            {
                                resultList.Add(delegateObj);
                            }
                        }                            
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - GetMyDelegationList Error: " + ex.ToString());
            }

            return resultList;
        }

        #region New Delegation
        public async Task<ViewModelNewDelegate> InitNewDelegate(string spHostURL, string accessToken)
        {
            ViewModelNewDelegate vmNewDelegate = new ViewModelNewDelegate();

            try
            {
                vmNewDelegate.ProcessTemplateList = ProjectHelper.GetProcessTemplateDDL();
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(clientContext);
                    vmNewDelegate.isAdmin = SharePointHelper.IsUserInGroup(clientContext, curUser.LoginName, ConstantHelper.SPSecurityGroup.SDCClaimsAdmin);
                    vmNewDelegate.curUserName = curUser.Title;
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("WorkflowBL - InitNewDelegate Error: " + ex.ToString());

                vmNewDelegate.HasError = true;
                vmNewDelegate.ErrorMessage = string.Format(ConstantHelper.ErrorMessage.UnexpectedErrorOccur, ex.Message);
            }
            return vmNewDelegate;
        }

        public async Task<string> SetDelegation(ViewModelNewDelegate vmNewDelegate, string spHostURL, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(clientContext);

                    PeoplePickerUser fromUser = new PeoplePickerUser();
                    if (vmNewDelegate.isAdmin)
                    {
                        fromUser = vmNewDelegate.txtFromUser ?? throw new ArgumentNullException("Delegation From field cannot be null value.");
                    }
                    else
                    {
                        //delegate from current user
                        fromUser = new PeoplePickerUser { Email = curUser.Email, Login = curUser.LoginName, Name = curUser.Title };
                    }

                    PeoplePickerUser toUser = vmNewDelegate.txtTouser;
                    if (toUser == null)
                    {
                        throw new ArgumentNullException("Delegation To field cannot be null value.");
                    }

                    string applicationName = ProjectHelper.GetRelativeUrlFromUrl(spHostURL);

                    DelegationInstance delegateObj = new DelegationInstance();
                    delegateObj.DelegationEndDate = DateTimeHelper.ConvertToUTCDateTime(vmNewDelegate.dateTo.AddDays(1).Date.AddSeconds(-1));
                    delegateObj.DelegationFrom = fromUser.Login;
                    delegateObj.DelegationFromEmail = fromUser.Email;
                    delegateObj.DelegationFromFriendlyName = fromUser.Name;
                    delegateObj.DelegationStartDate = DateTimeHelper.ConvertToUTCDateTime(vmNewDelegate.dateFrom);
                    delegateObj.DelegationTo = toUser.Login;
                    delegateObj.DelegationToEmail = toUser.Email;
                    delegateObj.DelegationToFriendlyName = toUser.Name;
                    delegateObj.ApplicationName = applicationName;
                    int processID = -1;

                    string connString = ConnectionStringHelper.GetGenericWFConnString();
                    DelegationBL delegateBl = new DelegationBL(connString);
                    Actioner actioner = new Actioner(fromUser.Login, fromUser.Name, fromUser.Email);
                    List<DelegationInstance> delegations = delegateBl.GetMyDelegations(actioner, applicationName);
                    bool hasError = true;

                    foreach (var d in delegations)
                    {
                        if (d.ProcessTemplateID == ConstantHelper.WorkflowTemplateId.EClaimApprovalWorkflow)
                        {
                            bool overlaps =
                           (delegateObj.DelegationStartDate <= d.DelegationEndDate &&
                            d.DelegationStartDate <= delegateObj.DelegationEndDate);

                            if (overlaps)
                            {
                                DateTime overlappedStartDate = DateTimeHelper.ConvertToLocalDateTime(d.DelegationStartDate);
                                DateTime overlappedEndDate = DateTimeHelper.ConvertToLocalDateTime(d.DelegationEndDate);

                                int tempProcessID = 0;
                                if (!int.TryParse(vmNewDelegate.ddlProcessTemplate, out tempProcessID))
                                {
                                    return string.Format("Unable to submit delegation for all processes because the selected dates overlap with an existing delegation ({0}: {1} to {2})",
                                        d.ProcessName, overlappedStartDate.ToString(appSettings.DefaultDateFormat), overlappedEndDate.ToString(appSettings.DefaultDateFormat));
                                }
                                else if (d.ProcessTemplateID == tempProcessID)
                                {
                                    return string.Format("Unable to submit delegation because the selected dates overlap with an existing delegation ({0}: {1} to {2})",
                                        d.ProcessName, overlappedStartDate.ToString(appSettings.DefaultDateFormat), overlappedEndDate.ToString(appSettings.DefaultDateFormat));
                                }
                            }
                        }
                           
                    }

                    //foreach (var d in delegations) OLD Version
                    //{
                    //    if (delegateObj.DelegationStartDate < d.DelegationStartDate && d.DelegationStartDate < delegateObj.DelegationEndDate)
                    //    {
                    //        hasError = true;
                    //    }
                    //    else if (delegateObj.DelegationStartDate < d.DelegationEndDate && d.DelegationEndDate <= delegateObj.DelegationEndDate)
                    //    {
                    //        hasError = true;
                    //    }
                    //    else if (d.DelegationStartDate < delegateObj.DelegationStartDate && d.DelegationEndDate > delegateObj.DelegationEndDate)
                    //    {
                    //        hasError = true;
                    //    }
                    //    else
                    //    {
                    //        hasError = false;
                    //    }
                    //    if (hasError)
                    //    {
                    //        DateTime overlappedStartDate = DateTimeHelper.ConvertToLocalDateTime(d.DelegationStartDate);
                    //        DateTime overlappedEndDate = DateTimeHelper.ConvertToLocalDateTime(d.DelegationEndDate);

                    //        int tempProcessID = 0;
                    //        // Selected all processes
                    //        if (!int.TryParse(vmNewDelegate.ddlProcessTemplate, out tempProcessID))
                    //        {
                    //            return string.Format("Unable to submit delegation for all processes because the selected dates overlap with an existing delegation ({0}: {1} to {2})", d.ProcessName, overlappedStartDate.ToString(appSettings.DefaultDateFormat), overlappedEndDate.ToString(appSettings.DefaultDateFormat));
                    //        }
                    //        else
                    //        {
                    //            if (d.ProcessTemplateID == tempProcessID)
                    //            {
                    //                return string.Format("Unable to submit delegation because the selected dates overlap with an existing delegation ({0}: {1} to {2})", d.ProcessName, overlappedStartDate.ToString(appSettings.DefaultDateFormat), overlappedEndDate.ToString(appSettings.DefaultDateFormat));
                    //            }
                    //        }
                    //    }
                    //}

                    if (!int.TryParse(vmNewDelegate.ddlProcessTemplate, out processID))
                    {
                        List<ProcessTemplate> processInstanceList = new List<ProcessTemplate>();
                        processInstanceList = ProjectHelper.GetWorkflowProcessTemplate();
                        foreach (ProcessTemplate obj in processInstanceList)
                        {
                            //if (obj.ProcessID == 1)
                            //{
                                delegateObj.ProcessTemplateID = obj.ProcessID;
                                InsertDelegation(delegateObj, curUser.LoginName);
                            //}
                        }
                    }
                    else
                    {

                        delegateObj.ProcessTemplateID = processID;
                        InsertDelegation(delegateObj, curUser.LoginName);
                    }

                    //Send delegation notification
                    SendGeneralWorkflowEmailNotification(ConstantHelper.EmailTemplateKeyTitle.WorkflowTaskDelegatedNotification, toUser, null, null, fromUser, vmNewDelegate.dateFrom, vmNewDelegate.dateTo, spHostURL, accessToken, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage("WorkflowBL - SetDelegation Error: " + ex.ToString());
                return ConstantHelper.ErrorMessage.UnexpectedErrorOccur;
            }

            return string.Empty;
        }

        public int InsertDelegation(DelegationInstance delegationObj, string curUserLogin)
        {
            string ConnString = ConnectionStringHelper.GetGenericWFConnString();
            DelegationBL delegateBl = new DelegationBL(ConnString);
            return delegateBl.InsertDelegationInstance(delegationObj, curUserLogin);
        }
        #endregion

        #region Remove Delegation
        public void DeleteDelegation(int delegationID, string spHostURL, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User curUser = SharePointHelper.GetCurrentUser(clientContext);

                    string curUserLogin = curUser.LoginName;

                    string ConnString = ConnectionStringHelper.GetGenericWFConnString();
                    DelegationBL delegateBl = new DelegationBL(ConnString);
                    delegateBl.DeleteDelegationInstance(delegationID, curUserLogin);
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage("WorkflowBL - DeleteDelegation Error: " + ex.ToString());
            }
        }
        #endregion

        #endregion

        #region Workflow Task Report

        public async Task<ViewModelWorkflowReport> CheckAuthorityWorkflowStatus(ViewModelWorkflowReport vm, string spHostUrl, string accessToken)
        {
            //ViewModelWorkflowReport obj = new();
            using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
            {
                User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                vm.CurrentUser = currentUser.LoginName;

                string[] groups = {
                ConstantHelper.SPSecurityGroup.SDCClaimsAdmin,
                ConstantHelper.SPSecurityGroup.RWCClaimsMembers
                };
                foreach (string group in groups)
                {
                    bool inGroup = SharePointHelper.IsUserInGroup(clientContext, string.Empty, group);
                    if (inGroup)
                    {
                        vm.AccessGroups.Add(group);
                        //vm.IsAdmin = true; //i think this can be better with the logic, may have some bugs
                    }
                }

                if (vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.SDCClaimsAdmin))
                {
                    vm.IsAdmin = true;
                }
            }

            return vm;
        }
        public async Task<ViewModelWorkflowReport> SearchWorkflowTaskReport(ViewModelWorkflowReport vm, string spHostUrl, string accessToken)
        {
            
            WorkflowDA reqDA = new WorkflowDA();

            using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
            {
                User currentUser = SharePointHelper.GetCurrentUser(clientContext);

                vm.CurrentUser = currentUser.Title;
                vm.CurrentUserLogin = currentUser.LoginName;

                // Workflow Started Dates
                if (vm.taskSearchModel.WorkflowStartedFrom != null)
                    vm.taskSearchModel.WorkflowStartedFrom = vm.taskSearchModel.WorkflowStartedFrom.Value.Date;
                if (vm.taskSearchModel.WorkflowStartedTo != null)
                    vm.taskSearchModel.WorkflowStartedTo = vm.taskSearchModel.WorkflowStartedTo.Value.Date.AddDays(1).AddTicks(-1);

                // Workflow Completed Dates
                if (vm.taskSearchModel.WorkflowCompletedFrom != null)
                    vm.taskSearchModel.WorkflowCompletedFrom = vm.taskSearchModel.WorkflowCompletedFrom.Value.Date;
                if (vm.taskSearchModel.WorkflowCompletedTo != null)
                    vm.taskSearchModel.WorkflowCompletedTo = vm.taskSearchModel.WorkflowCompletedTo.Value.Date.AddDays(1).AddTicks(-1);

                // Assigned Dates
                if (vm.taskSearchModel.AssignedDateFrom != null)
                    vm.taskSearchModel.AssignedDateFrom = vm.taskSearchModel.AssignedDateFrom.Value.Date;
                if (vm.taskSearchModel.AssignedDateTo != null)
                    vm.taskSearchModel.AssignedDateTo = vm.taskSearchModel.AssignedDateTo.Value.Date.AddDays(1).AddTicks(-1);

                // Actioned Dates
                if (vm.taskSearchModel.ActionedDateFrom != null)
                    vm.taskSearchModel.ActionedDateFrom = vm.taskSearchModel.ActionedDateFrom.Value.Date;
                if (vm.taskSearchModel.ActionedDateTo != null)
                    vm.taskSearchModel.ActionedDateTo = vm.taskSearchModel.ActionedDateTo.Value.Date.AddDays(1).AddTicks(-1);

                // Single date filters (exact match)
                if (vm.taskSearchModel.CompletionDate != null)
                    vm.taskSearchModel.CompletionDate = vm.taskSearchModel.CompletionDate.Value.Date;
                if (vm.taskSearchModel.AssignedDate != null)
                    vm.taskSearchModel.AssignedDate = vm.taskSearchModel.AssignedDate.Value.Date;
                if (vm.taskSearchModel.ActionedDate != null)
                    vm.taskSearchModel.ActionedDate = vm.taskSearchModel.ActionedDate.Value.Date;

                vm = reqDA.GetWorkflowTaskReport(vm);
            }
            return vm;
        }
        
        public async Task<ViewModelWorkflowReport> ExportToExcelWorkflowTaskReport(ViewModelWorkflowReport vm, string spHostUrl, string accessToken)
        {
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    if (vm.WorkflowReport == null || !vm.WorkflowReport.Any())
                    {
                        return vm;
                    }
                    ExportToExcel(vm, spHostUrl, clientContext);
                }
            }catch(Exception ex)
            {
                throw ex;
            }
          

            return vm;
        }

        //Export to Excel
        public static void ExportToExcel(ViewModelWorkflowReport vm, string spHostURL, ClientContext clientContext)
        {
            List<string> displayFields = new List<string>(); //use constatnt helper for naming 
            List<string> internalFields = new List<string>();
            List<string> dateFields = new List<string>();

            List<dynamic> dynamicObject = ProjectHelper.ClassToDynamic(vm.WorkflowReport);
            //use Constant Helper
            // Display field labels (as shown in grid headers)

            displayFields.Add("Reference No");
            displayFields.Add("Comapny Name");
            displayFields.Add("Workflow Stage");
            displayFields.Add("Task Actioner");
            displayFields.Add("Task Status");
            displayFields.Add("Workflow Start");
            displayFields.Add("Workflow Completed");
            displayFields.Add("Assigned Date");
            displayFields.Add("Actioned Date");
            displayFields.Add("Days Taken");
            displayFields.Add("Task Overdue Days");

            // Internal field names (bound property names)
            internalFields.Add("ReferenceNo");
            internalFields.Add("CompanyName");
            internalFields.Add("WorkflowStage");
            internalFields.Add("AssigneeName");
            internalFields.Add("Status");
            internalFields.Add("StartDate");
            internalFields.Add("CompletionDate");
            internalFields.Add("AssignedDate");
            internalFields.Add("ActionedDate");
            internalFields.Add("DaysTaken");
            internalFields.Add("TaskOverdueDays");


            ReportInfo reportInfo = new ReportInfo();

            ProjectHelper.GetReportInfoWithLogo(clientContext, spHostURL, reportInfo, "EClaims_WorkflowReport.xlsx");

            vm.ExcelFileBytes = ExcelHelper.CreateExcelReportWithLogo(displayFields, internalFields, dateFields, dynamicObject, reportInfo, clientContext);
            //List<string> displayFields, List<string> internalFields, List<string> dateFields, List<dynamic> dynamicListItem, ReportInfo vmReport, ClientContext clientContext, ClientContext rootClientContext
        }
        #endregion
    }
}
