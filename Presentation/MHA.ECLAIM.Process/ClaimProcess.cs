using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Search;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process.Interface;
using RestSharp;

namespace MHA.ECLAIM.Process
{
    public class ClaimProcess : IClaimProcess
    {
        private readonly RestClient _client;
        private static readonly JSONAppSettings appSettings;

        static ClaimProcess()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public ClaimProcess()
        {
            _client = new RestClient(appSettings.WebAPIUrl);
        }

        #region Init
        public async Task<MainClaimHeaderVM> InitClaim(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            MainClaimHeaderVM result = new MainClaimHeaderVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.InitClaim, Method.Post)
                    .AddJsonBody(new APIRequestDTO<MainClaimHeaderVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<MainClaimHeaderVM> response = await _client.ExecuteAsync<MainClaimHeaderVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    result.HasError = true;
                    result.ErrorMessage = response.ErrorMessage ?? response.ErrorException?.Message ?? "Unknown API error.";
                    logHelper.LogMessage($"[Process] InitClaim failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] InitClaim network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] InitClaim timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - InitClaim Error calling API: {ex}");
                throw;
            }

            return result;
        }

        public async Task<MainClaimHeaderVM> InitExpensesList(string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            MainClaimHeaderVM result = new MainClaimHeaderVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.InitExpensesList, Method.Post)
                    .AddJsonBody(new
                    {
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<MainClaimHeaderVM> response = await _client.ExecuteAsync<MainClaimHeaderVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    result.HasError = true;
                    result.ErrorMessage = response.ErrorMessage ?? response.ErrorException?.Message ?? "Unknown API error.";
                    logHelper.LogMessage($"[Process] InitExpensesList failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] InitExpensesList network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] InitExpensesList timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - InitExpensesList Error calling API: {ex}");
                throw;
            }

            return result;
        }

        public async Task<ViewClaimVM> InitClaimDisplayForm(int requestId, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            ViewClaimVM result = new ViewClaimVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.InitClaimDisplayForm, Method.Post)
                    .AddJsonBody(new APIRequestDTO<int>
                    {
                        Data = requestId,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<ViewClaimVM> response = await _client.ExecuteAsync<ViewClaimVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    result.HasError = true;
                    result.ErrorMessage = response.ErrorMessage ?? response.ErrorException?.Message ?? "Unknown API error.";
                    logHelper.LogMessage($"[Process] InitClaimDisplayForm failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] InitClaimDisplayForm network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] InitClaimDisplayForm timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - InitClaimDisplayForm Error calling API: {ex}");
                throw;
            }

            return result;
        }

        #endregion

        #region Save

        public async Task<MainClaimHeaderVM> SaveClaimDetails(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            MainClaimHeaderVM result = new MainClaimHeaderVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.SaveClaimDetails, Method.Post)
                    .AddJsonBody(new APIRequestDTO<MainClaimHeaderVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<MainClaimHeaderVM> response = await _client.ExecuteAsync<MainClaimHeaderVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    result.HasError = true;
                    result.ErrorMessage = response.ErrorMessage ?? response.ErrorException?.Message ?? "Unknown API error.";
                    logHelper.LogMessage($"[Process] SaveClaimDetails failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] SaveClaimDetails network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] SaveClaimDetails timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - SaveClaimDetails Error calling API: {ex}");
                throw;
            }

            return result;
        }

        public async Task<MainClaimHeaderVM> SaveNewRequestForm(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            MainClaimHeaderVM result = new MainClaimHeaderVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.SaveNewRequestForm, Method.Post)
                    .AddJsonBody(new APIRequestDTO<MainClaimHeaderVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<MainClaimHeaderVM> response = await _client.ExecuteAsync<MainClaimHeaderVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    result.HasError = true;
                    result.ErrorMessage = response.ErrorMessage ?? response.ErrorException?.Message ?? "Unknown API error.";
                    logHelper.LogMessage($"[Process] SaveNewRequestForm failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] SaveNewRequestForm network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] SaveNewRequestForm timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - SaveNewRequestForm Error calling API: {ex}");
                throw;
            }

            return result;
        }

        #endregion

        #region Submit

        public async Task<MainClaimHeaderVM> SubmitClaimRequestForm(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            MainClaimHeaderVM result = new MainClaimHeaderVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.SubmitClaimRequestForm, Method.Post)
                    .AddJsonBody(new APIRequestDTO<MainClaimHeaderVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<MainClaimHeaderVM> response = await _client.ExecuteAsync<MainClaimHeaderVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    result.HasError = true;
                    result.ErrorMessage = response.ErrorMessage ?? response.ErrorException?.Message ?? "Unknown API error.";
                    logHelper.LogMessage($"[Process] SubmitClaimRequestForm failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] SubmitClaimRequestForm network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] SubmitClaimRequestForm timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - SubmitClaimRequestForm Error calling API: {ex}");
                throw;
            }

            return result;
        }

        #endregion

        #region Search

        public async Task<SearchClaimVM> SearchClaims(MainClaimHeaderSearchModel search, string sortCol, string sortDirection, int skip, int take, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            SearchClaimVM result = new SearchClaimVM();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.SearchClaim, Method.Post)
                    .AddJsonBody(new APIRequestDTO<PagedRequest<MainClaimHeaderSearchModel>>
                    {
                        Data = new PagedRequest<MainClaimHeaderSearchModel>
                        {
                            Search = search,
                            SortColumn = sortCol,
                            SortDirection = sortDirection,
                            Skip = skip,
                            Take = take
                        },
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<SearchClaimVM> response = await _client.ExecuteAsync<SearchClaimVM>(request);

                if (response.IsSuccessful && response.Data != null)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage($"[Process] SearchClaims failed. StatusCode: {response.StatusCode}, Content: {response.Content}");
                    throw new ApplicationException($"API call failed or returned empty data. Status: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] SearchClaims network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] SearchClaims timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - SearchClaims Error calling API: {ex}");
                throw;
            }

            return result;
        }

        #endregion

        #region Delete

        public async Task<bool> DeleteClaimAttachment(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            bool result = false;

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.DeleteClaimAttachment, Method.Post)
                    .AddJsonBody(new APIRequestDTO<MainClaimHeaderVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<bool> response = await _client.ExecuteAsync<bool>(request);

                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage($"[Process] DeleteClaimAttachment failed. StatusCode: {response.StatusCode}, Content: {response.Content}, Error: {response.ErrorException}");
                    throw new ApplicationException($"API call failed with status {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] DeleteClaimAttachment network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] DeleteClaimAttachment timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - DeleteClaimAttachment Error calling API: {ex}");
                throw;
            }

            return result;
        }

        public async Task<bool> DeleteClaimDetails(MainClaimHeaderVM vm, string spHostUrl, string accessToken)
        {
            LogHelper logHelper = new LogHelper();
            bool result = false;

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.DeleteClaimDetails, Method.Post)
                    .AddJsonBody(new APIRequestDTO<MainClaimHeaderVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<bool> response = await _client.ExecuteAsync<bool>(request);

                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage($"[Process] DeleteClaimDetails failed. StatusCode: {response.StatusCode}, Content: {response.Content}, Error: {response.ErrorException}");
                    throw new ApplicationException($"API call failed with status {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                logHelper.LogMessage($"[Process] DeleteClaimDetails network error: {ex}");
                throw;
            }
            catch (TimeoutException ex)
            {
                logHelper.LogMessage($"[Process] DeleteClaimDetails timeout: {ex}");
                throw;
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"[Process] ClaimProcess - DeleteClaimDetails Error calling API: {ex}");
                throw;
            }

            return result;
        }

        #endregion
    }
}
