using MHA.ECLAIM.Framework.JSONConstants;
using ElmahCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class LogHelper
    {
        public string connString = string.Empty;
        private bool isDebug = false;

        private readonly JSONAppSettings appSettings;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        //public LogHelper(IHttpContextAccessor httpContextAccessor)
        public LogHelper()
        {
            appSettings = ConfigurationManager.GetAppSetting();
            //_httpContextAccessor = httpContextAccessor;

            connString = ConnectionStringHelper.GetDBLogConnString();

            string DebugMode = appSettings.DebugMode;
            if (!string.IsNullOrEmpty(DebugMode))
                bool.TryParse(DebugMode, out isDebug);
        }

        public void LogMessage(string message)
        {
            try
            {
                //var context = _httpContextAccessor?.HttpContext;
                //if (context != null)
                //    context.RaiseError(new Exception(message));
                //else
                ElmahExtensions.RaiseError(new Exception(message));
            }
            catch (Exception ex)
            {
                LogMessage(message + ", " + ex, DateTimeHelper.GetCurrentDateTime());
            }
        }

        public void LogMessage(Exception exMessage)
        {
            try
            {
                //var context = _httpContextAccessor.HttpContext;
                //if (context != null)
                //    context.RaiseError(exMessage);
                //else
                ElmahExtensions.RaiseError(exMessage);
            }
            catch (Exception ex)
            {
                LogMessage(exMessage + ", " + ex, DateTimeHelper.GetCurrentDateTime());
            }
        }

        private void LogMessage(String Message, DateTime logTime)
        {
            if (isDebug)
            {
                SqlConnection sqlConn = null;
                try
                {
                    sqlConn = new SqlConnection(connString);
                    sqlConn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = sqlConn;
                    cmd.CommandText = "Insert into tblTemp Values (@logTime, @logMessage)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@logTime", logTime);
                    cmd.Parameters.AddWithValue("@logMessage", Message);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    sqlConn = new SqlConnection(connString);
                    sqlConn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = sqlConn;
                    cmd.CommandText = "Insert into tblTemp Values (@logTime, @logMessage)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@logTime", DateTimeHelper.GetCurrentDateTime());
                    cmd.Parameters.AddWithValue("@logMessage", string.Format("Log error: {0}", ex.ToString()));
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    sqlConn.Close();
                }
            }

        }
    }
}
