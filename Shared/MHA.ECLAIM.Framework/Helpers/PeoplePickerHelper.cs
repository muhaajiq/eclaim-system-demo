using MHA.ECLAIM.Entities.ViewModel.Shared;
using Microsoft.SharePoint.ApplicationPages.ClientPickerQuery;
using Microsoft.SharePoint.Client.Utilities;
using System.Text.Json;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class PickerEntity
    {
        public string Key { get; set; }
        public string DisplayText { get; set; }
        public Dictionary<string, string> EntityData { get; set; }
    }

    public class PeoplePickerHelper
    {
        public async Task<List<PeoplePickerUser>> SearchPeople(string searchText, string spHostUrl, string accessToken)
        {
            return await Task.Run(() =>
            {
                var results = new List<PeoplePickerUser>();

                using (var context = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
                {
                    context.ExecutingWebRequest += (sender, args) =>
                    {
                        args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + accessToken;
                    };

                    var queryParams = new ClientPeoplePickerQueryParameters
                    {
                        AllowMultipleEntities = false,
                        MaximumEntitySuggestions = 10,
                        PrincipalSource = PrincipalSource.All,
                        PrincipalType = PrincipalType.User,
                        QueryString = searchText
                    };

                    var clientResult = ClientPeoplePickerWebServiceInterface.ClientPeoplePickerSearchUser(context, queryParams);
                    context.ExecuteQueryWithIncrementalRetry();

                    var jsonResult = clientResult.Value;
                    var entities = JsonSerializer.Deserialize<List<PickerEntity>>(jsonResult);

                    foreach (var entity in entities)
                    {
                        results.Add(new PeoplePickerUser
                        {
                            Name = entity.DisplayText,
                            Email = entity.EntityData.ContainsKey("Email") ? entity.EntityData["Email"] : "",
                            Login = entity.Key
                        });
                    }
                }

                return results;
            });
        }
    }
}
