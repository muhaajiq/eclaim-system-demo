using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MHA.ECLAIM.Web.Components.Pages.Create
{
    [Authorize]
    public partial class CreateRequestForm : ComponentBase
    {
        #region Injection Services
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] private NavigationHelper NavigationHelper { get; set; }
        [Inject] private TokenHelper TokenHelper { get; set; }
        [Inject] private IClaimProcess IClaimProcess { get; set; }
        [Inject] private LogHelper LogHelper { get; set; }
        [Inject] private TempMessageService MessageService { get; set; }
        [Inject] private IJSRuntime JS { get; set; }
        #endregion

        #region Query parameters & route parameters
        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.SPHostUrl)]
        [Parameter] public string spHostUrl { get; set; } = string.Empty;

        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.RequestId)]
        public string RequestId { get; set; } = string.Empty;
        #endregion

        #region View models & state
        public string accessToken = string.Empty;
        public MainClaimHeaderVM mainClaimHeaderVM = new();
        private CancellationTokenSource _errorAlertCts;
        private bool showErrorAlert = false;
        private bool showFormErrorAlert = false;
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
                await LoadDataAsync();
            }

            if (mainClaimHeaderVM.HasError)
            {
                await JS.InvokeVoidAsync("jsInterop.scrollToError", "errorAlertTop");
                StateHasChanged();
            }
        }

        private async Task OnClaimChanged()
        {
            StateHasChanged();
        }

        private void OnClaimCurrencyChanged(object value)
        {
            var selectedCurrency = value?.ToString();
            mainClaimHeaderVM.ClaimCurrency = selectedCurrency;

            if (mainClaimHeaderVM.SubClaimDetails != null)
            {
                foreach (var claim in mainClaimHeaderVM.SubClaimDetails)
                {
                    var exchangeRate = GetExchangeRate(claim.TransactionCurrency, selectedCurrency);
                    claim.ExchangeRate = Decimal.Round(exchangeRate, 6, MidpointRounding.AwayFromZero);

                    claim.SubtotalClaimAmount = Decimal.Round(
                        claim.TransactionAmount.GetValueOrDefault() * claim.ExchangeRate.GetValueOrDefault(),
                        2
                    );
                }
            }

            StateHasChanged();
        }

        #endregion

        #region Load Data
        private async Task LoadDataAsync()
        {
            try
            {
                await JS.InvokeVoidAsync("showSwalLoading", "Loading Request Form", "Please wait while we load the request form...");

                if (!string.IsNullOrEmpty(RequestId))
                {
                    mainClaimHeaderVM.ID = Convert.ToInt32(RequestId);
                }

                mainClaimHeaderVM = await IClaimProcess.InitClaim(mainClaimHeaderVM, spHostUrl, accessToken);

                if (mainClaimHeaderVM.HasError)
                {
                    showErrorAlert = true;
                }

                if (!string.IsNullOrWhiteSpace(mainClaimHeaderVM.TravelRequestFormRef))
                {
                    var selectedItem = mainClaimHeaderVM.TravelRequestRefNoList
                        .FirstOrDefault(x => x.Text == mainClaimHeaderVM.TravelRequestFormRef);

                    if (selectedItem != null)
                    {
                        mainClaimHeaderVM.SelectedTravelRequestId = selectedItem.Id;
                    }
                }

                DateOfRequest = mainClaimHeaderVM.RequestDate.Value.ToString(ConstantHelper.DateFormat.DefaultDateFormat) ?? string.Empty;
                await CloseLoadingAsync();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, "Failed to load request form.", "[UI] CreateRequestForm - LoadDataAsync Error");
            }
        }
        #endregion

        #region Button Handlers
        private async Task HandleSaveAsync()
        {
            if (!ValidateAndHandleErrors()) return;

            try
            {
                await JS.InvokeVoidAsync("showSwalLoading");
                await RefreshTokensAsync();

                mainClaimHeaderVM = await IClaimProcess.SaveNewRequestForm(mainClaimHeaderVM, spHostUrl, accessToken);
                await JS.InvokeVoidAsync("Swal.close");
                if (mainClaimHeaderVM.IsSuccessful)
                {
                    await JS.InvokeVoidAsync("showSwalSuccess", "Your request has been saved");
                    MessageService.SuccessMessage = ConstantHelper.SuccessMessage.SaveDraftSuccess;
                    Navigation.NavigateTo(NavigationHelper.BuildUrl("/Home"));
                }
                else if (mainClaimHeaderVM.HasError)
                {
                    showFormErrorAlert = true;
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, "Failed to save your request.", "[UI] CreateRequestForm - ClaimAction Error");
            }
        }

        private async Task HandleSubmitAsync()
        {
            if (!ValidateAndHandleErrors()) return;

            try
            {
                await JS.InvokeVoidAsync("showSwalLoading");
                await RefreshTokensAsync();

                mainClaimHeaderVM = await IClaimProcess.SubmitClaimRequestForm(mainClaimHeaderVM, spHostUrl, accessToken);
                await JS.InvokeVoidAsync("Swal.close");
                if (mainClaimHeaderVM.IsSuccessful)
                {
                    await JS.InvokeVoidAsync("showSwalSuccess", "Your request has been submitted");
                    MessageService.SuccessMessage = ConstantHelper.SuccessMessage.SubmitRequestSuccess;
                    Navigation.NavigateTo(NavigationHelper.BuildUrl("/Home"));
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, "Failed to submit your request.", "[UI] CreateRequestForm - ClaimAction Error");
            }
        }

        private void Close()
        {
            Navigation.NavigateTo(NavigationHelper.BuildUrl("/Home"));
        }
        #endregion

        #region Validation
        private bool ValidateAndHandleErrors()
        {
            var errorMessages = new List<string>();

            var missingFieldsMessage = GetMissingRequiredFieldsMessage(mainClaimHeaderVM);
            if (!string.IsNullOrEmpty(missingFieldsMessage))
            {
                errorMessages.Add(missingFieldsMessage);
            }

            if (errorMessages.Any())
            {
                _errorAlertCts?.Cancel();

                mainClaimHeaderVM.HasError = true;
                mainClaimHeaderVM.ErrorMessage = string.Join("<br/><br/>", errorMessages);
                showFormErrorAlert = true;

                _errorAlertCts = new CancellationTokenSource();
                var token = _errorAlertCts.Token;

                _ = AutoDismissErrorAsync(token);
                StateHasChanged();
                return false;
            }

            mainClaimHeaderVM.HasError = false;
            mainClaimHeaderVM.ErrorMessage = string.Empty;
            return true;
        }

        public string GetMissingRequiredFieldsMessage(MainClaimHeaderVM vm)
        {
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(vm.PurposeOfClaims))
                missingFields.Add("Purpose of Claims");

            if (string.IsNullOrWhiteSpace(vm.ClaimCurrency))
                missingFields.Add("Claim Currency");

            if (string.IsNullOrWhiteSpace(vm.SelectedClaimCategorySpId))
                missingFields.Add("Category");

            if (vm.SubClaimDetails == null || !vm.SubClaimDetails.Any())
                missingFields.Add("At least one Claim Item");

            if (missingFields.Count == 0)
                return string.Empty;

            return "Please fill in the following required fields before saving:<br /><br />" +
                string.Join("<br />", missingFields.Select(f => $"• {f}"));
        }
        #endregion

        #region UI Helpers
        private async Task CloseLoadingAsync()
        {
            StateHasChanged();
            await JS.InvokeVoidAsync("Swal.close");
        }

        private async Task HandleExceptionAsync(Exception ex, string defaultMessage, string logPrefix)
        {
            await CloseLoadingAsync();

            var message = ex switch
            {
                TimeoutException => ConstantHelper.ErrorMessage.TimedOutError,
                HttpRequestException => ConstantHelper.ErrorMessage.NetworkError,
                ApplicationException => ex.Message,
                _ => ConstantHelper.ErrorMessage.UnexpectedErrorOccur
            };

            await JS.InvokeVoidAsync("showSwalError", message);
            LogHelper.LogMessage($"{logPrefix}: {ex}");

            mainClaimHeaderVM.HasError = true;
            mainClaimHeaderVM.ErrorMessage = message;
            SetError(defaultMessage);
        }

        private void SetSelectedCategoryText()
        {
            var category = mainClaimHeaderVM.ClaimCategories?
                .FirstOrDefault(c => c.Id == mainClaimHeaderVM.SelectedClaimCategorySpId);

            if (category != null)
            {
                mainClaimHeaderVM.ClaimCategory = category.Text;
            }
        }

        private bool DisableClaimInfo => mainClaimHeaderVM?.SubClaimDetails != null
                                 && mainClaimHeaderVM.SubClaimDetails.Any();

        private bool IsTravelCategory(string selectedCategoryId)
        {
            var category = mainClaimHeaderVM.ClaimCategories
                ?.FirstOrDefault(c => c.Id == selectedCategoryId);

            if (category == null) return false;

            return category.Text == "Travel HR" || category.Text == "Travel Finance";
        }

        private void OnTravelRequestChanged(object value)
        {
            var selectedId = value?.ToString();
            mainClaimHeaderVM.SelectedTravelRequestId = selectedId;

            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                var selectedItem = mainClaimHeaderVM.TravelRequestRefNoList
                    .FirstOrDefault(x => x.Id == selectedId);

                mainClaimHeaderVM.TravelRequestFormRef = selectedItem?.Text ?? string.Empty;
            }

            StateHasChanged();
        }

        #endregion

        #region Helpers
        private async Task RefreshTokensAsync()
        {
            accessToken = await TokenHelper.GetUserAccessToken();
        }

        private void SetError(string message)
        {
            _errorAlertCts?.Cancel();

            mainClaimHeaderVM.HasError = true;
            mainClaimHeaderVM.ErrorMessage = message;
            showFormErrorAlert = true;

            _errorAlertCts = new CancellationTokenSource();
            var token = _errorAlertCts.Token;

            _ = AutoDismissErrorAsync(token);
            StateHasChanged();
        }

        private async Task AutoDismissErrorAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(10000, token);
                showFormErrorAlert = false;
                mainClaimHeaderVM.HasError = false;
                StateHasChanged();
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void DismissErrorAlert()
        {
            showFormErrorAlert = false;
            mainClaimHeaderVM.HasError = false;

            _errorAlertCts?.Cancel();
        }

        private string BuildTravelRequestUrl(string travelRequestHeaderId)
        {
            var baseUrl = JSONAppSettings.TravelAppUrl?.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return NavigationHelper.BuildUrl($"/DisplayForm", (ConstantHelper.ParameterQuery.RequestId, travelRequestHeaderId));
            }

            var url = $"{baseUrl}/DisplayForm";
            return NavigationHelper.BuildUrl(url, (ConstantHelper.ParameterQuery.RequestId, travelRequestHeaderId));
        }

        private decimal GetExchangeRate(string baseCurrency, string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
                return 1.000000m;

            var rate = mainClaimHeaderVM.CurrencyExchangeRates?
                .FirstOrDefault(r =>
                    string.Equals(r.BaseCurrency, baseCurrency, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.TargetCurrency, targetCurrency, StringComparison.OrdinalIgnoreCase));

            return rate?.Rate ?? 1.000000m; // default 1 if not found
        }

        #endregion
    }
}
