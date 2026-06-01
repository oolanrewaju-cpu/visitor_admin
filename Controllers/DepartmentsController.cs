using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using visitor_admin.Entities;
using visitor_admin.Models.Dtos;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IMapper _mapper;
        const int maxPageSize = 10;

        public DepartmentsController(ILogger<DepartmentsController> logger, IDepartmentRepository departmentRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAllDepartments(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (pageSize > maxPageSize)
                {
                    pageSize = maxPageSize;
                }

                _logger.LogInformation("Retrieving departments with Name: {Name}, SearchQuery: {SearchQuery}, PageNumber: {PageNumber}, PageSize: {PageSize}", name, searchQuery, pageNumber, pageSize);
                var departments = await _departmentRepository.GetAllDepartmentsAsync(name, searchQuery, pageNumber, pageSize);
                _logger.LogInformation("Retrieved {Count} departments from the repository.", departments.Count());
                return Ok(_mapper.Map<IEnumerable<DepartmentDto>>(departments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving departments.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartmentById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving department with ID: {Id}", id);
                var department = await _departmentRepository.GetDepartmentByIdAsync(id);

                if (department == null)
                {
                    _logger.LogWarning("Department with ID: {Id} not found.", id);
                    return NotFound();
                }

                return Ok(_mapper.Map<DepartmentDto>(department));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving department with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            try
            {
                _logger.LogInformation("Creating a new department with Name: {DepartmentName}", createDepartmentDto.DepartmentName);

                var departmentEntity = _mapper.Map<Department>(createDepartmentDto);
                await _departmentRepository.CreateDepartment(departmentEntity);

                var departmentDto = _mapper.Map<DepartmentDto>(departmentEntity);
                return CreatedAtAction(nameof(GetDepartmentById), new { id = departmentEntity.DepartmentID }, departmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new department.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<DepartmentDto>> PatchDepartment(int id, [FromBody] PatchDepartmentDto patchDepartmentDto)
        {
            try
            {
                _logger.LogInformation("Patching department with ID: {Id}", id);

                var departmentEntity = await _departmentRepository.GetDepartmentByIdAsync(id);
                if (departmentEntity == null)
                {
                    _logger.LogWarning("Department with ID: {Id} not found for patching.", id);
                    return NotFound();
                }

                _mapper.Map(patchDepartmentDto, departmentEntity);
                await _departmentRepository.UpdateDepartment(departmentEntity);

                _logger.LogInformation("Successfully patched department with ID: {Id}", id);
                return Ok(_mapper.Map<DepartmentDto>(departmentEntity));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while patching department with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDepartment(int id)
        {
            try
            {
                _logger.LogInformation("Deleting department with ID: {Id}", id);

                var departmentEntity = await _departmentRepository.GetDepartmentByIdAsync(id);
                if (departmentEntity == null)
                {
                    _logger.LogWarning("Department with ID: {Id} not found for deletion.", id);
                    return NotFound();
                }

                await _departmentRepository.DeleteDepartmentAsync(id);
                _logger.LogInformation("Successfully deleted department with ID: {Id}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting department with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
