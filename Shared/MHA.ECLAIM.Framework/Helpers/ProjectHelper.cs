using MHA.Framework.Core.SP;
using MHA.Framework.Core.Workflow.BL;
using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.SharePoint.Client;
using System.Data;
using System.Dynamic;
using System.Net;
using System.Text.RegularExpressions;
using RegExp = System.Text.RegularExpressions;

namespace MHA.ECLAIM.Framework.Helpers
{
    public static class ProjectHelperExtension
    {
        private static readonly JSONAppSettings appSettings;

        static ProjectHelperExtension()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public static void ExecuteQueryWithIncrementalRetry(this ClientContext context, string sourceFunction = "")
        {
            int retryAttempts = 0;
            int backoffInterval = 500;
            int backoffIncrease = 100;
            int retryCount = 1;
            bool retry = false;
            ClientRequestWrapper wrapper = null;
            int.TryParse(appSettings.NoOfRetry, out retryCount);
            int.TryParse(appSettings.ThrottlingSleeptime, out backoffInterval);
            int.TryParse(appSettings.ThrottlingTimeIncrease, out backoffIncrease);

            if (retryCount <= 0)
                throw new ArgumentException("Provide a retry count greater than zero.");
            if (backoffInterval <= 0)
                throw new ArgumentException("Provide a delay greater than zero.");

            while (retryAttempts < retryCount)
            {
                try
                {
                    if (!retry)
                    {
                        context.ExecuteQuery();
                        return;
                    }
                    else
                    {
                        //increment the retry count
                        retryAttempts++;

                        // retry the previous request using wrapper
                        if (wrapper != null && wrapper.Value != null)
                        {
                            context.RetryQuery(wrapper.Value);
                            return;
                        }
                        // retry the previous request as normal
                        else
                        {
                            context.ExecuteQuery();
                            return;
                        }
                    }
                }
                catch (WebException wex)
                {
                    var response = wex.Response as HttpWebResponse;
                    if (response != null && (response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503))
                    {
                        wrapper = (ClientRequestWrapper)wex.Data["ClientRequest"];
                        retry = true;

                        // Delay for the requested seconds
                        System.Threading.Thread.Sleep(backoffInterval);

                        // Increase counters
                        backoffInterval += backoffIncrease;

                    }
                    else
                    {
                        throw wex;
                    }
                }
            }
            throw new Exception(string.Format("Maximum retry attempts {0}, have been attempted.", retryCount));
        }
    }

    public class ProjectHelper
    {
        private static readonly JSONAppSettings appSettings;
        static ProjectHelper()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        #region Class
        public static List<dynamic> ClassToDynamic<T>(List<T> objects)
        {
            List<dynamic> lstResult = new List<dynamic>();
            foreach (var obj in objects)
            {
                IDictionary<string, object> dictionary = new ExpandoObject();

                foreach (var propertyInfo in typeof(T).GetProperties())
                {
                    var currentValue = propertyInfo.GetValue(obj, null);

                    dictionary.Add(propertyInfo.Name, currentValue);
                }
                lstResult.Add(dictionary);
            }

            return lstResult;
        }
        #endregion

        #region Character Validation
        private static RegExp.Regex _compiledUnicodeRegex = new RegExp.Regex(@"[\u0000-\u001F]", RegExp.RegexOptions.Compiled);
        public static string RemoveInvalidHexaCharacter(string rawValue)
        {
            if (String.IsNullOrEmpty(rawValue))
                return String.Empty;

            return _compiledUnicodeRegex.Replace(rawValue, String.Empty);
        }
        #endregion

        public static bool RequiredRetryException(string exceptionMessage)
        {
            bool result = false;
            exceptionMessage = exceptionMessage.ToLower();

            if (exceptionMessage.Contains(ConstantHelper.ExceptionType.SaveConflict) ||
               exceptionMessage.Contains(ConstantHelper.ExceptionType.VersionConflict) ||
               exceptionMessage.Contains(ConstantHelper.ExceptionType.OperationTimedOut) ||
               exceptionMessage.Contains(ConstantHelper.ExceptionType.HResult) ||
               exceptionMessage.Contains(ConstantHelper.ExceptionType.UnderlineClosed))
            {
                result = true;
            }

            return result;
        }

