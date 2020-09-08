using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{    
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthenticationRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }   

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegister userForRegister)
        {            
            //convert username to lowercase
            userForRegister.Username = userForRegister.Username.ToLower();

            if(await _repo.UserExists(userForRegister.Username))
                return BadRequest("Username already exists");

            var UserToCreate = new User
            {
                Username = userForRegister.Username
            };

            var createdUser = await _repo.Register(UserToCreate, userForRegister.Password);

            return StatusCode(201);
        }    

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto loginUserDto)
        {
            var userFromRepo = await _repo.Login(loginUserDto.Username.ToLower(), loginUserDto.Password);

            if(userFromRepo == null)
                return Unauthorized();

            //Generate token fr login user if it exists
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {token = tokenHandler.WriteToken(token)});
        } 
    }
}