using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Process.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using System.Data.SqlTypes;
using System.Text.Json;

namespace MHA.ECLAIM.Web.Components.PartialComponents
{
    public partial class ClaimDetailsTab
    {
        #region Injection Services
        [Inject] private IJSRuntime JS { get; set; }
        [Inject] private IClaimProcess IClaimProcess { get; set; }
        [Inject] private LogHelper LogHelper { get; set; }
        #endregion

        #region Parameters
        [Parameter, EditorRequired]
        public MainClaimHeaderVM MainClaimHeaderVM { get; set; }
        [SupplyParameterFromQuery(Name = ConstantHelper.ParameterQuery.SPHostUrl)]
        [Parameter] public string spHostUrl { get; set; }
        [Parameter] public string accessToken { get; set; }
        [Parameter] public EventCallback<SubClaimDetailsVM> OnClaimChanged { get; set; }
        #endregion

        #region State Variables
        private bool IsEdit = false;
        private string FileNameDisplay { get; set; } = string.Empty;
        private byte[]? FileBytes { get; set; }
        private bool IsSaveDisabled => string.IsNullOrEmpty(FileNameDisplay);
        private RadzenUpload? uploadRef;
        private bool _isClearing = false;
        private string tempAttachmentPathUrl = string.Empty;
        private string tempAttachmentFileName = string.Empty;
        private byte[]? tempAttachmentFileBytes;
        private bool isFileMarkedForDelete = false;
        private bool IsReadonly = false;
        private bool _allowManualTransactionAmount;
        private SubClaimDetailsVM backupClaimDetails = new();
        #endregion

        #region Filtered Expenses
        private IEnumerable<DropDownListItem> FilteredExpenses
        {
            get
            {
                if (MainClaimHeaderVM?.ExpensesList == null || MainClaimHeaderVM.ExpensesMaintenanceList == null)
                    return Enumerable.Empty<DropDownListItem>();

                var now = DateTime.Now.Date;

                var filtered = MainClaimHeaderVM.ExpensesList.Where(dd =>
                {
                    // Match Claim Category
                    if (!string.IsNullOrWhiteSpace(SelectedClaimCategoryText) && dd.SubText != SelectedClaimCategoryText)
                        return false;

                    var maintenance = MainClaimHeaderVM.ExpensesMaintenanceList
                        .FirstOrDefault(m => m.Expenses == dd.Value);

                    if (maintenance == null) return false;

                    // Match Claim Entitlement Type
                    if (MainClaimHeaderVM.CurrentClaimEntitlementTypeSpId.HasValue &&
                        maintenance.ClaimEntitlementTypeSpId.HasValue &&
                        maintenance.ClaimEntitlementTypeSpId.Value != MainClaimHeaderVM.CurrentClaimEntitlementTypeSpId.Value)
                        return false;

                    // Check validity
                    if (!IsEmployeeEntitlementCurrentlyValid(maintenance.ClaimEntitlementTypeSpId))
                        return false;

                    return true;
                });

                return filtered
                    .GroupBy(f => f.Text)
                    .Select(g => g.First())
                    .ToList();
            }
        }

        private IEnumerable<DropDownListItem> FilteredSubtypes
        {
            get
            {
                if (MainClaimHeaderVM?.ExpensesSubtypeList == null || MainClaimHeaderVM.CurrentClaimDetails == null || MainClaimHeaderVM.ExpensesMaintenanceList == null)
                    return Enumerable.Empty<DropDownListItem>();

                var selectedExpenseId = MainClaimHeaderVM.CurrentClaimDetails.Expenses;
                if (string.IsNullOrWhiteSpace(selectedExpenseId)) return Enumerable.Empty<DropDownListItem>();

                var now = DateTime.Now.Date;
                var expense = MainClaimHeaderVM.ExpensesList.FirstOrDefault(e => e.Value == selectedExpenseId);
                if (expense == null) return Enumerable.Empty<DropDownListItem>();

                var filteredSubtypes = MainClaimHeaderVM.ExpensesSubtypeList
                .Where(st =>
                    st.SubText == expense.Text
                    && (string.IsNullOrWhiteSpace(SelectedClaimCategoryText)
                        || MainClaimHeaderVM.ExpensesMaintenanceList.Any(m =>
                            m.ID.ToString() == st.Id &&
                            m.ClaimCategory == SelectedClaimCategoryText))
                )
                .Select(st =>
                {
                    var m = MainClaimHeaderVM.ExpensesMaintenanceList.FirstOrDefault(x => x.ID.ToString() == st.Id);
                    if (m == null) return null;

                    // Match Claim Entitlement Type
                    if (MainClaimHeaderVM.CurrentClaimEntitlementTypeSpId.HasValue &&
                        m.ClaimEntitlementTypeSpId.HasValue &&
                        m.ClaimEntitlementTypeSpId.Value != MainClaimHeaderVM.CurrentClaimEntitlementTypeSpId.Value)
                        return null;

                    // Check validity
                    if (!IsEmployeeEntitlementCurrentlyValid(m.ClaimEntitlementTypeSpId))
                        return null;

                    return new DropDownListItem
                    {
                        Text = st.Text,
                        Value = st.Value
                    };
                })
                .Where(st => st != null)
                .ToList();

                return filteredSubtypes;
            }
        }

