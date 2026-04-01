using System.Text;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class QueryHelper
    {
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

        public static string ConcatCriteria(string curQuery, string fieldName, string typename, string value, string operatorType, bool blnLookupID)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            if (!blnLookupID)
            {
                empty2 = "\r\n            <And>\r\n                {0}\r\n                <{4}>\r\n                    <FieldRef Name='{1}' />\r\n                    <Value Type='{2}'>{3}</Value>\r\n                </{4}>\r\n            </And>\r\n            ";
                empty = "\r\n              <{3}>\r\n                <FieldRef Name='{0}' />\r\n                <Value Type='{1}'>{2}</Value>\r\n              </{3}>\r\n            ";
            }
            else
            {
                empty2 = "\r\n            <And>\r\n                {0}\r\n                <{4}>\r\n                    <FieldRef Name='{1}' LookupId='TRUE'/>\r\n                    <Value Type='{2}'>{3}</Value>\r\n                </{4}>\r\n            </And>\r\n            ";
                empty = "\r\n              <{3}>\r\n                <FieldRef Name='{0}' LookupId='TRUE'/>\r\n                <Value Type='{1}'>{2}</Value>\r\n              </{3}>\r\n            ";
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(curQuery))
            {
                stringBuilder.AppendFormat(empty, fieldName, typename, value, operatorType);
            }
            else
            {
                stringBuilder.AppendFormat(empty2, curQuery, fieldName, typename, value, operatorType);
            }

            return stringBuilder.ToString();
        }

        public static string ConcatCriteriaOr(string curQuery, string fieldName, string typename, string value, string operatorType, bool blnLookupID)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            if (!blnLookupID)
            {
                empty2 = "\r\n            <Or>\r\n                {0}\r\n                <{4}>\r\n                    <FieldRef Name='{1}' />\r\n                    <Value Type='{2}'>{3}</Value>\r\n                </{4}>\r\n            </Or>\r\n            ";
                empty = "\r\n              <{3}>\r\n                <FieldRef Name='{0}' />\r\n                <Value Type='{1}'>{2}</Value>\r\n              </{3}>\r\n            ";
            }
            else
            {
                empty2 = "\r\n            <Or>\r\n                {0}\r\n                <{4}>\r\n                    <FieldRef Name='{1}' LookupId='TRUE'/>\r\n                    <Value Type='{2}'>{3}</Value>\r\n                </{4}>\r\n            </Or>\r\n            ";
                empty = "\r\n              <{3}>\r\n                <FieldRef Name='{0}' LookupId='TRUE'/>\r\n                <Value Type='{1}'>{2}</Value>\r\n              </{3}>\r\n            ";
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(curQuery))
            {
                stringBuilder.AppendFormat(empty, fieldName, typename, value, operatorType);
            }
            else
            {
                stringBuilder.AppendFormat(empty2, curQuery, fieldName, typename, value, operatorType);
            }

            return stringBuilder.ToString();
        }

        public static string ConcatCriteria(string curQuery, string fieldName, string operatorType, bool blnLookupID)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            if (!blnLookupID)
            {
                empty2 = "\r\n            <And>\r\n                {0}\r\n                <{2}>\r\n                    <FieldRef Name='{1}' />                    \r\n                </{2}>\r\n            </And>\r\n            ";
                empty = "\r\n              <{1}>\r\n                <FieldRef Name='{0}' />\r\n              </{1}>\r\n            ";
            }
            else
            {
                empty2 = "\r\n            <And>\r\n                {0}\r\n                <{2}>\r\n                    <FieldRef Name='{1}' LookupId='TRUE'/>\r\n                </{2}>\r\n            </And>\r\n            ";
                empty = "\r\n              <{1}>\r\n                <FieldRef Name='{0}' LookupId='TRUE'/>                \r\n              </{1}>\r\n            ";
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(curQuery))
            {
                stringBuilder.AppendFormat(empty, fieldName, operatorType);
            }
            else
            {
                stringBuilder.AppendFormat(empty2, curQuery, fieldName, operatorType);
            }

            return stringBuilder.ToString();
        }

        public static string ConcatCriteriaOr(string curQuery, string fieldName, string operatorType, bool blnLookupID)
        {
            string empty = string.Empty;
            string empty2 = string.Empty;
            if (!blnLookupID)
            {
                empty2 = "\r\n            <Or>\r\n                {0}\r\n                <{2}>\r\n                    <FieldRef Name='{1}' />                    \r\n                </{2}>\r\n            </Or>\r\n            ";
                empty = "\r\n              <{1}>\r\n                <FieldRef Name='{0}' />                \r\n              </{1}>\r\n            ";
            }
            else
            {
                empty2 = "\r\n            <Or>\r\n                {0}\r\n                <{2}>\r\n                    <FieldRef Name='{1}' LookupId='TRUE'/>\r\n                </{2}>\r\n            </Or>\r\n            ";
                empty = "\r\n              <{1}>\r\n                <FieldRef Name='{0}' LookupId='TRUE'/>\r\n              </{1}>\r\n            ";
            }

            StringBuilder stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(curQuery))
            {
                stringBuilder.AppendFormat(empty, fieldName, operatorType);
            }
            else
            {
                stringBuilder.AppendFormat(empty2, curQuery, fieldName, operatorType);
            }

            return stringBuilder.ToString();
        }

        public static string GetViewFields(string[] fieldRefs)
        {
            if (fieldRefs != null && fieldRefs.Length != 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendFormat("<ViewFields>");
                foreach (string arg in fieldRefs)
                {
                    stringBuilder.AppendFormat($"<FieldRef Name=\"{arg}\"></FieldRef>");
                }

                stringBuilder.AppendFormat("</ViewFields>");
                return stringBuilder.ToString();
            }

            return string.Empty;
        }
    }
}
