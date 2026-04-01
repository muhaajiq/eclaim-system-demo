using MHA.Framework.Core.SP;
using MHA.ECLAIM.Entities.ViewModel.RunningNumber;
using MHA.ECLAIM.Framework.Constants;
using Microsoft.SharePoint.Client;

namespace MHA.ECLAIM.Framework.Helpers
{
    public static class RunningNumberHelper
    {
        public static RunningNumber CreateRunningNumber(string title, string format, string prefix, ClientContext clientContext)
        {
            RunningNumber rnObj = new RunningNumber();

            try
            {
                Web spWeb = clientContext.Web;
                List list = spWeb.Lists.GetByTitle(ConstantHelper.SPList.RunningNumber);

                string query = string.Empty;
                query = GeneralQueryHelper.ConcatCriteria(query, ConstantHelper.SPColumn.RunningNumberList.Title, "Text", title, "Eq", false);
                query = GeneralQueryHelper.ConcatCriteria(query, ConstantHelper.SPColumn.RunningNumberList.Prefix, "Text", prefix, "Eq", false);
                ListItem li = GeneralQueryHelper.GetSPItem(clientContext, ConstantHelper.SPList.RunningNumber, query, null);
                if (li == null)
                {
                    ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                    ListItem newLi = list.AddItem(itemCreateInfo);
                    newLi[ConstantHelper.SPColumn.RunningNumberList.Title] = title;
                    newLi[ConstantHelper.SPColumn.RunningNumberList.Format] = format;
                    newLi[ConstantHelper.SPColumn.RunningNumberList.Prefix] = prefix;
                    newLi[ConstantHelper.SPColumn.RunningNumberList.Number] = 1;

                    newLi.Update();
                    clientContext.ExecuteQueryWithIncrementalRetry();

                    rnObj.Format = format;
                    rnObj.ID = newLi.Id;
                    rnObj.Number = 1;
                    rnObj.Prefix = prefix;
                    rnObj.Title = title;
                }
                else
                {
                    lock (li)
                    {
                        rnObj = ConvertEntitiesHelper.ConvertRunningNumberObject(li);
                        rnObj.Number = rnObj.Number + 1;
                        rnObj.Format = format;
                        rnObj.Prefix = prefix;
                        ListItem newLi = list.GetItemById(rnObj.ID);
                        newLi[ConstantHelper.SPColumn.RunningNumberList.Number] = rnObj.Number;
                        newLi[ConstantHelper.SPColumn.RunningNumberList.Format] = format;
                        newLi[ConstantHelper.SPColumn.RunningNumberList.Prefix] = prefix;
                        newLi.Update();
                        clientContext.ExecuteQueryWithIncrementalRetry();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RunningNumberHelper - CreateRunningNumber Error:" + ex.ToString());
            }

            return rnObj;
        }

        public static void RestoreRunningNumber(string title, string prefix, int numberCount, ClientContext clientContext)
        {
            try
            {
                string query = string.Empty;
                query = GeneralQueryHelper.ConcatCriteria(query, ConstantHelper.SPColumn.RunningNumberList.Title, "Text", title, "Eq", false);
                query = GeneralQueryHelper.ConcatCriteria(query, ConstantHelper.SPColumn.RunningNumberList.Prefix, "Text", prefix, "Eq", false);
                lock (query)
                {
                    ListItem runningNumberSPItem = GeneralQueryHelper.GetSPItem(clientContext, ConstantHelper.SPList.RunningNumber, query, null);
                    if (runningNumberSPItem != null)
                    {
                        int currentNumber = FieldHelper.GetFieldValueAsNumber(runningNumberSPItem, ConstantHelper.SPColumn.RunningNumberList.Number);
                        if (currentNumber == numberCount)
                        {
                            int newNumber = numberCount - 1;
                            runningNumberSPItem[ConstantHelper.SPColumn.RunningNumberList.Number] = newNumber;
                            runningNumberSPItem.Update();
                            clientContext.ExecuteQueryWithIncrementalRetry();

                            throw new Exception($"System restore running number {prefix} from {currentNumber} to {newNumber}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RunningNumberHelper - RestoreRunningNumber Error:" + ex.ToString());
            }
        }
    }
}
