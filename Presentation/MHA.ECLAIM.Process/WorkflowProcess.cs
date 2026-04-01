using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Workflow;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process.Interface;
using RestSharp;
using System.ServiceModel;

namespace MHA.ECLAIM.Process
{
    public class WorkflowProcess : IWorkflowProcess
    {
        private readonly RestClient _client;
        private static readonly JSONAppSettings appSettings;

        static WorkflowProcess()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public WorkflowProcess()
        {
            _client = new RestClient(appSettings.WebAPIUrl);
        }

        public void UpdateRequestWF(string refNo, List<string> paramNamesList, List<object> paramObjectsList)
        {
            throw new NotImplementedException();
        }

        public async Task<PartialModelWorkflowHistory> InitWorkflowHistory(int processID, string spHostURL, string accessToken)
        {
            PartialModelWorkflowHistory result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.InitWorkflowHistory, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<int>
                {
                    Data = processID,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<PartialModelWorkflowHistory>(request);
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

        public async Task<PartialModelAdminWorkflowHistory> InitAdminWFHistory(int processID, string spHostURL, string accessToken)
        {
            PartialModelAdminWorkflowHistory result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.InitAdminWFHistory, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<int>
                {
                    Data = processID,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<PartialModelAdminWorkflowHistory>(request);
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

        public async Task<bool> AddActioner(AddActionerVM model, string spHostURL, string accessToken, string appAccessToken)
        {
            bool result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.AddActioner, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<AddActionerVM>
                {
                    Data = model,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken,
                    AppAccessToken = appAccessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<bool>(request);
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

        public async Task<bool> ReassignActioner(ReassignActionerVM model, string spHostURL, string accessToken)
        {
            bool result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.ReassignActioner, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<ReassignActionerVM>
                {
                    Data = model,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<bool>(request);
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

        public async Task<bool> RemoveActioner(RemoveActionerVM model, string spHostURL, string accessToken, string appAccessToken)
        {
            bool result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.RemoveActioner, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<RemoveActionerVM>
                {
                    Data = model,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken,
                    AppAccessToken = appAccessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<bool>(request);
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

        public async Task<bool> RemoveAllActioner(RemoveAllActionerVM model, string spHostURL, string accessToken, string appAccessToken)
        {
            bool result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.RemoveAllActioner, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<RemoveAllActionerVM>
                {
                    Data = model,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken,
                    AppAccessToken = appAccessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<bool>(request);
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

        public async Task<string> SetDelegation(ViewModelNewDelegate vmNewDelegate, string spHostURL, string accessToken)
        {
            string result = default;
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.SetDelegation, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<ViewModelNewDelegate>
                {
                    Data = vmNewDelegate,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<string>(request);
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

        public async Task<ViewModelNewDelegate> InitNewDelegate(string spHostURL, string accessToken)
        {
            ViewModelNewDelegate result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.InitNewDelegate, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<object> // no payload, so use object
                {
                    Data = null,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<ViewModelNewDelegate>(request);
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

        public async Task<ViewModelMyDelegate> InitMyDelegate(string spHostURL, string accessToken)
        {
            ViewModelMyDelegate result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.InitMyDelegate, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<object>
                {
                    Data = null,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<ViewModelMyDelegate>(request);
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

        public async Task DeleteDelegation(int delegationID, string spHostURL, string accessToken)
        {
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.DeleteDelegation, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<int>
                {
                    Data = delegationID,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                _client.Execute(request);
            }
            catch (FaultException fex)
            {
                throw new ApplicationException(fex.Message);
            }
        }

        public async Task<ViewModelWorkflowReport> InitAuthWorkflowReportCheck(ViewModelWorkflowReport vm, string spHostURL, string accessToken)
        {
            ViewModelWorkflowReport result = new();
            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.InitAuthWorkflowReportCheck, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<object>
                {
                    Data = vm,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<ViewModelWorkflowReport>(request);
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
        public async Task<ViewModelWorkflowReport> SearchWorkflowTaskReport(ViewModelWorkflowReport vm, string spHostURL, string accessToken)
        {
            ViewModelWorkflowReport result = new();

            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.SearchWorflowTaskReport, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<object>
                {
                    Data = vm,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<ViewModelWorkflowReport>(request);
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

        public async Task<ViewModelWorkflowReport> ExportToExcelWorkflowTaskReport(ViewModelWorkflowReport vm, string spHostURL, string accessToken)
        {
            ViewModelWorkflowReport result = new();

            try
            {
                var request = new RestRequest(ConstantHelper.API.WorkflowPath.ExportToExcelWorkflowTaskReport, Method.Post);
                request.RequestFormat = DataFormat.Json;
                request.Timeout = RestClientHelper.GetRestClientTimeOutTimeSpan();

                var requestBody = new APIRequestDTO<object>
                {
                    Data = vm,
                    SpHostUrl = spHostURL,
                    AccessToken = accessToken
                };
                request.AddJsonBody(requestBody);

                var response = _client.Execute<ViewModelWorkflowReport>(request);
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

    }
}
