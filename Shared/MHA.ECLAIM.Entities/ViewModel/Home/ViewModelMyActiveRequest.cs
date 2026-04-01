using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.Entities;

namespace MHA.ECLAIM.Entities.ViewModel.Home
{
    public class ViewModelMyActiveRequest
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public PagedResultDto<MainClaimHeader>? Request { get; set; } = new();
    }
}
