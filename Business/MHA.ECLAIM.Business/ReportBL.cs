using MHA.Framework.Core.SP;
using MHA.ECLAIM.Entities.ViewModel.Report;
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
    public class ReportBL
    {
        public ViewModelReportListing InitReportListing(string spHostURL, string accessToken)
        {
            ViewModelReportListing report = new ViewModelReportListing();

            try
            {
                TokenHelper.CheckValidAccessToken(accessToken, spHostURL);
                using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
                {
                    string[] group = { ConstantHelper.SPSecurityGroup.SDCClaimsAdmin };
                    report.IsAuthorized = SharePointHelper.IsUserInGroups(clientContext, string.Empty, group);

                    if (report.IsAuthorized)
                    {
                        string[] viewFields = { ConstantHelper.SPColumn.ReportURLList.Title, ConstantHelper.SPColumn.ReportURLList.URL };
                        ListItemCollection reportItems = GeneralQueryHelper.GetSPItems(clientContext, ConstantHelper.SPList.ReportUrl, string.Empty, viewFields);
                        if (reportItems != null && reportItems.Count > 0)
                        {
                            foreach (ListItem reportItem in reportItems)
                            {
                                report.ReportLinks.Add(FieldHelper.GetFieldValueAsString(reportItem, ConstantHelper.SPColumn.ReportURLList.Title), FieldHelper.GetFieldValueAsString(reportItem, ConstantHelper.SPColumn.ReportURLList.URL));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return report;
        }
    }
}