        private bool IsEmployeeEntitlementCurrentlyValid(int? claimEntitlementSpId)
        {
            if (!claimEntitlementSpId.HasValue)
                return true;

            var now = DateTime.Now.Date;

            bool IsValid(DateTime? startDate, DateTime? endDate)
            {
                if (!startDate.HasValue)
                    return false;

                // If no end date still valid from start date onward
                if (!endDate.HasValue)
                    return now >= startDate.Value.Date;

                // Both start and end must be within range
                return now >= startDate.Value.Date && now <= endDate.Value.Date;
            }

            bool isType1Valid = IsValid(MainClaimHeaderVM.ClaimEntitlementType1StartDate, MainClaimHeaderVM.ClaimEntitlementType1EndDate);
            bool isType2Valid = IsValid(MainClaimHeaderVM.ClaimEntitlementType2StartDate, MainClaimHeaderVM.ClaimEntitlementType2EndDate);

            // Check if the provided entitlement ID corresponds to the active one
            if (MainClaimHeaderVM.ClaimEntitlementType1SpId == claimEntitlementSpId && isType1Valid)
                return true;

            if (MainClaimHeaderVM.ClaimEntitlementType2SpId == claimEntitlementSpId && isType2Valid)
                return true;

            if (isType1Valid || isType2Valid)
                return true;

            return false;
        }

        private void RecalculateAmounts(bool raiseStateChanged = true)
        {
            if (MainClaimHeaderVM?.CurrentClaimDetails == null) return;

            var current = MainClaimHeaderVM.CurrentClaimDetails;
            var qty = current.Quantity ?? 0m;

            var txnCurrency = string.IsNullOrWhiteSpace(current.TransactionCurrency)
                                ? MainClaimHeaderVM.ClaimCurrency
                                : current.TransactionCurrency;
            var claimCurrency = MainClaimHeaderVM.ClaimCurrency ?? txnCurrency;

            var rateTxnToClaim = GetExchangeRate(txnCurrency, claimCurrency);
            current.ExchangeRate = Decimal.Round(rateTxnToClaim, 6, MidpointRounding.AwayFromZero);

            if (!AllowManualTransactionAmount)
            {
                var rateClaimToTxn = GetExchangeRate(claimCurrency, txnCurrency);
                rateClaimToTxn = Decimal.Round(rateClaimToTxn, 6, MidpointRounding.AwayFromZero);

                var unitPrice = current.UnitPrice * rateClaimToTxn;
                var txnAmountOriginal = qty * unitPrice;

                current.TransactionAmount = txnAmountOriginal;
                current.UnitPriceConverted = unitPrice;
                current.TransactionAmountConverted = txnAmountOriginal * rateTxnToClaim;
            }
            else
            {
                var txnAmountOriginal = current.TransactionAmount ?? 0m;
                current.TransactionAmountConverted = txnAmountOriginal * rateTxnToClaim;
                current.UnitPriceConverted = 0m;
            }

            current.SubtotalClaimAmount = current.TransactionAmountConverted ?? 0m;
            MainClaimHeaderVM.TotalClaimAmount = MainClaimHeaderVM.SubClaimDetails?.Sum(x => x.SubtotalClaimAmount ?? 0) ?? 0m;

            if (raiseStateChanged) StateHasChanged();
        }
        #endregion

