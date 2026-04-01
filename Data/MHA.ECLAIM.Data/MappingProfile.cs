using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Entities.ViewModel.TravelRequest;
using MHA.ECLAIM.Framework.Helpers;
using AutoMapper;

namespace MHA.ECLAIM.Data
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            #region MainClaimHeader
            CreateMap<MainClaimHeaderVM, MainClaimHeader>()
                .ForMember(dest => dest.RequestDate, opt => opt.MapFrom(src =>
                    src.RequestDate != null && src.RequestDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.RequestDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src =>
                    src.CreatedDate != null && src.CreatedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.CreatedDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src =>
                    src.ModifiedDate != null && src.ModifiedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.ModifiedDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom(src =>
                    src.SubmittedDate != null && src.SubmittedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? (DateTime?)DateTimeHelper.ConvertToUTCDateTime(src.SubmittedDate.Value)
                        : null))
                .ForMember(dest => dest.SelectedClaimCategorySpId, opt => opt.MapFrom(src =>
                    string.IsNullOrWhiteSpace(src.SelectedClaimCategorySpId) ? 0 : int.Parse(src.SelectedClaimCategorySpId)))
                .ForMember(dest => dest.SubClaimDetails, opt => opt.Ignore());
            #endregion

            #region SubClaimDetails
            CreateMap<SubClaimDetailsVM, SubClaimDetails>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src =>
                    src.CreatedDate != null && src.CreatedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.CreatedDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src =>
                    src.ModifiedDate != null && src.ModifiedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.ModifiedDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.ClaimEntitlementType,
                    opt => opt.Condition(src => !string.IsNullOrWhiteSpace(src.ClaimEntitlementType)));
            #endregion

            #region TravelRequestHeader
            CreateMap<TravelRequestHeaderVM, TravelRequestHeaderEntity>().ReverseMap()
                .ForMember(dest => dest.DateOfRequest, opt => opt.MapFrom(src =>
                    src.DateOfRequest != null && src.DateOfRequest >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.DateOfRequest.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.CreatedByDate, opt => opt.MapFrom(src =>
                    src.CreatedByDate != null && src.CreatedByDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.CreatedByDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.ModifiedByDate, opt => opt.MapFrom(src =>
                    src.ModifiedDate != null && src.ModifiedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(src.ModifiedDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime()))
                .ForMember(dest => dest.SubmittedDate, opt => opt.MapFrom(src =>
                    src.SubmittedDate != null && src.SubmittedDate >= System.Data.SqlTypes.SqlDateTime.MinValue.Value
                        ? (DateTime?)DateTimeHelper.ConvertToUTCDateTime(src.SubmittedDate.Value)
                        : null));
            #endregion

            #region Reverse Maps
            CreateMap<MainClaimHeader, MainClaimHeaderVM>()
                .ForMember(dest => dest.ClaimEntitlementType1StartDate, opt => opt.MapFrom(src =>
                    src.ClaimEntitlementType1StartDate.HasValue
                        ? (DateTime?)DateTimeHelper.ConvertToLocalDateTime(src.ClaimEntitlementType1StartDate.Value)
                        : null))
                .ForMember(dest => dest.ClaimEntitlementType1EndDate, opt => opt.MapFrom(src =>
                    src.ClaimEntitlementType1EndDate.HasValue
                        ? (DateTime?)DateTimeHelper.ConvertToLocalDateTime(src.ClaimEntitlementType1EndDate.Value)
                        : null))
                .ForMember(dest => dest.ClaimEntitlementType2StartDate, opt => opt.MapFrom(src =>
                    src.ClaimEntitlementType2StartDate.HasValue
                        ? (DateTime?)DateTimeHelper.ConvertToLocalDateTime(src.ClaimEntitlementType2StartDate.Value)
                        : null))
                .ForMember(dest => dest.ClaimEntitlementType2EndDate, opt => opt.MapFrom(src =>
                    src.ClaimEntitlementType2EndDate.HasValue
                        ? (DateTime?)DateTimeHelper.ConvertToLocalDateTime(src.ClaimEntitlementType2EndDate.Value)
                        : null));
            CreateMap<SubClaimDetails, SubClaimDetailsVM>();
            #endregion
        }
    }
}
