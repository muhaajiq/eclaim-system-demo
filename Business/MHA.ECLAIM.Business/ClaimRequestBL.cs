using MHA.Framework.Core.SP;
using MHA.ECLAIM.Data;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.RunningNumber;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Entities.ViewModel.Workflow;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.Publishing;
using System.Data;
using System.Linq;
using FieldHelper = MHA.Framework.Core.SP.FieldHelper;
using Folder = Microsoft.SharePoint.Client.Folder;
using LogHelper = MHA.ECLAIM.Framework.Helpers.LogHelper;

namespace MHA.ECLAIM.Business
{
    public class ClaimRequestBL
    {
        private static readonly JSONAppSettings appSettings;
        private readonly ClaimRequestDA _da;

        static ClaimRequestBL()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public ClaimRequestBL()
        {
            _da = new ClaimRequestDA();
        }

        #region Init

        public async Task<MainClaimHeaderVM> InitClaim(MainClaimHeaderVM vm, string spHostURL, string accessToken)
        {
            try
            {
                TokenHelper.CheckValidAccessToken(accessToken, spHostURL);
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(ctx);
                    bool isAuthorized = SharePointHelper.IsUserInGroup(currentUser, ConstantHelper.SPSecurityGroup.RWCClaimsMembers);

                    //TODO: Check if my task
                    if (isAuthorized)
                    {
                        if (vm.ID > 0)
                        {
                            vm = await InitExistingClaimRequestForm(vm, ctx);
                        }
                        else if (vm.ID == null)
                        {
                            vm = await InitNewClaimRequestForm(vm, ctx);
                        }
                    }
                    else
                    {
                        vm.HasError = true;
                        vm.ErrorMessage = string.Format(ConstantHelper.ErrorMessage.NoAuthorized, ConstantHelper.PermissionConfigFunction.Request.CreateNewRequest);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[BL] ClaimRequestBL.InitClaim Error: {ex}");
                throw;
            }

            return vm;
        }

        public async Task<MainClaimHeaderVM> InitNewClaimRequestForm(MainClaimHeaderVM masterClaims, ClientContext ctx)
        {
            masterClaims.ClaimStatus = ConstantHelper.RequestForm.WorkflowNewRequest.WorkflowStatusEmpty;
            masterClaims.ReferenceNo = ConstantHelper.RequestForm.WorkflowNewRequest.ReferenceNoEmpty;
            User currentUser = SharePointHelper.GetCurrentUser(ctx);

            // Fields to retrieve
            String[] ViewFields = {
                ConstantHelper.SPColumn.EmployeeList.EmployeePerson,
                ConstantHelper.SPColumn.EmployeeList.CompanyCode,
                ConstantHelper.SPColumn.EmployeeList.CompanyName,
                ConstantHelper.SPColumn.EmployeeList.EmployeeID,
                ConstantHelper.SPColumn.EmployeeList.Department,
                ConstantHelper.SPColumn.EmployeeList.CostCenter,
                ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1,
                ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1StartDate,
                ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1EndDate,
                ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2,
                ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2StartDate,
                ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2EndDate

            };

            string queryCondition = $@"
            <Eq>
                <FieldRef Name='{ConstantHelper.SPColumn.EmployeeList.EmployeePerson}' LookupId='TRUE'/>
                <Value Type='Integer'>{currentUser.Id}</Value>
            </Eq>";

            ListItemCollection employees = GeneralQueryHelper.GetSPItems(ctx, ConstantHelper.SPList.Employee, queryCondition, ViewFields);
            ListItem? employeeListItem = employees?.FirstOrDefault();

            if (employeeListItem != null)
            {
                masterClaims.RequesterCompanyCode = FieldHelper.GetLookupValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.CompanyCode);
                masterClaims.RequesterCompanyName = FieldHelper.GetLookupValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.CompanyName);
                masterClaims.RequesterEmployeeID = FieldHelper.GetFieldValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.EmployeeID);

                var fieldUserValue = FieldHelper.GetFieldUserValue(employeeListItem, ConstantHelper.SPColumn.EmployeeList.EmployeePerson);
                masterClaims.RequesterName = fieldUserValue?.LookupValue ?? string.Empty;
                masterClaims.RequesterLogin = currentUser.LoginName ?? string.Empty;

                masterClaims.RequestDate = DateTimeHelper.GetCurrentDateTime();

