using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.ViewModel.Home;
using MHA.ECLAIM.Entities.ViewModel.Report;
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
    public class ReportProcess : IReportProcess
    {
        private readonly RestClient _client;
        private static readonly JSONAppSettings appSettings;

        static ReportProcess()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        public ReportProcess()
        {
            _client = new RestClient(appSettings.WebAPIUrl);
        }

        public async Task<ViewModelReportListing> GetReportListing(string spHostUrl, string accessToken)
        {
            ViewModelReportListing result = new ViewModelReportListing();
            try
            {
                var request = new RestRequest(ConstantHelper.API.ReportPath.GetReportListing, Method.Post)
                    .AddJsonBody(new APIRequestDTO<object>
                    {
                        Data = null,
                        SpHostUrl = spHostUrl,
                        AccessToken = accessToken
                    });

                var response = await _client.ExecuteAsync<ViewModelReportListing>(request);

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
