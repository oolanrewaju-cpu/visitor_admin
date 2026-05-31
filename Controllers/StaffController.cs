using AutoMapper;
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
        private readonly IMapper _mapper;
        const int maxPageSize = 10;

        public StaffController(ILogger<StaffController> logger, IStaffRepository staffRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetAllStaff(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageSize > maxPageSize)
                {
                    pageSize = maxPageSize;
                }

                _logger.LogInformation("Retrieving staff members with Name: {Name}, SearchQuery: {SearchQuery}, PageNumber: {PageNumber}, PageSize: {PageSize}", name, searchQuery, pageNumber, pageSize);
                var staffMembers = await _staffRepository.GetAllStaffAsync(name, searchQuery, pageNumber, pageSize);
                _logger.LogInformation("Retrieved {Count} staff members from the repository.", staffMembers.Count());
                return Ok(_mapper.Map<IEnumerable<StaffDto>>(staffMembers));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving staff members.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StaffDto>> GetStaffById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving staff member with ID: {Id}", id);
                var staffMember = await _staffRepository.GetStaffByIdAsync(id);

                if (staffMember == null)
                {
                    _logger.LogWarning("Staff member with ID: {Id} not found.", id);
                    return NotFound();
                }

                return Ok(_mapper.Map<StaffDto>(staffMember));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving staff member with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<StaffDto>> CreateStaff([FromBody] CreateStaffDto createStaffDto)
        {
            try
            {
                _logger.LogInformation("Creating a new staff member with Username: {Username}", createStaffDto.Username);

                var staffEntity = _mapper.Map<Entities.Staff>(createStaffDto);
                await _staffRepository.RegisterUser(staffEntity);

                var staffDto = _mapper.Map<StaffDto>(staffEntity);
                return CreatedAtAction(nameof(GetStaffById), new { id = staffEntity.UserID }, staffDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new staff member.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<StaffDto>> PatchStaff(int id, [FromBody] PatchStaffDto patchStaffDto)
        {
            try
            {
                _logger.LogInformation("Patching staff member with ID: {Id}", id);

                var staffEntity = await _staffRepository.GetStaffByIdAsync(id);
                if (staffEntity == null)
                {
                    _logger.LogWarning("Staff member with ID: {Id} not found for patching.", id);
                    return NotFound();
                }

                _mapper.Map(patchStaffDto, staffEntity);
                await _staffRepository.UpdateUser(staffEntity);

                _logger.LogInformation("Successfully patched staff member with ID: {Id}", id);
                return Ok(_mapper.Map<StaffDto>(staffEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while patching staff member with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStaff(int id)
        {
            try
            {
                _logger.LogInformation("Deleting staff member with ID: {Id}", id);

                var staffEntity = await _staffRepository.GetStaffByIdAsync(id);
                if (staffEntity == null)
                {
                    _logger.LogWarning("Staff member with ID: {Id} not found for deletion.", id);
                    return NotFound();
                }

                await _staffRepository.DeleteUserAsync(id);
                _logger.LogInformation("Successfully deleted staff member with ID: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting staff member with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
