using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Home;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using MHA.ECLAIM.Framework.JSONConstants;
using MHA.ECLAIM.Process.Interface;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Process
{
    public class AdministrationProcess : IAdministrationProcess
    {
        private readonly RestClient _client;
        private static readonly JSONAppSettings appSettings;

        static AdministrationProcess()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public AdministrationProcess()
        {
            _client = new RestClient(appSettings.WebAPIUrl);
        }

        public async Task<ViewModelAdministrationListing> InitAdministrationListing(string spHostUrl, string accessToken)
        {
            ViewModelAdministrationListing result = new ViewModelAdministrationListing();
            try
            {
                var request = new RestRequest(ConstantHelper.API.AdministrationPath.InitAdministrationListing, Method.Post)
                    .AddJsonBody(new APIRequestDTO<ViewModelAdministrationListing>
                    {
                        Data = null,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                var response = await _client.ExecuteAsync<ViewModelAdministrationListing>(request);

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
