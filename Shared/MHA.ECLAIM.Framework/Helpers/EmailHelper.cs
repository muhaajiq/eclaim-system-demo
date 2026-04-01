using MHA.Framework.Core.SP;
using MHA.ECLAIM.Entities.ViewModel.Shared;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.JSONConstants;
using Microsoft.SharePoint.Client;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class EmailHelper
    {
        private static readonly JSONAppSettings appSettings;

        static EmailHelper()
        {
            appSettings = ConfigurationManager.GetAppSetting();
        }

        #region Recipient Email Validation
        private static void CheckValidExternalRecipientNumber(string externalRecipient)
        {
            int maxRecipientNo = 0;
            int.TryParse(appSettings.MaximumEmailRecipients, out maxRecipientNo);
            int totalExternalRecipient = GetEmailFromString(externalRecipient).Length;

            if (totalExternalRecipient <= 0)
            {
                throw new Exception("Recipient should has at least 1 recipient.");
            }
            else if (totalExternalRecipient > maxRecipientNo)
            {
                throw new Exception("Total recipients have exceed the maximum number of allowed recipient.");
            }
        }

        public static bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
            var regex = new Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        public static bool CheckValidEmailFormat(string emailString)
        {
            bool isValid = false;
            try
            {
                if (!string.IsNullOrEmpty(emailString))
                {
                    string[] emails = GetEmailFromString(emailString);
                    foreach (string email in emails)
                    {
                        new MailAddress(email);
                    }
                }
                isValid = true;
            }
            catch (Exception)
            {
                isValid = false;
            }

            return isValid;
        }
        #endregion

        #region Send Email
        public static void SendEmailWithSender(string sender, string senderPassword, MailAddressCollection emailTo, MailAddressCollection emailCC, string emailSubject, string emailBody, List<System.Net.Mail.Attachment> emailAttachment, EmailStatusEntity emailStatusObj = null)
        {
            string smtp = appSettings.AG_EMAILHOST;
            string smtp_Port = appSettings.AG_EMAILPORT;
            string emailSender = !string.IsNullOrEmpty(sender) ? sender : appSettings.AG_EMAILFROM;
            MailAddress emailFrom = new MailAddress(emailSender);
            string decryptKey = appSettings.AG_ENCKEY;
            string decryptedKey = EncryptionHelper.Decrypt(decryptKey);
            string decryptedUsername = !string.IsNullOrEmpty(sender) ? sender : EncryptionHelper.Decrypt(appSettings.AG_EMAILUSERNAME, decryptedKey);
            string username = decryptedUsername;
            string encryptedPassword = !string.IsNullOrEmpty(senderPassword) ? senderPassword : appSettings.AG_EMAILPASSWORD;
            string password = EncryptionHelper.Decrypt(encryptedPassword, decryptedKey);
            bool useSSL = false;
            bool.TryParse(appSettings.AG_EMAILUSESSL, out useSSL);
            bool useDefaultCredential = false;
            bool.TryParse(appSettings.AG_EMAILUSEDEFAULTCREDENTIAL, out useDefaultCredential);

            SendEmailWithRetry(smtp, int.Parse(smtp_Port), emailFrom, emailTo, emailCC, emailSubject, emailBody, true, emailAttachment, useDefaultCredential, useSSL, username, password, emailStatusObj);
        }

        private static void SendEmailWithRetry(string smtp, int smtp_Port, MailAddress emailFrom, MailAddressCollection emailTo, MailAddressCollection emailCC, string emailSubject, string emailBody, bool isHtml, List<System.Net.Mail.Attachment> emailAttachment, bool useDefaultCredential, bool useSSL, string username, string password, EmailStatusEntity emailStatusObj = null)
        {
            List<MemoryStream> attacmentMemoryStreams = new List<MemoryStream>();
            List<byte[]> listByteArray = null;
            List<string> listFileName = new List<string>();
            List<System.Net.Mail.Attachment> eAttachment = new List<System.Net.Mail.Attachment>();

            if (emailAttachment.Count > 0)
            {
                listByteArray = new List<byte[]>();
                foreach (var attachment in emailAttachment)
                {
                    listByteArray.Add(ProjectHelper.StreamToByteArray(attachment.ContentStream));
                    listFileName.Add(attachment.Name);
                }
                for (int i = 0; i < listByteArray.Count; i++)
                {
                    MemoryStream attachmentStream = new MemoryStream(listByteArray[i]);
                    attacmentMemoryStreams.Add(attachmentStream);
                    eAttachment.Add(new System.Net.Mail.Attachment(attachmentStream, listFileName[i]));
                }
            }

            Task.Run(() =>
            {
                LogHelper logHelper = new LogHelper();
                try
                {
                    int retryAttempts = 0;
                    int backoffInterval = 30000;
                    int backoffIncrease = 100;
                    int retryCount = 1;
                    int.TryParse(appSettings.EmailNoOfRetry, out retryCount);
                    int.TryParse(appSettings.EmailThrottlingSleeptime, out backoffInterval);
                    int.TryParse(appSettings.ThrottlingTimeIncrease, out backoffIncrease);

                    if (retryCount <= 0)
                        throw new ArgumentException("Provide a retry count greater than zero.");
                    if (backoffInterval <= 0)
                        throw new ArgumentException("Provide a delay greater than zero.");

                    while (retryAttempts < retryCount)
                    {
                        try
                        {
                            retryAttempts++;
                            MHA.Framework.Core.General.EmailHelper.SendEmail(smtp, smtp_Port, emailFrom, emailTo, emailCC, emailSubject, emailBody, isHtml, eAttachment, useDefaultCredential, useSSL, username, password);

                            if (emailStatusObj != null)
                            {
                                emailStatusObj.SentSuccess = true;
                                UpdateEmailSendStatus(emailStatusObj, emailStatusObj.SPContext);
                            }

                            return;
                        }
                        catch (Exception ex)
                        {
                            //Get the exception message
                            //Based on the exception message to determine retry send email or using second account to send
                            string exStackTrace = ex.ToString();

                            string emailToStr = string.Empty;
                            string emailCCStr = string.Empty;

                            if (emailTo?.Count > 0)
                                emailToStr = String.Join(",", emailTo.Select(x => x.Address));

                            if (emailCC?.Count > 0)
                                emailCCStr = String.Join(",", emailCC.Select(x => x.Address));

                            if (exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.ConnectedPartyNotProperlyRespond)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.OperationTimedOut)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.MailboxServerIsTooBusy)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.MapiExceptionRpcServerTooBusy)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.ConcurrentConnectionLimitExceed)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.TemporaryServerError)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.ConnectionForcibltClosed)
                                || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.UnableReadData))
                            {
                                logHelper.LogMessage($"Start Retry Send Email: {retryAttempts} with total : {backoffInterval}, From : {username}, EmailTo: {emailToStr} | EmailCC: {emailCCStr} | Subject: {emailSubject} | Exception: {ex.ToString()}");

                                // Delay for the requested seconds (need to wait at least 30 sec to send email)
                                Thread.Sleep(backoffInterval);

                                // Increase counters
                            }
                            else if (exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.SubmissionQuotaExceededException)
                            || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.SenderSubmissionExceeded)
                            || exStackTrace.Contains(ConstantHelper.Email.ExceptionMessage.SenderThreadLimitExceeded))
                            {

                                logHelper.LogMessage($"Start Retry Send Email: {retryAttempts} with total : {backoffInterval}, From : {username}, EmailTo: {emailToStr} | EmailCC: {emailCCStr} | Subject: {emailSubject} | Exception: {ex.ToString()}");

                                //Second Account Info
                                string secondEmailAddress = appSettings.AG_SECONDEMAILFROM;
                                string ecryptedSecondUsername = appSettings.AG_SECONDEMAILUSERNAME;
                                string ecryptedSecondPassword = appSettings.AG_SECONDEMAILPASSWORD;

                                //Stop to send email if the missing second account information
                                if (string.IsNullOrEmpty(secondEmailAddress) || string.IsNullOrEmpty(ecryptedSecondUsername) || string.IsNullOrEmpty(ecryptedSecondPassword))
                                {
                                    throw ex;
                                }

                                MailAddress secondEmailFrom = new MailAddress(secondEmailAddress);
                                string decryptKey = appSettings.AG_ENCKEY;
                                string decryptedKey = EncryptionHelper.Decrypt(decryptKey);
                                string secondUsername = EncryptionHelper.Decrypt(ecryptedSecondUsername, decryptedKey);
                                string secondPassword = EncryptionHelper.Decrypt(ecryptedSecondPassword, decryptedKey);

                                //Resend the email by using second account when the first account exceed the sent amount
                                //Stop send email without retry if current email is using second account to sent
                                if (username == secondUsername)
                                {
                                    throw ex;
                                }
                                else if (username != secondUsername)
                                {
                                    SendEmailWithRetry(smtp, smtp_Port, secondEmailFrom, emailTo, emailCC, emailSubject, emailBody, isHtml, eAttachment, useDefaultCredential, useSSL, secondUsername, secondPassword, emailStatusObj);
                                    return;
                                }
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                    throw new Exception(string.Format("Maximum retry attempts {0}, have been attempted.", retryCount));
                }
                catch (Exception ex)
                {
                    string emailToStr = string.Empty;
                    string emailCCStr = string.Empty;

                    if (emailTo?.Count > 0)
                        emailToStr = String.Join(",", emailTo.Select(x => x.Address));

                    if (emailCC?.Count > 0)
                        emailCCStr = String.Join(",", emailCC.Select(x => x.Address));

                    logHelper.LogMessage($"Error SendEmailWithRetry: From: {username} | EmailTo: {emailToStr} | EmailCC: {emailCCStr} | Subject: {emailSubject} | Exception: {ex}");



                    if (emailStatusObj != null)
                    {
                        emailStatusObj.SentSuccess = false;
                        UpdateEmailSendStatus(emailStatusObj, emailStatusObj.SPContext);
                    }
                }
                finally
                {
                    foreach (MemoryStream memoryStream in attacmentMemoryStreams)
                    {
                        memoryStream?.Dispose();
                    }
                }
            });
        }

        public static void UpdateEmailSendStatus(EmailStatusEntity emailStatusObj, ClientContext clientContext)
        {
            LogHelper logHelper = new LogHelper();

            try
            {
                int retryAttempts = 0;
                int backoffInterval = 30000;
                int retryCount = 1;
                int.TryParse(appSettings.NoOfRetry, out retryCount);
                int.TryParse(appSettings.ThrottlingSleeptime, out backoffInterval);

                string fieldName = emailStatusObj.EmailStatusFieldName;
                string status = "fail";
                string newRecord = string.Empty;
                if (emailStatusObj.SentSuccess)
                {
                    status = "success";
                }
                newRecord = string.Format(ConstantHelper.InfoMessage.EmailSentStatusInfo, emailStatusObj.EmailType, status, DateTimeHelper.GetCurrentDateTime());

                do
                {
                    try
                    {
                        if (emailStatusObj != null && emailStatusObj.SPContext != null && !string.IsNullOrEmpty(emailStatusObj.ListName))
                        {
                            string[] viewFields = { "ID", fieldName };
                            ListItem targetSPItem = GeneralQueryHelper.GetSPItemById(clientContext, emailStatusObj.ListName, emailStatusObj.ItemId, viewFields);

                            string origRecord = MHA.Framework.Core.SP.FieldHelper.GetFieldValueAsString(targetSPItem, fieldName);
                            if (!string.IsNullOrEmpty(origRecord))
                            {
                                newRecord = string.Format("{0};{1}", origRecord, newRecord);
                            }

                            targetSPItem[fieldName] = newRecord;
                            if (!string.IsNullOrEmpty(emailStatusObj.IsSystemUpdateFieldName))
                            {
                                targetSPItem[emailStatusObj.IsSystemUpdateFieldName] = true;
                            }

                            targetSPItem.Update();
                            clientContext.ExecuteQueryWithIncrementalRetry();
                        }
                        break;
                    }
                    catch (Exception innerEx)
                    {
                        logHelper.LogMessage("EmailHelper - UpdateEmailSendStatus Error: " + innerEx.ToString());
                        string errorMsg = innerEx.Message.ToLower();
                        bool requiredRetry = ProjectHelper.RequiredRetryException(errorMsg);
                        if (requiredRetry && (retryAttempts < retryCount))
                        {
                            retryAttempts++;
                            logHelper.LogMessage($"Start Retry Update Email Send Status: {retryAttempts} with total : {retryCount}");
                            Thread.Sleep(backoffInterval);
                        }
                        else
                        {
                            break;
                        }
                    }
                } while (retryAttempts < retryCount);
            }
            catch (Exception ex)
            {
                logHelper.LogMessage($"EmailHelper - UpdateEmailSendStatus Error: {ex.ToString()}");
            }
        }
        #endregion

        #region Common fucntions
        public static void AddEmail(string email, ref MailAddressCollection emails)
        {
            if (!string.IsNullOrEmpty(email))
            {
                MailAddress mailAdd = new MailAddress(email);
                if (!emails.Contains(mailAdd))
                    emails.Add(mailAdd);
            }
        }

        public static string[] GetEmailFromString(String emailString)
        {
            string[] arrayEmailCollection = { };
            if (!string.IsNullOrEmpty(emailString))
            {
                arrayEmailCollection = emailString.Split(ConstantHelper.Delimiter.SemiColonDelimiter, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            }
            return arrayEmailCollection;
        }
        #endregion
    }
}
