using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Process;
using MHA.ECLAIM.Process.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Diagnostics;

namespace MHA.ECLAIM.Web.Components.Pages.Approve
{

    [Authorize]
    public partial class ApprovalForm : ComponentBase
    {
        #region Injection Services
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] private NavigationHelper NavigationHelper { get; set; }
        [Inject] private TokenHelper TokenHelper { get; set; }
        [Inject] private LogHelper LogHelper { get; set; }
        [Inject] private TempMessageService MessageService { get; set; }
        [Inject] private IApprovalProcess IApprovalProcess { get; set; }
        [Inject] private IWorkflowProcess IWorkflowProcess { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        #endregion

        #region Query parameters & route parameters
        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.SPHostUrl)]
        [Parameter] public string spHostUrl { get; set; } = string.Empty;

        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.RequestId)]
        public string RequestId { get; set; } = string.Empty;

        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.ProcessId)]
        public string ProcessId { get; set; } = string.Empty;

        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.TaskId)]
        public string TaskId { get; set; } = string.Empty;
        #endregion

        #region View models & state
        public string accessToken = string.Empty;
        public string appAccessToken = string.Empty;
        public ApprovalFormVM model = new();
        private DateTime? DueDate { get; set; }
        private CancellationTokenSource _errorAlertCts;
        private bool showErrorAlert = false;
        private bool showFormErrorAlert = false;
        private RequestFormVisibilitySettings visibilitySettings = new();
        private string PageTitle = string.Empty;
        private string DateOfRequest = string.Empty;
        #endregion

        #region Lifecycle
        protected override async Task OnInitializedAsync()
        {
            await RefreshTokensAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadData();
                if (!string.IsNullOrEmpty(model.CurrentStage))
                    visibilitySettings = await IApprovalProcess.SetVisibilitySettings(model.CurrentStage);

                if (model.CurrentStage == ConstantHelper.WorkflowStatus.PendingReportingManager1Approval
                    || model.CurrentStage == ConstantHelper.WorkflowStatus.PendingReportingManager2Approval
                    || model.CurrentStage == ConstantHelper.WorkflowStatus.PendingReportingManager3Approval)
                {
                    PageTitle = ConstantHelper.ApprovalForm.ApprovalFormTitle.ReportingManagerApproval;
                }
                else if (model.CurrentStage == ConstantHelper.WorkflowStatus.PendingClaimItemApproval)
                {
                    PageTitle = ConstantHelper.ApprovalForm.ApprovalFormTitle.ClaimItemApproval;

                }
                else if (model.CurrentStage == ConstantHelper.WorkflowStatus.PendingOriginatorResubmission)
                {
                    PageTitle = ConstantHelper.ApprovalForm.ApprovalFormTitle.Resubmission;

                }
                else if (model.CurrentStage == ConstantHelper.WorkflowStatus.PendingFinalApproval)
                {
                    PageTitle = ConstantHelper.ApprovalForm.ApprovalFormTitle.FinalApproval;

                }
                else
                {
                    PageTitle = "-";
                }

                model.IsLoaded = true;

                StateHasChanged();
            }

            if (model.HasError)
            {
                await JS.InvokeVoidAsync("jsInterop.scrollToError", "errorAlertTop");
                StateHasChanged();
            }
        }
        #endregion

        #region Data Load
        private async Task LoadData()
        {
            try
            {
                await JS.InvokeVoidAsync("showSwalLoading", "Loading Approval Form", "Please wait while we load the approval form...");

                model.RequestID = int.Parse(RequestId);
                model.ProcessID = int.Parse(ProcessId);
                model.TaskID = int.Parse(TaskId);

                model = await IApprovalProcess.InitApprovalForm(model, spHostUrl, accessToken);
                if (!model.IsMyTask)
                {
                    await JS.InvokeVoidAsync("Swal.close");
                    return;
                }

                if (model.HasError) showErrorAlert = true;

                int? processId = model.MainClaimHeaderVM.ProcessID;
                if (processId == null) return;

                //Workflow History
                model.WFHistory = await IWorkflowProcess.InitWorkflowHistory(model.MainClaimHeaderVM.ProcessID, accessToken, spHostUrl);
                DateOfRequest = model.MainClaimHeaderVM.RequestDate.Value.ToString(ConstantHelper.DateFormat.DefaultDateFormat) ?? string.Empty;

                await JS.InvokeVoidAsync("Swal.close");
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("Swal.close");
                await JS.InvokeVoidAsync("showSwalError", "Failed to load request form.");
                LogHelper.LogMessage("CreateRequestForm - InitNewRequestForm: " + ex);
                model.HasError = true;
                model.ErrorMessage = string.Format(ConstantHelper.ErrorMessage.UnexpectedErrorOccur, ex.Message);
                StateHasChanged();
            }
        }
        #endregion

        #region Event Callbacks
        private async Task OnClaimChanged()
        {
            StateHasChanged();
        }
        #endregion

        #region Button Handlers
        private async Task HandleApprove() => await HandleApprovalAction(ConstantHelper.WorkflowActionName.Approve);

        private async Task HandleRequireAmendment()
        {
            if (string.IsNullOrWhiteSpace(model.MainClaimHeaderVM.GeneralRemarks))
            {
                await JS.InvokeVoidAsync("showSwalWarning", "Remarks Required", ConstantHelper.ErrorMessage.RemarksForRequireAmendment);
                return;
            }
            await HandleApprovalAction(ConstantHelper.WorkflowActionName.RequireAmendment);
        }

        private async Task HandleReject()
        {
            if (string.IsNullOrWhiteSpace(model.MainClaimHeaderVM.GeneralRemarks))
            {
                await JS.InvokeVoidAsync("showSwalWarning", "Remarks Required", ConstantHelper.ErrorMessage.RemarksForReject);
                return;
            }
            await HandleApprovalAction(ConstantHelper.WorkflowActionName.Reject);
        }

        private async Task HandleSave()
        {
            var error = GetValidationMessage();
            if (!string.IsNullOrEmpty(error))
            {
                await JS.InvokeVoidAsync("showSwalValidationWarning", "Validation Error", error);
                return;
            }
            await HandleApprovalAction(ConstantHelper.WorkflowActionName.Save);
        }

        private async Task HandleResubmit()
        {
            var error = GetValidationMessage();
            if (!string.IsNullOrEmpty(error))
            {
                await JS.InvokeVoidAsync("showSwalValidationWarning", "Validation Error", error);
                return;
            }
            await HandleApprovalAction(ConstantHelper.WorkflowActionName.Resubmit);
        }

        private async Task HandleApprovalAction(string actionType)
        {
            try
            {
                await JS.InvokeVoidAsync("showSwalLoading");
                await RefreshTokensAsync();

                if (!int.TryParse(RequestId, out int requestId)) throw new ArgumentException("Invalid RequestId");
                if (!int.TryParse(ProcessId, out int processId)) throw new ArgumentException("Invalid ProcessId");
                if (!int.TryParse(TaskId, out int taskId)) throw new ArgumentException("Invalid TaskId");

                model.RequestID = requestId;
                model.ProcessID = processId;
                model.TaskID = taskId;

                switch (actionType)
                {
                    case ConstantHelper.WorkflowActionName.RequireAmendment:
                        model = await IApprovalProcess.RequireAmendmentRequest(model, spHostUrl, accessToken); break;
                    case ConstantHelper.WorkflowActionName.Approve:
                        model = await IApprovalProcess.ApproveRequest(model, spHostUrl, accessToken); break;
                    case ConstantHelper.WorkflowActionName.Reject:
                        model = await IApprovalProcess.RejectRequest(model, spHostUrl, accessToken); break;
                    case ConstantHelper.WorkflowActionName.Save:
                        model = await IApprovalProcess.SaveRequest(model, spHostUrl, accessToken); break;
                    case ConstantHelper.WorkflowActionName.Resubmit:
                        model = await IApprovalProcess.ReSubmitClaimRequest(model, spHostUrl, accessToken); break;
                    default: throw new InvalidOperationException("Unknown action type");
                }

                await JS.InvokeVoidAsync("Swal.close");

                if (model.HasSuccess)
                {
                    string swalTitle = actionType switch
                    {
                        ConstantHelper.WorkflowActionName.RequireAmendment => ConstantHelper.SuccessMessage.RequireAmendmentPopup,
                        ConstantHelper.WorkflowActionName.Approve => ConstantHelper.SuccessMessage.ApprovePopup,
                        ConstantHelper.WorkflowActionName.Reject => ConstantHelper.SuccessMessage.RejectPopup,
                        ConstantHelper.WorkflowActionName.Save => ConstantHelper.SuccessMessage.SavePopup,
                        ConstantHelper.WorkflowActionName.Resubmit => ConstantHelper.SuccessMessage.ResubmitPopup,
                        _ => "Your request has been saved"
                    };

                    await JS.InvokeVoidAsync("showSwalSuccess", swalTitle);

                    MessageService.SuccessMessage = actionType switch
                    {
                        ConstantHelper.WorkflowActionName.RequireAmendment => ConstantHelper.SuccessMessage.RequireAmendmentSuccess,
                        ConstantHelper.WorkflowActionName.Approve => ConstantHelper.SuccessMessage.ApproveRequestSuccess,
                        ConstantHelper.WorkflowActionName.Reject => ConstantHelper.SuccessMessage.RejectRequestSuccess,
                        ConstantHelper.WorkflowActionName.Save => ConstantHelper.SuccessMessage.SaveRequestSuccess,
                        ConstantHelper.WorkflowActionName.Resubmit => ConstantHelper.SuccessMessage.ResubmitRequestSuccess,
                        _ => "Action completed successfully"
                    };

                    Navigation.NavigateTo(NavigationHelper.BuildUrl("/Home"));
                }
                else
                {
                    await JS.InvokeVoidAsync("showSwalError", model.ErrorMessage ?? "An unexpected error occurred.");
                    MessageService.ErrorMessage = model.ErrorMessage;
                    showFormErrorAlert = true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogMessage($"ApprovalForm - {actionType} Error: {ex}");
                await JS.InvokeVoidAsync("showSwalError", ConstantHelper.ErrorMessage.ApprovalError);
                MessageService.ErrorMessage = ConstantHelper.ErrorMessage.ApprovalError;
            }
        }

        private void Close() => Navigation.NavigateTo(NavigationHelper.BuildUrl("/Home"));

        #endregion

        #region Validation & Helpers
        private string GetValidationMessage()
        {
            if (model.SubClaimDetailsVM == null || !model.SubClaimDetailsVM.Any())
            {
                return "Please add at least 1 claim detail(s) before completing.";
            }

            return string.Empty;
        }

        private async Task RefreshTokensAsync()
        {
            accessToken = await TokenHelper.GetUserAccessToken();
            //appAccessToken = await TokenHelper.GetAccessTokenFromHybridApp(spHostUrl);
        }
        #endregion
    }
}
