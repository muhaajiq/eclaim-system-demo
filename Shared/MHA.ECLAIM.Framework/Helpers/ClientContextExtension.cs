using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.SharePoint.Client;
using System.Net;

namespace MHA.ECLAIM.Framework.Helpers
{
    public static class ClientContextExtension
    {
        private static readonly JSONAppSettings appsettings;
        private static readonly JSONAppSettings connStrings;

        static ClientContextExtension()
        {
            appsettings = ConfigurationManager.GetAppSetting();
            connStrings = ConfigurationManager.GetConnStrings();
        }

        public static void InnerExecuteQueryWithIncrementalRetry(this ClientContext context, string sourceFunction = "")
        {
            int retryAttempts = 0;
            int backoffInterval = 500;
            int backoffIncrease = 100;
            int retryCount = 1;
            bool retry = false;
            ClientRequestWrapper wrapper = null;
            int.TryParse(appsettings.NoOfRetry, out retryCount);
            int.TryParse(appsettings.ThrottlingSleeptime, out backoffInterval);
            int.TryParse(appsettings.ThrottlingTimeIncrease, out backoffIncrease);
            Console.WriteLine(connStrings.dbLogConnString);

            if (retryCount <= 0)
                throw new ArgumentException("Provide a retry count greater than zero.");
            if (backoffInterval <= 0)
                throw new ArgumentException("Provide a delay greater than zero.");

            while (retryAttempts < retryCount)
            {
                try
                {
                    if (!retry)
                    {
                        context.ExecuteQuery();
                        return;
                    }
                    else
                    {
                        //increment the retry count
                        retryAttempts++;

                        // retry the previous request using wrapper
                        if (wrapper != null && wrapper.Value != null)
                        {
                            context.RetryQuery(wrapper.Value);
                            return;
                        }
                        // retry the previous request as normal
                        else
                        {
                            context.ExecuteQuery();
                            return;
                        }
                    }
                }
                catch (WebException wex)
                {
                    var response = wex.Response as HttpWebResponse;
                    if (response != null && (response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503))
                    {
                        wrapper = (ClientRequestWrapper)wex.Data["ClientRequest"];
                        retry = true;

                        // Delay for the requested seconds
                        System.Threading.Thread.Sleep(backoffInterval);

                        // Increase counters
                        backoffInterval += backoffIncrease;

                    }
                    else
                    {
                        throw wex;
                    }
                }
            }
            throw new Exception(string.Format("Maximum retry attempts {0}, have been attempted.", retryCount));
        }
    }
}
