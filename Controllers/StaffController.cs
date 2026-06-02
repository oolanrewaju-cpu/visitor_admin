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
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IRequestRoleRepository _requestRoleRepository;
        private readonly IMapper _mapper;
        const int maxPageSize = 10;

        public StaffController(ILogger<StaffController> logger, IStaffRepository staffRepository, IDepartmentRepository departmentRepository, IRequestRoleRepository requestRoleRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _requestRoleRepository = requestRoleRepository ?? throw new ArgumentNullException(nameof(requestRoleRepository));
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

                var staffEntity = _mapper.Map<Staff>(createStaffDto);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> PatchStaff(int id, PatchStaffDto patchStaffDto)
        {
            try
            {
                _logger.LogInformation("Patching staff member with ID: {Id}", id);

                // Retrieve existing staff entity
                var staffEntity = await _staffRepository.GetStaffByIdAsync(id);
                if (staffEntity == null)
                {
                    _logger.LogWarning("Staff member with ID: {Id} not found.", id);
                    return NotFound();
                }

                // Map only the patchable fields, ignoring UserID and DepartmentID
                _mapper.Map(patchStaffDto, staffEntity);

                if (patchStaffDto.Department != null)
                {
                    var dept = await _departmentRepository.GetByNameAsync(patchStaffDto.Department);
                    if (dept == null)
                    {
                        _logger.LogWarning("Department '{Department}' not found.", patchStaffDto.Department);
                        return BadRequest($"Department '{patchStaffDto.Department}' not found.");
                    }
                    staffEntity.DepartmentID = dept.DepartmentID;
                }

                if (patchStaffDto.RequestRoleName != null)
                {
                    var role = await _requestRoleRepository.GetByNameAsync(patchStaffDto.RequestRoleName);
                    if (role == null)
                    {
                        _logger.LogWarning("Request role '{RequestRoleName}' not found.", patchStaffDto.RequestRoleName);
                        return BadRequest($"Request role '{patchStaffDto.RequestRoleName}' not found.");
                    }
                    staffEntity.RequestRoleID = role.RequestRoleID;
                }

                staffEntity.LastModifiedBy = DateTime.UtcNow;

                // Update the staff in the database
                await _staffRepository.UpdateUser(staffEntity);

                _logger.LogInformation("Successfully patched staff member with ID: {Id}", id);
                return NoContent(); // 204 No Content is appropriate for a successful PUT/PATCH
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while patching staff member with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                  "An error occurred while processing your request.");
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
