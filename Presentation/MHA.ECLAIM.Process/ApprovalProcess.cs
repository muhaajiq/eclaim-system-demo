using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process.Interface;
using RestSharp;
using System.ServiceModel;

namespace MHA.ECLAIM.Process
{
    public class ApprovalProcess : IApprovalProcess
    {
        private readonly RestClient _client;
        private static readonly JSONAppSettings appSettings;

        static ApprovalProcess()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public ApprovalProcess()
        {
            _client = new RestClient(appSettings.WebAPIUrl);
        }

        public async Task<ApprovalFormVM> InitApprovalForm(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            ApprovalFormVM result = new ApprovalFormVM();
            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.InitApprovalForm, Method.Post)
                .AddJsonBody(new APIRequestDTO<ApprovalFormVM>
                {
                    Data = vm,
                    SpHostUrl = spHostUrl,
                    AccessToken = accessToken
                });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<ApprovalFormVM> response = await _client.ExecuteAsync<ApprovalFormVM>(request);

                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
            }
            catch (FaultException fex)
            {
                throw new ApplicationException(fex.Message);
            }
            return result;
        }

        public async Task<RequestFormVisibilitySettings> SetVisibilitySettings(string currentStage)
        {
            RequestFormVisibilitySettings result = new RequestFormVisibilitySettings();
            LogHelper logHelper = new LogHelper();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.SetVisibilitySettings, Method.Post)
                    .AddJsonBody(new APIRequestDTO<string>
                    {
                        Data = currentStage
                    });

                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var response = await _client.ExecuteAsync<RequestFormVisibilitySettings>(request);
                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage(response.ErrorException?.ToString());
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(ex.ToString());
                throw;
            }

            return result;
        }

        public async Task<PartialModelExpenseModal> GetExpenseModalInfo(SubClaimDetailsVM subVm, string spHostUrl, string accessToken)
        {
            PartialModelExpenseModal result = new PartialModelExpenseModal();
            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.GetExpenseModalInfo, Method.Post)
                .AddJsonBody(new APIRequestDTO<SubClaimDetailsVM>
                {
                    Data = subVm,
                    SpHostUrl = spHostUrl,
                    AccessToken = accessToken
                });
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                RestResponse<PartialModelExpenseModal> response = await _client.ExecuteAsync<PartialModelExpenseModal>(request);

                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
            }
            catch (FaultException fex)
            {
                throw new ApplicationException(fex.Message);
            }
            return result;
        }

        public async Task<ApprovalFormVM> RequireAmendmentRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            ApprovalFormVM result = new ApprovalFormVM();
            LogHelper logHelper = new LogHelper();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.RequireAmendmentRequest, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ApprovalFormVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var response = await _client.ExecuteAsync<ApprovalFormVM>(request);
                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage(response.ErrorException?.ToString());
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(ex.ToString());
                throw;
            }

            return result;
        }

        public async Task<ApprovalFormVM> ApproveRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            ApprovalFormVM result = new ApprovalFormVM();
            LogHelper logHelper = new LogHelper();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.ApproveRequest, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ApprovalFormVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var response = await _client.ExecuteAsync<ApprovalFormVM>(request);
                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage(response.ErrorException?.ToString());
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(ex.ToString());
                throw;
            }

            return result;
        }

        public async Task<ApprovalFormVM> RejectRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            ApprovalFormVM result = new ApprovalFormVM();
            LogHelper logHelper = new LogHelper();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.RejectRequest, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ApprovalFormVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var response = await _client.ExecuteAsync<ApprovalFormVM>(request);
                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage(response.ErrorException?.ToString());
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(ex.ToString());
                throw;
            }

            return result;
        }

        public async Task<ApprovalFormVM> SaveRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            ApprovalFormVM result = new ApprovalFormVM();
            LogHelper logHelper = new LogHelper();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.SaveRequest, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ApprovalFormVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var response = await _client.ExecuteAsync<ApprovalFormVM>(request);
                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage(response.ErrorException?.ToString());
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(ex.ToString());
                throw;
            }

            return result;
        }
        public async Task<ApprovalFormVM> ReSubmitClaimRequest(ApprovalFormVM vm, string spHostUrl, string accessToken)
        {
            ApprovalFormVM result = new ApprovalFormVM();
            LogHelper logHelper = new LogHelper();

            try
            {
                RestRequest request = new RestRequest(ConstantHelper.API.ClaimPath.ReSubmitClaimRequest, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ApprovalFormVM>
                    {
                        Data = vm,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var response = await _client.ExecuteAsync<ApprovalFormVM>(request);
                if (response.IsSuccessful)
                {
                    result = response.Data;
                }
                else
                {
                    logHelper.LogMessage(response.ErrorException?.ToString());
                }
            }
            catch (Exception ex)
            {
                logHelper.LogMessage(ex.ToString());
                throw;
            }

            return result;
        }

    }
}
