using MHA.ECLAIM.Entities.ViewModel.Home;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Business
{
    public class AdministrationBL
    {
        public ViewModelAdministrationListing InitAdministrationListing(string spHostUrl, string accessToken)
        {
            ViewModelAdministrationListing vm = new ViewModelAdministrationListing();

            try
            {
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    string[] groups = new[] { ConstantHelper.SPSecurityGroup.SDCClaimsAdmin };
                    vm.IsAuthorized = SharePointHelper.IsUserInGroups(clientContext, string.Empty, groups);

                    if (vm.IsAuthorized)
                    {
                        vm.ReportURL = string.Format(ConstantHelper.URLTemplate.ReportURLTemplate, spHostUrl);
                        vm.RunningNumberFormatURL = string.Format(ConstantHelper.URLTemplate.RunningNumberFormatUrlTemplate, spHostUrl);
                        vm.RunningNumberURL = string.Format(ConstantHelper.URLTemplate.RunningNumberUrlTemplate, spHostUrl);

                        vm.EmployeeURL = string.Format(ConstantHelper.URLTemplate.EmployeeUrlTemplate, spHostUrl);
                        vm.CountriesURL = string.Format(ConstantHelper.URLTemplate.CountriesUrlTemplate, spHostUrl);
                        vm.CostCenterURL = string.Format(ConstantHelper.URLTemplate.CostCenterUrlTemplate, spHostUrl);
                        vm.DepartmentURL = string.Format(ConstantHelper.URLTemplate.DepartmentUrlTemplate, spHostUrl);
                        vm.CompanyURL = string.Format(ConstantHelper.URLTemplate.CompanyUrlTemplate, spHostUrl);
                        vm.PositionURL = string.Format(ConstantHelper.URLTemplate.PositionUrlTemplate, spHostUrl);

                        vm.ClaimEntitlementTypeURL = string.Format(ConstantHelper.URLTemplate.ClaimEntitlementTypeUrlTemplate, spHostUrl);
                        vm.ClaimItemApproverURL = string.Format(ConstantHelper.URLTemplate.ClaimItemApproverUrlTemplate, spHostUrl);
                        vm.ClaimMasterApproverURL = string.Format(ConstantHelper.URLTemplate.ClaimMasterApproverUrlTemplate, spHostUrl);

                        vm.CurrencyURL = string.Format(ConstantHelper.URLTemplate.CurrencyUrlTemplate, spHostUrl);
                        vm.TaxCodeURL = string.Format(ConstantHelper.URLTemplate.TaxCodeUrlTemplate, spHostUrl);
                        vm.GLCodeURL = string.Format(ConstantHelper.URLTemplate.GLCodeUrlTemplate, spHostUrl);

                        vm.CategoryURL = string.Format(ConstantHelper.URLTemplate.CategoryUrlTemplate, spHostUrl);
                        vm.SubCategoryURL = string.Format(ConstantHelper.URLTemplate.SubCategoryUrlTemplate, spHostUrl);
                        vm.FrequencyURL = string.Format(ConstantHelper.URLTemplate.FrequencyUrlTemplate, spHostUrl);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return vm;
        }

    }
}
