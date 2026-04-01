using MHA.Framework.Core.Workflow.BO;
using MHA.ECLAIM.Data;
using MHA.ECLAIM.Entities.ViewModel.Home;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using Microsoft.SharePoint.Client;
using System.Data;

namespace MHA.ECLAIM.Business
{
    public class HomeBL
    {
        public async Task<ViewModelMyPendingTask> GetMyPendingTask(ViewModelMyPendingTask vmMyTask, string spHostURL, string accessToken)
        {
            using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
            {
                try
                {
                    //int pageSelectionLimit = int.Parse(appSettings.MyPagingSelectionShowLimit);
                    int pageSelectionLimit = ConstantHelper.MyPagingSelectionShowLimit;

                    #region Get My Pending Task
                    clientContext.Load(clientContext.Web, web => web.Title, user => user.CurrentUser, relativeURL => relativeURL.ServerRelativeUrl, url => url.Url);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    User spUser = clientContext.Web.CurrentUser;
                    Actioner currentUser = new Actioner(spUser.LoginName, spUser.Title, spUser.Email);
                    string applicationName = ProjectHelper.GetRelativeUrlFromUrl(spHostURL);

                    #region Get Current and Upcoming Task
                    HomeDA _homeDA = new HomeDA();

                    //Get My Pending And Incoming Task
                    DataTable myPendingTaskDataTable = _homeDA.GetMyPendingTaskData(vmMyTask, currentUser, applicationName);

                    //Get My Pending And Incoming Task Count
                    int totalRows = _homeDA.GetMyPendingTaskDataCount(currentUser, applicationName);
                    vmMyTask.TotalCount = totalRows;
                    #endregion

                    DateTime currentDate = DateTimeHelper.GetCurrentDateTime();

                    foreach (DataRow row in myPendingTaskDataTable.Rows)
                    {
                        MyPendingTask myTaskObj = new MyPendingTask();
                        myTaskObj = ConvertEntitiesHelper.ConvertMyPendingTaskObj(row, clientContext);
                        if (myTaskObj != null && myTaskObj.DueDate.HasValue && myTaskObj.DueDate != DateTime.MinValue)
                        {
                            myTaskObj.IsOverDue = currentDate.Date > myTaskObj.DueDate.Value.Date;
                        }

                        MyPendingTask duplicatedPendingTask = vmMyTask.MyTaskList.FirstOrDefault(x => x.TaskID > 0 && String.Equals(x.TaskURL, myTaskObj.TaskURL));
                        if (duplicatedPendingTask == null)
                        {
                            vmMyTask.MyTaskList.Add(myTaskObj);
                        }
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    LogHelper logHelper = new LogHelper();
                    logHelper.LogMessage("HomeBL - GetMyPendingTask Error: " + ex.ToString());
                }
            }

            return vmMyTask;
        }

        public async Task<ViewModelMyActiveRequest> GetMyActiveRequest(ViewModelMyActiveRequest vm, string spHostURL, string accessToken)
        {
            using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostURL, accessToken))
            {
                try
                {
                    clientContext.Load(clientContext.Web, web => web.Title, user => user.CurrentUser, relativeURL => relativeURL.ServerRelativeUrl, url => url.Url);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                    User spUser = clientContext.Web.CurrentUser;
                    Actioner currentUser = new Actioner(spUser.LoginName, spUser.Title, spUser.Email);

                    string currUserLogin = currentUser.LoginName;

                    List<string> workflowStatusList = new List<string>
                    {
                        ConstantHelper.WorkflowStatus.COMPLETED,
                        ConstantHelper.WorkflowStatus.REJECTED,
                        ConstantHelper.WorkflowStatus.TERMINATED,
                        ConstantHelper.WorkflowStatus.GO_TO,
                        ConstantHelper.WorkflowStatus.ERROR,
                        ConstantHelper.WorkflowStatus.DRAFT,
                        ConstantHelper.RequestForm.WorkflowNewRequest.WorkflowStatusEmpty
                    };

                    string workflowStatuses = string.Join(",", workflowStatusList);

                    ClaimRequestDA da = new ClaimRequestDA();
                    vm.Request = await da.GetPagedMyActiveRequest(vm.Skip, vm.Take, workflowStatusList, currUserLogin);
                }
                catch (Exception ex)
                {
                    LogHelper logHelper = new LogHelper();
                    logHelper.LogMessage("HomeBL - GetMyActiveRequest Error: " + ex.ToString());
                }
            }

            return vm;
        }
    }
}
