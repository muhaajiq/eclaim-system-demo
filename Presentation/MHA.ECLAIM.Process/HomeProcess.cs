using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Home;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process.Interface;
using RestSharp;
using System.ServiceModel;

namespace MHA.ECLAIM.Process
{
    public class HomeProcess : IHomeProcess
    {
        private readonly RestClient _client;
        private static readonly JSONAppSettings appSettings;

        static HomeProcess()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public HomeProcess()
        {
            _client = new RestClient(appSettings.WebAPIUrl);
        }

        public async Task<ViewModelMyPendingTask> GetMyPendingTask(ViewModelMyPendingTask vmMyTask, string spHostUrl, string accessToken)
        {
            ViewModelMyPendingTask result = new ViewModelMyPendingTask();
            try
            {
                var request = new RestRequest(ConstantHelper.API.HomePath.GetMyPendingTask, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ViewModelMyPendingTask>
                    {
                        Data = vmMyTask,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                var response = await _client.ExecuteAsync<ViewModelMyPendingTask>(request);

                if (response.IsSuccessful && response.Data != null)
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

        public async Task<ViewModelMyActiveRequest> GetMyActiveRequest(ViewModelMyActiveRequest vmMyTask, string spHostUrl, string accessToken)
        {
            ViewModelMyActiveRequest result = new ViewModelMyActiveRequest();
            try
            {
                var request = new RestRequest(ConstantHelper.API.HomePath.GetMyActiveRequest, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ViewModelMyActiveRequest>
                    {
                        Data = vmMyTask,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                var response = await _client.ExecuteAsync<ViewModelMyActiveRequest>(request);

                if (response.IsSuccessful && response.Data != null)
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
