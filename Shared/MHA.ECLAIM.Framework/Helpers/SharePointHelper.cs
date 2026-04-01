using MHA.Framework.Core.SP;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using Microsoft.SharePoint.Client;
using System.Text;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class SharePointHelper
    {
        #region ListItem
        public ListItem GetListItem(string listName, string fieldName, string fieldType, string value, string spHostUrl, string accessToken)
        {
            using (ClientContext clientContext = TokenHelper.GetClientContextWithAccessToken(spHostUrl, accessToken))
            {
                return GetListItem(listName, fieldName, fieldType, value, clientContext);
            }
        }

        public ListItem GetListItem(string listName, string fieldName, string fieldType, string value, ClientContext clientContext)
        {
            Web spWeb = clientContext.Web;
            List list = spWeb.Lists.GetByTitle(listName);
            CamlQuery query = new CamlQuery();
            if (!string.IsNullOrEmpty(value))
            {
                query.ViewXml = string.Format(
                      @"<View>  
                            <Query> 
                                <Where>
                                    <Eq>
                                        <FieldRef Name='{0}' /><Value Type='{1}'>{2}</Value>
                                    </Eq>
                                </Where> 
                            </Query> 
                    </View>", fieldName, fieldType, value);
            }
            else
            {
                query = CamlQuery.CreateAllItemsQuery();
            }

            ListItemCollection liColl = list.GetItems(query);
            clientContext.Load(liColl);
            clientContext.ExecuteQueryWithIncrementalRetry();

            if (liColl.Count > 0)
            {
                return liColl[0];
            }
            else
            {
                return null;
            }
        }

        public ListItem GetListItem(string listName, string fieldName, string fieldType, string value, string fieldName2, string fieldType2, string value2, ClientContext clientContext)
        {
            Web spWeb = clientContext.Web;
            List list = spWeb.Lists.GetByTitle(listName);
            CamlQuery query = new CamlQuery();
            if (!string.IsNullOrEmpty(value))
            {
                query.ViewXml = string.Format(
                      @"<View>  
                            <Query> 
                                <Where>
                                    <And>
                                        <Eq>
                                            <FieldRef Name='{0}' /><Value Type='{1}'>{2}</Value>
                                        </Eq>
                                        <Eq>
                                            <FieldRef Name='{3}' /><Value Type='{4}'>{5}</Value>
                                        </Eq>
                                    </And>
                                </Where> 
                            </Query> 
                    </View>", fieldName, fieldType, value, fieldName2, fieldType2, value2);
            }
            else
            {
                query = CamlQuery.CreateAllItemsQuery();
            }

            ListItemCollection liColl = list.GetItems(query);
            clientContext.Load(liColl);
            clientContext.ExecuteQueryWithIncrementalRetry();

            if (liColl.Count > 0)
            {
                return liColl[0];
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region ListItems
        public ListItemCollection GetListItemCollection(string listName, ClientContext clientContext)
        {
            Web spWeb = clientContext.Web;
            List list = spWeb.Lists.GetByTitle(listName);
            CamlQuery query = new CamlQuery();
            query = CamlQuery.CreateAllItemsQuery();
            ListItemCollection liColl = list.GetItems(query);
            clientContext.Load(liColl);
            clientContext.ExecuteQueryWithIncrementalRetry();
            return liColl;
        }

        public int GetLargestListItemID(ClientContext clientContext, List list)
        {
            int largestListItemID = 0;
            CamlQuery query = new CamlQuery();
            query.ViewXml = @"<View><RowLimit>1</RowLimit><Query><OrderBy><FieldRef Name='ID' Ascending='False' /></OrderBy></Query><ViewFields><FieldRef Name='ID' /></ViewFields></View>";

            if (list != null)
            {
                ListItemCollection listCol = list.GetItems(query);
                clientContext.Load(listCol);
                clientContext.ExecuteQueryWithIncrementalRetry();

                if (listCol != null && listCol.Count > 0)
                {
                    largestListItemID = listCol[0].Id;
                }
            }

            return largestListItemID;

        }

        public List<ListItem> GetItemsRepeatedlyWithItemSize(ClientContext ctx, string listName, string whereQuery, List<KeyValuePair<string, string>> orderByFields, List<string> viewFields, int itemSize)
        {
            List<ListItem> result = new List<ListItem>();
            string orderByFieldsXml = ConstructOrderByXML(orderByFields);
            string viewFieldsXml = ConstructViewFieldsXML(viewFields);

            List list = ctx.Web.Lists.GetByTitle(listName);
            ctx.Load(list);
            ctx.ExecuteQueryWithIncrementalRetry();

            int largestItemID = GetLargestListItemID(ctx, list);
            int startQueryItem = 0;
            int lastQueryItem = 0;

            CamlQuery spQuery = new CamlQuery();
            ListItemCollection tempListItems;
            do
            {
                startQueryItem = lastQueryItem + 1;
                lastQueryItem += itemSize;

                string whereBatchQuery = whereQuery;
                whereBatchQuery = GeneralQueryHelper.ConcatCriteria(whereBatchQuery, "ID", "Integer", startQueryItem.ToString(), "Geq", false);
                whereBatchQuery = GeneralQueryHelper.ConcatCriteria(whereBatchQuery, "ID", "Integer", lastQueryItem.ToString(), "Leq", false);

                spQuery.ViewXml = ConstructViewXML(whereBatchQuery, orderByFieldsXml, viewFieldsXml);

                tempListItems = list.GetItems(spQuery);
                ctx.Load(tempListItems);
                ctx.ExecuteQueryWithIncrementalRetry();

                result.AddRange(tempListItems);
            }
            while (lastQueryItem < largestItemID);

            return result;
        }

        public List<ListItem> GetItemsRepeatedlyWithItemSize(ClientContext ctx, List targetList, string whereQuery, List<KeyValuePair<string, string>> orderByFields, List<string> viewFields, int itemSize)
        {
            List<ListItem> result = new List<ListItem>();
            string orderByFieldsXml = ConstructOrderByXML(orderByFields);
            string viewFieldsXml = ConstructViewFieldsXML(viewFields);

            int largestItemID = GetLargestListItemID(ctx, targetList);
            int startQueryItem = 0;
            int lastQueryItem = 0;

            CamlQuery spQuery = new CamlQuery();
            ListItemCollection tempListItems;
            do
            {
                startQueryItem = lastQueryItem + 1;
                lastQueryItem += itemSize;

                string whereBatchQuery = whereQuery;
                whereBatchQuery = GeneralQueryHelper.ConcatCriteria(whereBatchQuery, "ID", "Integer", startQueryItem.ToString(), "Geq", false);
                whereBatchQuery = GeneralQueryHelper.ConcatCriteria(whereBatchQuery, "ID", "Integer", lastQueryItem.ToString(), "Leq", false);

                spQuery.ViewXml = ConstructViewXML(whereBatchQuery, orderByFieldsXml, viewFieldsXml);

                tempListItems = targetList.GetItems(spQuery);
                ctx.Load(tempListItems);
                ctx.ExecuteQueryWithIncrementalRetry();

                result.AddRange(tempListItems);
            }
            while (lastQueryItem < largestItemID);

            return result;
        }

        public List<ListItem> GetItemsRepeatedlyWithItemSize(ClientContext ctx, Guid listGuid, string whereQuery, List<KeyValuePair<string, string>> orderByFields, List<string> viewFields, int itemSize)
        {
            List<ListItem> result = new List<ListItem>();
            string orderByFieldsXml = ConstructOrderByXML(orderByFields);
            string viewFieldsXml = ConstructViewFieldsXML(viewFields);

            List list = ctx.Web.Lists.GetById(listGuid);
            ctx.Load(list);
            ctx.ExecuteQueryWithIncrementalRetry();

            int largestItemID = GetLargestListItemID(ctx, list);
            int startQueryItem = 0;
            int lastQueryItem = 0;

            CamlQuery spQuery = new CamlQuery();
            ListItemCollection tempListItems;
            do
            {
                startQueryItem = lastQueryItem + 1;
                lastQueryItem += itemSize;

                string whereBatchQuery = whereQuery;
                whereBatchQuery = GeneralQueryHelper.ConcatCriteria(whereBatchQuery, "ID", "Integer", startQueryItem.ToString(), "Geq", false);
                whereBatchQuery = GeneralQueryHelper.ConcatCriteria(whereBatchQuery, "ID", "Integer", lastQueryItem.ToString(), "Leq", false);

                spQuery.ViewXml = ConstructViewXML(whereBatchQuery, orderByFieldsXml, viewFieldsXml);

                tempListItems = list.GetItems(spQuery);
                ctx.Load(tempListItems);
                ctx.ExecuteQueryWithIncrementalRetry();

                result.AddRange(tempListItems);
            }
            while (lastQueryItem < largestItemID);

            return result;
        }
        #endregion

        #region List
        public ViewCollection GetListViews(ClientContext clientContext, string ListName)
        {
            List list = clientContext.Web.Lists.GetByTitle(ListName);
            ViewCollection listViews = list.Views;
            clientContext.Load(listViews, x => x.Where(y => y.Title != string.Empty));
            clientContext.ExecuteQueryWithIncrementalRetry();
            return listViews;
        }

        public string GetListDefaultDisplayFormUrl(ClientContext clientContext, string listName)
        {
            string defaultDisplayFormUrl = string.Empty;
            List spList = clientContext.Web.Lists.GetByTitle(listName);
            clientContext.Load(spList, list => list.DefaultDisplayFormUrl);
            clientContext.ExecuteQueryWithIncrementalRetry();
            defaultDisplayFormUrl = spList.DefaultDisplayFormUrl;
            return defaultDisplayFormUrl;
        }

        public string GetListTitleByListGuid(ClientContext clientContext, string listGuid)
        {
            string listTitle = string.Empty;
            List spList = clientContext.Web.Lists.GetById(new Guid(listGuid));
            clientContext.Load(spList, list => list.Title);
            clientContext.ExecuteQueryWithIncrementalRetry();
            listTitle = spList.Title;
            return listTitle;
        }

        public List GetListByListGuid(ClientContext clientContext, string listGuid)
        {
            string listTitle = string.Empty;
            List spList = clientContext.Web.Lists.GetById(new Guid(listGuid));
            clientContext.Load(spList, list => list.Title, list => list.Fields);
            clientContext.ExecuteQueryWithIncrementalRetry();
            return spList;
        }

        public List GetListByTitle(ClientContext clientContext, string listName)
        {
            List list = null;
            try
            {
                list = clientContext.Web.Lists.GetByTitle(listName);
                clientContext.Load(list);
                clientContext.Load(list, l => l.Id);
                clientContext.ExecuteQueryWithIncrementalRetry();
            }
            catch (Exception)
            {
                list = null;
            }

            return list;
        }
        #endregion

        #region Query
        public static string ConstructViewFieldsXML(List<string> fields)
        {
            if (fields != null && fields.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<ViewFields>");
                foreach (string field in fields)
                {
                    sb.AppendFormat(string.Format("<FieldRef Name=\"{0}\"></FieldRef>", field));
                }
                sb.AppendFormat("</ViewFields>");

                return sb.ToString();
            }

            return string.Empty;
        }

        public static string ConstructOrderByXML(List<KeyValuePair<string, string>> fields)
        {
            if (fields != null && fields.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("<OrderBy>");
                foreach (var field in fields)
                {
                    sb.AppendFormat(string.Format("<FieldRef Name=\"{0}\" Ascending=\"{1}\"></FieldRef>", field.Key, field.Value));
                }
                sb.AppendFormat("</OrderBy>");
                return sb.ToString();
            }

            return string.Empty;
        }

        public static string ConstructViewXML(string whereQuery, string orderByQuery, string viewFieldsQuery)
        {
            return String.Format(@"<View>
                                    <Query>
                                        <Where>{0}</Where>
                                        {1}
                                    </Query>
                                    {2}
                                </View>", whereQuery, orderByQuery, viewFieldsQuery);
        }
        #endregion

        #region User
        public static User GetCurrentUser(ClientContext clientContext)
        {
            User currentUser = clientContext.Web.CurrentUser;
            clientContext.Load(currentUser, x => x.Id, x => x.LoginName, x => x.Title, x => x.Email, x => x.Groups);
            clientContext.ExecuteQueryWithIncrementalRetry();
            return currentUser;
        }

        public static User GetUserById(ClientContext clientContext, int id)
        {
            User user = null;
            if (id > 0)
            {
                try
                {
                    user = clientContext.Web.SiteUsers.GetById(id);
                    clientContext.Load(user, x => x.Id, x => x.Title, x => x.LoginName, x => x.Email);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
                catch (Exception)
                {
                    user = null;
                }
            }

            return user;
        }

        public static User GetUserByLoginName(ClientContext clientContext, string loginName)
        {
            User user = null;
            if (!string.IsNullOrEmpty(loginName))
            {
                try
                {
                    user = clientContext.Web.EnsureUser(loginName);
                    clientContext.Load(user, x => x.Id, x => x.Title, x => x.LoginName, x => x.Email, x => x.Groups);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
                catch (Exception)
                {
                    user = null;
                }
            }

            return user;
        }

        public static User GetUserByEmail(ClientContext clientContext, string email)
        {
            User user = null;
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    user = clientContext.Web.SiteUsers.GetByEmail(email);
                    clientContext.Load(user, x => x.Id, x => x.Title, x => x.LoginName, x => x.Email);
                    clientContext.ExecuteQueryWithIncrementalRetry();
                }
                catch (Exception)
                {
                    user = null;
                }
            }

            return user;
        }

        public static List<PeoplePickerUser> GetUsersFromSharePointGroup(ClientContext clientContext, string groupName)
        {
            List<PeoplePickerUser> users = new List<PeoplePickerUser>();

            var group = clientContext.Web.SiteGroups.GetByName(groupName);
            clientContext.Load(group, g => g.Users);
            clientContext.ExecuteQueryWithIncrementalRetry();

            foreach (var spUser in group.Users)
            {
                PeoplePickerUser user = ConvertEntitiesHelper.ConvertPeoplePickerUser(spUser, clientContext);
                users.Add(user);
            }

            return users;
        }

        public static bool IsUserInGroup(User curUser, string groupName)
        {
            bool isExits = false;

            if (curUser != null)
            {
                foreach (Group group in curUser.Groups)
                {
                    if (group.Title.Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isExits = true;
                        break;
                    }
                }
            }

            return isExits;
        }

        public static bool IsUserInGroup(ClientContext clientContext, string userLoginName, string groupName)
        {
            bool isExits = false;
            User user = null;
            if (string.IsNullOrEmpty(userLoginName))
            {
                user = GetCurrentUser(clientContext);
            }
            else
            {
                user = GetUserByLoginName(clientContext, userLoginName);
            }
            if (user != null)
            {
                foreach (Group group in user.Groups)
                {
                    if (group.Title.Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        isExits = true;
                        break;
                    }
                }
            }

            return isExits;
        }

        public static bool IsUserInGroups(User curUser, string[] lstGroupName)
        {
            bool isExits = false;
            try
            {
                if (curUser != null)
                {
                    foreach (Group group in curUser.Groups)
                    {
                        foreach (string groupName in lstGroupName)
                        {
                            if (group.Title.Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                isExits = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (ServerUnauthorizedAccessException)
            {
                isExits = false;
            }

            return isExits;
        }

        public static bool IsUserInGroups(ClientContext clientContext, string userLoginName, string[] lstGroupName)
        {
            bool isExits = false;
            try
            {
                User user = null;
                if (string.IsNullOrEmpty(userLoginName))
                {
                    user = GetCurrentUser(clientContext);
                }
                else
                {
                    user = GetUserByLoginName(clientContext, userLoginName);
                }
                if (user != null)
                {
                    foreach (Group group in user.Groups)
                    {
                        foreach (string groupName in lstGroupName)
                        {
                            if (group.Title.Equals(groupName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                isExits = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (ServerUnauthorizedAccessException)
            {
                isExits = false;
            }

            return isExits;
        }
        #endregion
    }
}
