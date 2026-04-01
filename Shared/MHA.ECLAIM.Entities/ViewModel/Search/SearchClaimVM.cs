using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHA.ECLAIM.Entities.ViewModel.Search
{
    public class SearchClaimVM
    {
        public MainClaimHeaderSearchModel SearchModel { get; set; } = new();
        public PagedResultDto<MainClaimHeader> MainClaimHeaderListing { get; set; } = new();

        public bool ShowInactiveItems { get; set; } = false;
        public string CurrentUserLogin { get; set; } = string.Empty;
        public int Skip { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
    }
}
