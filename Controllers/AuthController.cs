using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using visitor_admin.Entities;
using visitor_admin.Helpers;
using visitor_admin.Models.Dtos;
using visitor_admin.Repositories.Interfaces;
using visitor_admin.Services;

namespace visitor_admin.Controllers
{
    // Marks the class as an API controller and sets its base route to api/<ControllerName>.
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IUserRepository _userRepository;
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly IOtpVerificationRepository _otpVerificationRepository;
        private readonly JwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IOtpService _otpService;
        private readonly IMapper _mapper;


        public AuthController(
            ILogger<AuthController> logger,
            IUserRepository userRepository,
            IEmailVerificationRepository emailVerificationRepository,
            IOtpVerificationRepository otpVerificationRepository,
            JwtService jwtService,
            IEmailService emailService,
            IOtpService otpService,
            IMapper mapper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _emailVerificationRepository = emailVerificationRepository ?? throw new ArgumentNullException(nameof(emailVerificationRepository));
            _otpVerificationRepository = otpVerificationRepository ?? throw new ArgumentNullException(nameof(otpVerificationRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _otpService = otpService ?? throw new ArgumentNullException(nameof(otpService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto loginRequest)
        {
            try
            {
                _logger.LogInformation("Login attempt for username: {Username}", loginRequest.Username);

                var user = await _userRepository.GetByUsernameAsync(loginRequest.Username);
                if (user == null || !PasswordHelper.Verify(loginRequest.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Invalid login attempt for username: {Username}", loginRequest.Username);
                    return Unauthorized("Invalid username or password.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive account login attempt for username: {Username}", loginRequest.Username);
                    return Unauthorized("Account is deactivated.");
                }

                var token = _jwtService.GenerateToken(user);
                var adminUserDto = _mapper.Map<AdminUserDto>(user);

                _logger.LogInformation("Successful login for username: {Username}", loginRequest.Username);
                return Ok(new AuthResponseDto { Token = token, AdminUser = adminUserDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpGet("login/google")]
        public IActionResult GoogleLogin([FromQuery] string returnUrl)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleLoginCallback", "Auth", new { returnUrl })
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("login/google/callback", Name = "GoogleLoginCallback")]
        public async Task<IActionResult> GoogleLoginCallback([FromQuery] string returnUrl)
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Google authentication failed.");
                    return BadRequest("Google authentication failed.");
                }

                var email = result.Principal.FindFirstValue(ClaimTypes.Email); // Get the email claim from the authenticated user
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Email not provided by Google.");

                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect($"{returnUrl}?error={Uri.EscapeDataString("User not found")}");

                    return Unauthorized("User not found");
                }

                if (!user.IsActive)
                {
                    return Unauthorized("Account is deactivated.");
                }

                var token = _jwtService.GenerateToken(user);
                var adminUserDto = _mapper.Map<AdminUserDto>(user);

                _logger.LogInformation("Successful Google login for email: {Email}", email);

                if (!string.IsNullOrEmpty(returnUrl))
                    return Redirect($"{returnUrl}?token={token}");

                return Ok(new AuthResponseDto { Token = token, AdminUser = adminUserDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during Google login callback.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Password reset requested for email: {Email}", request.Email);

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                    return Ok("If the email exists, a reset link has been sent.");
                }

                var token = Guid.NewGuid().ToString();
                var verification = new EmailVerification
                {
                    UserId = user.UserId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(1),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _emailVerificationRepository.CreateAsync(verification);
                await _emailService.SendPasswordResetEmailAsync(request.Email, token);

                _logger.LogInformation("Password reset token generated for email: {Email}", request.Email);
                return Ok("If the email exists, a reset link has been sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing password reset request.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset for non-existent email: {Email}", request.Email);
                    return BadRequest("Invalid request.");
                }

                var verification = await _emailVerificationRepository.GetByTokenAsync(request.Token);
                if (verification == null || verification.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid or expired password reset token for email: {Email}", request.Email);
                    return BadRequest("Invalid or expired token.");
                }

                verification.IsUsed = true;
                await _emailVerificationRepository.SaveChangesAsync(verification);

                user.PasswordHash = PasswordHelper.Hash(request.NewPassword);
                await _userRepository.SaveChangesAsync(user);

                _logger.LogInformation("Password reset successful for email: {Email}", request.Email);
                return Ok("Password has been reset successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting password.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("generate-otp")]
        public async Task<ActionResult> GenerateOtp([FromBody] ForgotPasswordRequestDto request)
        {
            try
            {
                _logger.LogInformation("OTP generation requested for email: {Email}", request.Email);

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("OTP generation for non-existent email: {Email}", request.Email);
                    return Ok("If the email exists, an OTP has been sent.");
                }

                var otpCode = _otpService.GenerateOtp();
                var otp = new OtpVerification
                {
                    UserId = user.UserId,
                    OtpCode = otpCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _otpVerificationRepository.CreateAsync(otp);
                await _otpService.SendOtpAsync(request.Email, otpCode);

                _logger.LogInformation("OTP generated for email: {Email}", request.Email);
                return Ok("If the email exists, an OTP has been sent.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating OTP.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpPost("verify-otp")]
        public async Task<ActionResult> VerifyOtp([FromBody] VerifyOtpRequestDto request)
        {
            try
            {
                _logger.LogInformation("OTP verification attempt for email: {Email}", request.Email);

                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("OTP verification for non-existent email: {Email}", request.Email);
                    return BadRequest("Invalid OTP.");
                }

                var otp = await _otpVerificationRepository.GetValidOtpAsync(user.UserId, request.OtpCode);
                if (otp == null)
                {
                    _logger.LogWarning("Invalid or expired OTP for email: {Email}", request.Email);
                    return BadRequest("Invalid or expired OTP.");
                }

                otp.IsUsed = true;
                await _otpVerificationRepository.SaveChangesAsync(otp);

                _logger.LogInformation("OTP verified successfully for email: {Email}", request.Email);
                return Ok("OTP verified successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying OTP.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

                _logger.LogInformation("Password change attempt for user ID: {UserId}", userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for password change. ID: {UserId}", userId);
                    return NotFound("User not found.");
                }

                if (!PasswordHelper.Verify(request.CurrentPassword, user.PasswordHash))
                {
                    _logger.LogWarning("Incorrect current password for user ID: {UserId}", userId);
                    return BadRequest("Current password is incorrect.");
                }

                user.PasswordHash = PasswordHelper.Hash(request.NewPassword);
                await _userRepository.SaveChangesAsync(user);

                _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
                return Ok("Password changed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while changing password.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [AllowAnonymous]
        [HttpGet("me")]
        public async Task<ActionResult<AdminUserDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized();
                }

                _logger.LogInformation("Fetching current user with ID: {UserId}", userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Current user not found. ID: {UserId}", userId);
                    return NotFound("User not found.");
                }

                return Ok(_mapper.Map<AdminUserDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching current user.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        [Authorize]
        [HttpPatch("{userId}")]
        public async Task<ActionResult<AdminUserDto>> UpdateAdminUser(int userId, [FromBody] UpdateAdminUserDto updateDto)
        {
            try
            {
                _logger.LogInformation("Updating admin user with ID: {UserId}", userId);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Admin user with ID: {UserId} not found for update.", userId);
                    return NotFound("User not found.");
                }

                _mapper.Map(updateDto, user);
                await _userRepository.SaveChangesAsync(user);

                _logger.LogInformation("Successfully updated admin user with ID: {UserId}", userId);
                return Ok(_mapper.Map<AdminUserDto>(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating admin user with ID: {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}
