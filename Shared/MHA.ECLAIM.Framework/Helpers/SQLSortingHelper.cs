using MHA.ECLAIM.Entities;
using MHA.ECLAIM.Framework.Constants;

namespace MHA.ECLAIM.Framework.Helpers
{
    public class SQLSortingHelper
    {
        public static string AscOrDesc(string s)
        {
            if (s == ConstantHelper.Sorting.GridAscending) return ConstantHelper.Sorting.SQLAscending;
            if (s == ConstantHelper.Sorting.GridDescending) return ConstantHelper.Sorting.SQLDescending;

            return string.Empty;
        }

        // Refer Ricoh project to implement SQLSortingMapping
    }
}
