using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using visitor_admin.Entities;
using visitor_admin.Models.Dtos;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IStaffRepository _staffRepository;
        const int maxPageSize = 10;

        public StaffController(ILogger<StaffController> logger, IStaffRepository staffRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Staff>>> GetAllStaff(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageSize > maxPageSize) // Enforce maximum page size to prevent excessive data retrieval
                {
                    pageSize = maxPageSize;
                }

                // Log the incoming request parameters for better traceability
                _logger.LogInformation("Retrieving staff members with Name: {Name}, SearchQuery: {SearchQuery}, PageNumber: {PageNumber}, PageSize: {PageSize}", name, searchQuery, pageNumber, pageSize);
                var staffMembers = await _staffRepository.GetAllStaffAsync(name, searchQuery, pageNumber, pageSize);
                _logger.LogInformation("Retrieved {Count} staff members from the repository.", staffMembers.Count());
                return Ok(staffMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving staff members.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