        #region OnChange
        private void OnExpenseChanged(object value)
        {
            MainClaimHeaderVM.CurrentClaimDetails.ExpensesSubtype = null;
            MainClaimHeaderVM.CurrentClaimDetails.Quantity = 1;
            MainClaimHeaderVM.CurrentClaimDetails.UnitPrice = 0m;
            MainClaimHeaderVM.CurrentClaimDetails.UnitPriceConverted = 0m;

            MainClaimHeaderVM.CurrentClaimDetails.ClaimEntitlementTypeSpId = null;
            MainClaimHeaderVM.CurrentClaimDetails.ClaimLimit = null;
            MainClaimHeaderVM.CurrentClaimDetails.EffectiveStartDate = null;
            MainClaimHeaderVM.CurrentClaimDetails.EffectiveEndDate = null;
        }

        private async Task OnSubtypeChanged(object value)
        {
            var selectedSubtype = value.ToString();
            if (string.IsNullOrWhiteSpace(selectedSubtype)) return;

            var m = MainClaimHeaderVM.ExpensesMaintenanceList.FirstOrDefault(x => x.ExpensesSubtype == selectedSubtype);
            if (m == null) return;

            if (m.FixedQuantity)
            {
                MainClaimHeaderVM.CurrentClaimDetails.Quantity = m.FixedQuantityValue.GetValueOrDefault(1);
            }

            if (m.FixedAmount)
            {
                MainClaimHeaderVM.CurrentClaimDetails.UnitPrice = m.FixedAmountValue.GetValueOrDefault(0m);
            }

            AllowManualTransactionAmount = MainClaimHeaderVM.CurrentClaimDetails.UnitPrice == null || MainClaimHeaderVM.CurrentClaimDetails.UnitPrice == 0;
            MainClaimHeaderVM.CurrentClaimDetails.ClaimEntitlementTypeSpId = m.ClaimEntitlementTypeSpId;
            MainClaimHeaderVM.CurrentClaimDetails.ClaimLimit = m.ClaimLimit;
            MainClaimHeaderVM.CurrentClaimDetails.EffectiveStartDate = m.EffectiveStartDate;
            MainClaimHeaderVM.CurrentClaimDetails.EffectiveEndDate = m.EffectiveEndDate;

            RecalculateAmounts();
        }

        private void OnQuantityChanged(decimal? value)
        {
            if (!value.HasValue || value.Value < 0) return;
            MainClaimHeaderVM.CurrentClaimDetails.Quantity = value.Value;
            RecalculateAmounts();
        }

        private void OnTransactionCurrencyChanged(object value)
        {
            var currency = value?.ToString();
            MainClaimHeaderVM.CurrentClaimDetails.TransactionCurrency = currency;
            RecalculateAmounts();
        }

        private void OnTransactionAmountChanged(decimal? value)
        {
            if (!AllowManualTransactionAmount) return;
            MainClaimHeaderVM.CurrentClaimDetails.TransactionAmount = value ?? 0m;
            RecalculateAmounts();
        }
        #endregion

