using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JWTAuthenDemo.Models;
using JWTAuthenDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuthenDemo.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public UserController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult<List<User>> Get() =>
            _userService.GetAllUser().ToList();

        [AllowAnonymous]
        [HttpPost]
        public ActionResult<User> Register([FromBody] User user)
        {
            try
            {
                _userService.Create(user, user.Password);
                return Ok();
            }
            catch (ApplicationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] User model)
        {
            var user = _userService.AuthorizUser(model.UserId, model.Password);
            if (user == null)
            {
                return BadRequest(new { message = "Username or password incorrect" });
            }

            IActionResult response = Unauthorized();

            if (user != null)
            {
                var tokenStr = GenerateJSONWebToken(user);
                response = Ok(new
                {
                    token = tokenStr
                });
            }
            return response;
        }

        private string GenerateJSONWebToken(User userData)
        {
            //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //var claims = new[]
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub,userData.UserId),
            //    new Claim(JwtRegisteredClaimNames.,userData.Email),
            //    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
            //};

            //var token = new JwtSecurityToken(
            //    issuer: _config["Jwt:Issuer"],
            //    audience: _config["Jwt:Issuer"],
            //    claims,
            //    expires: DateTime.Now.AddMinutes(120),
            //    signingCredentials: credentials);

            //var encodeToken = new JwtSecurityTokenHandler().WriteToken(token);
            //return encodeToken;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", userData.UserId.ToString()) }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost]
        public string PostValue()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            IList<Claim> claim = identity.Claims.ToList();
            var userName = claim[0].Value;
            return $"Welcome To {userName}";
        }

        [HttpGet]
        public string GetValue()
        {
            return "get value by authentication";
        }

    }
}
