using MHA.Framework.Core.SP;
using MHA.ECLAIM.Data;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Search;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.TRAVELREQUEST.Framework.Helpers;
using Microsoft.SharePoint.Client;

namespace MHA.ECLAIM.Business
{
    public class SearchBL
    {
        private readonly ClaimRequestDA _da;

        public SearchBL()
        {
            _da = new ClaimRequestDA();
        }

        public async Task<SearchClaimVM> GetPagedClaims(MainClaimHeaderSearchModel search, string? sortCol, string? sortDirection, int skip, int take, string spHostURL, string accessToken)
        {
            SearchClaimVM vm = new SearchClaimVM();

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    User currentUser = SharePointHelper.GetCurrentUser(clientContext);

                    search.CurrentUser = currentUser.Title;
                    search.CurrentUserLogin = currentUser.LoginName;
                    vm.CurrentUserLogin = currentUser.LoginName;

                    // Department Approver logic:
                    string query = GeneralQueryHelper.ConcatCriteria(null, ConstantHelper.SPColumn.DepartmentList.DepartmentApprover, "Integer", currentUser.Id.ToString(), "Contains", true);
                    ListItemCollection deptItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Department, query, [ConstantHelper.SPColumn.DepartmentList.Title]);

                    if (deptItems != null)
                    {
                        foreach (ListItem item in deptItems)
                        {
                            search.AccessDepartments.Add(FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.DepartmentList.ID));
                        }
                    }

                    string[] groups = [ConstantHelper.SPSecurityGroup.SDCClaimsAdmin, ConstantHelper.SPSecurityGroup.RWCClaimsMembers, ConstantHelper.SPSecurityGroup.DepartmentAdmin, ConstantHelper.SPSecurityGroup.Employee];

                    //Assign matched group
                    foreach (string group in groups)
                    {
                        bool inGroup = false;

                        inGroup = SharePointHelper.IsUserInGroup(currentUser, group);

                        if (inGroup == true) search.AccessGroups.Add(group);
                    }

                    //Permission checking
                    if (search.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.DepartmentAdmin))
                    {
                        string queryCondition = GeneralQueryHelper.ConcatCriteria(null, ConstantHelper.SPColumn.DepartmentList.Admin, "Integer", currentUser.Id.ToString(), "Contains", true);

                        ListItemCollection departmentItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.Department, queryCondition, [ConstantHelper.SPColumn.DepartmentList.Title]);

                        if (departmentItems != null)
                        {
                            foreach (ListItem item in departmentItems)
                            {
                                int departmentId = FieldHelper.GetFieldValueAsNumber(item, ConstantHelper.SPColumn.DepartmentList.ID);
                                if (!search.AccessDepartments.Contains(departmentId))
                                    search.AccessDepartments.Add(departmentId);
                            }
                        }
                    }

                    else if (search.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.Employee))
                    {
                        //See own records only
                        search.MemberLogin = search.CurrentUser;
                    }

                    if (search.AccessGroups.Contains(ConstantHelper.SPSecurityGroup.SDCClaimsAdmin))
                    {
                        //Full Access
                        search.AccessDepartments.Clear();
                        search.MemberLogin = string.Empty;
                    }

                    if (search.RequestEndDate != null && search.RequestEndDate != DateTime.MinValue)
                        search.RequestEndDate = search.RequestEndDate.Value.Date.AddDays(1).AddTicks(-1);

                    PagedResultDto<MainClaimHeader> data = await _da.GetPagedAsync(search, sortCol, sortDirection, skip, take);

                    vm.MainClaimHeaderListing.Items = data.Items;
                    vm.MainClaimHeaderListing.TotalCount = data.TotalCount;

                    if (search.IsExportToExcel)
                    {
                        if (vm.MainClaimHeaderListing.TotalCount > 0)
                        {
                            ExportToExcel(vm, spHostURL, clientContext);
                        }
                    }

                    vm.MainClaimHeaderListing.IsSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("SearchBL - GetPagedClaims Error: " + ex.ToString());
            }

            return vm;
        }

        public static void ExportToExcel(SearchClaimVM vm, string spHostUrl , ClientContext clientContext)
        {
            List<string> displayFields = new List<string>();
            List<string> internalFields = new List<string>();
            List<string> dateFields = new List<string>();

            List<dynamic> dynamicObject = ProjectHelper.ClassToDynamic(vm.MainClaimHeaderListing.Items);

            displayFields.Add("Reference No");
            displayFields.Add("Requester Name");
            displayFields.Add("Requester Employee ID");
            displayFields.Add("Requester Department");
            displayFields.Add("Request Date");
            displayFields.Add("Claim Status");
            displayFields.Add("Purpose Of Claim");
            displayFields.Add("Claim Currency");
            displayFields.Add("Submitted Date");
            displayFields.Add("Created Date");

            internalFields.Add("ReferenceNo");
            internalFields.Add("RequesterCompanyCode");
            internalFields.Add("RequesterEmployeeID");
            internalFields.Add("RequesterDepartment");
            internalFields.Add("RequestDate");
            internalFields.Add("ClaimStatus");
            internalFields.Add("PurposeOfClaims");
            internalFields.Add("ClaimCurrency");
            internalFields.Add("SubmittedDate");
            internalFields.Add("CreatedDate");

            ReportInfo reportInfo = new ReportInfo();

            ProjectHelper.GetReportInfoWithLogo(clientContext,  spHostUrl, reportInfo, "EClaims_SearchResults.xlsx");

            vm.SearchModel.ExcelFileBytes = ExcelHelper.CreateExcelReportWithLogo(displayFields, internalFields, dateFields, dynamicObject, reportInfo, clientContext);
        }

    }
}
