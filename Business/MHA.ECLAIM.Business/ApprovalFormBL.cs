using MHA.Framework.Core.SP;
using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Data;
using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
//using Microsoft.Graph;
using Microsoft.SharePoint.Client;
using static MHA.ECLAIM.Framework.Constants.ConstantHelper;

namespace MHA.ECLAIM.Business
{
    public class ApprovalFormBL
    {
        private static readonly JSONAppSettings appSettings;
        private readonly ClaimRequestDA _claimDA;

        static ApprovalFormBL()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public ApprovalFormBL()
        {
            _claimDA = new ClaimRequestDA();
        }

        #region Init
        public async Task<ApprovalFormVM> InitApprovalForm(ApprovalFormVM model, string spHostUrl, string accessToken)
        {
            ApprovalFormVM vm = new ApprovalFormVM();
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);

                    vm.CurrentUser = currentUser.LoginName;

                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);

                    var isMyTask = wfBL.CheckIsMyTask(model.TaskID.Value, actioner);

                    if (isMyTask)
                    {
                        vm.IsMyTask = isMyTask;

                        string[] groups = [ConstantHelper.SPSecurityGroup.SDCClaimsAdmin, ConstantHelper.SPSecurityGroup.HeadOfDepartment, ConstantHelper.SPSecurityGroup.HRDepartment, ConstantHelper.SPSecurityGroup.FinanceDepartment];

                        foreach (string group in groups)
                        {
                            bool inGroup = false;

                            inGroup = SharePointHelper.IsUserInGroup(currentUser, group);

                            vm.AccessGroups.Add(group);
                        }

                        if (vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.FinanceDepartment) || vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.HRDepartment))
                        {
                            string queryCondition = GeneralQueryHelper.ConcatCriteria(null, "Admin", "Integer", currentUser.Id.ToString(), "Contains", true);

                            ListItemCollection departmentItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Department, queryCondition, [ConstantHelper.SPColumn.DepartmentList.Title]);

                            if (departmentItems != null)
                            {
                                foreach (ListItem item in departmentItems)
                                {
                                    vm.AccessDepartments.Add(FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.DepartmentList.ID));
                                }
                            }
                        }

                        var headerVm = await _claimDA.RetrieveMainClaimHeaderByIDAsync(model.RequestID.Value);
                        if (headerVm == null)
                            throw new Exception($"No Travel Request Header found with ID {model.RequestID.Value}");

                        vm.MainClaimHeaderVM = headerVm;
                        vm.SubClaimDetailsVM = headerVm.SubClaimDetails;

                        if(vm.MainClaimHeaderVM.ClaimStatus == ConstantHelper.WorkflowStatus.PendingOriginatorResubmission)
                        {
                            #region Drop down list from maintenance list
                            ListItemCollection claimCategoryList = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.ClaimCategory, string.Empty, null);
                            vm.MainClaimHeaderVM.ClaimCategories = ProjectHelper.GetListItemsAsDDLItems(claimCategoryList, ConstantHelper.SPColumn.ClaimCategoryList.Title, ConstantHelper.SPColumn.ClaimCategoryList.Status);
                            ListItemCollection currencyListItem = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Currency, string.Empty, null);
                            vm.MainClaimHeaderVM.CurrencyList = ProjectHelper.GetListItemsAsDDLItems(currencyListItem, ConstantHelper.SPColumn.CurrencyList.ISOCode, ConstantHelper.SPColumn.CurrencyList.Status);
                            #endregion

                            #region Get Travel Request List for dropdown
                            TravelRequestDA daTravel = new TravelRequestDA();
                            var travelRequestList = await daTravel.GetByEmployeeLogin(vm.MainClaimHeaderVM.RequesterLogin);
                            if (travelRequestList != null && travelRequestList.Any())
                            {
                                vm.MainClaimHeaderVM.TravelRequestRefNoList = travelRequestList
                                .Select(m => new DropDownListItem
                                {
                                    Id = m.ID.ToString(),
                                    Text = m.ReferenceNo,
                                    Value = m.ID.ToString()
                                })
                                .ToList();
                            }
                            #endregion

                            #region Get Currency Exchange Rate list
                            string currentYear = DateTime.Now.Year.ToString();
                            string currentMonth = DateTime.Now.Month.ToString();

                            string queryConditionCurExc = $@"
                                <And>
                                    <Eq>
                                        <FieldRef Name='{ConstantHelper.SPColumn.CurrencyExchangeRate.Year}' />
                                        <Value Type='Text'>{currentYear}</Value>
                                    </Eq>
                                    <Eq>
                                        <FieldRef Name='{ConstantHelper.SPColumn.CurrencyExchangeRate.Month}' />
                                        <Value Type='Text'>{currentMonth}</Value>
                                    </Eq>
                                </And>";

                            ListItemCollection curExchangeRateItems = GeneralQueryHelper.GetSPItems(
                                clientContext,
                                ConstantHelper.SPList.CurrencyExchangeRate,
                                queryConditionCurExc,
                                null
                            );

                            if (curExchangeRateItems != null && curExchangeRateItems.Any())
                            {
                                vm.MainClaimHeaderVM.CurrencyExchangeRates = curExchangeRateItems.Select(item => new ViewModelCurrencyExchangeRate
                                {
                                    Year = FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.CurrencyExchangeRate.Year),
                                    Month = FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.CurrencyExchangeRate.Month),
                                    BaseCurrency = FieldHelper.GetLookupValueAsString(item, ConstantHelper.SPColumn.CurrencyExchangeRate.BaseCurrency),
                                    TargetCurrency = FieldHelper.GetLookupValueAsString(item, ConstantHelper.SPColumn.CurrencyExchangeRate.TargetCurrency),
                                    Rate = Convert.ToDecimal(FieldHelper.GetFieldValueAsDouble(item, ConstantHelper.SPColumn.CurrencyExchangeRate.Rate)),

                                }).ToList();
                            }
                            #endregion

                            #region Get Expenses List
                            ListItemCollection expensesList = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Expenses, string.Empty, null);

                            var maintenanceList = new List<SubClaimDetailsVM>();
                            if (expensesList != null && expensesList.Count > 0)
                            {
                                foreach (ListItem li in expensesList)
                                {
                                    var approver1 = FieldHelper.GetFieldUserValue(li, ConstantHelper.SPColumn.ExpensesList.ClaimItemApprover);

                                    var item = new SubClaimDetailsVM
                                    {
                                        ID = FieldHelper.GetFieldValueAsNumber(li, "ID"),
                                        Expenses = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.ExpensesList.Title),
                                        ExpensesSubtype = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.ExpensesList.ExpensesSubType),
                                        ClaimCategory = FieldHelper.GetLookupValueAsString(li, ConstantHelper.SPColumn.ExpensesList.ClaimCategory),
                                        ClaimEntitlementType = FieldHelper.GetLookupValueAsString(li, ConstantHelper.SPColumn.ExpensesList.ClaimEntitlementType),
                                        ClaimEntitlementTypeSpId = FieldHelper.GetLookupIdAsNumber(li, ConstantHelper.SPColumn.ExpensesList.ClaimEntitlementType),
                                        FixedQuantityValue = FieldHelper.GetFieldValueAsNumber(li, ConstantHelper.SPColumn.ExpensesList.FixedQuantity),
                                        FixedAmountValue = FieldHelper.GetFieldValueAsDecimal(li, ConstantHelper.SPColumn.ExpensesList.FixedAmount),
                                        Frequency = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.ExpensesList.Frequency),
                                        EffectiveStartDate = FieldHelper.GetFieldValueAsDateTime(li, ConstantHelper.SPColumn.ExpensesList.EffectiveStartDate, TimeZoneInfo.FindSystemTimeZoneById(appSettings.CentralTimeZone)),
                                        EffectiveEndDate = FieldHelper.GetFieldValueAsDateTime(li, ConstantHelper.SPColumn.ExpensesList.EffectiveEndDate, TimeZoneInfo.FindSystemTimeZoneById(appSettings.CentralTimeZone)),
                                        ClaimLimit = FieldHelper.GetFieldValueAsDecimal(li, ConstantHelper.SPColumn.ExpensesList.ClaimLimit),
                                        ClaimItemApprover = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(approver1, clientContext)
                                    };

                                    item.FixedQuantity = item.FixedQuantityValue.HasValue;
                                    item.FixedAmount = item.FixedAmountValue.HasValue;

                                    maintenanceList.Add(item);
                                }
                            }

                            vm.MainClaimHeaderVM.ExpensesMaintenanceList = maintenanceList;
                            vm.MainClaimHeaderVM.ExpensesList = ProjectHelper.GetExpensesAsDDLItems(expensesList, ConstantHelper.SPColumn.ExpensesList.Title, ConstantHelper.SPColumn.ExpensesList.ClaimCategory);
                            vm.MainClaimHeaderVM.ExpensesSubtypeList = ProjectHelper.GetExpensesSubtypeAsDDLItems(expensesList, ConstantHelper.SPColumn.ExpensesList.ExpensesSubType, ConstantHelper.SPColumn.ExpensesList.Title);
                            #endregion
                        }

                        if (model.ProcessID.HasValue)
                        {
                            int processId = model.ProcessID.Value;
                            vm.CurrentStage = wfBL.GetCurrentStepName(processId);
                            vm.HasSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.ErrorMessage = "Error while initializing approval form.";
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("ApprovalFormBL - InitApprovalForm Error: " + ex.ToString());
            }
            return vm;
        }

        public async Task<RequestFormVisibilitySettings> SetVisibilitySettings(string currentStage)
        {
            RequestFormVisibilitySettings vm = new();

            if (currentStage == null) return vm;

            if (currentStage == ConstantHelper.WorkflowStatus.PendingReportingManager1Approval
                || currentStage == ConstantHelper.WorkflowStatus.PendingReportingManager2Approval
                || currentStage == ConstantHelper.WorkflowStatus.PendingReportingManager3Approval)
            {
                vm.ShowRequireAmendment = true;
                vm.ShowApprove = true;
                vm.ShowReject = true;
                vm.ShowClose = true;
                vm.EnableRemarks = true;
            }
            else if (currentStage == ConstantHelper.WorkflowStatus.PendingClaimItemApproval)
            {
                vm.ShowApprove = true;
                vm.ShowReject = true;
                vm.ShowClose = true;
                vm.EnableRemarks = true;
            }
            else if (currentStage == ConstantHelper.WorkflowStatus.PendingOriginatorResubmission)
            {
                vm.ShowSave = true;
                vm.ShowSubmit = true;
                vm.ShowClose = true;
            }
            else if (currentStage == ConstantHelper.WorkflowStatus.PendingFinalApproval)
            {
                vm.ShowApprove = true;
                vm.ShowReject = true;
                vm.ShowClose = true;
            }
            else
            {
                vm.ShowClose = true;
            }

            return vm;
        }
        #endregion

        #region Expense Modal Info
        public async Task<PartialModelExpenseModal> GetExpenseModalInfo(SubClaimDetails model, string spHostUrl, string accessToken)
        {
            PartialModelExpenseModal vm = new PartialModelExpenseModal();
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    string expenseTitle = model.Expenses;

                    if (string.IsNullOrEmpty(expenseTitle))
                        throw new Exception("Expense is empty for this claim details.");

                    string[] viewFields = { ConstantHelper.SPColumn.ExpensesList.Frequency, ConstantHelper.SPColumn.ExpensesList.ClaimLimit };

                    string queryCondition = GeneralQueryHelper.ConcatCriteria(null, ConstantHelper.SPColumn.ExpensesList.Title, ConstantHelper.SharePoint.ColumnType.Text, expenseTitle, ConstantHelper.SharePoint.OperatorType.Eq, true);

                    ListItem li = GeneralQueryHelper.GetSPItem(clientContext, ConstantHelper.SPList.Expenses, queryCondition, viewFields);

                    vm.Frequency = FieldHelper.GetFieldValueAsString(li, ConstantHelper.SPColumn.ExpensesList.Frequency);
                    vm.ClaimLimit = FieldHelper.GetFieldValueAsDecimal(li, ConstantHelper.SPColumn.ExpensesList.ClaimLimit);
                    vm.Description = model.Description;
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("ApprovalFormBL - GetExpenseModalInfo Error: " + ex.ToString());
            }
            return vm;
        }
        #endregion

        #region Require Amendment
        public async Task<ApprovalFormVM> RequireAmendmentRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            bool isTaskNotStarted = false;
            int processID = vm.ProcessID.GetValueOrDefault();
            int taskID = vm.TaskID.GetValueOrDefault();
            bool ActionSuccess = false;
            string remarks = vm.MainClaimHeaderVM.GeneralRemarks;

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);

                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    bool isTaskActive = wfBL.CheckIsMyTask(taskID, actioner);
                    isTaskNotStarted = WorkflowBL.CheckIsTaskNotStarted(taskID.ToString(), isTaskActive);

                    if (isTaskActive && isTaskNotStarted)
                    {
                        await _claimDA.UpdateApprovalRequestAsync(vm);

                        if (processID > 0)
                        {
                            ProcessKeywords keywords = wfBL.GetKeywords(processID);
                            keywords.AddKeywordValue(WorkflowKeywords.Common.Remarks, remarks);
                            wfBL.UpdateKeywords(processID, keywords);

                            // Assign back to originator
                            string originatorLogin = keywords.GetKeywordValue(WorkflowKeywords.Common.OriginatorSubmittedByLogin);
                            string originatorName = keywords.GetKeywordValue(WorkflowKeywords.Common.OriginatorSubmittedByName);
                            string originatorEmail = keywords.GetKeywordValue(WorkflowKeywords.Common.OriginatorSubmittedByEmail);

                            if (!string.IsNullOrEmpty(originatorLogin) && !string.IsNullOrEmpty(originatorName) && !string.IsNullOrEmpty(originatorEmail))
                            {
                                var originator = new Actioner(originatorLogin, originatorName, originatorEmail);
                                string resubmissionStage = WorkflowStepName.EClaimWorkflow.PendingOriginatorResubmission;

                                MHA.Framework.Core.Workflow.DL.InstanceDL instanceDL = new MHA.Framework.Core.Workflow.DL.InstanceDL(WFConnString);
                                StepInstance stepInstance = instanceDL.GetStepInstanceByStepName(processID, resubmissionStage);

                                wfBL.UpdateActionersForStep(processID, stepInstance.InternalStepName, new List<Actioner> { originator }, true, true, actioner);

                                // Update due date
                                string workflowCycleDueDays = keywords.GetKeywordValue(WorkflowKeywords.Common.WorkflowCycleDueDays);
                                int dueDay = WorkflowBL.GetNewDueDay(resubmissionStage, workflowCycleDueDays);
                                wfBL.UpdateStepTaskDueDate(processID, resubmissionStage, dueDay);
                            }
                        }

                        ActionSuccess = WorkflowBL.CompleteAWorkflowStep(
                            processID.ToString(),
                            taskID.ToString(),
                            remarks,
                            WorkflowActionName.RequireAmendment,
                            spHostUrl,
                            accessToken
                        );

                        vm.HasSuccess = ActionSuccess;
                    }
                    else if (!isTaskActive)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ErrorMessage.NonActiveTask;
                    }
                    else if (!isTaskNotStarted)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ErrorMessage.TaskInProgressErrorMsg;
                    }
                    else
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ErrorMessage.NonActiveTask;
                    }
                }
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.ErrorMessage = ErrorMessage.UnexpectedErrorOccur;
            }
            finally
            {
                WorkflowBL.CloseRunningTask(taskID.ToString(), isTaskNotStarted);
            }

            return vm;
        }
        #endregion

        #region Approve
        public async Task<ApprovalFormVM> ApproveRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            bool isTaskNotStarted = false;
            int processID = vm.ProcessID.GetValueOrDefault();
            int taskID = vm.TaskID.GetValueOrDefault();
            bool ActionSuccess = false;
            string remarks = vm.MainClaimHeaderVM.GeneralRemarks;

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                    vm.ModifiedBy = currentUser.Title;
                    vm.ModifiedByLogin = currentUser.LoginName;
                    vm.ModifiedByDate = DateTimeHelper.GetCurrentDateTime();

                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);

                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    bool isTaskActive = wfBL.CheckIsMyTask(taskID, actioner);
                    isTaskNotStarted = WorkflowBL.CheckIsTaskNotStarted(taskID.ToString(), isTaskActive);

                    if (isTaskActive && isTaskNotStarted)
                    {
                        await _claimDA.UpdateApprovalRequestAsync(vm);

                        if (processID > 0)
                        {
                            ProcessKeywords keywords = wfBL.GetKeywords(processID);
                            keywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.Remarks, remarks);
                            wfBL.UpdateKeywords(processID, keywords);
                        }

                        ActionSuccess = WorkflowBL.CompleteAWorkflowStep(
                            processID.ToString(),
                            taskID.ToString(),
                            remarks,
                            ConstantHelper.WorkflowActionName.Approve,
                            spHostUrl,
                            accessToken);

                        vm.HasSuccess = ActionSuccess;
                    }
                    else if (!isTaskActive)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.NonActiveTask;
                    }
                    else if (!isTaskNotStarted)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.TaskInProgressErrorMsg;
                    }
                    else
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.NonActiveTask;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                WorkflowBL.CloseRunningTask(taskID.ToString(), isTaskNotStarted);
            }

            return vm;
        }
        #endregion

        #region Reject
        public async Task<ApprovalFormVM> RejectRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            bool isTaskNotStarted = false;
            int processID = vm.ProcessID.GetValueOrDefault();
            int taskID = vm.TaskID.GetValueOrDefault();
            bool ActionSuccess = false;
            string rejectRemarks = vm.MainClaimHeaderVM.GeneralRemarks;

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);

                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    bool isTaskActive = wfBL.CheckIsMyTask(taskID, actioner);
                    isTaskNotStarted = WorkflowBL.CheckIsTaskNotStarted(taskID.ToString(), isTaskActive);

                    if (isTaskActive && isTaskNotStarted)
                    {
                        await _claimDA.UpdateApprovalRequestAsync(vm);

                        if (processID > 0)
                        {
                            ProcessKeywords keywords = wfBL.GetKeywords(processID);
                            rejectRemarks = string.Format(ConstantHelper.InfoMessage.RejectRemarks, rejectRemarks);
                            keywords.AddKeywordValue(ConstantHelper.WorkflowKeywords.Common.Remarks, rejectRemarks);
                            wfBL.UpdateKeywords(processID, keywords);
                        }

                        ActionSuccess = WorkflowBL.CompleteAWorkflowStep(
                            processID.ToString(),
                            taskID.ToString(),
                            rejectRemarks,
                            ConstantHelper.WorkflowActionName.Reject,
                            spHostUrl,
                            accessToken);

                        vm.HasSuccess = ActionSuccess;
                    }
                    else if (!isTaskActive)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.NonActiveTask;
                    }
                    else if (!isTaskNotStarted)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.TaskInProgressErrorMsg;
                    }
                    else
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.NonActiveTask;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                WorkflowBL.CloseRunningTask(taskID.ToString(), isTaskNotStarted);
            }

            return vm;
        }
        #endregion

        #region Save
        public async Task<ApprovalFormVM> SaveRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            int processID = vm.ProcessID.GetValueOrDefault();
            int taskID = vm.TaskID.GetValueOrDefault();

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                    vm.ModifiedBy = currentUser.Title;
                    vm.ModifiedByLogin = currentUser.LoginName;
                    vm.ModifiedByDate = DateTimeHelper.GetCurrentDateTime();

                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);
                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    bool isTaskActive = wfBL.CheckIsMyTask(taskID, actioner);

                    if (isTaskActive)
                    {
                        await _claimDA.UpdateApprovalRequestAsync(vm);

                        vm.HasSuccess = true;
                    }
                    else
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ConstantHelper.ErrorMessage.NonActiveTask;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return vm;
        }
        #endregion

        #region Resubmit
        public async Task<ApprovalFormVM> ReSubmitClaimRequest(ApprovalFormVM vm, string spHostURL, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            bool isTaskNotStarted = false;
            int processID = vm.ProcessID.GetValueOrDefault();
            int taskID = vm.TaskID.GetValueOrDefault();
            bool ActionSuccess = false;
            string remarks = vm.MainClaimHeaderVM.GeneralRemarks;

            try
            {
                TokenHelper.CheckValidAccessToken(accessToken, spHostURL);
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);
                    vm.ModifiedBy = currentUser.Title;
                    vm.ModifiedByLogin = currentUser.LoginName;
                    vm.ModifiedByDate = DateTimeHelper.GetCurrentDateTime();

                    Actioner actioner = new Actioner(currentUser.LoginName, currentUser.Title, currentUser.Email);

                    string WFConnString = ConnectionStringHelper.GetGenericWFConnString();
                    MHA.Framework.Core.Workflow.BL.WorkflowBL wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(WFConnString);
                    bool isTaskActive = wfBL.CheckIsMyTask(taskID, actioner);
                    isTaskNotStarted = WorkflowBL.CheckIsTaskNotStarted(taskID.ToString(), isTaskActive);

                    if (isTaskActive && isTaskNotStarted)
                    {
                        //Save data into db
                        await _claimDA.UpdateApprovalRequestAsync(vm);

                        if (processID > 0)
                        {
                            ProcessKeywords keywords = wfBL.GetKeywords(processID);
                            keywords.AddKeywordValue(WorkflowKeywords.Common.Remarks, remarks);
                            keywords.AddKeywordValue(WorkflowKeywords.Common.EmployeeName, vm.MainClaimHeaderVM.RequesterName);
                            keywords.AddKeywordValue(WorkflowKeywords.Common.EmployeeId, vm.MainClaimHeaderVM.RequesterEmployeeID);
                            wfBL.UpdateKeywords(processID, keywords);
                        }

                        ActionSuccess = WorkflowBL.CompleteAWorkflowStep(
                            processID.ToString(),
                            taskID.ToString(),
                            remarks,
                            WorkflowActionName.Resubmit,
                            spHostURL,
                            accessToken);

                        vm.HasSuccess = ActionSuccess;
                    }
                    else if (!isTaskActive)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ErrorMessage.NonActiveTask;
                    }
                    else if (!isTaskNotStarted)
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ErrorMessage.TaskInProgressErrorMsg;
                    }
                    else
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = ErrorMessage.NonActiveTask;
                    }
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage("ApprovalFormBL - ResubmitEClaimRequest Error: " + ex.ToString());

                vm.HasError = true;
                vm.ErrorMessage = ErrorMessage.SubmitError;
            }
            finally
            {
                WorkflowBL.CloseRunningTask(taskID.ToString(), isTaskNotStarted);
            }

            return vm;
        }

        #endregion
    }
}