        #region Modal Handlers
        private async Task openAddClaimDetailsModal()
        {
            IsEdit = false;

            if (string.IsNullOrWhiteSpace(MainClaimHeaderVM.ClaimCurrency) || string.IsNullOrWhiteSpace(SelectedClaimCategoryText))
            {
                await JS.InvokeVoidAsync("showSwalValidationWarning", "Validation Error", "Please select both Claim Currency and Claim Category before adding an item.");
                return;
            }

            try
            {
                await JS.InvokeVoidAsync("showSwalLoading", "Loading Expenses", "Please wait while we load the claim details...");

                var newMainClaim = await IClaimProcess.InitExpensesList(spHostUrl, accessToken);

                await JS.InvokeVoidAsync("Swal.close");

                if (newMainClaim.HasError)
                {
                    await JS.InvokeVoidAsync("showSwalError", "Failed to get maintenance list", MainClaimHeaderVM.ErrorMessage);
                    return;
                }

                MainClaimHeaderVM.ExpensesList = newMainClaim.ExpensesList;
                MainClaimHeaderVM.ExpensesSubtypeList = newMainClaim.ExpensesSubtypeList;
                MainClaimHeaderVM.ExpensesMaintenanceList = newMainClaim.ExpensesMaintenanceList;
                MainClaimHeaderVM.CurrentClaimDetails = newMainClaim.CurrentClaimDetails;

                tempAttachmentPathUrl = null;
                tempAttachmentFileName = null;
                tempAttachmentFileBytes = null;
                FileNameDisplay = string.Empty;
                isFileMarkedForDelete = false;

                AllowManualTransactionAmount = MainClaimHeaderVM.CurrentClaimDetails.UnitPrice == null || MainClaimHeaderVM.CurrentClaimDetails.UnitPrice == 0;
                await JS.InvokeVoidAsync("jsInterop.showModal", "#claimDetailsModal");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("Swal.close");

                string userFriendlyMessage = "Unable to load claim maintenance list.";

                if (ex is TimeoutException)
                    userFriendlyMessage = ConstantHelper.ErrorMessage.TimedOutError;
                else if (ex is HttpRequestException)
                    userFriendlyMessage = ConstantHelper.ErrorMessage.NetworkError;
                else if (ex is ApplicationException)
                    userFriendlyMessage = ex.Message;
                else
                    userFriendlyMessage = ConstantHelper.ErrorMessage.UnexpectedErrorOccur;

                await JS.InvokeVoidAsync("showSwalError", "Error", userFriendlyMessage);
                LogHelper.LogMessage("[UI] openAddClaimDetailsModal - Error fetching Expenses list: " + ex);
            }
        }

        private async Task openEditClaimModal(SubClaimDetailsVM claim)
        {
            if (claim == null) return;

            IsReadonly = false;
            IsEdit = true;
            isFileMarkedForDelete = false;

            var clone = JsonSerializer.Deserialize<SubClaimDetailsVM>(JsonSerializer.Serialize(claim));
            if (clone != null)
            {
                backupClaimDetails = clone;
            }

            MainClaimHeaderVM.CurrentClaimDetails = claim;
            AllowManualTransactionAmount = MainClaimHeaderVM.CurrentClaimDetails.UnitPrice == null || MainClaimHeaderVM.CurrentClaimDetails.UnitPrice == 0;

            tempAttachmentPathUrl = claim.AttachmentPathUrl;
            tempAttachmentFileName = claim.AttachmentFileName;
            tempAttachmentFileBytes = null;
            FileNameDisplay = tempAttachmentFileName ?? string.Empty;

            await JS.InvokeVoidAsync("jsInterop.showModal", "#claimDetailsModal");
        }

        private async Task openDeleteSelectedModal()
        {
            if (!MainClaimHeaderVM.SubClaimDetails.Any(x => x.IsSelected))
            {
                await JS.InvokeVoidAsync("showSwalWarning", "Selection Required", ConstantHelper.ErrorMessage.RemoveClaimDetailsWarning);
                return;
            }
            await JS.InvokeVoidAsync("jsInterop.showModal", "#deleteClaimDetailsModal");
        }

        private string SelectedClaimCategoryText
        {
            get
            {
                if (MainClaimHeaderVM?.ClaimCategories == null || string.IsNullOrEmpty(MainClaimHeaderVM.SelectedClaimCategorySpId))
                    return string.Empty;

                foreach (var c in MainClaimHeaderVM.ClaimCategories)
                {
                    if (c.Id == MainClaimHeaderVM.SelectedClaimCategorySpId)
                    {
                        MainClaimHeaderVM.ClaimCategory = c.Text;
                        return c.Text;
                    }
                }

                return string.Empty;
            }
        }

        public bool AllowManualTransactionAmount
        {
            get => _allowManualTransactionAmount;
            private set
            {
                _allowManualTransactionAmount = value;

                if (value && MainClaimHeaderVM?.CurrentClaimDetails != null)
                {
                    MainClaimHeaderVM.CurrentClaimDetails.Quantity = 1;
                }
            }
        }

        #endregion

