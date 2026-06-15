using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using visitor_admin.Entities;
using visitor_admin.Helpers;
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

        [HttpPost("bulk-upload")]
        public async Task<ActionResult<BulkUploadResultDto>> BulkUploadStaff(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Only CSV files are allowed.");
                }

                _logger.LogInformation("Starting bulk upload from file: {FileName}", file.FileName);

                List<CsvStaffImportDto> csvRecords;
                try
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            MissingFieldFound = null,
                            HeaderValidated = null,
                            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
                        };
                        using var csv = new CsvReader(reader, config);
                        csv.Context.RegisterClassMap<CsvStaffImportDtoMap>();
                        csvRecords = csv.GetRecords<CsvStaffImportDto>().ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "CSV parsing failed.");
                    return BadRequest("Invalid CSV format. Expected columns: Username, Firstname, Surname, Email, Department, StatusID, RoleID, RequestRoleName.");
                }

                if (csvRecords.Count == 0)
                {
                    return BadRequest("CSV file contains no data.");
                }

                var existingUsernames = new HashSet<string>(await _staffRepository.GetAllUsernamesAsync(), StringComparer.OrdinalIgnoreCase);
                var existingEmails = new HashSet<string>(await _staffRepository.GetAllEmailsAsync(), StringComparer.OrdinalIgnoreCase);
                var departments = await _departmentRepository.GetAllDepartmentsAsync(null, null);
                var departmentLookup = departments.ToDictionary(d => d.DepartmentName, d => d, StringComparer.OrdinalIgnoreCase);
                var requestRoles = await _requestRoleRepository.GetAllRequestRolesAsync();
                var requestRoleLookup = requestRoles.ToDictionary(r => r.RequestRoleName, r => r, StringComparer.OrdinalIgnoreCase);

                var defaultPassword = PasswordHelper.Hash("staff@lbs");
                var validStaff = new List<Staff>();
                var result = new BulkUploadResultDto { TotalRows = csvRecords.Count };
                var processedUsernames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var processedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < csvRecords.Count; i++)
                {
                    var record = csvRecords[i];
                    var rowNumber = i + 2;
                    var errors = new List<string>();

                    if (string.IsNullOrWhiteSpace(record.Username))
                        errors.Add("Username is required.");
                    if (string.IsNullOrWhiteSpace(record.Firstname))
                        errors.Add("Firstname is required.");
                    if (string.IsNullOrWhiteSpace(record.Surname))
                        errors.Add("Surname is required.");
                    if (string.IsNullOrWhiteSpace(record.Email))
                        errors.Add("Email is required.");
                    if (string.IsNullOrWhiteSpace(record.Department))
                        errors.Add("Department is required.");
                    if (string.IsNullOrWhiteSpace(record.RequestRoleName))
                        errors.Add("RequestRoleName is required.");

                    if (errors.Count > 0)
                    {
                        result.Failed++;
                        result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Message = string.Join(" ", errors) });
                        continue;
                    }

                    if (existingUsernames.Contains(record.Username) || processedUsernames.Contains(record.Username))
                    {
                        result.Failed++;
                        result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Message = $"Username '{record.Username}' already exists." });
                        continue;
                    }

                    if (existingEmails.Contains(record.Email) || processedEmails.Contains(record.Email))
                    {
                        result.Failed++;
                        result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Message = $"Email '{record.Email}' already exists." });
                        continue;
                    }

                    if (!departmentLookup.TryGetValue(record.Department, out var dept))
                    {
                        result.Failed++;
                        result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Message = $"Department '{record.Department}' not found." });
                        continue;
                    }

                    if (!requestRoleLookup.TryGetValue(record.RequestRoleName, out var role))
                    {
                        result.Failed++;
                        result.Errors.Add(new BulkUploadErrorDto { Row = rowNumber, Message = $"RequestRole '{record.RequestRoleName}' not found." });
                        continue;
                    }

                    validStaff.Add(new Staff
                    {
                        Username = record.Username,
                        Firstname = record.Firstname,
                        Surname = record.Surname,
                        Email = record.Email,
                        Department = dept.DepartmentName,
                        DepartmentID = dept.DepartmentID,
                        Password = defaultPassword,
                        StatusID = record.StatusID,
                        RoleID = record.RoleID,
                        RequestRoleID = role.RequestRoleID,
                        LastModifiedBy = DateTime.UtcNow
                    });

                    processedUsernames.Add(record.Username);
                    processedEmails.Add(record.Email);
                }

                if (validStaff.Count > 0)
                {
                    await _staffRepository.BulkRegisterUsersAsync(validStaff);
                }

                result.Succeeded = validStaff.Count;
                _logger.LogInformation("Bulk upload completed. Total: {Total}, Succeeded: {Succeeded}, Failed: {Failed}", result.TotalRows, result.Succeeded, result.Failed);

                return StatusCode(StatusCodes.Status207MultiStatus, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during bulk upload.");
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
