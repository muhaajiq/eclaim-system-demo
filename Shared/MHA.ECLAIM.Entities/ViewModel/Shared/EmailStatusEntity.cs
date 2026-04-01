using Microsoft.SharePoint.Client;
using System.Runtime.Serialization;

namespace MHA.ECLAIM.Entities.ViewModel.Shared
{
    [Serializable]
    [DataContract]
    public class EmailStatusEntity
    {
        [DataMember]
        public string ListName { get; set; }
        [DataMember]
        public int ItemId { get; set; }
        [DataMember]
        public ClientContext SPContext { get; set; }

        [DataMember]
        public string EmailStatusFieldName { get; set; }
        [DataMember]
        public string EmailType { get; set; }

        [DataMember]
        public string IsSystemUpdateFieldName { get; set; }

        [DataMember]
        public bool SentSuccess { get; set; }

        public EmailStatusEntity()
        {

        }

        public EmailStatusEntity(int itemId, string listName, string emailStatusFieldName, string emailType, ClientContext clientContext, string isSystemUpdateFieldName = "")
        {
            ItemId = itemId;
            ListName = listName;
            EmailStatusFieldName = emailStatusFieldName;
            EmailType = emailType;
            SPContext = clientContext;
            IsSystemUpdateFieldName = isSystemUpdateFieldName;
        }
    }
}