        #region URL-based data retrieval
        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        public static string GetRelativeUrlFromUrl(string url)
        {
            string relativeUrl = url;
            bool isAbsoluteUrl = IsAbsoluteUrl(url);
            if (isAbsoluteUrl)
            {
                Uri uri = new Uri(url);
                relativeUrl = uri.AbsolutePath;

                if (!string.IsNullOrEmpty(uri.Fragment))
                {
                    relativeUrl += uri.Fragment;
                }
            }
            relativeUrl = Uri.UnescapeDataString(relativeUrl);
            return relativeUrl.TrimEnd('/');
        }

        public static string GetSPHostURLDomain(string spHostUrl, string spWebServerRelativeURL)
        {
            string spHostDomain = string.Empty;
            if (!string.IsNullOrEmpty(spHostUrl) && !string.IsNullOrEmpty(spWebServerRelativeURL))
            {
                spHostDomain = spHostUrl.Replace(spWebServerRelativeURL, string.Empty);
            }
            if (spHostDomain[spHostDomain.Length - 1] == '/')
            {
                spHostDomain = spHostDomain.Substring(0, spHostDomain.Length - 1);
            }
            return spHostDomain;
        }
        #endregion

        public List<string> GetTaskStatus()
        {
            List<string> obj = new();

            obj.Add(ConstantHelper.TaskStatus.Completed);
            obj.Add(ConstantHelper.TaskStatus.Reassigned);
            obj.Add(ConstantHelper.TaskStatus.IN_Progress);
            obj.Add(ConstantHelper.TaskStatus.Removed);

            return obj;
        }

        public static string ReplaceKeywordWithValue(string orignalKeyword, string sentence, int number)
        {
            string preKeyword = orignalKeyword.Substring(0, orignalKeyword.Length - 1) + ":";
            string postKeyword = "}";
            string keywordPattern = preKeyword + "\\d+" + postKeyword;
            Regex regex = new Regex(keywordPattern);
            if (regex.IsMatch(sentence))
            {
                string osub = regex.Match(sentence).Value;
                string sub = osub.Replace(preKeyword, string.Empty);
                sub = sub.Replace(postKeyword, string.Empty);
                int runningLength = Convert.ToInt32(sub);
                string runningFormat = string.Empty;

                for (int i = 0; i < runningLength; i++)
                {
                    runningFormat += "0";
                }
                sentence = sentence.Replace(osub, string.Format("{0:" + runningFormat + postKeyword, number));
            }
            return sentence;
        }

        #region Drop down list items
        public static List<DropDownListItem> GetListItemsAsDDLItems(ListItemCollection listItemCollection, string displayColumnName, string displayColumnName2, bool isIncludeBlank = false, bool showInactive = false)
        {
            List<DropDownListItem> listResult = new List<DropDownListItem>();

            if (listItemCollection != null && listItemCollection.Count > 0)
            {
                foreach (ListItem listItem in listItemCollection)
                {
                    string title = FieldHelper.GetFieldValueAsString(listItem, displayColumnName);
                    string status = FieldHelper.GetFieldValueAsString(listItem, displayColumnName2);
                    string id = FieldHelper.GetFieldValueAsString(listItem, "ID");

                    if (status == ConstantHelper.ItemStatus.Inactive && !showInactive)
                        continue;

                    DropDownListItem listOption = new DropDownListItem
                    {
                        Text = title,
                        Value = id,
                        Status = status,
                        Id = id
                    };

                    listResult.Add(listOption);
                }
            }

            listResult = listResult.OrderBy(x => x.Text).ToList();
            return listResult;
        }

