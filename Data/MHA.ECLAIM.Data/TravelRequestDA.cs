using MHA.ECLAIM.Data.Context;
using MHA.ECLAIM.Entities.ViewModel.TravelRequest;
using MHA.ECLAIM.Framework.Helpers;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MHA.ECLAIM.Data
{
    public class TravelRequestDA
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TravelRequestDA()
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

        public async Task<List<TravelRequestHeaderVM>> GetByEmployeeLogin(string employeeLogin)
        {
            var travelRequestHeaders = await _context.TravelRequestHeader
                .Where(a => a.EmployeeLogin == employeeLogin && a.WorkflowStatus == "Completed")
                .ToListAsync();

            return _mapper.Map<List<TravelRequestHeaderVM>>(travelRequestHeaders);
        }
    }
}
