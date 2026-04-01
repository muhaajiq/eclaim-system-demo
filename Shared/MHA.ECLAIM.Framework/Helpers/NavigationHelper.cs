using MHA.ECLAIM.Framework.Constants;
using Microsoft.AspNetCore.Components;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class NavigationHelper
    {
        private readonly NavigationManager _navigationManager;
        public string spHostUrl { get; private set; }

        public NavigationHelper(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public void SetSPHostUrl(string url)
        {
            spHostUrl = url;
        }

        public bool isSpHostUrlAvailable => !string.IsNullOrEmpty(spHostUrl);

        public void NavigateTo(string uri, bool forceLoad = false)
        {
            if (isSpHostUrlAvailable)
            {
                var separator = uri.Contains('?') ? "&" : "?";
                uri = $"{uri}{separator}SPHostUrl={Uri.EscapeDataString(spHostUrl)}";
            }

            _navigationManager.NavigateTo(uri, forceLoad);
        }

        public string BuildUrl(string baseUri, params (string Key, string Value)[] additionalParams)
        {
            var queryParams = new List<(string Key, string Value)>();

            bool hasRequestIdParam = additionalParams?.Any(p => p.Key == ConstantHelper.ParameterQuery.RequestId) ?? false;

            if (ShouldAppendRequestIdFromCurrentUrl(baseUri))
            {
                var uri = new Uri(_navigationManager.Uri);
                var currentQuery = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);

                if (currentQuery.TryGetValue(ConstantHelper.ParameterQuery.RequestId, out var requestIdValue) && !string.IsNullOrEmpty(requestIdValue))
                {
                    if (!queryParams.Any(q => q.Key == ConstantHelper.ParameterQuery.RequestId))
                        queryParams.Add((ConstantHelper.ParameterQuery.RequestId, requestIdValue));
                }

                if (currentQuery.TryGetValue(ConstantHelper.ParameterQuery.ProcessId, out var processIdValue) && !string.IsNullOrEmpty(processIdValue))
                {
                    if (!queryParams.Any(q => q.Key == ConstantHelper.ParameterQuery.ProcessId))
                        queryParams.Add((ConstantHelper.ParameterQuery.ProcessId, processIdValue));
                }

                if (currentQuery.TryGetValue(ConstantHelper.ParameterQuery.TaskId, out var taskIdValue) && !string.IsNullOrEmpty(taskIdValue))
                {
                    if (!queryParams.Any(q => q.Key == ConstantHelper.ParameterQuery.TaskId))
                        queryParams.Add((ConstantHelper.ParameterQuery.TaskId, taskIdValue));
                }
            }

            if (additionalParams != null)
            {
                foreach (var param in additionalParams)
                {
                    queryParams.Add(param);
                }
            }

            if (isSpHostUrlAvailable)
            {
                var decodedUrl = Uri.UnescapeDataString(spHostUrl);
                queryParams.Add((ConstantHelper.ParameterQuery.SPHostUrl, decodedUrl));
            }

            if (!queryParams.Any())
            {
                return baseUri;
            }

            var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var separator = baseUri.Contains('?') ? "&" : "?";
            return $"{baseUri}{separator}{queryString}";
        }

        private bool ShouldAppendRequestIdFromCurrentUrl(string baseUri)
        {
            return baseUri.StartsWith("/ViewRequestForm", StringComparison.OrdinalIgnoreCase)
                || baseUri.StartsWith("/ApprovalForm", StringComparison.OrdinalIgnoreCase);
        }
    }
}