        public static List<DropDownListItem> GetProcessTemplateDDL()
        {
            List<ProcessTemplate> processTemplateList = new List<ProcessTemplate>();
            List<DropDownListItem> processTemplateDDL = new List<DropDownListItem>();

            processTemplateList = GetWorkflowProcessTemplate();

            foreach (ProcessTemplate obj in processTemplateList)
            {
                if (obj.ProcessName == (ConstantHelper.WorkflowName.EclaimWorkflow))
                    processTemplateDDL.Add(new DropDownListItem { Text = obj.ProcessName, Value = obj.ProcessID.ToString() });
            }
            return processTemplateDDL;
        }

        public static List<ProcessTemplate> GetWorkflowProcessTemplate()
        {
            List<ProcessTemplate> processTemplateList = new List<ProcessTemplate>();
            string ConnString = ConnectionStringHelper.GetGenericWFConnString();
            WorkflowBL wfBL = new WorkflowBL(ConnString);
            processTemplateList = wfBL.GetAllProcessTemplate();
            return processTemplateList;
        }

        public List<DropDownListItem> LoadDropDownListItem(string listName, bool showInactive, string spHostUrl, string accessToken)
        {
            using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
            {
                ListItemCollection listItem = GeneralQueryHelper.GetSPItems(clientContext, listName, string.Empty, null);

                List<DropDownListItem> dropDownListItem = new List<DropDownListItem>();

                dropDownListItem = ProjectHelper.GetListItemsAsDDLItems(listItem, ConstantHelper.SPColumn.Title, ConstantHelper.SPColumn.Status, false, showInactive);

                return dropDownListItem;
            }
        }

        public static List<DropDownListItem> GetExpensesAsDDLItems(ListItemCollection listItemCollection, string expenseColumn, string claimCategoryColumn, bool isIncludeBlank = false)
        {
            var listResult = new List<DropDownListItem>();

            if (listItemCollection != null && listItemCollection.Count > 0)
            {
                foreach (ListItem listItem in listItemCollection)
                {
                    string expense = FieldHelper.GetFieldValueAsString(listItem, expenseColumn);
                    string claimCategory = FieldHelper.GetLookupValueAsString(listItem, claimCategoryColumn);
                    string id = FieldHelper.GetFieldValueAsString(listItem, "ID");

                    if (string.IsNullOrWhiteSpace(expense))
                        continue;

                    listResult.Add(new DropDownListItem
                    {
                        Text = expense,
                        Value = expense,
                        SubText = claimCategory,
                        Id = id
                    });
                }
            }

            return listResult.OrderBy(x => x.Text).ToList();
        }

        public static List<DropDownListItem> GetExpensesSubtypeAsDDLItems(ListItemCollection listItemCollection, string subtypeColumn, string expenseColumn)
        {
            var listResult = new List<DropDownListItem>();

            if (listItemCollection != null && listItemCollection.Count > 0)
            {
                foreach (ListItem listItem in listItemCollection)
                {
                    string subtype = FieldHelper.GetFieldValueAsString(listItem, subtypeColumn);
                    string expense = FieldHelper.GetFieldValueAsString(listItem, expenseColumn);
                    string id = FieldHelper.GetFieldValueAsString(listItem, "ID");

                    if (string.IsNullOrWhiteSpace(subtype))
                        continue;

                    listResult.Add(new DropDownListItem
                    {
                        Text = subtype,
                        Value = subtype,
                        SubText = expense,
                        Id = id
                    });
                }
            }

            return listResult.OrderBy(x => x.Text).ToList();
        }

        public List<string> GetClaimStatuses()
        {
            List<string> obj = new();

            obj.Add(ConstantHelper.WorkflowStatus.PendingReportingManager1Approval);
            obj.Add(ConstantHelper.WorkflowStatus.PendingReportingManager2Approval);
            obj.Add(ConstantHelper.WorkflowStatus.PendingReportingManager3Approval);
            obj.Add(ConstantHelper.WorkflowStatus.PendingClaimItemApproval);
            obj.Add(ConstantHelper.WorkflowStatus.PendingOriginatorResubmission);
            obj.Add(ConstantHelper.WorkflowStatus.COMPLETED);
            obj.Add(ConstantHelper.WorkflowStatus.REJECTED);
            obj.Add(ConstantHelper.WorkflowStatus.TERMINATED);
            obj.Add(ConstantHelper.WorkflowStatus.GO_TO);
            obj.Add(ConstantHelper.WorkflowStatus.ERROR);
            obj.Add(ConstantHelper.WorkflowStatus.DRAFT);
            obj.Add(ConstantHelper.WorkflowStatus.APPROVED);
            obj.Add(ConstantHelper.WorkflowStatus.IN_PROGRESS);

            return obj;
        }
        #endregion