        #region Claim Save/Delete
        private async Task saveClaimDetails()
        {
            var currentClaimDetails = MainClaimHeaderVM.CurrentClaimDetails;
            if (currentClaimDetails == null) return;

            var validationError = GetMissingQuotationFieldsMessage(currentClaimDetails);
            if (!string.IsNullOrEmpty(validationError))
            {
                await JS.InvokeVoidAsync("showSwalValidationWarning", "Validation Error", validationError);
                return;
            }

            SubClaimDetailsVM maintenance = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(currentClaimDetails.ExpensesSubtype))
                {
                    maintenance = MainClaimHeaderVM.ExpensesMaintenanceList.FirstOrDefault(m => m.ExpensesSubtype == currentClaimDetails.ExpensesSubtype);
                }
                else if (!string.IsNullOrWhiteSpace(currentClaimDetails.Expenses))
                {
                    var expenseText = MainClaimHeaderVM.ExpensesList.FirstOrDefault(e => e.Value == currentClaimDetails.Expenses)?.Text;
                    maintenance = MainClaimHeaderVM.ExpensesMaintenanceList.FirstOrDefault(m => m.Expenses == expenseText);
                }

                if (maintenance != null)
                {
                    var txnDate = currentClaimDetails.TransactionDate.Value.Date;
                    // 1) Effective range
                    if (maintenance.EffectiveStartDate.HasValue && maintenance.EffectiveEndDate.HasValue)
                    {
                        if (txnDate < maintenance.EffectiveStartDate.Value.Date || txnDate > maintenance.EffectiveEndDate.Value.Date)
                        {
                            await JS.InvokeVoidAsync("showSwalValidationWarning", "Validation Error",
                                string.Format(ConstantHelper.ErrorMessage.EffectiveDateWarning,
                                    maintenance.EffectiveStartDate.Value.ToString(ConstantHelper.DateFormat.DefaultDateFormat),
                                    maintenance.EffectiveEndDate.Value.ToString(ConstantHelper.DateFormat.DefaultDateFormat)));
                            return;
                        }
                    }

                    // 2) Entitlement
                    if (maintenance.ClaimEntitlementTypeSpId.HasValue)
                    {
                        if (!IsEmployeeEntitlementValidForDate(maintenance.ClaimEntitlementTypeSpId.Value, txnDate))
                        {
                            await JS.InvokeVoidAsync("showSwalValidationWarning", "Validation Error", ConstantHelper.ErrorMessage.EntitlementWarning);
                            return;
                        }
                    }

                    // 3) Frequency + Limit check
                    if (maintenance.ClaimFrequencyEnum.HasValue)
                    {
                        var frequency = maintenance.ClaimFrequencyEnum.Value;
                        var claimLimit = maintenance.ClaimLimit.Value;

                        var existingClaims = MainClaimHeaderVM.SubClaimDetails
                            .Where(c => c.ExpensesSubtype == currentClaimDetails.ExpensesSubtype && c != currentClaimDetails)
                            .ToList();

                        decimal totalAmount = 0;
                        bool exceedsFrequency = false;

                        switch (frequency)
                        {
                            case ClaimFrequency.PerReceipt:
                                totalAmount = (currentClaimDetails.TransactionAmountConverted ?? 0);
                                exceedsFrequency = totalAmount > claimLimit;
                                break;

                            case ClaimFrequency.Daily:
                                var sameDayClaims = existingClaims
                                    .Where(c => c.TransactionDate.HasValue && c.TransactionDate.Value.Date == txnDate)
                                    .Sum(c => c.TransactionAmountConverted ?? 0);

                                totalAmount = sameDayClaims + (currentClaimDetails.TransactionAmountConverted ?? 0);
                                exceedsFrequency = totalAmount > claimLimit;
                                break;

                            case ClaimFrequency.Monthly:
                                var sameMonthClaims = existingClaims
                                    .Where(c => c.TransactionDate.HasValue &&
                                                c.TransactionDate.Value.Year == txnDate.Year &&
                                                c.TransactionDate.Value.Month == txnDate.Month)
                                    .Sum(c => c.TransactionAmountConverted ?? 0);

                                var totalMonth = sameMonthClaims + (currentClaimDetails.TransactionAmountConverted ?? 0);
                                exceedsFrequency = totalMonth > claimLimit;
                                break;

                            case ClaimFrequency.Lifetime:
                                var lifetimeClaims = existingClaims.Sum(c => c.TransactionAmountConverted ?? 0);
                                totalAmount = lifetimeClaims + (currentClaimDetails.TransactionAmountConverted ?? 0);
                                exceedsFrequency = totalAmount > claimLimit;
                                break;
                        }

                        if (exceedsFrequency)
                        {
                            var totalStr = $"<b>{totalAmount:N2} {MainClaimHeaderVM.ClaimCurrency}</b>";
                            var limitStr = $"<b>{claimLimit:N2} {MainClaimHeaderVM.ClaimCurrency}</b>";
                            var warningMsg = string.Format(ConstantHelper.ErrorMessage.ClaimLimitWarning, totalStr, frequency, limitStr);

                            bool proceed = await JS.InvokeAsync<bool>("showSwalConfirm", "Limit Exceeded", warningMsg);
                            if (!proceed) return;
                        }
                    }

                    currentClaimDetails.ClaimEntitlementTypeSpId = maintenance.ClaimEntitlementTypeSpId;
                    currentClaimDetails.ClaimEntitlementType = maintenance.ClaimEntitlementType;
                    currentClaimDetails.ClaimLimit = maintenance.ClaimLimit;
                    currentClaimDetails.EffectiveStartDate = maintenance.EffectiveStartDate;
                    currentClaimDetails.EffectiveEndDate = maintenance.EffectiveEndDate;
                }

