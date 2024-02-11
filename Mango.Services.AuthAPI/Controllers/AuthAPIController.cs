using eComm.MessageBus;
using eComm.Services.AuthAPI.Models.Dto;
using eComm.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace eComm.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMessageBus _messageBus;
        private readonly IConfiguration _configuration;
        protected ResponseDto _response;

        public AuthAPIController(IAuthService authService, IMessageBus messageBus, IConfiguration configuration)
        {
            _authService = authService;
            _messageBus = messageBus;
            _configuration = configuration;
            _response = new();
        }

        [HttpGet("{id:long}")]
        public IActionResult GetById(long id)
        {
            var todo = "aaaa";

            if (todo is null)
            {
                return NotFound();
            }

            return Ok(todo);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
        {

            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }
            //Logic to send message
            _messageBus.PublishMessage(model.Email, _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"));
            // After this to consume this message configure EmailAPI
            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "User name or password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
        {
            var assignRole = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRole)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            //_response.Result = assignRole;
            return Ok(_response);
        }


    }
}