        #region Document
        //public static void GetReportInfo(ClientContext clientContext, ReportInfo reportInfo, string reportTitle)
        //{
        //    try
        //    {
        //        using (clientContext)
        //        {
        //            User spUser = clientContext.Web.CurrentUser;

        //            clientContext.Load(spUser, u => u.Title);
        //            clientContext.Load(clientContext.Web, user => user.CurrentUser);
        //            clientContext.ExecuteQueryWithIncrementalRetry();

        //            string generatedDate = SPDateTimeHelper.ConvertFromUTCWithSPTimeZone(DateTime.UtcNow, clientContext).ToString(appSettings.DefaultDateTimeFormat);

        //            reportInfo.GeneratedDate = generatedDate;
        //            reportInfo.ReportTitle = reportTitle;
        //            reportInfo.GeneratedBy = spUser.Title;
        //            reportInfo.ProjectInfo = ConstantHelper.Module.Eclaim;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        (new LogHelper()).LogMessage(string.Format("ProjectHelper - Get Report Info : {0}", ex));
        //    }
        //}
        public static void GetReportInfoWithLogo(ClientContext clientContext, string spHostURL, ReportInfo reportInfo, string reportTitle)
        {
            try
            {
                using (clientContext)
                {
                    User spUser = clientContext.Web.CurrentUser;

                    clientContext.Load(spUser, u => u.Title);
                    clientContext.Load(clientContext.Web, user => user.CurrentUser);
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    string generatedDate = SPDateTimeHelper.ConvertFromUTCWithSPTimeZone(DateTime.UtcNow, clientContext).ToString(appSettings.DefaultDateTimeFormat);

                    reportInfo.GeneratedDate = generatedDate;
                    reportInfo.ReportTitle = reportTitle;
                    reportInfo.GeneratedBy = spUser.Title;
                    reportInfo.ProjectInfo = ConstantHelper.Module.Eclaim;
                    GetSharepointLogo(spHostURL, reportInfo);
                }
            }
            catch (Exception ex)
            {
                (new LogHelper()).LogMessage(string.Format("ProjectHelper - Get Report Info : {0}", ex));
            }
        }

        private static void GetSharepointLogo(string spHostURL, ReportInfo vm)
        {

            vm.LogoUrl = spHostURL + appSettings.DefaultSPSiteLogoURL;
        }
        #endregion

        #region Files
        public static byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[input.Length];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        #endregion

        #region Folder
        public static Folder EnsureFolder(ClientContext clientContext, string libraryName, string folderPath)
        {
            Web web = clientContext.Web;
            List docLibrary = web.Lists.GetByTitle(libraryName);
            clientContext.Load(docLibrary.RootFolder);
            clientContext.ExecuteQueryWithIncrementalRetry();

            Folder currentFolder = docLibrary.RootFolder;

            string[] folderNames = folderPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var folderName in folderNames)
            {
                FolderCollection subFolders = currentFolder.Folders;
                clientContext.Load(subFolders);
                clientContext.ExecuteQueryWithIncrementalRetry();

                Folder nextFolder = subFolders.FirstOrDefault(f => f.Name.Equals(folderName, StringComparison.OrdinalIgnoreCase));
                if (nextFolder == null)
                {
                    nextFolder = currentFolder.Folders.Add(folderName);
                    clientContext.Load(nextFolder);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }

                currentFolder = nextFolder;
            }

            return currentFolder;
        }
        #endregion
    }
}
