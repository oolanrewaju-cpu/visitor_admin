using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using visitor_admin.Models.Dtos;
using visitor_admin.Repositories.Interfaces;

namespace visitor_admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestRolesController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IRequestRoleRepository _requestRoleRepository;
        private readonly IMapper _mapper;

        public RequestRolesController(ILogger<RequestRolesController> logger, IRequestRoleRepository requestRoleRepository, IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _requestRoleRepository = requestRoleRepository ?? throw new ArgumentNullException(nameof(requestRoleRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RequestRoleDto>>> GetAllRequestRoles()
        {
            try
            {
                _logger.LogInformation("Retrieving all request roles...");
                var requestRoles = await _requestRoleRepository.GetAllRequestRolesAsync();
                _logger.LogInformation("Retrieved {Count} request roles.", requestRoles.Count());
                return Ok(_mapper.Map<IEnumerable<RequestRoleDto>>(requestRoles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving request roles.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RequestRoleDto>> GetRequestRoleById(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving request role with ID: {Id}", id);
                var requestRole = await _requestRoleRepository.GetRequestRoleByIdAsync(id);

                if (requestRole == null)
                {
                    _logger.LogWarning("Request role with ID: {Id} not found.", id);
                    return NotFound();
                }

                return Ok(_mapper.Map<RequestRoleDto>(requestRole));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving request role with ID: {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
