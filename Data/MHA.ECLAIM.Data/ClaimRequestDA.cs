using MHA.ECLAIM.Data.Context;
using MHA.ECLAIM.Entities.DTO;
using MHA.ECLAIM.Entities.Entities;
using MHA.ECLAIM.Entities.ViewModel.Claim;
using MHA.ECLAIM.Framework.Constants;
using MHA.ECLAIM.Framework.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Data.SqlTypes;
using System.Linq.Dynamic.Core;
using System.Reflection.Metadata;

namespace MHA.ECLAIM.Data
{
    public class ClaimRequestDA
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClaimRequestDA()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionStringHelper.GetGenericWFConnString())
            .Options;
            _context = new AppDbContext(options);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, new NullLoggerFactory());

            _mapper = mapperConfig.CreateMapper();
        }

        #region Create
        public async Task<MainClaimHeaderVM> CreateNewRequestAsync(MainClaimHeaderVM vm)
        {
            var header = _mapper.Map<MainClaimHeader>(vm);
            _context.MainClaimHeaders.Add(header);

            if (vm.SubClaimDetails != null && vm.SubClaimDetails.Any())
            {
                foreach (var subVm in vm.SubClaimDetails)
                {
                    var subClaim = _mapper.Map<SubClaimDetails>(subVm);
                    subClaim.MainClaimHeaderID = header.ID;
                    subClaim.TotalClaimAmount = vm.TotalClaimAmount;
                    header.SubClaimDetails.Add(subClaim);
                }
            }

            await _context.SaveChangesAsync();

            int i = 0;
            foreach (var subClaim in header.SubClaimDetails)
            {
                vm.SubClaimDetails[i].ID = subClaim.ID;
                vm.SubClaimDetails[i].MainClaimHeaderID = header.ID;
                i++;
            }

            vm.ID = header.ID;
            vm.IsSuccessful = true;
            return vm;
        }
        #endregion

        #region Read
        public async Task<PagedResultDto<MainClaimHeader>> GetPagedAsync(MainClaimHeaderSearchModel search, string? sortCol, string? sortDirection, int skip, int take)
        {
            var query = _context.MainClaimHeaders.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(search.ReferenceNo))
                query = query.Where(x => x.ReferenceNo.Contains(search.ReferenceNo));
            if (!string.IsNullOrEmpty(search.RequesterCompanyCode))
                query = query.Where(x => x.RequesterCompanyCode.Contains(search.RequesterCompanyCode));
            if (!string.IsNullOrEmpty(search.RequesterName))
                query = query.Where(x => x.RequesterName.Contains(search.RequesterName));
            if (!string.IsNullOrEmpty(search.RequesterEmployeeID))
                query = query.Where(x => x.RequesterEmployeeID.Contains(search.RequesterEmployeeID));
            if (search.RequestStartDate != null && search.RequestStartDate != DateTime.MinValue)
                query = query.Where(x => x.RequestDate >= search.RequestStartDate);
            if (search.RequestEndDate != null && search.RequestEndDate != DateTime.MinValue)
                query = query.Where(x => x.RequestDate <= search.RequestEndDate);
            if (search.RequesterCostCenter.Count > 0)
                query = query.Where(x => search.RequesterCostCenter.Contains(x.RequesterCostCenter));
            if (search.RequesterDepartment.Count > 0)
                query = query.Where(x => search.RequesterDepartment.Contains(x.RequesterDepartment));
            if (search.ClaimStatus.Count > 0)
                query = query.Where(x => search.ClaimStatus.Contains(x.ClaimStatus));
            if (search.AccessDepartments.Count > 0)
                query = query.Where(x => x.DepartmentID.HasValue && search.AccessDepartments.Contains(x.DepartmentID.Value));
            if (!string.IsNullOrEmpty(search.MemberLogin))
                query = query.Where(x => search.MemberLogin.Equals(x.CreatedByLogin));

            query = query.Where(
                x => x.ClaimStatus != ConstantHelper.WorkflowStatus.DRAFT ||
                (x.ClaimStatus == ConstantHelper.WorkflowStatus.DRAFT && x.CreatedByLogin == search.CurrentUserLogin));

            var totalCount = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(sortCol))
            {
                var order = $"{sortCol} {sortDirection ?? "asc"}";
                query = query.OrderBy(order);
            }
            else
            {
                var order = $"{"CreatedDate"} {"desc"}";
                query = query.OrderBy(order);
            }

            // Paging
            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return new PagedResultDto<MainClaimHeader>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<PagedResultDto<MainClaimHeader>> GetPagedMyActiveRequest(int skip, int take, List<string> claimStatuses, string currUserLogin)
        {
            var query = _context.MainClaimHeaders
                .Where(x => !claimStatuses.Contains(x.ClaimStatus)
                && x.CreatedByLogin == currUserLogin)
                .OrderBy(x => x.ID);

            int totalCount = await query.CountAsync();

            // Paging
            var items = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return new PagedResultDto<MainClaimHeader>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<MainClaimHeaderVM?> RetrieveMainClaimHeaderByIDAsync(int mainClaimHeaderID)
        {
            var header = await _context.MainClaimHeaders
                .Include(h => h.SubClaimDetails)
                .FirstOrDefaultAsync(h => h.ID == mainClaimHeaderID);

            return _mapper.Map<MainClaimHeaderVM>(header);
        }
        #endregion

        #region Update
        public async Task<bool> UpdateRequestAsync(MainClaimHeaderVM vm, bool fromSubmission)
        {
            if (vm.ID == null || vm.ID <= 0)
                throw new ArgumentException("Request ID cannot be null or <= 0 for update.");

            var header = await _context.MainClaimHeaders
                .Include(h => h.SubClaimDetails)
                .FirstOrDefaultAsync(h => h.ID == vm.ID);

            if (header == null)
                throw new Exception($"No TravelRequestHeader found with ID {vm.ID}");

            _mapper.Map(vm, header);

            if (fromSubmission)
            {
                header.ClaimStatus = vm.ClaimStatus;
                header.SubmittedByLogin = vm.SubmittedByLogin;
                header.SubmittedBy = vm.SubmittedBy;
                header.SubmittedDate = vm.SubmittedDate != null &&
                                       vm.SubmittedDate >= SqlDateTime.MinValue.Value
                    ? DateTimeHelper.ConvertToUTCDateTime(vm.SubmittedDate.Value)
                    : null;
            }

            var existingSubs = header.SubClaimDetails.ToList();
            var vmSubs = vm.SubClaimDetails ?? new List<SubClaimDetailsVM>();

            foreach (var subVm in vmSubs)
            {
                subVm.UnitPriceConverted ??= 0m;
                subVm.TransactionAmountConverted ??= 0m;
                subVm.TotalClaimAmount = vm.TotalClaimAmount;
                var existing = existingSubs.FirstOrDefault(s => s.ID == subVm.ID);
                if (existing != null)
                {
                    // update existing
                    _mapper.Map(subVm, existing);
                }
                else
                {
                    // add new
                    var newSub = _mapper.Map<SubClaimDetails>(subVm);
                    newSub.MainClaimHeaderID = header.ID;
                    newSub.CreatedBy = vm.ModifiedBy;
                    newSub.CreatedByLogin = vm.ModifiedByLogin;
                    header.SubClaimDetails.Add(newSub);
                }
            }

            // remove deleted ones
            foreach (var existing in existingSubs)
            {
                if (!vmSubs.Any(s => s.ID == existing.ID))
                {
                    _context.SubClaimDetails.Remove(existing);
                }
            }

            await _context.SaveChangesAsync();

            return true;
        }
        
        public async Task<bool> UpdateApprovalRequestAsync(ApprovalFormVM vm)
        {
            if (vm.MainClaimHeaderVM.GeneralRemarks == null || vm.MainClaimHeaderVM.ID <= 0)
                throw new ArgumentException("Request ID cannot be null or <= 0 for update.");

            try
            {
                var header = await _context.MainClaimHeaders
                        .Include(h => h.SubClaimDetails)
                        .FirstOrDefaultAsync(h => h.ID == vm.MainClaimHeaderVM.ID);

                if (header == null)
                    throw new Exception($"No ecMainClaimHeader found with ID {vm.MainClaimHeaderVM.ID}");

                _mapper.Map(vm.MainClaimHeaderVM, header);

                header.ModifiedDate =
                    vm.MainClaimHeaderVM.ModifiedDate != null &&
                    vm.MainClaimHeaderVM.ModifiedDate >= SqlDateTime.MinValue.Value
                        ? DateTimeHelper.ConvertToUTCDateTime(vm.MainClaimHeaderVM.ModifiedDate.Value)
                        : DateTimeHelper.GetCurrentUtcDateTime();

                #region Resubmission
                if (vm.MainClaimHeaderVM.ClaimStatus == ConstantHelper.WorkflowStatus.PendingOriginatorResubmission)
                {
                    var existingSubs = header.SubClaimDetails.ToList();
                    var vmSubs = vm.MainClaimHeaderVM.SubClaimDetails ?? new List<SubClaimDetailsVM>();

                    foreach (var subVm in vmSubs)
                    {
                        subVm.UnitPriceConverted ??= 0m;
                        subVm.TransactionAmountConverted ??= 0m;
                        subVm.TotalClaimAmount = vm.MainClaimHeaderVM.TotalClaimAmount;
                        var existing = existingSubs.FirstOrDefault(s => s.ID == subVm.ID);
                        if (existing != null)
                        {
                            // update existing
                            _mapper.Map(subVm, existing);
                        }
                        else
                        {
                            // add new
                            var newSub = _mapper.Map<SubClaimDetails>(subVm);
                            newSub.MainClaimHeaderID = header.ID;
                            newSub.CreatedBy = vm.ModifiedBy;
                            newSub.CreatedByLogin = vm.ModifiedByLogin;
                            header.SubClaimDetails.Add(newSub);
                        }
                    }

                    // remove deleted ones
                    foreach (var existing in existingSubs)
                    {
                        if (!vmSubs.Any(s => s.ID == existing.ID))
                        {
                            _context.SubClaimDetails.Remove(existing);
                        }
                    }
                }
                #endregion

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper logHelper = new LogHelper();
                logHelper.LogMessage("[DA] ClaimRequestDA - UpdateApprovalRequestAsync Error: " + ex);
                throw;
            }
        }
        #endregion

        #region Delete
        public async Task<bool> DeleteClaimDetailsAsync(int mainClaimHeaderId, int claimDetailId)
        {
            var claim = await _context.SubClaimDetails
                .FirstOrDefaultAsync(q => q.MainClaimHeaderID == mainClaimHeaderId
                                       && q.ID == claimDetailId);

            if (claim == null)
                return false;

            _context.SubClaimDetails.Remove(claim);
            await _context.SaveChangesAsync();

            var remaining = await _context.SubClaimDetails
                .Where(q => q.MainClaimHeaderID == mainClaimHeaderId)
                .ToListAsync();

            var newTotal = remaining.Sum(x => x.SubtotalClaimAmount ?? 0m);

            foreach (var item in remaining)
            {
                item.TotalClaimAmount = newTotal;
            }

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var entity = await _context.MainClaimHeaders.FindAsync(id);
            if (entity == null) return false;

            _context.MainClaimHeaders.Remove(entity);
            await _context.SaveChangesAsync();

            return true;
        }
        #endregion

        #region Workflow
        public async Task<bool> UpdateRequestWFAsync(string curStep, int reqID)
        {
            var header = await _context.MainClaimHeaders
                .FirstOrDefaultAsync(h => h.ID == reqID);

            if (header == null)
                return false;

            header.ClaimStatus = curStep;
            header.ModifiedDate = DateTimeHelper.GetCurrentUtcDateTime();

            await _context.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}