                if (isFileMarkedForDelete)
                {
                    await IClaimProcess.DeleteClaimAttachment(MainClaimHeaderVM, spHostUrl, accessToken);
                    currentClaimDetails.AttachmentPathUrl = null;
                    currentClaimDetails.AttachmentFileName = null;
                }

                if (!string.IsNullOrWhiteSpace(tempAttachmentFileName) && tempAttachmentFileBytes != null)
                {
                    currentClaimDetails.AttachmentFileName = tempAttachmentFileName;
                    currentClaimDetails.AttachmentFileBytes = tempAttachmentFileBytes;
                }

                if (!IsEdit)
                {
                    MainClaimHeaderVM.SubClaimDetails.Add(currentClaimDetails);
                }

                await JS.InvokeVoidAsync("showSwalLoading", "Saving Claim", "Saving claim details...Please wait.");
                var model = await IClaimProcess.SaveClaimDetails(MainClaimHeaderVM, spHostUrl, accessToken);
                if (model.HasError)
                {
                    await JS.InvokeVoidAsync("showSwalError", "Save Failed", model.ErrorMessage ?? ConstantHelper.ErrorMessage.SaveFailed);
                    LogHelper.LogMessage("[UI] saveClaimDetails - API returned error: " + model.ErrorMessage);
                    return;
                }
                await JS.InvokeVoidAsync("showSwalSuccess", ConstantHelper.SuccessMessage.AddClaimPopup);
                SubClaimDetailsVM updatedSubClaim = model.SubClaimDetails.FirstOrDefault(q => q.TempId == currentClaimDetails.TempId);

                // Update the same object in UI list
                if (updatedSubClaim != null)
                {
                    currentClaimDetails.AttachmentPathUrl = updatedSubClaim.AttachmentPathUrl;
                    currentClaimDetails.AttachmentFileName = updatedSubClaim.AttachmentFileName;
                    currentClaimDetails.AttachmentFileBytes = updatedSubClaim.AttachmentFileBytes;

                    var rate = GetExchangeRate(updatedSubClaim.TransactionCurrency, MainClaimHeaderVM.ClaimCurrency);
                    currentClaimDetails.ExchangeRate = Decimal.Round(rate, 6, MidpointRounding.AwayFromZero);
                    currentClaimDetails.ClaimCurrency = MainClaimHeaderVM.ClaimCurrency;
                    currentClaimDetails.ClaimCategory = MainClaimHeaderVM.ClaimCategory;
                    currentClaimDetails.SubtotalClaimAmount = updatedSubClaim.TransactionAmountConverted;
                }
                else
                {
                    MainClaimHeaderVM.CurrentClaimDetails = updatedSubClaim;
                }