                masterClaims.RequesterDepartment = FieldHelper.GetLookupValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.Department);
                masterClaims.RequesterCostCenter = FieldHelper.GetLookupValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.CostCenter);

                masterClaims.ClaimEntitlementType1 = FieldHelper.GetLookupValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1);
                masterClaims.ClaimEntitlementType1SpId = FieldHelper.GetLookupIdAsNumber(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1);
                DateTime CEType1StartDate = FieldHelper.GetFieldValueAsDateTime(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1StartDate);
                masterClaims.ClaimEntitlementType1StartDate = CEType1StartDate != DateTime.MinValue ? DateTimeHelper.ConvertToLocalDateTime(CEType1StartDate) : null;
                DateTime CEType1EndDate = FieldHelper.GetFieldValueAsDateTime(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement1EndDate);
                masterClaims.ClaimEntitlementType1EndDate = CEType1EndDate != DateTime.MinValue ? DateTimeHelper.ConvertToLocalDateTime(CEType1EndDate) : null;

                masterClaims.ClaimEntitlementType2 = FieldHelper.GetLookupValueAsString(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2);
                masterClaims.ClaimEntitlementType2SpId = FieldHelper.GetLookupIdAsNumber(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2);
                DateTime CEType2StartDate = FieldHelper.GetFieldValueAsDateTime(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2StartDate);
                masterClaims.ClaimEntitlementType2StartDate = CEType2StartDate != DateTime.MinValue ? DateTimeHelper.ConvertToLocalDateTime(CEType2StartDate) : null;
                DateTime CEType2EndDate = FieldHelper.GetFieldValueAsDateTime(employeeListItem, ConstantHelper.SPColumn.EmployeeList.ClaimEntitlement2EndDate);
                masterClaims.ClaimEntitlementType2EndDate = CEType2EndDate != DateTime.MinValue ? DateTimeHelper.ConvertToLocalDateTime(CEType2EndDate) : null;

                masterClaims.CurrentClaimEntitlementTypeSpId = GetCurrentEntitlement(masterClaims);

                #region Drop down list from maintenance list
                ListItemCollection claimCategoryList = GeneralQueryHelper.GetSPItems(ctx, ConstantHelper.SPList.ClaimCategory, string.Empty, null);
                masterClaims.ClaimCategories = ProjectHelper.GetListItemsAsDDLItems(claimCategoryList, ConstantHelper.SPColumn.ClaimCategoryList.Title, ConstantHelper.SPColumn.ClaimCategoryList.Status);
                ListItemCollection currencyListItem = GeneralQueryHelper.GetSPItems(ctx, ConstantHelper.SPList.Currency, string.Empty, null);
                masterClaims.CurrencyList = ProjectHelper.GetListItemsAsDDLItems(currencyListItem, ConstantHelper.SPColumn.CurrencyList.ISOCode, ConstantHelper.SPColumn.CurrencyList.Status);
                #endregion

                #region Get Travel Request List for dropdown
                TravelRequestDA daTravel = new TravelRequestDA();
                var travelRequestList = await daTravel.GetByEmployeeLogin(masterClaims.RequesterLogin);
                if (travelRequestList != null && travelRequestList.Any())
                {
                    masterClaims.TravelRequestRefNoList = travelRequestList
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
                    ctx,
                    ConstantHelper.SPList.CurrencyExchangeRate,
                    queryConditionCurExc,
                    null
                );

                if (curExchangeRateItems != null && curExchangeRateItems.Any())
                {
                    masterClaims.CurrencyExchangeRates = curExchangeRateItems.Select(item => new ViewModelCurrencyExchangeRate
                    {
                        Year = FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.CurrencyExchangeRate.Year),
                        Month = FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.CurrencyExchangeRate.Month),
                        BaseCurrency = FieldHelper.GetLookupValueAsString(item, ConstantHelper.SPColumn.CurrencyExchangeRate.BaseCurrency),
                        TargetCurrency = FieldHelper.GetLookupValueAsString(item, ConstantHelper.SPColumn.CurrencyExchangeRate.TargetCurrency),
                        Rate = Convert.ToDecimal(FieldHelper.GetFieldValueAsDouble(item, ConstantHelper.SPColumn.CurrencyExchangeRate.Rate)),

                    }).ToList();
                }
                #endregion

                if (string.IsNullOrEmpty(masterClaims.TempFolderGuid))
                {
                    masterClaims.TempFolderGuid = Guid.NewGuid().ToString();
                }

                masterClaims.IsSuccessful = true;
            }
            else
            {
                masterClaims.ErrorMessage = "No employee found for the current user. Please create it in Maintenance List";
                masterClaims.HasError = true;
                masterClaims.IsSuccessful = false;
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - InitClaimsCreationForm: No employee found for the current user.");
            }

            return masterClaims;
        }

        public async Task<MainClaimHeaderVM> InitExistingClaimRequestForm(MainClaimHeaderVM vm, ClientContext ctx)
        {
            int mainClaimHeaderID = (int)vm.ID;
            ClaimRequestDA da = new ClaimRequestDA();

            #region Get Data using EF
            var headerEntity = await da.RetrieveMainClaimHeaderByIDAsync(mainClaimHeaderID);

            if (headerEntity == null)
                throw new Exception($"No Claim Header found with ID {mainClaimHeaderID}");

            vm = headerEntity;
            #endregion

            #region Drop down list from maintenance list
            ListItemCollection claimCategoryList = GeneralQueryHelper.GetSPItems(ctx, ConstantHelper.SPList.ClaimCategory, string.Empty, null);
            vm.ClaimCategories = ProjectHelper.GetListItemsAsDDLItems(claimCategoryList, ConstantHelper.SPColumn.ClaimCategoryList.Title, ConstantHelper.SPColumn.ClaimCategoryList.Status);
            ListItemCollection currencyListItem = GeneralQueryHelper.GetSPItems(ctx, ConstantHelper.SPList.Currency, string.Empty, null);
            vm.CurrencyList = ProjectHelper.GetListItemsAsDDLItems(currencyListItem, ConstantHelper.SPColumn.CurrencyList.ISOCode, ConstantHelper.SPColumn.CurrencyList.Status);
            #endregion

            #region Get Travel Request List for dropdown
            TravelRequestDA daTravel = new TravelRequestDA();
            var travelRequestList = await daTravel.GetByEmployeeLogin(vm.RequesterLogin);
            if (travelRequestList != null && travelRequestList.Any())
            {
                vm.TravelRequestRefNoList = travelRequestList
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
                ctx,
                ConstantHelper.SPList.CurrencyExchangeRate,
                queryConditionCurExc,
                null
            );

            if (curExchangeRateItems != null && curExchangeRateItems.Any())
            {
                vm.CurrencyExchangeRates = curExchangeRateItems.Select(item => new ViewModelCurrencyExchangeRate
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
            ListItemCollection expensesList = GeneralQueryHelper.GetSPItems(ctx, ConstantHelper.SPList.Expenses, string.Empty, null);

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
                        ClaimItemApprover = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(approver1, ctx)
                    };

                    item.FixedQuantity = item.FixedQuantityValue.HasValue;
                    item.FixedAmount = item.FixedAmountValue.HasValue;

                    maintenanceList.Add(item);
                }
            }

            vm.ExpensesMaintenanceList = maintenanceList;
            vm.ExpensesList = ProjectHelper.GetExpensesAsDDLItems(expensesList, ConstantHelper.SPColumn.ExpensesList.Title, ConstantHelper.SPColumn.ExpensesList.ClaimCategory);
            vm.ExpensesSubtypeList = ProjectHelper.GetExpensesSubtypeAsDDLItems(expensesList, ConstantHelper.SPColumn.ExpensesList.ExpensesSubType, ConstantHelper.SPColumn.ExpensesList.Title);
            #endregion

            vm.IsSuccessful = true;

            return vm;
        }

        public async Task<MainClaimHeaderVM> InitExpensesList(string spHostUrl, string accessToken)
        {
            MainClaimHeaderVM vm = new MainClaimHeaderVM();
            try
            {
                TokenHelper.CheckValidAccessToken(accessToken, spHostUrl);
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
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
                                EffectiveStartDate = FieldHelper.GetFieldValueAsDateTime(li, ConstantHelper.SPColumn.ExpensesList.EffectiveStartDate),
                                EffectiveEndDate = FieldHelper.GetFieldValueAsDateTime(li, ConstantHelper.SPColumn.ExpensesList.EffectiveEndDate),
                                ClaimLimit = FieldHelper.GetFieldValueAsDecimal(li, ConstantHelper.SPColumn.ExpensesList.ClaimLimit),
                                ClaimItemApprover = ConvertEntitiesHelper.ConvertFieldUserValueToPeoplePicker(approver1, clientContext)
                            };

                            item.FixedQuantity = item.FixedQuantityValue.HasValue;
                            item.FixedAmount = item.FixedAmountValue.HasValue && item.FixedAmountValue.Value > 0;

                            maintenanceList.Add(item);
                        }
                    }

                    vm.ExpensesMaintenanceList = maintenanceList;
                    vm.ExpensesList = ProjectHelper.GetExpensesAsDDLItems(expensesList, ConstantHelper.SPColumn.ExpensesList.Title, ConstantHelper.SPColumn.ExpensesList.ClaimCategory);
                    vm.ExpensesSubtypeList = ProjectHelper.GetExpensesSubtypeAsDDLItems(expensesList, ConstantHelper.SPColumn.ExpensesList.ExpensesSubType, ConstantHelper.SPColumn.ExpensesList.Title);
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage($"[BL] ClaimRequestBL.InitExpensesList Error: {ex}");
                throw;
            }

            return vm;
        }
        #endregion

        #region View Claim
        public async Task<ViewClaimVM> InitClaimDisplayForm(int requestId, string spHostUrl, string accessToken)
        {
            ViewClaimVM vm = new ViewClaimVM();
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);

                    vm.CurrentUser = currentUser.LoginName;

                    string query = GeneralQueryHelper.ConcatCriteria(null, ConstantHelper.SPColumn.DepartmentList.DepartmentApprover, "Integer", currentUser.Id.ToString(), "Contains", true);
                    ListItemCollection deptItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Department, query, [ConstantHelper.SPColumn.DepartmentList.Title]);

                    if (deptItems != null)
                    {
                        foreach (ListItem item in deptItems)
                        {
                            vm.AccessDepartments.Add(FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.DepartmentList.ID));
                        }
                    }

                    string[] groups = [ConstantHelper.SPSecurityGroup.SDCClaimsAdmin, ConstantHelper.SPSecurityGroup.RWCClaimsMembers, ConstantHelper.SPSecurityGroup.DepartmentAdmin, ConstantHelper.SPSecurityGroup.Employee];

                    foreach (string group in groups)
                    {
                        bool inGroup = false;

                        inGroup = SharePointHelper.IsUserInGroup(currentUser, group);

                        if (inGroup == true) vm.AccessGroups.Add(group);
                    }

                    if (vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.DepartmentAdmin))
                    {
                        string queryCondition = GeneralQueryHelper.ConcatCriteria(null, "Admin", "Integer", currentUser.Id.ToString(), "Contains", true);

                        ListItemCollection departmentItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Department, queryCondition, [ConstantHelper.SPColumn.DepartmentList.Title]);

                        if (departmentItems != null)
                        {
                            foreach (ListItem item in departmentItems)
                            {
                                int departmentId = FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.DepartmentList.ID);
                                if (!vm.AccessDepartments.Contains(departmentId))
                                    vm.AccessDepartments.Add(departmentId);
                            }
                        }
                    }

                    else if (vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.Employee))
                    {
                        //See own records only
                        vm.MemberLogin = vm.CurrentUser;
                    }

                    if (vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.SDCClaimsAdmin))
                    {
                        //Full Access
                        vm.AccessDepartments.Clear();
                        vm.MemberLogin = string.Empty;
                    }

                    var headerVm = await _da.RetrieveMainClaimHeaderByIDAsync(requestId);
                    if (headerVm == null)
                        throw new Exception($"No Eclaim Request Header found with ID {requestId}");

                    if (!vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.SDCClaimsAdmin))
                    {
                        if (vm.AccessDepartments.Count > 0)
                        {
                            // Check if user is the dept admin / dept approver to view this form
                            int departmentID = headerVm.DepartmentID.Value;

                            if (!vm.AccessDepartments.Contains(departmentID))
                            {
                                vm.HasError = true;
                                vm.ErrorMessage = string.Format(ConstantHelper.ErrorMessage.NotAuthorizedView, ConstantHelper.PermissionConfigFunction.Request.DisplayForm);
                                return vm;
                            }
                        }

                        if (vm.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.Employee))
                        {
                            if (!headerVm.CreatedByLogin.Equals(vm.MemberLogin))
                            {
                                vm.HasError = true;
                                vm.ErrorMessage = string.Format(ConstantHelper.ErrorMessage.NotAuthorizedView, ConstantHelper.PermissionConfigFunction.Request.DisplayForm);
                                return vm;
                            }
                        }
                    }

                    vm.MainClaimHeaderVM = headerVm;
                    vm.SubClaimDetailsVM = headerVm.SubClaimDetails;

                    vm.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.ErrorMessage = "Error while retrieving claim request details.";
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - InitClaimDisplayForm Error: " + ex.ToString());
                throw;
            }
            return vm;
        }
        #endregion

        #region Save Claims
        public async Task<MainClaimHeaderVM> SaveSubmitEclaimRequest(MainClaimHeaderVM vm, ClientContext ctx, bool isSubmit, string spHostUrl, string accessToken)
        {
            try
            {
                User user = SharePointHelper.GetCurrentUser(ctx);

                #region Modification Info
                vm.ModifiedBy = user.Title;
                vm.ModifiedByLogin = user.LoginName;
                vm.ModifiedDate = DateTimeHelper.GetCurrentDateTime();

                if (vm.SubClaimDetails != null && vm.SubClaimDetails.Any())
                {
                    foreach (var detail in vm.SubClaimDetails)
                    {
                        detail.ModifiedBy = user.Title;
                        detail.ModifiedByLogin = user.LoginName;
                        detail.ModifiedDate = DateTimeHelper.GetCurrentDateTime();
                    }
                }
                #endregion

                #region Department ID
                // Store snapshot of DepartmentId associated with the travel request
                int departmentID = -1;

                ListItem departmentItem = GeneralQueryHelper.GetSPItemByTitle(ctx, ConstantHelper.SPList.Department, vm.RequesterDepartment);

                departmentID = FieldHelper.GetFieldValueAsNumber(departmentItem, ConstantHelper.SPColumn.DepartmentList.ID);
                vm.DepartmentID = departmentID;
                #endregion

                if (vm.ID == 0 || vm.ID == null)
                {
                    vm.ClaimStatus = ConstantHelper.WorkflowStatus.DRAFT;
                    vm.ReferenceNo = ConstantHelper.RequestForm.WorkflowNewRequest.ReferenceNoDraft;

                    #region Creation Info
                    vm.CreatedBy = user.Title;
                    vm.CreatedByLogin = user.LoginName;
                    vm.CreatedDate = DateTimeHelper.GetCurrentDateTime();

                    if (vm.SubClaimDetails != null && vm.SubClaimDetails.Any())
                    {
                        foreach (var detail in vm.SubClaimDetails)
                        {
                            detail.CreatedBy = user.Title;
                            detail.CreatedByLogin = user.LoginName;
                            detail.CreatedDate = DateTimeHelper.GetCurrentDateTime();
                        }
                    }
                    #endregion

                    vm = await _da.CreateNewRequestAsync(vm);
                }
                else if (vm.ID > 0)
                {
                    if (isSubmit)
                    {
                        if(string.IsNullOrEmpty(vm.SubmittedBy))
                        {
                            #region Submission Info
                            vm.SubmittedBy = user.Title;
                            vm.SubmittedByLogin = user.LoginName;
                            vm.SubmittedDate = DateTimeHelper.GetCurrentDateTime();
                            #endregion
                        }
                    }

                    bool success = await _da.UpdateRequestAsync(vm, isSubmit);
                    vm.IsSuccessful = success;
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - SaveSubmitEclaimRequest Error: " + ex.ToString());
                throw;
            }
            return vm;
        }

        public async Task<MainClaimHeaderVM> SaveNewRequestForm(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            try
            {
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    vm = await SaveSubmitEclaimRequest(vm, ctx, false, spHostUrl, accessToken);
                }
            }
            catch (Exception ex)
            {
                vm.HasError = true;
                vm.ErrorMessage = ConstantHelper.ErrorMessage.SaveError;
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - SaveNewRequestForm Error: " + ex.ToString());
                throw;
            }
            return vm;
        }
        #endregion

        #region Submit Claims
        public async Task<MainClaimHeaderVM> SubmitClaimRequestForm(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            string errorMessage = string.Empty;

            try
            {
                TokenHelper.CheckValidAccessToken(accessToken, spHostUrl);
                using (ClientContext ctx = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    if (vm.ID <= 0 || vm.ID == null)
                    {
                        vm = await SaveSubmitEclaimRequest(vm, ctx, false, spHostUrl, accessToken);
                        vm = await InitExistingClaimRequestForm(vm, ctx);
                    }

                    // Generate ref no
                    int selectedNumber = -1;

                    vm.ReferenceNo = GenerateRequestRefNumber(spHostUrl, accessToken, ref selectedNumber);

                    if (!string.IsNullOrEmpty(vm.ReferenceNo) || vm.ReferenceNo != ConstantHelper.RequestForm.WorkflowNewRequest.ReferenceNoDraft)
                    {
                        User currentUser = SharePointHelper.GetCurrentUser(ctx);

                        if (vm.ID > 0 && (string.IsNullOrEmpty(vm.SubmittedBy) || string.IsNullOrEmpty(vm.SubmittedByLogin) || string.IsNullOrEmpty(vm.SubmittedByEmail)))
                        {
                            #region Submission Info
                            vm.SubmittedBy = currentUser.Title;
                            vm.SubmittedByLogin = currentUser.LoginName;
                            vm.SubmittedByEmail = currentUser.Email;
                            vm.SubmittedDate = DateTimeHelper.GetCurrentDateTime();
                            #endregion
                        }

                        PeoplePickerUser originator = new PeoplePickerUser();
                        originator.Name = currentUser.Title;
                        originator.Login = currentUser.LoginName;
                        originator.Email = currentUser.Email;

                        StartWorkflowObject startWFObject = new StartWorkflowObject();
                        startWFObject.Originator = originator;
                        int processId = -1;
                        errorMessage = WorkflowBL.StartWorkflow(startWFObject, vm, spHostUrl, ctx, accessToken, ref processId);

                        if (string.IsNullOrEmpty(errorMessage))
                        {
                            vm.ProcessID = processId;
                            string connString = ConnectionStringHelper.GetGenericWFConnString();
                            var wfBL = new MHA.Framework.Core.Workflow.BL.WorkflowBL(connString);
                            string currentStep = wfBL.GetCurrentStepName(vm.ProcessID);
                            vm.ClaimStatus = currentStep;

                            #region Rename GUID folder to ReferenceNo
                            string libraryName = ConstantHelper.SPList.Library.EclaimDocuments;
                            string parentFolderName = $"{vm.ClaimCategory}_{vm.RequesterEmployeeID}";
                            RenameGuidFolderToReferenceNo(ctx, libraryName, parentFolderName, vm.TempFolderGuid, vm.ReferenceNo);

                            foreach (var claimItem in vm.SubClaimDetails)
                            {
                                if (!string.IsNullOrWhiteSpace(claimItem.AttachmentPathUrl))
                                {
                                    claimItem.AttachmentPathUrl = claimItem.AttachmentPathUrl.Replace($"/{vm.TempFolderGuid}/", $"/{vm.ReferenceNo}/");
                                }
                            }
                            #endregion

                            //Save data into db
                            vm = await SaveSubmitEclaimRequest(vm, ctx, true, spHostUrl, accessToken);

                            vm.IsSuccessful = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(vm.ReferenceNo))
                            {
                                //Restore running number here
                                GenerateRequestRefNumber(spHostUrl, accessToken, ref selectedNumber, true);

                                vm.ReferenceNo = ConstantHelper.RequestForm.WorkflowNewRequest.ReferenceNoDraft;
                                vm.ClaimStatus = ConstantHelper.WorkflowStatus.DRAFT;
                                vm = await SaveSubmitEclaimRequest(vm, ctx, true, spHostUrl, accessToken);

                                vm.IsSuccessful = false;
                                vm.HasError = true;
                                vm.ErrorMessage = errorMessage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vm.IsSuccessful = false;
                vm.HasError = true;
                vm.ErrorMessage = ConstantHelper.ErrorMessage.SubmitError;

                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - SubmitClaimRequestForm Error: " + ex.ToString());
                throw;
            }
            return vm;
        }
        #endregion

        #region Generate Reference Number
        public string GenerateRequestRefNumber(string spHostUrl, string appAccessToken, ref int selectedNumber, bool isDowngrade = false)
        {
            string newRequestRefNo = string.Empty;

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, appAccessToken))
                {
                    string runningNumberTitle = ConstantHelper.RunningNumberFormatType.EclaimRequestReferenceNumber;

                    RunningNumberFormat rnfObj = new RunningNumberFormat();
                    string query = GeneralQueryHelper.ConcatCriteria(string.Empty, ConstantHelper.SPColumn.RunningNumberList.Title, "Text", runningNumberTitle, "Eq", false);
                    ListItemCollection runningNumberItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.RunningNumberFormat, query, null);
                    if (runningNumberItems != null && runningNumberItems.Count > 0)
                    {
                        string year = DateTimeHelper.GetCurrentDateTime().Year.ToString();

                        rnfObj = ConvertEntitiesHelper.ConvertRunningNumberFormatObject(runningNumberItems[0]);
                        string prefix = rnfObj.Prefix;
                        prefix = prefix.Replace(ConstantHelper.RunningNumberFormatInstance.RequestYear, year);

                        if (isDowngrade)
                        {
                            RunningNumberHelper.RestoreRunningNumber(runningNumberTitle, prefix, selectedNumber, clientContext);
                        }
                        else
                        {
                            RunningNumber rnObj = new RunningNumber();
                            rnObj = RunningNumberHelper.CreateRunningNumber(runningNumberTitle, rnfObj.Format, prefix, clientContext);

                            string format = rnObj.Format;
                            format = format.Replace(ConstantHelper.RunningNumberFormatInstance.RequestYear, year);

                            newRequestRefNo = ProjectHelper.ReplaceKeywordWithValue(ConstantHelper.RunningNumberFormatInstance.RunningNo, format, rnObj.Number);
                            selectedNumber = rnObj.Number;
                        }
                    }
                    else
                    {
                        throw new EclaimActionException($"Running number format {runningNumberTitle} not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] RequestBL - GenerateRequestRefNumber Error:" + ex.ToString());

                throw new EclaimActionException("Unable to generate reference number.");
            }
            return newRequestRefNo;
        }
        #endregion

        #region General Function & Helpers
        private int? GetCurrentEntitlement(MainClaimHeaderVM vm)
        {
            // Priority: whichever has no end date
            if (!string.IsNullOrEmpty(vm.ClaimEntitlementType1) && vm.ClaimEntitlementType1EndDate == DateTime.MinValue)
                return vm.ClaimEntitlementType1SpId;

            if (!string.IsNullOrEmpty(vm.ClaimEntitlementType2) && vm.ClaimEntitlementType2EndDate == DateTime.MinValue)
                return vm.ClaimEntitlementType2SpId;

            // If both ended, take the most recent
            if (!string.IsNullOrEmpty(vm.ClaimEntitlementType1) && !string.IsNullOrEmpty(vm.ClaimEntitlementType2))
            {
                return vm.ClaimEntitlementType1EndDate >= vm.ClaimEntitlementType2EndDate
                    ? vm.ClaimEntitlementType1SpId
                    : vm.ClaimEntitlementType2SpId;
            }

            // Fallbacks
            if (!string.IsNullOrEmpty(vm.ClaimEntitlementType1))
                return vm.ClaimEntitlementType1SpId;

            if (!string.IsNullOrEmpty(vm.ClaimEntitlementType2))
                return vm.ClaimEntitlementType2SpId;

            return null;
        }

        private void RenameGuidFolderToReferenceNo(ClientContext ctx, string libraryName, string parentFolderName, string guidFolderName, string referenceNo)
        {
            var parentFolder = ctx.Web.GetFolderByServerRelativeUrl($"{libraryName}/{parentFolderName}");
            var guidFolder = parentFolder.Folders.GetByUrl(guidFolderName);

            ctx.Load(guidFolder, f => f.ServerRelativeUrl);
            ctx.ExecuteQuery();

            // Update folder name
            guidFolder.ListItemAllFields["FileLeafRef"] = referenceNo;
            guidFolder.ListItemAllFields.Update();
            ctx.ExecuteQuery();
        }
        #endregion

        #region Exception
        public class DataNotFoundException : Exception
        {
            public DataNotFoundException(string message) : base(message) { }
        }

        public class EclaimActionException : Exception
        {
            public EclaimActionException(string message) : base(message) { }
        }
        #endregion

        #region Claim Details
        public async Task<MainClaimHeaderVM> SaveClaimDetails(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    string libraryName = ConstantHelper.SPList.Library.EclaimDocuments;
                    string applicationName = ProjectHelper.GetRelativeUrlFromUrl(spHostUrl);
                    string spHostDomain = ProjectHelper.GetSPHostURLDomain(spHostUrl, applicationName);

                    string parentFolderName = string.Format("{0}_{1}", vm.ClaimCategory, vm.RequesterEmployeeID);
                    Folder parentFolder = ProjectHelper.EnsureFolder(clientContext, libraryName, parentFolderName);

                    string subFolderName;

                    // Use ReferenceNo for resubmission
                    if (!string.IsNullOrEmpty(vm.ReferenceNo)
                        && vm.ReferenceNo != ConstantHelper.RequestForm.WorkflowNewRequest.ReferenceNoDraft
                        && vm.ReferenceNo != ConstantHelper.RequestForm.WorkflowNewRequest.ReferenceNoEmpty)
                    {
                        subFolderName = vm.ReferenceNo;
                    }
                    else
                    {
                        // Original submission — use temp GUID
                        if (string.IsNullOrEmpty(vm.TempFolderGuid))
                            vm.TempFolderGuid = Guid.NewGuid().ToString();

                        subFolderName = vm.TempFolderGuid;
                    }

                    Folder targetFolder = ProjectHelper.EnsureFolder(clientContext, libraryName, $"{parentFolderName}/{subFolderName}");

                    foreach (var claimItem in vm.SubClaimDetails)
                    {
                        if (claimItem.AttachmentFileBytes != null && !string.IsNullOrWhiteSpace(claimItem.AttachmentFileName))
                        {
                            string fileName = claimItem.AttachmentFileName;

                            FileCreationInformation newFile = new FileCreationInformation
                            {
                                Content = claimItem.AttachmentFileBytes,
                                Url = fileName,
                                Overwrite = true
                            };

                            Microsoft.SharePoint.Client.File uploadFile = targetFolder.Files.Add(newFile);
                            clientContext.Load(uploadFile, f => f.ListItemAllFields, f => f.ServerRelativeUrl);
                            clientContext.ExecuteQueryWithIncrementalRetry();

                            ListItem fileItem = uploadFile.ListItemAllFields;
                            fileItem.Update();
                            clientContext.ExecuteQueryWithIncrementalRetry();

                            claimItem.AttachmentPathUrl = string.Format(
                                ConstantHelper.URLTemplate.AttachmentPathUrlTemplate,
                                spHostDomain,
                                uploadFile.ServerRelativeUrl);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - SaveClaimDetails Error: " + ex);
                throw;
            }

            return vm;
        }

        public async Task<bool> DeleteClaimDetails(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            if (vm.CurrentClaimDetails == null)
                return false;

            try
            {
                if (!string.IsNullOrWhiteSpace(vm.CurrentClaimDetails.AttachmentPathUrl))
                {
                    using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                    {
                        Uri fileUri = new Uri(vm.CurrentClaimDetails.AttachmentPathUrl);
                        string serverRelativeUrl = fileUri.AbsolutePath;

                        Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
                        file.DeleteObject();
                        await clientContext.ExecuteQueryAsync();

                        // If GUID folder is empty, delete the folder
                        if (!string.IsNullOrEmpty(vm.TempFolderGuid))
                        {
                            string libraryName = ConstantHelper.SPList.Library.EclaimDocuments;
                            string parentFolderName = $"{vm.ClaimCategory}_{vm.RequesterEmployeeID}";
                            string guidFolderUrl = $"{libraryName}/{parentFolderName}/{vm.TempFolderGuid}";

                            Folder guidFolder = clientContext.Web.GetFolderByServerRelativeUrl(guidFolderUrl);
                            clientContext.Load(guidFolder, f => f.Files, f => f.Folders);
                            await clientContext.ExecuteQueryAsync();

                            if (!guidFolder.Files.Any() && !guidFolder.Folders.Any())
                            {
                                guidFolder.DeleteObject();
                                await clientContext.ExecuteQueryAsync();
                            }
                        }
                    }
                }

                if (vm.ID == 0)
                {
                    vm.CurrentClaimDetails.AttachmentPathUrl = null;
                    vm.CurrentClaimDetails.AttachmentFileName = null;
                    vm.CurrentClaimDetails.AttachmentFileBytes = null;
                    return true;
                }

                var success = await _da.DeleteClaimDetailsAsync(vm.ID.Value, vm.CurrentClaimDetails.ID);

                if (success)
                {
                    vm.CurrentClaimDetails.AttachmentPathUrl = null;
                    vm.CurrentClaimDetails.AttachmentFileName = null;
                    vm.CurrentClaimDetails.AttachmentFileBytes = null;
                }

                return success;
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - DeleteClaimDetails Error: " + ex);
                throw;
            }
        }

        public async Task<bool> DeleteClaimAttachment(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            if (vm.CurrentClaimDetails == null || string.IsNullOrWhiteSpace(vm.CurrentClaimDetails.AttachmentPathUrl))
                return false;

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    Uri fileUri = new Uri(vm.CurrentClaimDetails.AttachmentPathUrl);
                    string serverRelativeUrl = fileUri.AbsolutePath;

                    Microsoft.SharePoint.Client.File file = clientContext.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
                    file.DeleteObject();
                    await clientContext.ExecuteQueryAsync();

                    // Clear model after deletion
                    vm.CurrentClaimDetails.AttachmentPathUrl = null;
                    vm.CurrentClaimDetails.AttachmentFileName = null;
                    vm.CurrentClaimDetails.AttachmentFileBytes = null;

                    // If GUID folder is empty, delete the folder
                    if (!string.IsNullOrEmpty(vm.TempFolderGuid))
                    {
                        string libraryName = ConstantHelper.SPList.Library.EclaimDocuments;
                        string parentFolderName = $"{vm.ClaimCategory}_{vm.RequesterEmployeeID}";
                        string guidFolderUrl = $"{libraryName}/{parentFolderName}/{vm.TempFolderGuid}";

                        Folder guidFolder = clientContext.Web.GetFolderByServerRelativeUrl(guidFolderUrl);
                        clientContext.Load(guidFolder, f => f.Files, f => f.Folders);
                        await clientContext.ExecuteQueryAsync();

                        if (!guidFolder.Files.Any() && !guidFolder.Folders.Any())
                        {
                            guidFolder.DeleteObject();
                            await clientContext.ExecuteQueryAsync();
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[BL] ClaimRequestBL - DeleteClaimAttachment Error: " + ex);
                throw;
            }
        }
        #endregion
    }
}
