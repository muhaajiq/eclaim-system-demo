using MHA.Framework.Core.SP;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;

namespace MHA.ECLAIM.Business
{
    public class EmailTemplateBL
    {
        #region Common method
        public static void GetEmailTemplateByTemplateTitle(string emailTemplateTitle, ref string emailSubject, ref string emailBody, string centralSiteURL, string accessToken)
        {
            try
            {
                TokenHelper.CheckValidAccessToken(accessToken, centralSiteURL);
                using (ClientContext clientContextCentral = TokenHelper.GetClientContextWithAccessToken(centralSiteURL, accessToken))
                {
                    String[] ViewFields = { ConstantHelper.SPColumn.EmailTemplatesList.Subject, ConstantHelper.SPColumn.EmailTemplatesList.Body };
                    ListItem emailTemplateItem = GeneralQueryHelper.GetSPItemByTitle(clientContextCentral, ConstantHelper.SPList.EmailTemplate, emailTemplateTitle, ViewFields);
                    if (emailTemplateItem != null)
                    {
                        emailSubject = MHA.Framework.Core.SP.FieldHelper.GetFieldValueAsString(emailTemplateItem, ConstantHelper.SPColumn.EmailTemplatesList.Subject);
                        emailBody = MHA.Framework.Core.SP.FieldHelper.GetFieldValueAsString(emailTemplateItem, ConstantHelper.SPColumn.EmailTemplatesList.Body);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("Access denied"))
                {
                    GetEmailTemplateByTemplateTitleWithRootURL(emailTemplateTitle, ref emailSubject, ref emailBody, centralSiteURL);
                }
                else
                {
                    throw;
                }
            }
        }

        //public void GetEmailTemplateByTemplateTitle(string emailTemplateTitle, ref string emailSubject, ref string emailBody, ClientContext rootCtx)
        //{
        //    try
        //    {
        //        String[] ViewFields = { ConstantHelper.SPColumn.EmailTemplates.Subject, ConstantHelper.SPColumn.EmailTemplates.Body };
        //        ListItem emailTemplateItem = GeneralQueryHelper.GetSPItemByTitle(rootCtx, ConstantHelper.SPList.EmailTemplate, emailTemplateTitle, ViewFields);
        //        if (emailTemplateItem != null)
        //        {
        //            emailSubject = MHA.Framework.SP.FieldHelper.GetFieldValueAsString(emailTemplateItem, ConstantHelper.SPColumn.EmailTemplates.Subject);
        //            emailBody = MHA.Framework.SP.FieldHelper.GetFieldValueAsString(emailTemplateItem, ConstantHelper.SPColumn.EmailTemplates.Body);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.ToString().Contains("Access denied"))
        //        {
        //            GetEmailTemplateByTemplateTitleWithRootURL(emailTemplateTitle, ref emailSubject, ref emailBody, ConstantHelper.AppSettings.EDMS_Root_URL);
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}


        //public List<ListItem> GetEmailTemplateListItemByTemplateTitle(List<string> emailTemplateTitlesList, ClientContext rootCtx)
        //{
        //    try
        //    {
        //        String[] ViewFields = { ConstantHelper.SPColumn.EmailTemplates.Title, ConstantHelper.SPColumn.EmailTemplates.Subject, ConstantHelper.SPColumn.EmailTemplates.Body };
        //        string query = CSOMHelper.CamlQueryIn(ConstantHelper.SPColumn.EmailTemplates.Title, emailTemplateTitlesList.ToArray(), "Text");
        //        ListItemCollection targetDDMsListItemCol = GeneralQueryHelper.GetSPItems(rootCtx, ConstantHelper.SPList.EmailTemplate, query, ViewFields);
        //        if (targetDDMsListItemCol != null && targetDDMsListItemCol.Count > 0)
        //        {
        //            return targetDDMsListItemCol.ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.ToString().Contains("Access denied"))
        //        {
        //            GetEmailTemplateByTemplateTitleWithRootURL(emailTemplateTitlesList, ConstantHelper.AppSettings.EDMS_Root_URL);
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //    return null;
        //}

        private static void GetEmailTemplateByTemplateTitleWithRootURL(string emailTemplateTitle, ref string emailSubject, ref string emailBody, string rootURL)
        {
            //using (ClientContext rootCtx = TokenHelper.GetClientContextFromHybridApp(rootURL))
            //{
            //    String[] ViewFields = { ConstantHelper.SPColumn.EmailTemplates.Subject, ConstantHelper.SPColumn.EmailTemplates.Body };
            //    ListItem emailTemplateItem = GeneralQueryHelper.GetSPItemByTitle(rootCtx, ConstantHelper.SPList.EmailTemplate, emailTemplateTitle, ViewFields);
            //    if (emailTemplateItem != null)
            //    {
            //        emailSubject = MHA.Framework.Core.SP.FieldHelper.GetFieldValueAsString(emailTemplateItem, ConstantHelper.SPColumn.EmailTemplates.Subject);
            //        emailBody = MHA.Framework.Core.SP.FieldHelper.GetFieldValueAsString(emailTemplateItem, ConstantHelper.SPColumn.EmailTemplates.Body);
            //    }
            //}
        }

        //private List<ListItem> GetEmailTemplateByTemplateTitleWithRootURL(List<string> emailTemplateTitlesList, string rootURL)
        //{
        //    using (ClientContext rootCtx = CSOMHelper.GetClientContextFromHybridApp(rootURL))
        //    {
        //        String[] ViewFields = { ConstantHelper.SPColumn.EmailTemplates.Title, ConstantHelper.SPColumn.EmailTemplates.Subject, ConstantHelper.SPColumn.EmailTemplates.Body };
        //        string query = CSOMHelper.CamlQueryIn(ConstantHelper.SPColumn.EmailTemplates.Title, emailTemplateTitlesList.ToArray(), "Text");
        //        ListItemCollection targetDDMsListItemCol = GeneralQueryHelper.GetSPItems(rootCtx, ConstantHelper.SPList.EmailTemplate, query, ViewFields);
        //        if (targetDDMsListItemCol != null && targetDDMsListItemCol.Count > 0)
        //        {
        //            return targetDDMsListItemCol.ToList();
        //        }
        //        return null;
        //    }
        //}

        #endregion
    }
}
