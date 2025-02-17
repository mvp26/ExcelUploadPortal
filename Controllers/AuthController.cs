//using ExcelUploadPortal.Services;
//using Microsoft.AspNetCore.Mvc;

//namespace ExcelUploadPortal.Controllers
//{
//    [ApiController]
//    [Route("api/auth")]
//    public class AuthController : Controller
//    {
       
        
//           private readonly LdapAuthenticationService _ldapService;

//            public AuthController(LdapAuthenticationService ldapService)
//            {
//                _ldapService = ldapService;
//            }

//            [HttpPost("login")]
//            public IActionResult Login([FromBody] LoginRequest request)
//            {
//                if (_ldapService.AuthenticateUser(request.Username, request.Password))
//                {
//                    return Ok(new { Message = "Authentication Successful" });
//                }
//                return Unauthorized(new { Message = "Invalid Credentials" });
//            }
//        }

//        public class LoginRequest
//        {
//            public string Username { get; set; }
//            public string Password { get; set; }
//        }

//    }