                MainClaimHeaderVM.TotalClaimAmount = TotalClaimAmount;
                tempAttachmentPathUrl = currentClaimDetails.AttachmentPathUrl;
                tempAttachmentFileName = currentClaimDetails.AttachmentFileName;
                tempAttachmentFileBytes = null;
                FileNameDisplay = string.Empty;
                isFileMarkedForDelete = false;

                await OnClaimChanged.InvokeAsync(MainClaimHeaderVM.CurrentClaimDetails);
                await JS.InvokeVoidAsync("jsInterop.hideModal", "#claimDetailsModal");
            }
            catch (Exception ex)
            {
                await JS.InvokeVoidAsync("Swal.close");

                string userFriendlyMessage;
                if (ex is TimeoutException)
                    userFriendlyMessage = ConstantHelper.ErrorMessage.TimedOutError;
                else if (ex is HttpRequestException)
                    userFriendlyMessage = ConstantHelper.ErrorMessage.NetworkError;
                else if (ex is ApplicationException)
                    userFriendlyMessage = ex.Message;
                else
                    userFriendlyMessage = ConstantHelper.ErrorMessage.UnexpectedErrorOccur;

                await JS.InvokeVoidAsync("showSwalError", "Failed to Save", userFriendlyMessage);
                LogHelper.LogMessage("[UI] saveClaimDetails - Error: " + ex);
                StateHasChanged();
            }
        }

        private bool IsEmployeeEntitlementValidForDate(int entitlementSpId, DateTime txnDate)
        {
            txnDate = txnDate.Date;

            bool IsValid(DateTime? startDate, DateTime? endDate)
            {
                if (!startDate.HasValue)
                    return false;

                // If no end date still valid from start date onward
                if (!endDate.HasValue)
                    return txnDate >= startDate.Value.Date;

                // Both start and end must be within range
                return txnDate >= startDate.Value.Date && txnDate <= endDate.Value.Date;
            }

            bool isType1Valid = IsValid(MainClaimHeaderVM.ClaimEntitlementType1StartDate, MainClaimHeaderVM.ClaimEntitlementType1EndDate);
            bool isType2Valid = IsValid(MainClaimHeaderVM.ClaimEntitlementType2StartDate, MainClaimHeaderVM.ClaimEntitlementType2EndDate);

            // Check if the provided entitlement ID corresponds to the active one
            if (MainClaimHeaderVM.ClaimEntitlementType1SpId == entitlementSpId && isType1Valid)
                return true;

            if (MainClaimHeaderVM.ClaimEntitlementType2SpId == entitlementSpId && isType2Valid)
                return true;

            if (isType1Valid || isType2Valid)
                return true;

            return false;
        }

        private async Task deleteSelectedClaimDetails()
        {
            var selectedItems = MainClaimHeaderVM.SubClaimDetails.Where(x => x.IsSelected).ToList();

            foreach (var item in selectedItems)
            {
                if (!string.IsNullOrWhiteSpace(item.AttachmentPathUrl))
                {
                    try
                    {
                        MainClaimHeaderVM.CurrentClaimDetails = item;
                        var success = await IClaimProcess.DeleteClaimDetails(MainClaimHeaderVM, spHostUrl, accessToken);

                        if (!success)
                        {
                            await JS.InvokeVoidAsync("showSwalWarning", "Delete Failed", $"Unable to delete {item.AttachmentFileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.LogMessage("Error deleting attachment: " + ex);
                        await JS.InvokeVoidAsync("showSwalError", "Delete Error", $"Failed to delete {item.AttachmentFileName}");
                    }
                }

                MainClaimHeaderVM.SubClaimDetails.Remove(item);
            }

            await OnClaimChanged.InvokeAsync(MainClaimHeaderVM.CurrentClaimDetails);
            await JS.InvokeVoidAsync("jsInterop.hideModal", "#deleteClaimDetailsModal");
        }

        private async Task CloseClaimDetailsModal()
        {
            if(IsEdit && backupClaimDetails != null)
            {
                var index = MainClaimHeaderVM.SubClaimDetails.FindIndex(x => x.ID == backupClaimDetails.ID);

                if (index != -1)
                {
                    var restoredClaim = JsonSerializer.Deserialize<SubClaimDetailsVM>(JsonSerializer.Serialize(backupClaimDetails));

                    if (restoredClaim != null)
                    {
                        MainClaimHeaderVM.SubClaimDetails[index] = restoredClaim;
                    }
                }
            }

            isFileMarkedForDelete = false;
            await JS.InvokeVoidAsync("jsInterop.hideModal", "#claimDetailsModal");
        }

        #endregion

        #region File Handling
        private async Task OnFileSelected(UploadChangeEventArgs args)
        {
            if (_isClearing) return;

            var file = args.Files.FirstOrDefault();
            if (file != null)
            {
                string[] allowedExtensions = { ".pdf", ".jpg", ".png" };

                if (!allowedExtensions.Any(ext => file.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                {
                    await JS.InvokeVoidAsync("showSwalError", "Invalid File", ConstantHelper.ErrorMessage.InvalidFileFormat);
                    ClearFileSelection();
                    return;
                }

                try
                {
                    using var stream = file.OpenReadStream(10 * 1024 * 1024); // 10 MB
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    FileBytes = ms.ToArray();

                    FileNameDisplay = file.Name;
                    tempAttachmentFileName = file.Name;
                    tempAttachmentFileBytes = FileBytes;
                }
                catch (IOException)
                {
                    await JS.InvokeVoidAsync("showSwalWarning", "File Too Large", ConstantHelper.ErrorMessage.FileSizeLimitWarning);
                    ClearFileSelection();
                }
            }
            else
            {
                ClearFileSelection();
            }
        }

        private void ClearFileSelection()
        {
            _isClearing = true;

            FileNameDisplay = string.Empty;
            FileBytes = null;

            var currentClaim = MainClaimHeaderVM.CurrentClaimDetails;
            if (currentClaim != null)
            {
                currentClaim.AttachmentFileName = null;
                currentClaim.AttachmentFileBytes = null;
                currentClaim.AttachmentPathUrl = null;
            }

            uploadRef?.ClearFiles();

            _isClearing = false;
            StateHasChanged();
        }

        private void ClearExistingFileSelected()
        {
            isFileMarkedForDelete = true;
            tempAttachmentPathUrl = null;
            tempAttachmentFileName = null;
            FileNameDisplay = string.Empty;
            FileBytes = null;
            StateHasChanged();
        }
        #endregion

        #region Validation & Helpers
        public string GetMissingQuotationFieldsMessage(SubClaimDetailsVM claim)
        {
            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(claim.Expenses)) missingFields.Add("Expenses");
            if (string.IsNullOrWhiteSpace(claim.ExpensesSubtype)) missingFields.Add("Expense Subtype");
            if (claim.TransactionDate == null || claim.TransactionDate <= SqlDateTime.MinValue.Value) missingFields.Add("Transaction Date");
            if (string.IsNullOrWhiteSpace(claim.TransactionCurrency)) missingFields.Add("Transaction Currency");
            if (claim.Quantity <= 0) missingFields.Add("Quantity (must be greater than 0)");
            if (claim.UnitPrice <= 0 && !AllowManualTransactionAmount) missingFields.Add("Unit Price (must be greater than 0)");
            if (string.IsNullOrWhiteSpace(claim.Description)) missingFields.Add("Description");

            if (missingFields.Any())
            {
                return "<div style='text-align:left'><ul><li>" + string.Join("</li><li>", missingFields) + "</li></ul></div>";
            }

            return string.Empty;
        }

        private decimal GetExchangeRate(string baseCurrency, string targetCurrency)
        {
            if (string.IsNullOrWhiteSpace(baseCurrency) || string.IsNullOrWhiteSpace(targetCurrency))
                return 1.000000m;

            var rate = MainClaimHeaderVM.CurrencyExchangeRates?
                .FirstOrDefault(r =>
                    string.Equals(r.BaseCurrency, baseCurrency, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(r.TargetCurrency, targetCurrency, StringComparison.OrdinalIgnoreCase));

            return rate?.Rate ?? 1.000000m; // default 1 if not found
        }

        private decimal TotalClaimAmount => MainClaimHeaderVM.SubClaimDetails?.Sum(x => x.SubtotalClaimAmount ?? 0) ?? 0m;

        #endregion
    }
}
